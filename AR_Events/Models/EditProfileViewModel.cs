using System.ComponentModel.DataAnnotations;

namespace AR_Events.Models
{
    public class EditProfileViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, ErrorMessage = "Username cannot be longer than 100 characters")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [StringLength(15, ErrorMessage = "Mobile number cannot exceed 15 digits")]
        public string MobileNo { get; set; }

        public IFormFile PhotoFile { get; set; }

        public string? PhotoPath { get; set; }  // ✅ For storing existing filename
    }

}