using Infertility_Treatment_Managements.DTOs;
using Infertility_Treatment_Managements.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Infertility_Treatment_Managements.Helpers
{
    public static class MappingExtensions
    {
        #region Date Conversions
        // Convert DateTime? to DateOnly?
        private static DateOnly? ToDateOnly(this DateTime? dateTime)
        {
            return dateTime.HasValue ? DateOnly.FromDateTime(dateTime.Value) : null;
        }

        // Convert DateOnly? to DateTime?
        private static DateTime? ToDateTime(this DateOnly? dateOnly)
        {
            return dateOnly.HasValue ? dateOnly.Value.ToDateTime(TimeOnly.MinValue) : null;
        }
        #endregion

        #region Role Mapping
        public static RoleDTO ToDTO(this Role entity)
        {
            if (entity == null) return null!;

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
            if (entity == null) return null!;

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
                DateOfBirth = entity.DateOfBirth, // Just use the DateOnly? property directly
                Role = entity.Role?.ToDTO(),
                Doctor = entity.Doctor?.ToBasicDTO(),
                Patients = entity.Patients?.Select(p => p.ToBasicDTO()).ToList() ?? new List<PatientBasicDTO>()
            };
        }

        public static UserBasicDTO ToBasicDTO(this User entity)
        {
            if (entity == null) return null!;

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
                Address = dto.Address,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth // Just use the DateOnly? value directly
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
            entity.DateOfBirth = dto.DateOfBirth; // Just use the DateOnly? value directly
        }
        #endregion

        #region Doctor Mapping
        public static DoctorDTO ToDTO(this Doctor doctor)
        {
            return new DoctorDTO
            {
                DoctorId = doctor.DoctorId,
                UserId = doctor.UserId,
                DoctorName = doctor.DoctorName,
                Specialization = doctor.Specialization,
                Phone = doctor.Phone,
                Email = doctor.Email,
                User = doctor.User != null ? new UserBasicDTO
                {
                    UserId = doctor.User.UserId,
                    FullName = doctor.User.FullName,
                    Email = doctor.User.Email,
                    Phone = doctor.User.Phone,
                    Username = doctor.User.Username,
                    Gender = doctor.User.Gender
                } : null,
                Bookings = doctor.Bookings?.Select(b => new BookingBasicDTO
                {
                    BookingId = b.BookingId,
                    DateBooking = b.DateBooking,
                    Description = b.Description,
                    Note = b.Note
                }).ToList() ?? new List<BookingBasicDTO>()
            };
        }

        public static DoctorBasicDTO ToBasicDTO(this Doctor entity)
        {
            if (entity == null) return null!;

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
            if (entity == null) return null!;

            return new PatientDTO
            {
                PatientId = entity.PatientId,
                UserId = entity.UserId,
                Name = entity.Name,
                Phone = entity.Phone,
                Email = entity.Email,
                DateOfBirth = entity.DateOfBirth.HasValue ? ToDateOnly(entity.DateOfBirth) : null,
                Address = entity.Address,
                Gender = entity.Gender,
                BloodType = entity.BloodType,
                EmergencyPhoneNumber = entity.EmergencyPhoneNumber,
                User = entity.User?.ToBasicDTO(),
                Booking = entity.BookingFk?.FirstOrDefault()?.ToBasicDTO(),
                PatientDetails = entity.PatientDetails?.Select(pd => pd.ToBasicDTO()).ToList() ?? new List<PatientDetailBasicDTO>()
            };
        }

        public static PatientBasicDTO ToBasicDTO(this Patient entity)
        {
            if (entity == null) return null!;

            return new PatientBasicDTO
            {
                PatientId = entity.PatientId,
                Name = entity.Name,
                Phone = entity.Phone,
                Email = entity.Email,
                DateOfBirth = entity.DateOfBirth.HasValue ? ToDateOnly(entity.DateOfBirth) : null,
                Gender = entity.Gender
            };
        }

        public static Patient ToEntity(this PatientCreateDTO dto)
        {
            return new Patient
            {
                UserId = dto.UserId,
                Name = dto.Name ?? "",
                Phone = dto.Phone ?? "",
                Email = dto.Email ?? "",
                DateOfBirth = dto.DateOfBirth.HasValue ? ToDateTime(dto.DateOfBirth) : null,
                Address = dto.Address ?? "",
                Gender = dto.Gender ?? "",
                BloodType = dto.BloodType ?? "Unknown", // Giá trị mặc định nếu null
                EmergencyPhoneNumber = dto.EmergencyPhoneNumber ?? dto.Phone ?? "" // Sử dụng SĐT chính nếu không có
            };
        }

        public static void UpdateEntity(this PatientUpdateDTO dto, Patient entity)
        {
            entity.UserId = dto.UserId;
            entity.Name = dto.Name;
            entity.Phone = dto.Phone;
            entity.Email = dto.Email;
            entity.DateOfBirth = dto.DateOfBirth.HasValue ? ToDateTime(dto.DateOfBirth) : null;
            entity.Address = dto.Address;
            entity.Gender = dto.Gender;
            entity.BloodType = dto.BloodType;
            entity.EmergencyPhoneNumber = dto.EmergencyPhoneNumber;
        }
        #endregion

        #region PatientDetail Mapping
        public static PatientDetailDTO ToDTO(this PatientDetail entity)
        {
            if (entity == null) return null!;

            return new PatientDetailDTO
            {
                PatientDetailId = entity.PatientDetailId,
                PatientId = entity.PatientId,
                TreatmentStatus = entity.TreatmentStatus,
                Patient = entity.Patient?.ToBasicDTO(),
                TreatmentProcesses = entity.TreatmentProcessesFk?.Select(tp => tp.ToBasicDTO()).ToList() ?? new List<TreatmentProcessBasicDTO>(),
                TreatmentPlans = entity.TreatmentPlansFk?.Select(tp => tp.ToBasicDTO()).ToList() ?? new List<TreatmentPlanBasicDTO>()
            };
        }

        public static PatientDetailBasicDTO ToBasicDTO(this PatientDetail entity)
        {
            if (entity == null) return null!;

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
            if (entity == null) return null!;

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
                Status = entity.Status,
                Patient = entity.Patient?.ToBasicDTO(),
                Service = entity.Service?.ToBasicDTO(),
                Payment = entity.Payment?.ToBasicDTO(),
                Doctor = entity.Doctor?.ToBasicDTO(),
                Slot = entity.Slot?.ToBasicDTO(),
                Examinations = entity.Examinations?.Select(e => e.ToBasicDTO()).ToList() ?? new List<ExaminationBasicDTO>()
            };
        }

        public static BookingBasicDTO ToBasicDTO(this Booking entity)
        {
            if (entity == null) return null!;

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
            // Update these checks to work with string IDs
            if (!string.IsNullOrEmpty(dto.ServiceId)) entity.ServiceId = dto.ServiceId;
            if (!string.IsNullOrEmpty(dto.PaymentId)) entity.PaymentId = dto.PaymentId;
            if (!string.IsNullOrEmpty(dto.DoctorId)) entity.DoctorId = dto.DoctorId;
            if (!string.IsNullOrEmpty(dto.SlotId)) entity.SlotId = dto.SlotId;
            entity.DateBooking = dto.DateBooking;
            if (!string.IsNullOrEmpty(dto.Description)) entity.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.Note)) entity.Note = dto.Note;
        }
        #endregion

        #region Service Mapping
        public static ServiceDTO ToDTO(this Service entity)
        {
            if (entity == null) return null!;

            return new ServiceDTO
            {
                ServiceId = entity.ServiceId,
                Name = entity.Name ?? string.Empty,
                Description = entity.Description ?? string.Empty,
                Price = entity.Price ?? 0,
                Status = entity.Status ?? string.Empty,
                Category = entity.Category ?? string.Empty, // Thêm mapping cho Category
                Bookings = entity.BookingsFk?.Select(b => b.ToBasicDTO()).ToList() ?? new List<BookingBasicDTO>()
            };
        }

        public static ServiceBasicDTO ToBasicDTO(this Service entity)
        {
            if (entity == null) return null!;

            return new ServiceBasicDTO
            {
                ServiceId = entity.ServiceId,
                Name = entity.Name ?? string.Empty,
                Description = entity.Description ?? string.Empty,
                Price = entity.Price,
                Status = entity.Status ?? string.Empty,
                Category = entity.Category ?? string.Empty // Thêm mapping cho Category
            };
        }

        public static Service ToEntity(this ServiceCreateDTO dto)
        {
            return new Service
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Status = dto.Status,
                Category = dto.Category // Thêm mapping cho Category
            };
        }

        public static void UpdateEntity(this ServiceUpdateDTO dto, Service entity)
        {
            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.Price = dto.Price;
            entity.Status = dto.Status;
            entity.Category = dto.Category; // Thêm mapping cho Category
        }
        #endregion

        #region Payment Mapping
        public static PaymentDTO ToDTO(this Payment entity)
        {
            if (entity == null) return null!;

            return new PaymentDTO
            {
                PaymentId = entity.PaymentId,
                BookingId = entity.BookingId,
                TotalAmount = entity.TotalAmount,
                Status = entity.Status,
                Confirmed = entity.Confirmed,
                Method = entity.Method,
                Booking = entity.Booking?.ToBasicDTO()
            };
        }

        public static PaymentBasicDTO ToBasicDTO(this Payment entity)
        {
            if (entity == null) return null!;

            return new PaymentBasicDTO
            {
                PaymentId = entity.PaymentId,
                TotalAmount = entity.TotalAmount,
                Status = entity.Status,
                Confirmed = entity.Confirmed,
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
                Confirmed = false, // Mặc định là false khi tạo mới
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
            if (entity == null) return null!;

            return new SlotDTO
            {
                SlotId = entity.SlotId,
                SlotName = entity.SlotName,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                Bookings = entity.Bookings?.Select(b => b.ToBasicDTO()).ToList() ?? new List<BookingBasicDTO>()
            };
        }

        public static SlotBasicDTO ToBasicDTO(this Slot entity)
        {
            if (entity == null) return null!;

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
            if (entity == null) return null!;

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
            if (entity == null) return null!;

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
            if (entity == null) return null!;

            return new TreatmentProcessDTO
            {
                TreatmentProcessId = entity.TreatmentProcessId,
                PatientDetailId = entity.PatientDetailId,
                ScheduledDate = entity.ScheduledDate.HasValue ? ToDateOnly(entity.ScheduledDate) : null,
                Result = entity.Result,
                Status = entity.Status,
                PatientDetail = entity.PatientDetail?.ToBasicDTO()
            };
        }

        public static TreatmentProcessBasicDTO ToBasicDTO(this TreatmentProcess entity)
        {
            if (entity == null) return null!;

            return new TreatmentProcessBasicDTO
            {
                TreatmentProcessId = entity.TreatmentProcessId,
                ScheduledDate = entity.ScheduledDate.HasValue ? ToDateOnly(entity.ScheduledDate) : null,
                Status = entity.Status,
                Result = entity.Result
            };
        }

        public static TreatmentProcess ToEntity(this TreatmentProcessCreateDTO dto)
        {
            return new TreatmentProcess
            {
                PatientDetailId = dto.PatientDetailId,
                ScheduledDate = dto.ScheduledDate.HasValue ? ToDateTime(dto.ScheduledDate) : null,
                Result = dto.Result,
                Status = dto.Status
            };
        }

        public static void UpdateEntity(this TreatmentProcessUpdateDTO dto, TreatmentProcess entity)
        {
            entity.PatientDetailId = dto.PatientDetailId;
            entity.ScheduledDate = dto.ScheduledDate.HasValue ? ToDateTime(dto.ScheduledDate) : null;
            entity.ScheduledDate = dto.ScheduledDate.HasValue ? ToDateTime(dto.ScheduledDate) : null;
            entity.Result = dto.Result;
            entity.Status = dto.Status;
        }
        #endregion

        #region TreatmentPlan Mapping
        public static TreatmentPlanDTO ToDTO(this TreatmentPlan entity)
        {
            if (entity == null) return null!;

            return new TreatmentPlanDTO
            {
                TreatmentPlanId = entity.TreatmentPlanId,
                DoctorId = entity.DoctorId,
                Method = entity.Method,
                PatientDetailId = entity.PatientDetailId,
                StartDate = entity.StartDate.HasValue ? ToDateOnly(entity.StartDate) : null,
                EndDate = entity.EndDate.HasValue ? ToDateOnly(entity.EndDate) : null,
                Status = entity.Status,
                TreatmentDescription = entity.TreatmentDescription,
                Doctor = entity.Doctor?.ToBasicDTO(),
                PatientDetail = entity.PatientDetail?.ToBasicDTO(),
                TreatmentProcesses = entity.TreatmentProcesses?.Select(tp => tp.ToBasicDTO()).ToList() ?? new List<TreatmentProcessBasicDTO>()
            };
        }

        public static TreatmentPlanBasicDTO ToBasicDTO(this TreatmentPlan entity)
        {
            if (entity == null) return null!;

            return new TreatmentPlanBasicDTO
            {
                TreatmentPlanId = entity.TreatmentPlanId,
                Method = entity.Method,
                StartDate = entity.StartDate.HasValue ? ToDateOnly(entity.StartDate) : null,
                EndDate = entity.EndDate.HasValue ? ToDateOnly(entity.EndDate) : null,
                Status = entity.Status,
                TreatmentDescription = entity.TreatmentDescription
            };
        }

        public static TreatmentPlan ToEntity(this TreatmentPlanCreateDTO dto)
        {
            return new TreatmentPlan
            {
                DoctorId = dto.DoctorId,
                Method = dto.Method,
                PatientDetailId = dto.PatientDetailId,
                StartDate = dto.StartDate.HasValue ? ToDateTime(dto.StartDate) : null,
                EndDate = dto.EndDate.HasValue ? ToDateTime(dto.EndDate) : null,
                Status = dto.Status,
                TreatmentDescription = dto.TreatmentDescription
            };
        }

        public static void UpdateEntity(this TreatmentPlanUpdateDTO dto, TreatmentPlan entity)
        {
            entity.DoctorId = dto.DoctorId;
            entity.Method = dto.Method;
            entity.PatientDetailId = dto.PatientDetailId;
            entity.StartDate = dto.StartDate.HasValue ? ToDateTime(dto.StartDate) : null;
            entity.EndDate = dto.EndDate.HasValue ? ToDateTime(dto.EndDate) : null;
            entity.Status = dto.Status;
            entity.TreatmentDescription = dto.TreatmentDescription;
        }
        #endregion
    }
    public class ServiceDTO
    {
        public string ServiceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Thêm thuộc tính Category
        public ICollection<BookingBasicDTO> Bookings { get; set; } = new List<BookingBasicDTO>();
    }
    
}