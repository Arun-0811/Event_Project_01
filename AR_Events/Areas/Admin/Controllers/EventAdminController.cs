using AR_Events.Areas.Login.Controllers;
using AR_Events.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AR_Events.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Policy = "AdminPolicy")]
    public class EventAdminController : BaseController
    {
        private readonly IConfiguration _configuration;
        
        private readonly IWebHostEnvironment _env;

        public EventAdminController(IConfiguration config, IWebHostEnvironment env)
        {
            _configuration = config;
            _env = env;
        }

        [Authorize]
        // GET: EventAdminController
        public ActionResult Index()
        {
            // Check session validity
            var session = HttpContext.Session.GetString("my-session");
            if (string.IsNullOrEmpty(session) || session != "admin-session-id")
            {
                return RedirectToAction("Index", "Login");
            }

            List<EventBookModel> eventList = new List<EventBookModel>();

            string constr = _configuration.GetConnectionString("DevEventConnection");

            using (SqlConnection con = new SqlConnection(constr))
            {
                SqlCommand cmd = new SqlCommand("sp_event_fetch_all", con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                con.Open();
                using (SqlDataReader sdr = cmd.ExecuteReader())
                {
                    while (sdr.Read()) // ⬅ Use while to fetch all rows
                    {
                        EventBookModel eventItem = new EventBookModel
                        {
                            Id = Convert.ToInt32(sdr["Event_Id"]),
                            Event_Name = sdr["Event_Name"].ToString(),
                            Event_StartDate = Convert.ToDateTime(sdr["Event_StartDate"]),
                            Event_EndDate = Convert.ToDateTime(sdr["Event_EndDate"]),
                            Event_StartTime = TimeSpan.Parse(sdr["Event_StartTime"].ToString()),
                            Event_EndTime = TimeSpan.Parse(sdr["Event_EndTime"].ToString()),
                            Event_location = sdr["Event_location"].ToString(),
                            Event_Organizer = sdr["Event_Organizer"].ToString(),
                            EventCost_PerPerson = Convert.ToDecimal(sdr["EventCost_PerPerson"]),
                            GST = Convert.ToDecimal(sdr["GST"]),
                            Conv_Fee = Convert.ToDecimal(sdr["Conv_Fee"]),
                            Total_Amt = Convert.ToDecimal(sdr["Total_Amt"]),
                            ImagePath = sdr["ImagePath"].ToString()
                        };

                        eventList.Add(eventItem); // ⬅ Add to the list
                    }
                }
            }

            return View(eventList); // ⬅ Pass list to the view
        }

        [Authorize]
        // GET: EventAdminController/Details/5
        public ActionResult Details(int id)
        {
            

            EventBookModel eventItem = null;

            string constr = _configuration.GetConnectionString("DevEventConnection");

            using (SqlConnection con = new SqlConnection(constr))
            {
                SqlCommand cmd = new SqlCommand("sp_event_fetch_by_id", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Event_Id", id);

                con.Open();
                using (SqlDataReader sdr = cmd.ExecuteReader())
                {
                    if (sdr.Read()) // Only one record expected
                    {
                        eventItem = new EventBookModel
                        {
                            Id = Convert.ToInt32(sdr["Event_Id"]),
                            Event_Name = sdr["Event_Name"].ToString(),
                            Event_StartDate = Convert.ToDateTime(sdr["Event_StartDate"]),
                            Event_EndDate = Convert.ToDateTime(sdr["Event_EndDate"]),
                            Event_StartTime = TimeSpan.Parse(sdr["Event_StartTime"].ToString()),
                            Event_EndTime = TimeSpan.Parse(sdr["Event_EndTime"].ToString()),
                            Event_location = sdr["Event_location"].ToString(),
                            Event_Organizer = sdr["Event_Organizer"].ToString(),
                            EventCost_PerPerson = Convert.ToDecimal(sdr["EventCost_PerPerson"]),
                            GST = Convert.ToDecimal(sdr["GST"]),
                            Conv_Fee = Convert.ToDecimal(sdr["Conv_Fee"]),
                            Total_Amt = Convert.ToDecimal(sdr["Total_Amt"]),
                            ImagePath = sdr["ImagePath"].ToString()
                        };
                    }
                }
            }

            if (eventItem == null)
            {
                return NotFound(); // Optional: Handle if no event is found
            }

            return View(eventItem); // Pass single object to the view
        }

        [Authorize]
        // GET: EventAdminController/Create
        public ActionResult Create()
        {
            

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EventBookModel model, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                string fileName = null;

                if (imageFile != null && imageFile.Length > 0)
                {
                    // Create unique filename for the image
                    fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string imagePath = Path.Combine(_env.WebRootPath, "images", fileName);

                    // Save the image to the disk
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        imageFile.CopyTo(stream);
                    }
                }

                string constr = _configuration.GetConnectionString("DevEventConnection");

                using (SqlConnection con = new SqlConnection(constr))
                {
                    SqlCommand cmd = new SqlCommand("sp_event_create", con)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    // Add parameters for the event
                    cmd.Parameters.AddWithValue("@Event_Name", model.Event_Name);
                    cmd.Parameters.AddWithValue("@Event_StartDate", model.Event_StartDate);
                    cmd.Parameters.AddWithValue("@Event_EndDate", model.Event_EndDate);
                    cmd.Parameters.AddWithValue("@Event_StartTime", model.Event_StartTime);
                    cmd.Parameters.AddWithValue("@Event_EndTime", model.Event_EndTime);
                    cmd.Parameters.AddWithValue("@Event_location", model.Event_location);
                    cmd.Parameters.AddWithValue("@Event_Organizer", model.Event_Organizer);
                    cmd.Parameters.AddWithValue("@EventCost_PerPerson", model.EventCost_PerPerson);
                    cmd.Parameters.AddWithValue("@GST", model.GST);
                    cmd.Parameters.AddWithValue("@Conv_Fee", model.Conv_Fee);
                    cmd.Parameters.AddWithValue("@Total_Amt", model.Total_Amt);
                    cmd.Parameters.AddWithValue("@ImagePath", fileName);


                    // Execute the command
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                return RedirectToAction("Index");
            }

            // Log model validation errors for debugging
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                foreach (var error in state.Errors)
                {
                    Console.WriteLine($"Key: {key}, Error: {error.ErrorMessage}");
                }
            }

            return View(model);
        }



        [HttpGet]
        [Authorize]
        public IActionResult Edit(int id)
        {
            EventBookModel eventItem = null;

            string constr = _configuration.GetConnectionString("DevEventConnection");

            using (SqlConnection con = new SqlConnection(constr))
            {
                SqlCommand cmd = new SqlCommand("sp_event_fetch_by_id", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Event_Id", id);

                con.Open();
                using (SqlDataReader sdr = cmd.ExecuteReader())
                {
                    if (sdr.Read())
                    {
                        eventItem = new EventBookModel
                        {
                            Id = Convert.ToInt32(sdr["Event_Id"]),
                            Event_Name = sdr["Event_Name"].ToString(),
                            Event_StartDate = Convert.ToDateTime(sdr["Event_StartDate"]),
                            Event_EndDate = Convert.ToDateTime(sdr["Event_EndDate"]),
                            Event_StartTime = TimeSpan.Parse(sdr["Event_StartTime"].ToString()),
                            Event_EndTime = TimeSpan.Parse(sdr["Event_EndTime"].ToString()),
                            Event_location = sdr["Event_location"].ToString(),
                            Event_Organizer = sdr["Event_Organizer"].ToString(),
                            EventCost_PerPerson = Convert.ToDecimal(sdr["EventCost_PerPerson"]),
                            GST = Convert.ToDecimal(sdr["GST"]),
                            Conv_Fee = Convert.ToDecimal(sdr["Conv_Fee"]),
                            Total_Amt = Convert.ToDecimal(sdr["Total_Amt"]),
                            ImagePath = sdr["ImagePath"].ToString()
                        };
                    }
                }
            }

            if (eventItem == null)
            {
                return NotFound();
            }

            return View(eventItem); // Sends model to Edit.cshtml
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(EventBookModel model, IFormFile? imageFile, string? existingImagePath)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Retain the existing image path if no new image is uploaded
                    string fileName = existingImagePath ?? model.ImagePath;

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Only delete the old image if a new one is uploaded
                        if (!string.IsNullOrEmpty(existingImagePath))
                        {
                            string oldImagePath = Path.Combine(_env.WebRootPath, "images", existingImagePath);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Save new image
                        fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        string newImagePath = Path.Combine(_env.WebRootPath, "images", fileName);
                        using (var stream = new FileStream(newImagePath, FileMode.Create))
                        {
                            imageFile.CopyTo(stream);
                        }
                    }

                    // Update database with the image file name and other event details
                    string constr = _configuration.GetConnectionString("DevEventConnection");
                    using (SqlConnection con = new SqlConnection(constr))
                    {
                        SqlCommand cmd = new SqlCommand("sp_event_update_by_id", con)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        cmd.Parameters.AddWithValue("@Event_Id", model.Id);
                        cmd.Parameters.AddWithValue("@Event_Name", model.Event_Name);
                        cmd.Parameters.AddWithValue("@Event_StartDate", model.Event_StartDate);
                        cmd.Parameters.AddWithValue("@Event_EndDate", model.Event_EndDate);
                        cmd.Parameters.AddWithValue("@Event_StartTime", model.Event_StartTime);
                        cmd.Parameters.AddWithValue("@Event_EndTime", model.Event_EndTime);
                        cmd.Parameters.AddWithValue("@Event_location", model.Event_location);
                        cmd.Parameters.AddWithValue("@Event_Organizer", model.Event_Organizer);
                        cmd.Parameters.AddWithValue("@EventCost_PerPerson", model.EventCost_PerPerson);
                        cmd.Parameters.AddWithValue("@GST", model.GST);
                        cmd.Parameters.AddWithValue("@Conv_Fee", model.Conv_Fee);
                        cmd.Parameters.AddWithValue("@Total_Amt", model.Total_Amt);

                        cmd.Parameters.Add("@ImagePath", SqlDbType.NVarChar, 255).Value = string.IsNullOrEmpty(fileName) ? (object)DBNull.Value : fileName;

                        con.Open();
                        cmd.ExecuteNonQuery();
                    }

                    TempData["SuccessMessage"] = "Event updated successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
                    return View(model);
                }
            }

            return View(model);
        }





        [HttpGet]
        [Authorize]
        public IActionResult Delete(int id)
        {
            

            EventBookModel eventItem = null;

            string constr = _configuration.GetConnectionString("DevEventConnection");

            using (SqlConnection con = new SqlConnection(constr))
            {
                SqlCommand cmd = new SqlCommand("sp_event_fetch_by_id", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Event_Id", id);

                con.Open();
                using (SqlDataReader sdr = cmd.ExecuteReader())
                {
                    if (sdr.Read())
                    {
                        eventItem = new EventBookModel
                        {
                            Id = Convert.ToInt32(sdr["Event_Id"]),
                            Event_Name = sdr["Event_Name"].ToString(),
                            Event_StartDate = Convert.ToDateTime(sdr["Event_StartDate"]),
                            Event_EndDate = Convert.ToDateTime(sdr["Event_EndDate"]),
                            Event_StartTime = TimeSpan.Parse(sdr["Event_StartTime"].ToString()),
                            Event_EndTime = TimeSpan.Parse(sdr["Event_EndTime"].ToString()),
                            Event_location = sdr["Event_location"].ToString(),
                            Event_Organizer = sdr["Event_Organizer"].ToString(),
                            EventCost_PerPerson = Convert.ToDecimal(sdr["EventCost_PerPerson"]),
                            GST = Convert.ToDecimal(sdr["GST"]),
                            Conv_Fee = Convert.ToDecimal(sdr["Conv_Fee"]),
                            Total_Amt = Convert.ToDecimal(sdr["Total_Amt"]),
                            ImagePath = sdr["ImagePath"].ToString()
                        };
                    }
                }
            }

            if (eventItem == null)
            {
                return NotFound(); // or return RedirectToAction("Index");
            }

            return View(eventItem); // ✅ pass model to the view
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                string imagePath = null;
                string constr = _configuration.GetConnectionString("DevEventConnection");

                // Get image path before deleting record
                using (SqlConnection con = new SqlConnection(constr))
                {
                    SqlCommand cmdFetch = new SqlCommand("sp_event_fetch_by_id", con)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmdFetch.Parameters.AddWithValue("@Event_Id", id);
                    con.Open();

                    using (SqlDataReader sdr = cmdFetch.ExecuteReader())
                    {
                        if (sdr.Read())
                        {
                            imagePath = sdr["ImagePath"].ToString();
                        }
                    }
                }

                // Delete image file
                if (!string.IsNullOrEmpty(imagePath))
                {
                    string fullPath = Path.Combine(_env.WebRootPath, "images", imagePath);
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }

                // Delete event from database
                using (SqlConnection con = new SqlConnection(constr))
                {
                    SqlCommand cmd = new SqlCommand("sp_event_delete_by_id", con)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.AddWithValue("@Event_Id", id);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["Message"] = "Event deleted successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex +  "An error occurred while deleting the event. Please try again later.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult DeleteAll()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Index", "Login", new { area = "Login" });

            string constr = _configuration.GetConnectionString("DevEventConnection");
            using (SqlConnection con = new SqlConnection(constr))
            {
                SqlCommand cmd = new SqlCommand("sp_event_truncate_all", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                con.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["Message"] = "All events deleted successfully.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize]
        public IActionResult ViewEnquiries()
        {
            List<GetAllEnquiryViewModel> enquiries = new List<GetAllEnquiryViewModel>();

            string connStr = _configuration.GetConnectionString("DevEventConnection");

            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("sp_get_all_enquiries", con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    enquiries.Add(new GetAllEnquiryViewModel
                    {
                        EnquiryId = Convert.ToInt32(reader["EnquiryId"]),
                        Name = reader["Name"].ToString(),
                        Email = reader["Email"].ToString(),
                        Subject = reader["Subject"].ToString(),
                        Message = reader["Message"].ToString(),
                        CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                    });
                }
            }

            return View(enquiries);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteEnquiry(int enquiryId)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DevEventConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("sp_delete_enquiry_by_id", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EnquiryId", enquiryId);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Enquiry deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error deleting enquiry: " + ex.Message;
            }

            return RedirectToAction("ViewEnquiries");
        }

    }
}
