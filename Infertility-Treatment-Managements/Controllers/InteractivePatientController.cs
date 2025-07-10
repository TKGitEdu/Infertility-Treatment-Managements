using Infertility_Treatment_Managements.DTOs;
using Infertility_Treatment_Managements.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Infertility_Treatment_Managements.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InteractivePatientController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public InteractivePatientController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy thông tin booking theo ID mà không bao gồm các tham chiếu
        /// </summary>
        /// <param name="bookingId">ID của booking cần lấy</param>
        /// <returns>Thông tin cơ bản của booking</returns>
        [HttpGet("booking/{bookingId}")]
        [Authorize(Roles = "Doctor,Patient")]
        public async Task<ActionResult<object>> GetBookingByIdWithoutReferences(string bookingId)
        {
            if (string.IsNullOrEmpty(bookingId))
            {
                return BadRequest("BookingId is required");
            }

            // Tìm booking theo ID
            var booking = await _context.Bookings
                .AsNoTracking() // Tối ưu hóa hiệu suất vì chỉ đọc dữ liệu
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return NotFound($"Booking with ID {bookingId} not found");
            }

            // Trả về thông tin booking mà không bao gồm các tham chiếu
            return Ok(new
            {
                BookingId = booking.BookingId,
                PatientId = booking.PatientId,
                DoctorId = booking.DoctorId,
                ServiceId = booking.ServiceId,
                SlotId = booking.SlotId,
                DateBooking = booking.DateBooking,
                Description = booking.Description,
                Note = booking.Note,
                Status = booking.Status,
                CreateAt = booking.CreateAt
            });
        }
        /// <summary>
        /// Lấy thông tin cơ bản của bệnh nhân theo ID mà không bao gồm các tham chiếu
        /// </summary>
        /// <param name="patientId">ID của bệnh nhân cần lấy thông tin</param>
        /// <returns>Thông tin cơ bản của bệnh nhân</returns>
        [HttpGet("patient/{patientId}")]
        [Authorize(Roles = "Doctor,Patient")]
        public async Task<ActionResult<object>> GetPatientById(string patientId)
        {
            if (string.IsNullOrEmpty(patientId))
            {
                return BadRequest("PatientId is required");
            }

            // Tìm thông tin bệnh nhân theo ID mà không load các tham chiếu
            var patient = await _context.Patients
                .AsNoTracking() // Tối ưu hóa hiệu suất vì chỉ đọc dữ liệu
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (patient == null)
            {
                return NotFound($"Patient with ID {patientId} not found");
            }

            // Trả về tất cả thông tin của bệnh nhân mà không bao gồm các tham chiếu
            return Ok(new
            {
                PatientId = patient.PatientId,
                UserId = patient.UserId,
                Name = patient.Name,
                Email = patient.Email,
                Phone = patient.Phone,
                Address = patient.Address,
                Gender = patient.Gender,
                DateOfBirth = patient.DateOfBirth,
                BloodType = patient.BloodType,
                EmergencyPhoneNumber = patient.EmergencyPhoneNumber,
                // Các trường khác của bệnh nhân nếu có
            });
        }
        /// <summary>
        /// Tạo mới một bản ghi khám bệnh (Examination)
        /// </summary>
        /// <param name="examinationCreateDTO">Dữ liệu để tạo bản ghi khám bệnh</param>
        /// <returns>Bản ghi khám bệnh đã được tạo</returns>
        [HttpPost("examination")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<object>> CreateExamination([FromBody] ExaminationCreateDTO examinationCreateDTO)
        {
            if (examinationCreateDTO == null)
            {
                return BadRequest("Examination data is required");
            }

            if (string.IsNullOrEmpty(examinationCreateDTO.BookingId))
            {
                return BadRequest("BookingId is required");
            }

            // Tìm booking liên quan
            var booking = await _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Doctor)
                .FirstOrDefaultAsync(b => b.BookingId == examinationCreateDTO.BookingId);

            if (booking == null)
            {
                return NotFound($"Booking with ID {examinationCreateDTO.BookingId} not found");
            }

            // ✅ 1. Kiểm tra xem đã có examination cho booking này chưa
            var existingExamination = await _context.Examinations
                .FirstOrDefaultAsync(e => e.BookingId == examinationCreateDTO.BookingId);

            if (existingExamination != null)
            {
                return BadRequest("Examination already exists for this booking.");
            }

            // Lấy thông tin từ booking
            string patientId = booking.PatientId;
            string doctorId = booking.DoctorId;

            // ✅ 2. Tạo mới examination
            var examination = new Examination
            {
                ExaminationId = "EXM_" + Guid.NewGuid().ToString().Substring(0, 8),
                BookingId = examinationCreateDTO.BookingId,
                PatientId = patientId,
                DoctorId = doctorId,
                // With this corrected line:
                ExaminationDate = examinationCreateDTO.ExaminationDate != default ? examinationCreateDTO.ExaminationDate : DateTime.Now,
                ExaminationDescription = examinationCreateDTO.ExaminationDescription,
                Result = examinationCreateDTO.Result,
                Status = examinationCreateDTO.Status,
                Note = examinationCreateDTO.Note,
                CreateAt = DateTime.Now
            };

            _context.Examinations.Add(examination);

            // ✅ 3. Cập nhật trạng thái booking thành "completed" nếu examination đã hoàn thành
            if (examination.Status?.Trim().ToLower() == "completed" || examination.Status?.Trim().ToLower() == "hoàn thành")
            {
                booking.Status = "completed";
                _context.Entry(booking).State = EntityState.Modified;
            }

            // ✅ 4. Tạo thông báo cho bệnh nhân
            var notification = new Notification
            {
                NotificationId = "NOTIF_" + Guid.NewGuid().ToString().Substring(0, 8),
                PatientId = patientId,
                DoctorId = doctorId,
                Message = $"Bạn đã hoàn tất buổi khám. Mã khám: {examination.ExaminationId}",
                MessageForDoctor = $"Bạn đã hoàn tất buổi khám cho bệnh nhân {booking.Patient?.Name}. Mã khám: {examination.ExaminationId}",
                Time = DateTime.Now,
                Type = "Examination",
                BookingId = examination.BookingId,
                DoctorIsRead = false,
                PatientIsRead = false
            };

            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            // Trả về kết quả
            return Ok(new
            {
                ExaminationId = examination.ExaminationId,
                BookingId = examination.BookingId,
                PatientId = patientId,
                DoctorId = doctorId,
                PatientName = booking.Patient?.Name,
                DoctorName = booking.Doctor?.DoctorName,
                ExaminationDate = examination.ExaminationDate,
                ExaminationDescription = examination.ExaminationDescription,
                Result = examination.Result,
                Status = examination.Status,
                Note = examination.Note,
                CreateAt = examination.CreateAt,
                Message = "Examination record created successfully"
            });
        }

        /// <summary>
        /// Cập nhật trạng thái của bản ghi khám bệnh (Examination) thành "completed"
        /// </summary>
        /// <param name="examinationId">ID của bản ghi khám bệnh cần cập nhật</param>
        /// <returns>Thông báo kết quả cập nhật</returns>
        [HttpPut("examination/{examinationId}/complete")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<object>> CompleteExamination(string examinationId)
        {
            if (string.IsNullOrEmpty(examinationId))
            {
                return BadRequest("ExaminationId is required");
            }

            // Tìm examination theo ID
            var examination = await _context.Examinations
                .Include(e => e.Booking)
                .FirstOrDefaultAsync(e => e.ExaminationId == examinationId);

            if (examination == null)
            {
                return NotFound($"Examination with ID {examinationId} not found");
            }

            // Cập nhật trạng thái examination thành "completed"
            examination.Status = "completed";
            _context.Entry(examination).State = EntityState.Modified;

            // Cập nhật trạng thái booking thành "completed" nếu liên kết với examination này
            if (examination.Booking != null)
            {
                examination.Booking.Status = "completed";
                _context.Entry(examination.Booking).State = EntityState.Modified;
            }

            // Tạo thông báo cho bệnh nhân
            if (!string.IsNullOrEmpty(examination.PatientId))
            {
                var notification = new Notification
                {
                    NotificationId = "NOTIF_" + Guid.NewGuid().ToString().Substring(0, 8),
                    PatientId = examination.PatientId,
                    DoctorId = examination.DoctorId,
                    Message = "Kết quả khám bệnh của bạn đã được hoàn thành.",
                    Time = DateTime.Now,
                    Type = "Examination",
                    BookingId = examination.BookingId,
                    DoctorIsRead = false,
                    PatientIsRead = false
                };

                _context.Notifications.Add(notification);
            }

            await _context.SaveChangesAsync();

            // Trả về kết quả
            return Ok(new
            {
                ExaminationId = examination.ExaminationId,
                Status = examination.Status,
                Message = "Examination status updated to completed successfully"
            });
        }

    }
}