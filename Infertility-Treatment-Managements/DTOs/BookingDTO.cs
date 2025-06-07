using System;

namespace Infertility_Treatment_Managements.DTOs
{
    public class BookingDTO
    {
        public int BookingId { get; set; }
        public int? PatientId { get; set; }
        public int? ServiceId { get; set; }
        public int? PaymentId { get; set; }
        public int? DoctorId { get; set; }
        public int? SlotId { get; set; }
        public DateTime? DateBooking { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
        public DateTime? CreateAt { get; set; }

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
        public int BookingId { get; set; }
        public DateTime? DateBooking { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
    }

    public class BookingCreateDTO
    {
        public int PatientId { get; set; }
        public int ServiceId { get; set; }
        public int? PaymentId { get; set; }
        public int DoctorId { get; set; }
        public int SlotId { get; set; }
        public DateTime DateBooking { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
    }

    public class BookingUpdateDTO
    {
        public int BookingId { get; set; }
        public int PatientId { get; set; }
        public int ServiceId { get; set; }
        public int? PaymentId { get; set; }
        public int DoctorId { get; set; }
        public int SlotId { get; set; }
        public DateTime DateBooking { get; set; }
        public string Description { get; set; }
        public string Note { get; set; }
    }

}