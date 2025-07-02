using System;

namespace Infertility_Treatment_Managements.DTOs
{
    public class NotificationDTO
    {
        public string NotificationId { get; set; }
        public string PatientId { get; set; }
        public string DoctorId { get; set; }
        public string BookingId { get; set; }
        public string TreatmentProcessId { get; set; }

        // Trường mới cho notification
        public string Type { get; set; } // "appointment" | "test" | "treatment"
        public string Message { get; set; }
        public DateTime Time { get; set; }
        public bool? IsRead { get; set; }
    }

    public class NotificationCreateDTO
    {
        public string PatientId { get; set; }
        public string DoctorId { get; set; }
        public string BookingId { get; set; }
        public string TreatmentProcessId { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }
        public bool? IsRead { get; set; }
    }

    public class NotificationUpdateDTO
    {
        public string NotificationId { get; set; }
        public string PatientId { get; set; }
        public string DoctorId { get; set; }
        public string BookingId { get; set; }
        public string TreatmentProcessId { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }
        public bool? IsRead { get; set; }
    }
    public class NotificationBasicDTO
    {
        public string NotificationId { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }
        public bool? IsRead { get; set; }
    }
}
