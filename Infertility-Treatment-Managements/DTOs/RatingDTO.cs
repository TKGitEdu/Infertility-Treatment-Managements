using System;

namespace Infertility_Treatment_Managements.DTOs
{
    public class RatingDTO
    {
        public string RatingId { get; set; }
        public string PatientId { get; set; }
        public string DoctorId { get; set; }
        public string ServiceId { get; set; }
        public string BookingId { get; set; }
        public int Score { get; set; }
        public string Comment { get; set; }
        public string RatingType { get; set; }
        public DateTime RatingDate { get; set; }
        public string Status { get; set; }
        public bool IsAnonymous { get; set; }

        // Related entities information
        public PatientBasicDTO Patient { get; set; }
        public DoctorBasicDTO Doctor { get; set; }
        public ServiceBasicDTO Service { get; set; }
        public BookingBasicDTO Booking { get; set; }
    }

    public class RatingCreateDTO
    {
        public string PatientId { get; set; }
        public string DoctorId { get; set; }
        public string ServiceId { get; set; }
        public string BookingId { get; set; }
        public int Score { get; set; }
        public string Comment { get; set; }
        public string RatingType { get; set; }
        public bool IsAnonymous { get; set; }
    }

    public class RatingUpdateDTO
    {
        public string RatingId { get; set; }
        public int Score { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; }
    }

    public class RatingBasicDTO
    {
        public string RatingId { get; set; }
        public int Score { get; set; }
        public string RatingType { get; set; }
        public DateTime RatingDate { get; set; }
    }
}