using System;
using System.Collections.Generic;

namespace Infertility_Treatment_Management.DTOs
{
    public class PatientDTO
    {
        public int PatientId { get; set; }
        public int? UserId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public string BloodType { get; set; }
        public string EmergencyPhoneNumber { get; set; }

        // Related entities
        public UserBasicDTO User { get; set; }
        public BookingBasicDTO Booking { get; set; }
        public ICollection<PatientDetailBasicDTO> PatientDetails { get; set; } = new List<PatientDetailBasicDTO>();
    }

    public class PatientBasicDTO
    {
        public int PatientId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string Gender { get; set; }
    }

    public class PatientCreateDTO
    {
        public int? UserId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public string BloodType { get; set; }
        public string EmergencyPhoneNumber { get; set; }
    }

    public class PatientUpdateDTO
    {
        public int PatientId { get; set; }
        public int? UserId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public string BloodType { get; set; }
        public string EmergencyPhoneNumber { get; set; }
    }
}