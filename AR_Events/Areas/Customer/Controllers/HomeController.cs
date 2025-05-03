using AR_Events.Areas.Login.Controllers;
using AR_Events.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Diagnostics;

namespace AR_Events.Areas.Customer.Controllers
{
    [Area("Customer")]
    //[Authorize(Policy = "CustomerPolicy")]
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IConfiguration _configuration;        

        public HomeController(ILogger<HomeController> logger, IConfiguration config)
        {
            _logger = logger;
            _configuration = config;
        }
        [Authorize]
        public IActionResult Index()
        {
            // Check session validity
            var session = HttpContext.Session.GetString("my-session");
            if (string.IsNullOrEmpty(session) || session != "customer-session-id")
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }

        [Route("About Us")]
        [Authorize]
        public IActionResult AboutUs()
        {
            // Check session validity
            var session = HttpContext.Session.GetString("my-session");
            if (string.IsNullOrEmpty(session) || session != "customer-session-id")
            {
                return RedirectToAction("Index", "Login");
            }


            return View();
        }

        [Authorize]
        public IActionResult Services()
        {
            // Check session validity
            var session = HttpContext.Session.GetString("my-session");
            if (string.IsNullOrEmpty(session) || session != "customer-session-id")
            {
                return RedirectToAction("Index", "Login");
            }


            return View();
        }

        [Route("Corporate Events")]
        [Authorize]
        public IActionResult CorporateEvents()
        {
            // Check session validity
            var session = HttpContext.Session.GetString("my-session");
            if (string.IsNullOrEmpty(session) || session != "customer-session-id")
            {
                return RedirectToAction("Index", "Login");
            }


            return View();
        }

        [Authorize]
        public IActionResult Enquiry()
        {
            // Check session validity
            var session = HttpContext.Session.GetString("my-session");
            if (string.IsNullOrEmpty(session) || session != "customer-session-id")
            {
                return RedirectToAction("Index", "Login");
            }


            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitEnquiry(Cus_EnquiryViewModel model)
        {
            

            int CusId = HttpContext.Session.GetInt32("UserId") ?? 0;
            string Name = HttpContext.Session.GetString("Username");

            if (!ModelState.IsValid)
            {
                // Debug or log validation errors here
                foreach (var entry in ModelState)
                {
                    if (entry.Value.Errors.Any())
                    {
                        var field = entry.Key;
                        var errorMessages = entry.Value.Errors.Select(e => e.ErrorMessage).ToList();
                        // You can log them or inspect in debugger
                        _logger.LogWarning($"Validation failed for field: {field}, Errors: {string.Join(", ", errorMessages)}");
                    }
                }

                return View("Enquiry", model); // Re-show form with errors
            }

            try
            {
                var connectionString = _configuration.GetConnectionString("DevEventConnection");

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "INSERT INTO Cus_EnquiryTable (cus_id, name, subject, Message, CreatedAt) " +
                        "VALUES (@CusId, @Name, @Subject, @Message, @CreatedAt)", conn))
                    {
                        cmd.Parameters.AddWithValue("@CusId", CusId);
                        cmd.Parameters.AddWithValue("@Name", Name);
                        cmd.Parameters.AddWithValue("@Subject", model.Subject ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@Message", model.Message ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                        conn.Open();
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                TempData["Success"] = "Your enquiry has been submitted successfully!";
                return RedirectToAction("Enquiry");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting enquiry.");
                ModelState.AddModelError("", "There was an error submitting your enquiry. Please try again later.");
                return View("Enquiry", model);
            }
        }


    }
}

