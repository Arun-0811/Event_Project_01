using AR_Events.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AR_Events.Areas.Guest.Controllers
{
    [Area("Guest")]
    
    public class GuestController : Controller
    {
        private readonly IConfiguration _configuration;
        public GuestController(IConfiguration config)
        {
            _configuration = config;
        }
        [HttpGet("")]
        [HttpGet("GuestIndex")]
        // GET: Guest/Index
        public IActionResult Index()
        {
            ViewData["Title"] = "Welcome to AREvents";
            return View();
        }


        [HttpPost]
        public IActionResult SubmitEnquiry(EnquiryViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Log the values of the model to see if they're being populated
                Console.WriteLine($"Name: {model.Name}, Email: {model.Email}, Subject: {model.Subject}, Message: {model.Message}");

                string connectionString = _configuration.GetConnectionString("DevEventConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand("InsertEnquiry", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Add parameters corresponding to the stored procedure
                    cmd.Parameters.AddWithValue("@Name", model.Name);
                    cmd.Parameters.AddWithValue("@Email", model.Email);
                    cmd.Parameters.AddWithValue("@Subject", model.Subject);
                    cmd.Parameters.AddWithValue("@Message", model.Message);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Your Enquiry has been submitted successfully!";
                return RedirectToAction("Enquiry", "Home", new { area = "Customer" });


            }
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    Console.WriteLine($"Key: {state.Key}, Error: {error.ErrorMessage}");
                }
            }

            return View(model);
        }

    }
}
