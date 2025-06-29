using System;

namespace Infertility_Treatment_Managements.DTOs
{
    public class BookingDTO
    {
        public string BookingId { get; set; }
        public string? PatientId { get; set; }
        public string? ServiceId { get; set; }
        public string? PaymentId { get; set; }
        public string? DoctorId { get; set; }
        public string? SlotId { get; set; }
        public DateTime? DateBooking { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public DateTime? CreateAt { get; set; }
        public string Status { get; set; } // hoặc StatusId nếu bạn dùng bảng Status riêng

        // Basic doctor information
        public DoctorBasicDTO Doctor { get; set; }

        // Basic patient information
        public PatientBasicDTO Patient { get; set; }

        // Basic payment information
        public PaymentBasicDTO Payment { get; set; }

        // Basic service information
        public ServiceBasicDTO Service { get; set; }

        // Basic slot information
        public SlotBasicDTO Slot { get; set; }

        // Basic examination information
        public ExaminationBasicDTO Examination { get; set; }
    }
    public class BookingBasicDTO
    {
        public string BookingId { get; set; }
        public DateTime? DateBooking { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
    }

    public class BookingCreateDTO
    {
        public string PatientId { get; set; }
        public string ServiceId { get; set; }
        public string? PaymentId { get; set; }
        public string DoctorId { get; set; }
        public string SlotId { get; set; }
        public DateTime DateBooking { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
    }

    public class BookingUpdateDTO
    {
        public string? BookingId { get; set; }
        public string? PatientId { get; set; }
        public string? ServiceId { get; set; }
        public string? PaymentId { get; set; }
        public string? DoctorId { get; set; }
        public string? SlotId { get; set; }
        public DateTime DateBooking { get; set; }
        public string? Description { get; set; }
        public string? Note { get; set; }
    }

}