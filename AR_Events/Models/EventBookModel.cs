using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AR_Events.Models
{
    public class EventBookModel
    {
        [Key]
        [Display(Name = "Event Id")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "User Id")]
        public int UserId { get; set; }

        [Required]
        [Display(Name = "Event Name")]
        public string Event_Name { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime Event_StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "End Date")]
        public DateTime Event_EndDate { get; set; }

        [Required]
        [Display(Name = "Start Time")]
        [Column(TypeName = "time")]
        public TimeSpan Event_StartTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        [Column(TypeName = "time")]
        public TimeSpan Event_EndTime { get; set; }

        [Required]
        [Display(Name = "Event Location")]
        public string Event_location { get; set; }

        [Display(Name = "Organizer")]
        public string Event_Organizer { get; set; }

        [Display(Name = "Tickets Booked")]
        public int TicketCount { get; set; }

        [Display(Name = "Booking Date")]
        [DataType(DataType.DateTime)]
        public DateTime BookedDate { get; set; }

        [Display(Name = "Cost Per Person")]
        [DataType(DataType.Currency)]
        public decimal EventCost_PerPerson { get; set; }

        [Display(Name = "GST (%)")]
        [Range(0, 100)]
        public decimal GST { get; set; }

        [Display(Name = "Convenience Fee")]
        [DataType(DataType.Currency)]
        public decimal Conv_Fee { get; set; }

        [Display(Name = "Total Amount")]
        [DataType(DataType.Currency)]
        public decimal Total_Amt { get; set; }

        [Display(Name = "Event Image")]
        public string? ImagePath { get; set; } 

    }
}
