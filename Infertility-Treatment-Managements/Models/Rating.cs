using System;

namespace Infertility_Treatment_Managements.Models
{
    public partial class Rating
    {
        public string RatingId { get; set; }
        public string? PatientId { get; set; }
        public string? DoctorId { get; set; }
        public string? ServiceId { get; set; }
        public string? BookingId { get; set; }
        public int Score { get; set; } // 1-5 star rating
        public string? Comment { get; set; }
        public string RatingType { get; set; } // Doctor, Service, Overall
        public DateTime RatingDate { get; set; }
        public string Status { get; set; } // Approved, pending, Rejected
        public bool IsAnonymous { get; set; }

        // Navigation properties
        public virtual Patient Patient { get; set; }
        public virtual Doctor Doctor { get; set; }
        public virtual Service Service { get; set; }
        public virtual Booking Booking { get; set; }
    }
}