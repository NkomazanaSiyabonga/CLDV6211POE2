using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CloudDevelopmentPOE1.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        [Required]
        public int VenueId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }

        // Navigation properties for the related Venue and Event
        public Venue? Venue { get; set; }
        public Event? Event { get; set; }
    }
}
