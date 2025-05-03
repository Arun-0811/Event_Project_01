namespace AR_Events.Models
{
    public class GetAllEnquiryViewModel
    {
        public int EnquiryId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
