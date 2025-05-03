using AR_Events.Areas.Login.Controllers;
using AR_Events.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Security.Claims;


namespace AR_Events.Areas.Customer.Controllers
{
    [Area("Customer")]
    //[Authorize(Policy = "CustomerPolicy")]
    public class EventDetailsController : BaseController
    {
        private readonly IConfiguration _configuration;

        public EventDetailsController(IConfiguration config)
        {
            _configuration = config;
        }

        [HttpGet]
        [Authorize]
        public ActionResult Index()
        {
            // Check session validity
            var session = HttpContext.Session.GetString("my-session");
            if (string.IsNullOrEmpty(session) || session != "customer-session-id")
            {
                return RedirectToAction("Index", "Login");
            }


            List<EventBookModel> eventList = new List<EventBookModel>();

            string constr = _configuration.GetConnectionString("DevEventConnection");

            using (SqlConnection con = new SqlConnection(constr))
            {
                SqlCommand cmd = new SqlCommand("sp_event_fetch_cardData", con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                con.Open();
                using (SqlDataReader sdr = cmd.ExecuteReader())
                {
                    while (sdr.Read())
                    {
                        eventList.Add(new EventBookModel
                        {
                            Id = Convert.ToInt32(sdr["Event_Id"]),
                            Event_Name = sdr["Event_Name"].ToString(),
                            Event_StartDate = Convert.ToDateTime(sdr["Event_StartDate"]),
                            Event_EndDate = Convert.ToDateTime(sdr["Event_EndDate"]),
                            EventCost_PerPerson = Convert.ToDecimal(sdr["EventCost_PerPerson"]),
                            GST = Convert.ToDecimal(sdr["GST"]),
                            ImagePath = sdr["ImagePath"].ToString()
                        });
                    }
                }
            }

            return View(eventList);
        }



        [HttpGet]
        [Authorize]
        // GET: EventDetailsController/Details/5
        public IActionResult Details(int id)
        {
            // Check session validity
            var session = HttpContext.Session.GetString("my-session");
            if (string.IsNullOrEmpty(session) || session != "customer-session-id")
            {
                return RedirectToAction("Index", "Login");
            }


            EventBookModel eventItem = null;
            List<EventBookModel> relatedEvents = new List<EventBookModel>();
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
                            
                            BookedDate = sdr["BookedDate"] != DBNull.Value ? Convert.ToDateTime(sdr["BookedDate"]) : DateTime.MinValue,
                            EventCost_PerPerson = Convert.ToDecimal(sdr["EventCost_PerPerson"]),
                            GST = Convert.ToDecimal(sdr["GST"]),
                            Conv_Fee = Convert.ToDecimal(sdr["Conv_Fee"]),
                            Total_Amt = Convert.ToDecimal(sdr["Total_Amt"]),
                            ImagePath = sdr["ImagePath"].ToString()
                        };
                    }
                }

                if (eventItem != null)
                {
                    SqlCommand relCmd = new SqlCommand("sp_events_by_organizer", con)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    relCmd.Parameters.AddWithValue("@Organizer", eventItem.Event_Organizer);
                    relCmd.Parameters.AddWithValue("@CurrentEventId", eventItem.Id);

                    using (SqlDataReader sdr = relCmd.ExecuteReader())
                    {
                        while (sdr.Read())
                        {
                            relatedEvents.Add(new EventBookModel
                            {
                                Id = Convert.ToInt32(sdr["Event_Id"]),
                                Event_Name = sdr["Event_Name"].ToString(),
                                Event_StartDate = Convert.ToDateTime(sdr["Event_StartDate"]),
                                Event_EndDate = Convert.ToDateTime(sdr["Event_EndDate"]),
                                EventCost_PerPerson = Convert.ToDecimal(sdr["EventCost_PerPerson"]),
                                GST = Convert.ToDecimal(sdr["GST"]),
                                ImagePath = sdr["ImagePath"].ToString()
                            });
                        }
                    }
                }
            }

            if (eventItem == null)
                return NotFound();

            ViewBag.RelatedEvents = relatedEvents;
            return View(eventItem);
        }

        [HttpGet]
        [Authorize]
        private RelatedEventModel GetEventById(int id)
        {
            
            RelatedEventModel eventDetail = null;
            string connectionString = _configuration.GetConnectionString("DevEventConnection");

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM EventDetails_TableForBook WHERE Event_Id = @Id", con))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                con.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        eventDetail = new RelatedEventModel
                        {
                            Id = Convert.ToInt32(reader["Event_Id"]),
                            Event_Name = reader["Event_Name"].ToString(),
                            Event_StartDate = Convert.ToDateTime(reader["Event_StartDate"]),
                            Event_EndDate = Convert.ToDateTime(reader["Event_EndDate"]),
                            EventCost_PerPerson = Convert.ToDecimal(reader["EventCost_PerPerson"]),
                            GST = Convert.ToDecimal(reader["GST"]),
                            Event_Organizer = reader["Event_Organizer"].ToString()
                            
                        };
                    }
                }
            }

            return eventDetail;
        }




        [HttpGet]
        [Authorize]
        public IActionResult MoreEventDetails(RelatedEventModel REModel, int id)
        {
            // Check session validity
            var session = HttpContext.Session.GetString("my-session");
            if (string.IsNullOrEmpty(session) || session != "customer-session-id")
            {
                return RedirectToAction("Index", "Login");
            }


            var eventDetails = GetEventById(id); // Your logic to get main event details
            var organizer = eventDetails?.Event_Organizer;

            if (!string.IsNullOrEmpty(organizer))
            {
                var relatedEvents = new List<RelatedEventModel>();
                string connectionString = _configuration.GetConnectionString("DevEventConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("sp_events_by_organizer", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Organizer", organizer);
                    cmd.Parameters.AddWithValue("@CurrentEventId", id);

                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            relatedEvents.Add(new RelatedEventModel
                            {
                                Id = Convert.ToInt32(reader["Event_Id"]),
                                Event_Name = reader["Event_Name"].ToString(),
                                Event_StartDate = Convert.ToDateTime(reader["Event_StartDate"]),
                                Event_EndDate = Convert.ToDateTime(reader["Event_EndDate"]),
                                EventCost_PerPerson = Convert.ToDecimal(reader["EventCost_PerPerson"]),
                                GST = Convert.ToDecimal(reader["GST"]),
                                Event_Organizer = reader["Event_Organizer"].ToString()
                            });
                        }
                    }
                }

                ViewBag.RelatedEvents = relatedEvents;
            }

            return View(eventDetails); // Assuming your main event is strongly typed
        }

        [HttpGet("Customer/EventDetails/BookedHistory")]
        [Authorize]
        public IActionResult BookedHistory()
        {
            // Check session validity
            var session = HttpContext.Session.GetString("my-session");
            if (string.IsNullOrEmpty(session) || session != "customer-session-id")
            {
                return RedirectToAction("Index", "Login");
            }


            // 🔐 Ensure user is authenticated
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Login", new { area = "Login" });

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Logged-in user's ID

            List<EventBookModel> bookedEvents = new List<EventBookModel>();
            string connectionString = _configuration.GetConnectionString("DevEventConnection");

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("sp_get_all_booked_events", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        bookedEvents.Add(new EventBookModel
                        {
                            Id = reader["Event_Id"] != DBNull.Value ? Convert.ToInt32(reader["Event_Id"]) : 0,
                            
                            Event_Name = reader["Event_Name"]?.ToString(),
                            Event_StartDate = reader["Event_StartDate"] != DBNull.Value ? Convert.ToDateTime(reader["Event_StartDate"]) : DateTime.MinValue,
                            Event_EndDate = reader["Event_EndDate"] != DBNull.Value ? Convert.ToDateTime(reader["Event_EndDate"]) : DateTime.MinValue,
                            Event_StartTime = reader["Event_StartTime"] != DBNull.Value ? TimeSpan.Parse(reader["Event_StartTime"].ToString()) : TimeSpan.Zero,
                            Event_EndTime = reader["Event_EndTime"] != DBNull.Value ? TimeSpan.Parse(reader["Event_EndTime"].ToString()) : TimeSpan.Zero,
                            Event_location = reader["Event_location"]?.ToString(),
                            Event_Organizer = reader["Event_Organizer"]?.ToString(),
                            EventCost_PerPerson = reader["EventCost_PerPerson"] != DBNull.Value ? Convert.ToDecimal(reader["EventCost_PerPerson"]) : 0,
                            GST = reader["GST"] != DBNull.Value ? Convert.ToDecimal(reader["GST"]) : 0,
                            Conv_Fee = reader["Conv_Fee"] != DBNull.Value ? Convert.ToDecimal(reader["Conv_Fee"]) : 0,
                            TicketCount = reader["TicketCount"] != DBNull.Value ? Convert.ToInt32(reader["TicketCount"]) : 0,
                            Total_Amt = reader["Total_Amt"] != DBNull.Value ? Convert.ToDecimal(reader["Total_Amt"]) : 0,
                            BookedDate = reader["BookedDate"] != DBNull.Value ? Convert.ToDateTime(reader["BookedDate"]) : DateTime.MinValue
                        });
                    }
                }
            }

            return View(bookedEvents);
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmBooking(EventBookModel model)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                    return RedirectToAction("Index", "Login", new { area = "Login" });

                //// 🔐 Get logged-in user's ID
                //string userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                //if (!int.TryParse(userIdStr, out int userId))
                //{
                //    TempData["Error"] = "Invalid user ID.";
                //    return RedirectToAction("Index");
                //}

                string connectionString = _configuration.GetConnectionString("DevEventConnection");

                using (SqlConnection con = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand("sp_insert_booked_event", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // 📥 Add parameters to stored procedure
                    cmd.Parameters.AddWithValue("@Event_Id", model.Id);
                    
                    cmd.Parameters.AddWithValue("@Event_Name", model.Event_Name);
                    cmd.Parameters.AddWithValue("@Event_StartDate", model.Event_StartDate);
                    cmd.Parameters.AddWithValue("@Event_EndDate", model.Event_EndDate);
                    cmd.Parameters.AddWithValue("@Event_StartTime", model.Event_StartTime);
                    cmd.Parameters.AddWithValue("@Event_EndTime", model.Event_EndTime);
                    cmd.Parameters.AddWithValue("@Event_location", model.Event_location);
                    cmd.Parameters.AddWithValue("@Event_Organizer", model.Event_Organizer);
                    cmd.Parameters.AddWithValue("@TicketCount", model.TicketCount);
                    cmd.Parameters.AddWithValue("@EventCost_PerPerson", model.EventCost_PerPerson);
                    cmd.Parameters.AddWithValue("@GST", model.GST);
                    cmd.Parameters.AddWithValue("@Conv_Fee", model.Conv_Fee);
                    cmd.Parameters.AddWithValue("@Total_Amt", model.Total_Amt);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["Message"] = "Event booked successfully.";
                return RedirectToAction("BookedHistory");
            }
            catch (Exception ex)
            {
                // 🛑 Log the error and show a friendly message
                Console.WriteLine("Booking failed: " + ex.Message); // You can replace this with a proper logger
                TempData["Error"] = "An error occurred while booking the event. Please try again.";
                return RedirectToAction("Index");
            }
        }

        [Area("Customer")]
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteBooking(int eventId)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                    return RedirectToAction("Index", "Login", new { area = "Login" });

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                
                if (userIdClaim == null)
                    return Unauthorized();

                string constr = _configuration.GetConnectionString("DevEventConnection");

                using (SqlConnection con = new SqlConnection(constr))
                using (SqlCommand cmd = new SqlCommand("DeleteBookedEventsByEventId", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@EventId", eventId);
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                return RedirectToAction("BookedHistory");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting event booking: " + ex.Message);
                return StatusCode(500, "An error occurred while deleting the booking.");
            }
        }


        [Area("Customer")]
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAllBookings()
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                    return RedirectToAction("Index", "Login", new { area = "Login" });

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized();

                int userId = Convert.ToInt32(userIdClaim.Value);

                string constr = _configuration.GetConnectionString("DevEventConnection");

                using (SqlConnection con = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM BookedEvents WHERE UserId = @UserId", con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                }

                // Optionally, show a success message using TempData
                TempData["SuccessMessage"] = "All bookings have been deleted.";
                return RedirectToAction("BookedHistory");
            }
            catch (Exception ex)
            {
                // Log the error to console or file
                Console.WriteLine("Error deleting bookings: " + ex.Message);

                // Optionally set an error message
                TempData["ErrorMessage"] = "An error occurred while deleting bookings.";
                return RedirectToAction("BookedHistory");
            }
        }
    }
}
