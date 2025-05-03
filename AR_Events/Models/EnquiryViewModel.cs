using System.ComponentModel.DataAnnotations;

namespace AR_Events.Models
{
    public class EnquiryViewModel
    {


        [Required(ErrorMessage = "The Name field is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "The Email field is required.")]
        [EmailAddress(ErrorMessage = "The Email field is not a valid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "The Subject field is required.")]
        public string Subject { get; set; }

        [Required(ErrorMessage = "The Message field is required.")]
        public string Message { get; set; }


    }
}
