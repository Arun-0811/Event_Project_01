using AR_Events.Areas.Login.Controllers;
using AR_Events.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;

namespace AR_Events.Areas.Customer.Controllers
{
    [Area("Customer")]
    //[Authorize(Policy = "CustomerPolicy")]

    public class DeleteEventHistoryController : BaseController
    {
        private readonly IConfiguration _configuration;

        public DeleteEventHistoryController(IConfiguration config)
        {
            _configuration = config;
        }



        [HttpGet]
        [Authorize]
        public IActionResult EditHistoryById(int id)
        {
            BookedEventModel bookedEvent = null;
            string constr = _configuration.GetConnectionString("DevEventConnection");

            using (SqlConnection con = new SqlConnection(constr))
            {
                SqlCommand cmd = new SqlCommand("sp_booked_event_fetch_by_id", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Booking_Id", id);

                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows && reader.Read())
                    {
                        bookedEvent = new BookedEventModel
                        {
                            Booking_Id = Convert.ToInt32(reader["Booking_Id"]),
                            Event_Id = Convert.ToInt32(reader["Event_Id"]),
                            Event_Name = reader["Event_Name"].ToString(),
                            TicketCount = Convert.ToInt32(reader["TicketCount"]),
                            EventCost_PerPerson = Convert.ToDecimal(reader["EventCost_PerPerson"]),
                            GST = Convert.ToDecimal(reader["GST"]),
                            Conv_Fee = Convert.ToDecimal(reader["Conv_Fee"]),
                            Total_Amt = Convert.ToDecimal(reader["Total_Amt"]),
                            BookedDate = Convert.ToDateTime(reader["BookedDate"]),
                            Event_StartDate = Convert.ToDateTime(reader["Event_StartDate"]),
                            Event_EndDate = Convert.ToDateTime(reader["Event_EndDate"]),
                            Event_StartTime = TimeSpan.Parse(reader["Event_StartTime"].ToString()),
                            Event_EndTime = TimeSpan.Parse(reader["Event_EndTime"].ToString()),
                            Event_location = reader["Event_location"].ToString(),
                            Event_Organizer = reader["Event_Organizer"].ToString()
                        };
                    }
                    else
                    {
                        return NotFound();
                    }
                }
            }

            return View(bookedEvent);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditHistoryById(BookedEventModel model, int Id)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                string constr = _configuration.GetConnectionString("DevEventConnection");

                using (SqlConnection con = new SqlConnection(constr))
                {
                    SqlCommand cmd = new SqlCommand("sp_booked_event_update_by_id", con)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.Parameters.AddWithValue("@Booking_Id", Id);
                    cmd.Parameters.AddWithValue("@Event_Id", model.Event_Id);
                    cmd.Parameters.AddWithValue("@Event_Name", model.Event_Name);
                    cmd.Parameters.AddWithValue("@TicketCount", model.TicketCount);
                    cmd.Parameters.AddWithValue("@EventCost_PerPerson", model.EventCost_PerPerson);
                    cmd.Parameters.AddWithValue("@GST", model.GST);
                    cmd.Parameters.AddWithValue("@Conv_Fee", model.Conv_Fee);
                    cmd.Parameters.AddWithValue("@Total_Amt", model.Total_Amt);
                    cmd.Parameters.AddWithValue("@BookedDate", model.BookedDate);

                    // Additional fields now handled by the updated stored procedure
                    cmd.Parameters.AddWithValue("@Event_StartDate", model.Event_StartDate);
                    cmd.Parameters.AddWithValue("@Event_EndDate", model.Event_EndDate);
                    cmd.Parameters.AddWithValue("@Event_StartTime", model.Event_StartTime);
                    cmd.Parameters.AddWithValue("@Event_EndTime", model.Event_EndTime);
                    cmd.Parameters.AddWithValue("@Event_location", model.Event_location);
                    cmd.Parameters.AddWithValue("@Event_Organizer", model.Event_Organizer);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["SuccessMessage"] = "Booking updated successfully!";
                return RedirectToAction("BookedHistory", "EventDetails", new { area = "Customer" });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error while updating: " + ex.Message;
                return View(model);
            }
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DeleteHistoryById")]
        public IActionResult DeleteHistoryById(int id)
        {
            try
            {               

                string constr = _configuration.GetConnectionString("DevEventConnection");

                using (SqlConnection con = new SqlConnection(constr))
                {
                    SqlCommand cmd = new SqlCommand("DeleteBookedEventsByEventId", con)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.AddWithValue("@Booking_Id", id); // Use the action parameter directly
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["Message"] = "Event deleted successfully.";
                return RedirectToAction("BookedHistory", "EventDetails", new { area = "Customer" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting event: " + ex.Message);
                TempData["Error"] = "An error occurred while deleting the event. Please try again later.";
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DeleteAllHistory")]
        public IActionResult DeleteAllHistory(BookedEventModel BEModel)
        {
            try
            {

                string constr = _configuration.GetConnectionString("DevEventConnection");

                using (SqlConnection con = new SqlConnection(constr))
                {
                    SqlCommand cmd = new SqlCommand("DeleteAllBookedEvents", con)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    
                    con.Open();
                    cmd.ExecuteNonQuery();
                }

                TempData["Message"] = "Event deleted successfully.";
                return RedirectToAction("BookedHistory", "EventDetails", new { area = "Customer" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting event: " + ex.Message);
                TempData["Error"] = "An error occurred while deleting the event. Please try again later.";
                return RedirectToAction("Index");
            }
        }
    }
}
