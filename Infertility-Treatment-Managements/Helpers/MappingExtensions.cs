using Infertility_Treatment_Management.DTOs;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infertility_Treatment_Management.Helpers
{
    public static class MappingExtensions
    {
        #region Role Mapping
        public static RoleDTO ToDTO(this Role entity)
        {
            if (entity == null) return null;

            return new RoleDTO
            {
                RoleId = entity.RoleId,
                RoleName = entity.RoleName
            };
        }

        public static Role ToEntity(this RoleCreateDTO dto)
        {
            return new Role
            {
                RoleName = dto.RoleName
            };
        }

        public static void UpdateEntity(this RoleUpdateDTO dto, Role entity)
        {
            entity.RoleName = dto.RoleName;
        }
        #endregion

        #region User Mapping
        public static UserDTO ToDTO(this User entity)
        {
            if (entity == null) return null;

            return new UserDTO
            {
                UserId = entity.UserId,
                FullName = entity.FullName,
                Email = entity.Email,
                Phone = entity.Phone,
                Username = entity.Username,
                RoleId = entity.RoleId,
                Address = entity.Address,
                Gender = entity.Gender,
                DateOfBirth = entity.DateOfBirth,
                Role = entity.Role?.ToDTO(),
                Doctor = entity.Doctor?.ToBasicDTO(),
                Patients = entity.Patients?.Select(p => p.ToBasicDTO()).ToList()
            };
        }

        public static UserBasicDTO ToBasicDTO(this User entity)
        {
            if (entity == null) return null;

            return new UserBasicDTO
            {
                UserId = entity.UserId,
                FullName = entity.FullName,
                Email = entity.Email,
                Phone = entity.Phone,
                Username = entity.Username,
                Gender = entity.Gender
            };
        }

        public static User ToEntity(this UserCreateDTO dto)
        {
            return new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                Username = dto.Username,
                Password = dto.Password,
                RoleId = dto.RoleId,
                Address = dto.Address,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth
            };
        }

        public static void UpdateEntity(this UserUpdateDTO dto, User entity)
        {
            entity.FullName = dto.FullName;
            entity.Email = dto.Email;
            entity.Phone = dto.Phone;
            entity.Username = dto.Username;
            entity.RoleId = dto.RoleId;
            entity.Address = dto.Address;
            entity.Gender = dto.Gender;
            entity.DateOfBirth = dto.DateOfBirth;
        }
        #endregion

        #region Doctor Mapping
        public static DoctorDTO ToDTO(this Doctor entity)
        {
            if (entity == null) return null;

            return new DoctorDTO
            {
                DoctorId = entity.DoctorId,
                UserId = entity.UserId,
                DoctorName = entity.DoctorName,
                Specialization = entity.Specialization,
                Phone = entity.Phone,
                Email = entity.Email,
                User = entity.User?.ToBasicDTO(),
                Bookings = entity.Bookings?.Select(b => b.ToBasicDTO()).ToList()
            };
        }

        public static DoctorBasicDTO ToBasicDTO(this Doctor entity)
        {
            if (entity == null) return null;

            return new DoctorBasicDTO
            {
                DoctorId = entity.DoctorId,
                UserId = entity.UserId,
                DoctorName = entity.DoctorName,
                Specialization = entity.Specialization,
                Phone = entity.Phone,
                Email = entity.Email
            };
        }

        public static Doctor ToEntity(this DoctorCreateDTO dto)
        {
            return new Doctor
            {
                UserId = dto.UserId,
                DoctorName = dto.DoctorName,
                Specialization = dto.Specialization,
                Phone = dto.Phone,
                Email = dto.Email
            };
        }

        public static void UpdateEntity(this DoctorUpdateDTO dto, Doctor entity)
        {
            entity.UserId = dto.UserId;
            entity.DoctorName = dto.DoctorName;
            entity.Specialization = dto.Specialization;
            entity.Phone = dto.Phone;
            entity.Email = dto.Email;
        }
        #endregion

        #region Patient Mapping
        public static PatientDTO ToDTO(this Patient entity)
        {
            if (entity == null) return null;

            return new PatientDTO
            {
                PatientId = entity.PatientId,
                UserId = entity.UserId,
                Name = entity.Name,
                Phone = entity.Phone,
                Email = entity.Email,
                DateOfBirth = entity.DateOfBirth,
                Address = entity.Address,
                Gender = entity.Gender,
                BloodType = entity.BloodType,
                EmergencyPhoneNumber = entity.EmergencyPhoneNumber,
                User = entity.User?.ToBasicDTO(),
                Booking = entity.Booking?.ToBasicDTO(),
                PatientDetails = entity.PatientDetails?.Select(pd => pd.ToBasicDTO()).ToList()
            };
        }

        public static PatientBasicDTO ToBasicDTO(this Patient entity)
        {
            if (entity == null) return null;

            return new PatientBasicDTO
            {
                PatientId = entity.PatientId,
                Name = entity.Name,
                Phone = entity.Phone,
                Email = entity.Email,
                DateOfBirth = entity.DateOfBirth,
                Gender = entity.Gender
            };
        }

        public static Patient ToEntity(this PatientCreateDTO dto)
        {
            return new Patient
            {
                UserId = dto.UserId,
                Name = dto.Name,
                Phone = dto.Phone,
                Email = dto.Email,
                DateOfBirth = dto.DateOfBirth,
                Address = dto.Address,
                Gender = dto.Gender,
                BloodType = dto.BloodType,
                EmergencyPhoneNumber = dto.EmergencyPhoneNumber
            };
        }

        public static void UpdateEntity(this PatientUpdateDTO dto, Patient entity)
        {
            entity.UserId = dto.UserId;
            entity.Name = dto.Name;
            entity.Phone = dto.Phone;
            entity.Email = dto.Email;
            entity.DateOfBirth = dto.DateOfBirth;
            entity.Address = dto.Address;
            entity.Gender = dto.Gender;
            entity.BloodType = dto.BloodType;
            entity.EmergencyPhoneNumber = dto.EmergencyPhoneNumber;
        }
        #endregion

        #region PatientDetail Mapping
        public static PatientDetailDTO ToDTO(this PatientDetail entity)
        {
            if (entity == null) return null;

            return new PatientDetailDTO
            {
                PatientDetailId = entity.PatientDetailId,
                PatientId = entity.PatientId,
                TreatmentStatus = entity.TreatmentStatus,
                Patient = entity.Patient?.ToBasicDTO(),
                TreatmentProcesses = entity.TreatmentProcess?.Select(tp => tp.ToBasicDTO()).ToList()
            };
        }

        public static PatientDetailBasicDTO ToBasicDTO(this PatientDetail entity)
        {
            if (entity == null) return null;

            return new PatientDetailBasicDTO
            {
                PatientDetailId = entity.PatientDetailId,
                PatientId = entity.PatientId,
                TreatmentStatus = entity.TreatmentStatus
            };
        }

        public static PatientDetail ToEntity(this PatientDetailCreateDTO dto)
        {
            return new PatientDetail
            {
                PatientId = dto.PatientId,
                TreatmentStatus = dto.TreatmentStatus
            };
        }

        public static void UpdateEntity(this PatientDetailUpdateDTO dto, PatientDetail entity)
        {
            entity.PatientId = dto.PatientId;
            entity.TreatmentStatus = dto.TreatmentStatus;
        }
        #endregion

        #region Booking Mapping
        public static BookingDTO ToDTO(this Booking entity)
        {
            if (entity == null) return null;

            return new BookingDTO
            {
                BookingId = entity.BookingId,
                PatientId = entity.PatientId,
                ServiceId = entity.ServiceId,
                PaymentId = entity.PaymentId,
                DoctorId = entity.DoctorId,
                SlotId = entity.SlotId,
                DateBooking = entity.DateBooking,
                Description = entity.Description,
                Note = entity.Note,
                CreateAt = entity.CreateAt,
                Patient = entity.Patient?.ToBasicDTO(),
                Service = entity.Service?.ToBasicDTO(),
                Payment = entity.Payment?.ToBasicDTO(),
                Doctor = entity.Doctor?.ToBasicDTO(),
                Slot = entity.Slot?.ToBasicDTO(),
                Examination = entity.Examination?.ToBasicDTO()
            };
        }

        public static BookingBasicDTO ToBasicDTO(this Booking entity)
        {
            if (entity == null) return null;

            return new BookingBasicDTO
            {
                BookingId = entity.BookingId,
                DateBooking = entity.DateBooking,
                Description = entity.Description,
                Note = entity.Note
            };
        }

        public static Booking ToEntity(this BookingCreateDTO dto)
        {
            return new Booking
            {
                PatientId = dto.PatientId,
                ServiceId = dto.ServiceId,
                PaymentId = dto.PaymentId,
                DoctorId = dto.DoctorId,
                SlotId = dto.SlotId,
                DateBooking = dto.DateBooking,
                Description = dto.Description,
                Note = dto.Note,
                CreateAt = DateTime.Now
            };
        }

        public static void UpdateEntity(this BookingUpdateDTO dto, Booking entity)
        {
            entity.PatientId = dto.PatientId;
            entity.ServiceId = dto.ServiceId;
            entity.PaymentId = dto.PaymentId;
            entity.DoctorId = dto.DoctorId;
            entity.SlotId = dto.SlotId;
            entity.DateBooking = dto.DateBooking;
            entity.Description = dto.Description;
            entity.Note = dto.Note;
        }
        #endregion

        #region Service Mapping
        public static ServiceDTO ToDTO(this Service entity)
        {
            if (entity == null) return null;

            return new ServiceDTO
            {
                ServiceId = entity.ServiceId,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price,
                Status = entity.Status,
                Booking = entity.Booking?.ToBasicDTO()
            };
        }

        public static ServiceBasicDTO ToBasicDTO(this Service entity)
        {
            if (entity == null) return null;

            return new ServiceBasicDTO
            {
                ServiceId = entity.ServiceId,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price,
                Status = entity.Status
            };
        }

        public static Service ToEntity(this ServiceCreateDTO dto)
        {
            return new Service
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Status = dto.Status
            };
        }

        public static void UpdateEntity(this ServiceUpdateDTO dto, Service entity)
        {
            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.Price = dto.Price;
            entity.Status = dto.Status;
        }
        #endregion

        #region Payment Mapping
        public static PaymentDTO ToDTO(this Payment entity)
        {
            if (entity == null) return null;

            return new PaymentDTO
            {
                PaymentId = entity.PaymentId,
                BookingId = entity.BookingId,
                TotalAmount = entity.TotalAmount,
                Status = entity.Status,
                Method = entity.Method,
                Booking = entity.Booking?.ToBasicDTO()
            };
        }

        public static PaymentBasicDTO ToBasicDTO(this Payment entity)
        {
            if (entity == null) return null;

            return new PaymentBasicDTO
            {
                PaymentId = entity.PaymentId,
                TotalAmount = entity.TotalAmount,
                Status = entity.Status,
                Method = entity.Method
            };
        }

        public static Payment ToEntity(this PaymentCreateDTO dto)
        {
            return new Payment
            {
                BookingId = dto.BookingId,
                TotalAmount = dto.TotalAmount,
                Status = dto.Status,
                Method = dto.Method
            };
        }

        public static void UpdateEntity(this PaymentUpdateDTO dto, Payment entity)
        {
            entity.BookingId = dto.BookingId;
            entity.TotalAmount = dto.TotalAmount;
            entity.Status = dto.Status;
            entity.Method = dto.Method;
        }
        #endregion

        #region Slot Mapping
        public static SlotDTO ToDTO(this Slot entity)
        {
            if (entity == null) return null;

            return new SlotDTO
            {
                SlotId = entity.SlotId,
                SlotName = entity.SlotName,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                Bookings = entity.Bookings?.Select(b => b.ToBasicDTO()).ToList()
            };
        }

        public static SlotBasicDTO ToBasicDTO(this Slot entity)
        {
            if (entity == null) return null;

            return new SlotBasicDTO
            {
                SlotId = entity.SlotId,
                SlotName = entity.SlotName,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime
            };
        }

        public static Slot ToEntity(this SlotCreateDTO dto)
        {
            return new Slot
            {
                SlotName = dto.SlotName,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime
            };
        }

        public static void UpdateEntity(this SlotUpdateDTO dto, Slot entity)
        {
            entity.SlotName = dto.SlotName;
            entity.StartTime = dto.StartTime;
            entity.EndTime = dto.EndTime;
        }
        #endregion

        #region Examination Mapping
        public static ExaminationDTO ToDTO(this Examination entity)
        {
            if (entity == null) return null;

            return new ExaminationDTO
            {
                ExaminationId = entity.ExaminationId,
                BookingId = entity.BookingId,
                ExaminationDate = entity.ExaminationDate,
                ExaminationDescription = entity.ExaminationDescription,
                Result = entity.Result,
                Status = entity.Status,
                Note = entity.Note,
                CreateAt = entity.CreateAt,
                Booking = entity.Booking?.ToBasicDTO()
            };
        }

        public static ExaminationBasicDTO ToBasicDTO(this Examination entity)
        {
            if (entity == null) return null;

            return new ExaminationBasicDTO
            {
                ExaminationId = entity.ExaminationId,
                ExaminationDate = entity.ExaminationDate,
                ExaminationDescription = entity.ExaminationDescription,
                Result = entity.Result,
                Status = entity.Status
            };
        }

        public static Examination ToEntity(this ExaminationCreateDTO dto)
        {
            return new Examination
            {
                BookingId = dto.BookingId,
                ExaminationDate = dto.ExaminationDate,
                ExaminationDescription = dto.ExaminationDescription,
                Result = dto.Result,
                Status = dto.Status,
                Note = dto.Note,
                CreateAt = DateTime.Now
            };
        }

        public static void UpdateEntity(this ExaminationUpdateDTO dto, Examination entity)
        {
            entity.BookingId = dto.BookingId;
            entity.ExaminationDate = dto.ExaminationDate;
            entity.ExaminationDescription = dto.ExaminationDescription;
            entity.Result = dto.Result;
            entity.Status = dto.Status;
            entity.Note = dto.Note;
        }
        #endregion

        #region TreatmentProcess Mapping
        public static TreatmentProcessDTO ToDTO(this TreatmentProcess entity)
        {
            if (entity == null) return null;

            return new TreatmentProcessDTO
            {
                TreatmentProcessId = entity.TreatmentProcessId,
                Method = entity.Method,
                PatientDetailId = entity.PatientDetailId,
                ScheduledDate = entity.ScheduledDate,
                ActualDate = entity.ActualDate,
                Result = entity.Result,
                Status = entity.Status,
                PatientDetail = entity.PatientDetail?.ToBasicDTO()
            };
        }

        public static TreatmentProcessBasicDTO ToBasicDTO(this TreatmentProcess entity)
        {
            if (entity == null) return null;

            return new TreatmentProcessBasicDTO
            {
                TreatmentProcessId = entity.TreatmentProcessId,
                Method = entity.Method,
                ScheduledDate = entity.ScheduledDate,
                ActualDate = entity.ActualDate,
                Status = entity.Status,
                Result = entity.Result
            };
        }

        public static TreatmentProcess ToEntity(this TreatmentProcessCreateDTO dto)
        {
            return new TreatmentProcess
            {
                Method = dto.Method,
                PatientDetailId = dto.PatientDetailId,
                ScheduledDate = dto.ScheduledDate,
                ActualDate = dto.ActualDate,
                Result = dto.Result,
                Status = dto.Status
            };
        }

        public static void UpdateEntity(this TreatmentProcessUpdateDTO dto, TreatmentProcess entity)
        {
            entity.Method = dto.Method;
            entity.PatientDetailId = dto.PatientDetailId;
            entity.ScheduledDate = dto.ScheduledDate;
            entity.ActualDate = dto.ActualDate;
            entity.Result = dto.Result;
            entity.Status = dto.Status;
        }
        #endregion
    }
}