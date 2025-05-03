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
                    cmd.Parameters.AddWithValue("@EventId", id); // Use the action parameter directly
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
