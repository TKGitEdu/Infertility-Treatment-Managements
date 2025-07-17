using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Infertility_Treatment_Managements.DTOs
{
    public class PatientDTO
    {
        public string PatientId { get; set; }
        public string? UserId { get; set; }
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
        public string PatientId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string Gender { get; set; }
    }

    public class PatientCreateDTO
    {
        public string? UserId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public string BloodType { get; set; }
        public string EmergencyPhoneNumber { get; set; }

        // Additional fields for user creation when UserId is not provided
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class PatientUpdateDTO
    {
        public string PatientId { get; set; }
        public string UserId { get; set; }
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