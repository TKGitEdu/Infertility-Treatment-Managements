using System;
using System.Collections.Generic;

namespace Infertility_Treatment_Management.DTOs
{
    public class DoctorDTO
    {
        public int DoctorId { get; set; }
        public int? UserId { get; set; }  // Add this line
        public string DoctorName { get; set; }
        public string Specialization { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        // Related entities
        public UserBasicDTO User { get; set; }
        public ICollection<BookingBasicDTO> Bookings { get; set; } = new List<BookingBasicDTO>();
    }

    public class DoctorBasicDTO
    {
        public int DoctorId { get; set; }
        public int? UserId { get; set; }  // Add this line
        public string DoctorName { get; set; }
        public string Specialization { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }

    public class DoctorCreateDTO
    {
        public int? UserId { get; set; }  // Add this line
        public string DoctorName { get; set; }
        public string Specialization { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        // The UserId can be set automatically from the User entity since they share the same ID
    }

    public class DoctorUpdateDTO
    {
        public int DoctorId { get; set; }
        public int? UserId { get; set; }  // Add this line
        public string DoctorName { get; set; }
        public string Specialization { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}