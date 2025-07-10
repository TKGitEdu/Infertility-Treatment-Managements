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
        /// Lấy tất cả bản ghi khám bệnh của một bệnh nhân theo ID
        /// </summary>
        /// <param name="patientId">ID của bệnh nhân</param>
        /// <returns>Danh sách các bản ghi khám bệnh của bệnh nhân</returns>
        [HttpGet("patient/{patientId}/examinations")]
        [Authorize(Roles = "Doctor,Patient")]
        public async Task<ActionResult<IEnumerable<object>>> GetExaminationsByPatientId(string patientId)
        {
            if (string.IsNullOrEmpty(patientId))
            {
                return BadRequest("PatientId là bắt buộc");
            }

            // Kiểm tra bệnh nhân tồn tại
            var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == patientId);
            if (!patientExists)
            {
                return NotFound($"Không tìm thấy bệnh nhân với ID {patientId}");
            }

            // Lấy tất cả bản ghi khám bệnh của bệnh nhân
            var examinations = await _context.Examinations
                .Include(e => e.Doctor)
                .Include(e => e.Booking)
                    .ThenInclude(b => b.Service)
                .Where(e => e.PatientId == patientId)
                .OrderByDescending(e => e.ExaminationDate)
                .Select(e => new
                {
                    ExaminationId = e.ExaminationId,
                    DoctorId = e.DoctorId,
                    DoctorName = e.Doctor != null ? e.Doctor.DoctorName : null,
                    PatientId = e.PatientId,
                    BookingId = e.BookingId,
                    ExaminationDate = e.ExaminationDate,
                    ExaminationDescription = e.ExaminationDescription,
                    Status = e.Status,
                    Result = e.Result,
                    Note = e.Note,
                    CreateAt = e.CreateAt,
                    ServiceName = e.Booking != null && e.Booking.Service != null ? e.Booking.Service.Name : null
                })
                .ToListAsync();

            if (!examinations.Any())
            {
                return Ok(new
                {
                    Message = "Bệnh nhân chưa có bản ghi khám bệnh nào",
                    Examinations = new List<object>()
                });
            }

            return Ok(new
            {
                PatientId = patientId,
                ExaminationCount = examinations.Count,
                Examinations = examinations
            });
        }
    }
}