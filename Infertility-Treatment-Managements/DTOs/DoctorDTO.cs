using System;
using System.Collections.Generic;

namespace Infertility_Treatment_Managements.DTOs
{
    public class DoctorDTO
    {
        public int DoctorId { get; set; }
        public int? UserId { get; set; }
        public string DoctorName { get; set; }
        public string? Specialization { get; set; }  // Thêm ? để chỉ rõ là nullable
        public string Phone { get; set; }
        public string Email { get; set; }

        // Related entities
        public UserBasicDTO User { get; set; }
        public ICollection<BookingBasicDTO> Bookings { get; set; } = new List<BookingBasicDTO>();
    }

    public class DoctorBasicDTO
    {
        public int DoctorId { get; set; }
        public int? UserId { get; set; }
        public string DoctorName { get; set; }
        public string? Specialization { get; set; }  // Thêm ? để chỉ rõ là nullable
        public string Phone { get; set; }
        public string Email { get; set; }
    }

    public class DoctorCreateDTO
    {
        public int? UserId { get; set; }
        public string DoctorName { get; set; }
        public string? Specialization { get; set; }  // Thêm ? để chỉ rõ là nullable
        public string Phone { get; set; }
        public string Email { get; set; }

        // Additional fields for user creation when UserId is not provided
        public string Username { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
    }

    public class DoctorUpdateDTO
    {
        public int DoctorId { get; set; }
        public int? UserId { get; set; }
        public string DoctorName { get; set; }
        public string? Specialization { get; set; }  // Thêm ? để chỉ rõ là nullable
        public string Phone { get; set; }
        public string Email { get; set; }
    }
}