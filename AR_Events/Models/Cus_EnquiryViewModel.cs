using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace AR_Events.Models
{
    public class Cus_EnquiryViewModel
    {
        [Key]
        public int EnquiryId { get; set; }

        

        [StringLength(200)]
        public string Subject { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
