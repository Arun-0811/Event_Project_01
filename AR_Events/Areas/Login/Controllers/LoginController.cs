using AR_Events.Models;
using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace AR_Events.Areas.Login.Controllers
{
    [Area("Login")]
    

    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;
        private static string _otp;
        private static string _email;
        private readonly IWebHostEnvironment _env;

        public LoginController(IConfiguration config, IWebHostEnvironment env)
        {
            _configuration = config;
            _env = env;
        }

        
        [HttpGet("Index")]
        // GET: Login
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View("Index", model);

            string email = model.Email?.Trim();
            string password = model.Password?.Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Message = "Please enter email and password.";
                return View("Index", model);
            }

            string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(email, emailPattern))
            {
                ViewBag.Message = "Invalid email format.";
                return View("Index", model);
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("DevEventConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("sp_LoginUser", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", password);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        int userId = reader.GetInt32(0); // UserId
                        string username = reader.GetString(1); // Username
                        string photoPath = reader.IsDBNull(2) ? "default-avatar.png" : reader.GetString(2); // PhotoPath

                        // Save the UserId, Username, and PhotoPath to session
                        HttpContext.Session.SetInt32("UserId", userId);
                        HttpContext.Session.SetString("Username", username);
                        HttpContext.Session.SetString("PhotoPath", photoPath);

                        // Add claims and identity for authentication
                        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name, username)
                };

                        if (email == "admin@gmail.com" && password == "admin432")
                        {
                            HttpContext.Session.SetString("my-session", "admin-session-id");

                            // Add Admin-specific role claim
                            claims.Add(new Claim("Role", "Admin"));

                            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            var principal = new ClaimsPrincipal(identity);

                            await HttpContext.SignInAsync(
                                CookieAuthenticationDefaults.AuthenticationScheme,
                                principal,
                                new AuthenticationProperties
                                {
                                    IsPersistent = model.RememberMe,
                                    ExpiresUtc = model.RememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddMinutes(30)
                                });

                            TempData["Message"] = $"Welcome Admin {username}!";
                            return RedirectToAction("Index", "EventAdmin", new { area = "Admin" });
                        }
                        else
                        {
                            HttpContext.Session.SetString("my-session", "customer-session-id");

                            // Add Customer-specific role claim
                            claims.Add(new Claim("Role", "Customer"));

                            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            var principal = new ClaimsPrincipal(identity);

                            await HttpContext.SignInAsync(
                                CookieAuthenticationDefaults.AuthenticationScheme,
                                principal,
                                new AuthenticationProperties
                                {
                                    IsPersistent = model.RememberMe,
                                    ExpiresUtc = model.RememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddMinutes(30)
                                });

                            TempData["Message"] = $"Welcome {username}!";
                            return RedirectToAction("Index", "Home", new { area = "Customer" });
                        }
                    }
                    else
                    {
                        TempData["Message"] = "Invalid email or password.";
                        return View("Index", model);
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
                return View("Index", model);
            }
        }


        public IActionResult Logout()
        {
            // Clear all session data
            HttpContext.Session.Clear();

            // Redirect to login page (adjust controller name if needed)
            return RedirectToAction("Index", "Login");
        }


        // GET: /Login/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }


        [HttpPost]
        public IActionResult ForgotPassword(ForgotPasswordModel model, string actionType)
        {
            if (actionType == "SendOtp")
            {
                // We are not verifying OTP here, so ignore it during validation
                ModelState.Remove("Otp");

                if (string.IsNullOrWhiteSpace(model.Email) || !ModelState.IsValid)
                {
                    ViewBag.Message = "Please provide a valid email address.";
                    return View("ForgotPassword", model);
                }

                try
                {
                    Random rand = new Random();
                    _otp = rand.Next(100000, 999999).ToString();
                    _email = model.Email;

                    string from = "type8meenu@gmail.com";
                    string pass = "xjrn gjre jwky liwh";
                    string messagebox = "Your Verification OTP: " + _otp;

                    MailMessage message = new MailMessage
                    {
                        From = new MailAddress(from),
                        Subject = "OTP Verification",
                        Body = messagebox
                    };
                    message.To.Add(model.Email);

                    SmtpClient smtp = new SmtpClient("smtp.gmail.com")
                    {
                        EnableSsl = true,
                        Port = 587,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        Credentials = new NetworkCredential(from, pass)
                    };

                    smtp.Send(message);
                    ViewBag.Message = "OTP sent successfully to your email.";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = $"Error occurred while sending OTP: {ex.Message}";
                }
            }
            else if (actionType == "VerifyOtp")
            {
                // Validate everything including OTP
                if (!ModelState.IsValid)
                {
                    return View("ForgotPassword", model);
                }

                if (string.IsNullOrWhiteSpace(model.Otp) || model.Otp != _otp)
                {
                    ViewBag.Message = "Invalid OTP. Please try again.";
                    return View("ForgotPassword", model);
                }

                // Success
                return RedirectToAction("ResetPassword", "Login", new { email = model.Email });
            }

            return View("ForgotPassword", model);
        }





        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // File validation: Check for allowed image extensions and size limits (e.g., 5MB)
            string fileName = null;
            if (model.PhotoPath != null && model.PhotoPath.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(model.PhotoPath.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("PhotoPath", "Only image files are allowed (JPG, JPEG, PNG, GIF).");
                    return View(model);
                }

                if (model.PhotoPath.Length > 5 * 1024 * 1024) // 5MB limit
                {
                    ModelState.AddModelError("PhotoPath", "File size should not exceed 5MB.");
                    return View(model);
                }

                try
                {
                    // Generate a unique filename for the uploaded image
                    fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.PhotoPath.FileName);
                    string imagePath = Path.Combine(_env.WebRootPath, "Profile", fileName);

                    // Save the image to disk
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await model.PhotoPath.CopyToAsync(stream);
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = $"Error uploading image: {ex.Message}";
                    return View(model);
                }
            }

            // Hash the password before saving it to the database
            //string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password.Trim());

            // Insert the user into the database with or without the photo
            try
            {
                string connectionString = _configuration.GetConnectionString("DevEventConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("sp_RegisterUser", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Username", model.Username.Trim());
                    cmd.Parameters.AddWithValue("@Email", model.Email.Trim());
                    cmd.Parameters.AddWithValue("@MobileNo", model.MobileNo.Trim());
                    cmd.Parameters.AddWithValue("@City", model.City.Trim());
                    cmd.Parameters.AddWithValue("@State", model.State.Trim());
                    cmd.Parameters.AddWithValue("@Password", model.Password); // Save hashed password
                    cmd.Parameters.AddWithValue("@PhotoPath", fileName ?? string.Empty); // Save file path or empty if no file uploaded

                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        ViewBag.Message = "Registration successful!";
                        ViewBag.ProfileImage = model.PhotoPath;
                        ModelState.Clear(); // Clears validation messages
                        return RedirectToAction("Index", "Login", new { area = "Login" });
                    }
                    else
                    {
                        ViewBag.Message = "Registration failed. Please try again.";
                        return View(model);
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"An error occurred: {ex.Message}";
                return View(model);
            }
        }

        // GET: /Login/ResetPasswordSuccess
        // GET: /Login/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("ForgotPassword", "Login", new { area = "Login" });
            }

            return View(new ResetPasswordModel { EmailId = email });
        }

        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordModel model)
        {
            if (string.IsNullOrWhiteSpace(model.EmailId) ||
        string.IsNullOrWhiteSpace(model.NewPassword) ||
        string.IsNullOrWhiteSpace(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "All fields are required.");
                return View(model);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "New Password and Confirm Password do not match.");
                return View(model);
            }

            string connectionString = _configuration.GetConnectionString("DevEventConnection");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                // Check if the user exists
                string checkUserQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
                SqlCommand checkCmd = new SqlCommand(checkUserQuery, con);
                checkCmd.Parameters.AddWithValue("@Email", model.EmailId);

                int userExists = (int)checkCmd.ExecuteScalar();
                if (userExists == 0)
                {
                    ModelState.AddModelError("", "Email not found.");
                    return View(model);
                }

                // Update password
                string updateQuery = "UPDATE Users SET Password = @Password WHERE Email = @Email";
                SqlCommand updateCmd = new SqlCommand(updateQuery, con);
                updateCmd.Parameters.AddWithValue("@Email", model.EmailId);
                updateCmd.Parameters.AddWithValue("@Password", model.NewPassword); // ⚠️ Consider hashing this in production

                int rowsAffected = updateCmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    ViewBag.Message = "Password has been reset successfully!";
                    ModelState.Clear(); // Clears validation messages
                    return RedirectToAction("Index", "Login", new { area = "Login" });
                }
                else
                {
                    ModelState.AddModelError("", "Something went wrong while updating the password.");
                    return View(model);
                }
            }
        }
        public IActionResult AccessDenied()
        {
            return View("AccessDenied");
        }

    }
}
