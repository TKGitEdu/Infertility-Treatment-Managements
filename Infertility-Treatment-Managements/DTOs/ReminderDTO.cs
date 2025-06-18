using System;

namespace Infertility_Treatment_Managements.DTOs
{
    public class ReminderDTO
    {
        public string ReminderId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string PatientId { get; set; }
        public string DoctorId { get; set; }
        public string BookingId { get; set; }
        public string TreatmentProcessId { get; set; }
        public DateTime ScheduledTime { get; set; }
        public DateTime? SentTime { get; set; }
        public string ReminderType { get; set; }
        public string Status { get; set; }
        public bool IsRepeating { get; set; }
        public string RepeatPattern { get; set; }
        public bool IsEmailNotification { get; set; }
        public bool IsSmsNotification { get; set; }
        public DateTime CreateDate { get; set; }

        // Related entities information
        public PatientBasicDTO Patient { get; set; }
        public DoctorBasicDTO Doctor { get; set; }
        public BookingBasicDTO Booking { get; set; }
        public TreatmentProcessBasicDTO TreatmentProcess { get; set; }
    }

    public class ReminderCreateDTO
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string PatientId { get; set; }
        public string DoctorId { get; set; }
        public string BookingId { get; set; }
        public string TreatmentProcessId { get; set; }
        public DateTime ScheduledTime { get; set; }
        public string ReminderType { get; set; }
        public bool IsRepeating { get; set; }
        public string RepeatPattern { get; set; }
        public bool IsEmailNotification { get; set; }
        public bool IsSmsNotification { get; set; }
    }    public class ReminderUpdateDTO
    {
        public string ReminderId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? PatientId { get; set; }
        public string? DoctorId { get; set; }
        public string? BookingId { get; set; }
        public string? TreatmentProcessId { get; set; }
        public DateTime ScheduledTime { get; set; }
        public string? ReminderType { get; set; }
        public string Status { get; set; }
        public bool IsRepeating { get; set; }
        public string? RepeatPattern { get; set; }
        public bool IsEmailNotification { get; set; }
        public bool IsSmsNotification { get; set; }
    }

    public class ReminderBasicDTO
    {
        public string ReminderId { get; set; }
        public string Title { get; set; }
        public string ReminderType { get; set; }
        public DateTime ScheduledTime { get; set; }
        public string Status { get; set; }
    }
}