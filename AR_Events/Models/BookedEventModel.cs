namespace AR_Events.Models
{
    public class BookedEventModel
    {
        public int Booking_Id { get; set; }
        public int Event_Id { get; set; }
        public string Event_Name { get; set; }
        public DateTime Event_StartDate { get; set; }
        public DateTime Event_EndDate { get; set; }
        public TimeSpan Event_StartTime { get; set; }
        public TimeSpan Event_EndTime { get; set; }
        public string Event_location { get; set; }
        public string Event_Organizer { get; set; }
        public int TicketCount { get; set; }
        public decimal EventCost_PerPerson { get; set; }
        public decimal GST { get; set; }
        public decimal Conv_Fee { get; set; }
        public decimal Total_Amt { get; set; }
        public DateTime BookedDate { get; set; }
    }
}
