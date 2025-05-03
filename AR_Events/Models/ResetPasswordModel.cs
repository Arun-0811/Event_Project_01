namespace AR_Events.Models
{
    public class ResetPasswordModel
    {
        public string EmailId { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
