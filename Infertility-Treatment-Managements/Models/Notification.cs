using System;

namespace Infertility_Treatment_Managements.Models
{
    public partial class Notification
    {
        public string NotificationId { get; set; } // Thay cho ReminderId
        public string? PatientId { get; set; }
        public string? DoctorId { get; set; }
        public string? BookingId { get; set; }
        public string? TreatmentProcessId { get; set; }

        // Trường mới
        public string Type { get; set; } // "appointment" | "test" | "treatment"
        public string Message { get; set; }
        public DateTime Time { get; set; }

        // Navigation properties
        public virtual Patient? Patient { get; set; }
        public virtual Doctor? Doctor { get; set; }
        public virtual Booking? Booking { get; set; }
        public virtual TreatmentProcess? TreatmentProcess { get; set; }


    }
}