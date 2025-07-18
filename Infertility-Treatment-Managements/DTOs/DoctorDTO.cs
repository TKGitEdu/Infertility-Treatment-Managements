﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Infertility_Treatment_Managements.DTOs
{
    public class DoctorDTO
    {
        public string DoctorId { get; set; }
        public string? UserId { get; set; }
        public string DoctorName { get; set; }
        public string? Specialization { get; set; }  // Thêm ? để chỉ rõ là nullable
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        // Related entities
        public UserBasicDTO User { get; set; }
        public ICollection<BookingBasicDTO> Bookings { get; set; } = new List<BookingBasicDTO>();
    }

    public class DoctorBasicDTO
    {
        public string DoctorId { get; set; }
        public string? UserId { get; set; }
        public string DoctorName { get; set; }
        public string? Specialization { get; set; }  // Thêm ? để chỉ rõ là nullable
        public string Phone { get; set; }
        public string Email { get; set; }
    }

    public class DoctorCreateDTO
    {
        public string? UserId { get; set; }
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
        [Required]
        public string DoctorId { get; set; }
        public string? UserId { get; set; }
        public string? DoctorName { get; set; }
        public string? Specialization { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }

    public class DoctorRegistrationDTO
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string DoctorName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Phone { get; set; }

        public string Address { get; set; }

        public string Gender { get; set; }

        [Required]
        public string Specialization { get; set; }
    }

}