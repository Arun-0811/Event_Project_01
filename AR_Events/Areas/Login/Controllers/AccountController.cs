using AR_Events.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Data;
using System.Linq;
using System.Security.Claims;

namespace AR_Events.Areas.Login.Controllers
{
    [Area("Login")]
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AccountController> _logger;
        public AccountController(IConfiguration configuration, IWebHostEnvironment env, ILogger<AccountController> logger)
        {
            _configuration = configuration;
            _env = env;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> MyProfile()
        {
            // Retrieve UserId from session
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null) // Ensure session exists
            {
                return RedirectToAction("Index", "Login"); // Redirect if the user is not logged in
            }

            var profile = new EditProfileViewModel();

            try
            {
                using (var con = new SqlConnection(_configuration.GetConnectionString("DevEventConnection")))
                using (var cmd = new SqlCommand("sp_GetUserById", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", userId.Value); // Pass UserId to the stored procedure

                    await con.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync()) // No matching user
                        {
                            TempData["Message"] = "User profile not found!";
                            return RedirectToAction("Index", "Login");
                        }

                        // Map reader values to UserProfileModel with null checks
                        profile.UserId = reader.GetInt32(reader.GetOrdinal("UserId"));
                        profile.Username = reader.GetString(reader.GetOrdinal("Username"));
                        profile.Email = reader.GetString(reader.GetOrdinal("Email"));
                        profile.MobileNo = reader.GetString(reader.GetOrdinal("MobileNo"));
                        profile.PhotoPath = reader.IsDBNull(reader.GetOrdinal("PhotoPath"))
                            ? "default-avatar.png" // Use default if PhotoPath is null
                            : reader.GetString(reader.GetOrdinal("PhotoPath"));
                    }
                }

                // Welcome message via TempData
                TempData["Message"] = $"Welcome {profile.Username}!";
                return View(profile); // Return the profile view
            }
            catch (Exception ex)
            {
                // Log the exception (if logging is configured)
                _logger.LogError(ex, "An error occurred while fetching user profile");

                // Handle exception gracefully
                ViewBag.ErrorMessage = "An unexpected error occurred. Please try again later.";
                return View("Error"); // Redirect to an error page
            }
        }




        [HttpGet]
        public async Task<IActionResult> EditProfile(int id)
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (sessionUserId == null || sessionUserId != id)
            {
                TempData["Message"] = "Unauthorized access or session expired. Please log in again.";
                return RedirectToAction("Index", "Login");
            }

            var profile = new EditProfileViewModel();

            try
            {
                using (var con = new SqlConnection(_configuration.GetConnectionString("DevEventConnection")))
                using (var cmd = new SqlCommand("sp_GetUserById", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", id);

                    await con.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (!await reader.ReadAsync())
                        {
                            TempData["Message"] = "User profile not found!";
                            return RedirectToAction("MyProfile");
                        }

                        profile.UserId = reader.GetInt32(reader.GetOrdinal("UserId"));
                        profile.Username = reader.GetString(reader.GetOrdinal("Username"));
                        profile.Email = reader.GetString(reader.GetOrdinal("Email"));
                        profile.MobileNo = reader.GetString(reader.GetOrdinal("MobileNo"));
                        profile.PhotoPath = reader.IsDBNull(reader.GetOrdinal("PhotoPath"))
                            ? "default-avatar.png"
                            : reader.GetString(reader.GetOrdinal("PhotoPath"));
                    }
                }

                ViewBag.PhotoPath = profile.PhotoPath; // ✅ Pass current photo to view
                return View(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user profile for editing.");
                ViewBag.ErrorMessage = "An unexpected error occurred. Please try again later.";
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.PhotoPath = model.PhotoPath; // Show existing image on validation error
                return View(model);
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Message"] = "Session expired, please log in again.";
                return RedirectToAction("Index", "Login");
            }

            string photoPath = model.PhotoPath ?? "default-avatar.png";

            // ✅ Handle new photo upload
            if (model.PhotoFile != null && model.PhotoFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "Profile");
                Directory.CreateDirectory(uploadsFolder); // Ensure folder exists

                photoPath = $"{userId.Value}_{Path.GetFileName(model.PhotoFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, photoPath);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.PhotoFile.CopyToAsync(stream);
                }
            }

            try
            {
                using (var con = new SqlConnection(_configuration.GetConnectionString("DevEventConnection")))
                using (var cmd = new SqlCommand("sp_UpdateUserProfile", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", userId.Value);
                    cmd.Parameters.AddWithValue("@Username", model.Username);
                    cmd.Parameters.AddWithValue("@Email", model.Email);
                    cmd.Parameters.AddWithValue("@MobileNo", model.MobileNo);
                    cmd.Parameters.AddWithValue("@PhotoPath", photoPath);

                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }

                TempData["Message"] = "Profile updated successfully!";
                return RedirectToAction("MyProfile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating user profile.");
                ViewBag.PhotoPath = model.PhotoPath;
                ViewBag.ErrorMessage = "An unexpected error occurred. Please try again later.";
                return View(model);
            }
        }

    }

}
