using System;

namespace Infertility_Treatment_Managements.Models
{
    public partial class Reminder
    {
        public string ReminderId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? PatientId { get; set; }
        public string? DoctorId { get; set; }
        public string? BookingId { get; set; }
        public string? TreatmentProcessId { get; set; }
        public DateTime ScheduledTime { get; set; }
        public DateTime? SentTime { get; set; }
        public string ReminderType { get; set; } // Medication, Appointment, Test, etc.
        public string Status { get; set; } // Pending, Sent, Cancelled, etc.
        public bool IsRepeating { get; set; }
        public string? RepeatPattern { get; set; } // Daily, Weekly, Monthly, etc.
        public bool IsEmailNotification { get; set; }
        public bool IsSmsNotification { get; set; }
        public DateTime CreateDate { get; set; }

        // Navigation properties
        public virtual Patient Patient { get; set; }
        public virtual Doctor Doctor { get; set; }
        public virtual Booking Booking { get; set; }
        public virtual TreatmentProcess TreatmentProcess { get; set; }
    }
}