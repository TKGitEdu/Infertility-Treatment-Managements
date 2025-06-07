using Infertility_Treatment_Managements.DTOs;
using System;
using System.Collections.Generic;

namespace Infertility_Treatment_Managements.DTOs
{
    public class UserDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Username { get; set; }
        public int? RoleId { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }

        // Related entities
        public RoleDTO Role { get; set; }
        public DoctorBasicDTO Doctor { get; set; }
        public ICollection<PatientBasicDTO> Patients { get; set; } = new List<PatientBasicDTO>();
    }

    public class UserBasicDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Username { get; set; }
        public string Gender { get; set; }
    }

    public class UserCreateDTO
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int? RoleId { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
    }

    public class UserUpdateDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Username { get; set; }
        public int? RoleId { get; set; }
        public string Address { get; set; }
        public string Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
    }

    public class UserPasswordUpdateDTO
    {
        public int UserId { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}