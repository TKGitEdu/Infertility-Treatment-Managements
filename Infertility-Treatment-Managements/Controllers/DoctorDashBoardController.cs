using Infertility_Treatment_Managements.DTOs;
using Infertility_Treatment_Managements.Helpers;
using Infertility_Treatment_Managements.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Infertility_Treatment_Managements.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorDashBoardController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public DoctorDashBoardController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        [HttpGet("mybookings")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetMyDoctorBookings([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("userId is required");
            }

            // Truy xuất DoctorId từ UserId
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                return NotFound("Doctor not found for the given userId");
            }

            var doctorId = doctor.DoctorId;

            var bookings = await _context.Bookings
                .Include(b => b.Service)
                .Include(b => b.Patient)
                .Include(b => b.Slot)
                .Where(b => b.DoctorId == doctorId)
                .OrderByDescending(b => b.DateBooking)
                .ToListAsync();

            return Ok(bookings.Select(b => b.ToDTO()));
        }

        // xác nhận lịch hẹn của bệnh nhân, bác sĩ sẽ xác nhận, gửi thông báo tới bệnh nhân
        [HttpPut("booking/{bookingId}")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<BookingDTO>> UpdateBooking(string bookingId, [FromBody] UpdateBookingDTO updateBookingDTO)
        {
            if (string.IsNullOrEmpty(bookingId))
            {
                return BadRequest("Booking ID is required");
            }

            // Find booking by ID
            var booking = await _context.Bookings
                .Include(b => b.Service)
                .Include(b => b.Patient)
                .Include(b => b.Slot)
                .Include(b => b.Doctor)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return NotFound($"Booking with ID {bookingId} not found");
            }

            // Verify the doctor has permission to update this booking
            string doctorId = updateBookingDTO.DoctorId;
            if (string.IsNullOrEmpty(doctorId))
            {
                return BadRequest("Doctor ID is required for booking update");
            }

            if (booking.DoctorId != doctorId)
            {
                return Forbid("You don't have permission to update this booking");
            }

            // Update booking fields if provided
            if (updateBookingDTO.ServiceId != null)
            {
                // Verify service exists
                var service = await _context.Services.FindAsync(updateBookingDTO.ServiceId);
                if (service == null)
                {
                    return BadRequest($"Service with ID {updateBookingDTO.ServiceId} not found");
                }
                booking.ServiceId = updateBookingDTO.ServiceId;
            }

            if (updateBookingDTO.SlotId != null)
            {
                // Verify slot exists
                var slot = await _context.Slots.FindAsync(updateBookingDTO.SlotId);
                if (slot == null)
                {
                    return BadRequest($"Slot with ID {updateBookingDTO.SlotId} not found");
                }

                // Check if the slot is available on the new date
                if (updateBookingDTO.DateBooking.HasValue)
                {
                    var isSlotTaken = await _context.Bookings
                        .AnyAsync(b =>
                            b.SlotId == updateBookingDTO.SlotId &&
                            b.DoctorId == doctorId &&
                            b.DateBooking.Date == updateBookingDTO.DateBooking.Value.Date &&
                            b.BookingId != bookingId);  // Exclude current booking

                    if (isSlotTaken)
                    {
                        return BadRequest("The selected slot is already booked for this date");
                    }
                }
                else
                {
                    var isSlotTaken = await _context.Bookings
                        .AnyAsync(b =>
                            b.SlotId == updateBookingDTO.SlotId &&
                            b.DoctorId == doctorId &&
                            b.DateBooking.Date == booking.DateBooking.Date &&
                            b.BookingId != bookingId);  // Exclude current booking

                    if (isSlotTaken)
                    {
                        return BadRequest("The selected slot is already booked for this date");
                    }
                }

                booking.SlotId = updateBookingDTO.SlotId;
            }

            if (updateBookingDTO.DateBooking.HasValue)
            {
                // If date is changed but slot remains the same, verify the slot is available on the new date
                if (updateBookingDTO.SlotId == null && booking.SlotId != null)
                {
                    var isSlotTaken = await _context.Bookings
                        .AnyAsync(b =>
                            b.SlotId == booking.SlotId &&
                            b.DoctorId == doctorId &&
                            b.DateBooking.Date == updateBookingDTO.DateBooking.Value.Date &&
                            b.BookingId != bookingId);  // Exclude current booking

                    if (isSlotTaken)
                    {
                        return BadRequest("The current slot is already booked for the new date");
                    }
                }

                booking.DateBooking = updateBookingDTO.DateBooking.Value;
            }

            if (updateBookingDTO.Description != null)
            {
                booking.Description = updateBookingDTO.Description;
            }

            if (updateBookingDTO.Note != null)
            {
                booking.Note = updateBookingDTO.Note;
            }

            if (updateBookingDTO.Status != null)
            {
                booking.Status = updateBookingDTO.Status;
            }
            else
            {
                // Nếu không có status được cung cấp, mặc định đánh dấu là "confirmed"
                booking.Status = "confirmed";
            }

            // Update the record
            _context.Entry(booking).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Tạo thông báo xác nhận cho bệnh nhân
                var notification = new Notification
                {
                    NotificationId = "NOTIF_" + Guid.NewGuid().ToString().Substring(0, 8),
                    PatientId = booking.PatientId,
                    DoctorId = booking.DoctorId,
                    Message = $"Lịch hẹn của bạn với ID {booking.BookingId} đã được bác sĩ {booking.Doctor?.DoctorName} xác nhận cho ngày {booking.DateBooking:dd/MM/yyyy}.",
                    MessageForDoctor = $"Bạn đã xác nhận lịch hẹn với bệnh nhân {booking.Patient?.Name} (ID: {booking.PatientId}) vào ngày {booking.DateBooking:dd/MM/yyyy}.",
                    Time = DateTime.UtcNow,
                    Type = "Booking",
                    BookingId = booking.BookingId,
                    PatientIsRead = false,
                    DoctorIsRead = false
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Reload the booking to get updated related entities
                booking = await _context.Bookings
                    .Include(b => b.Service)
                    .Include(b => b.Patient)
                    .Include(b => b.Slot)
                    .Include(b => b.Doctor)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                return Ok(new
                {
                    Message = "Đã xác nhận lịch hẹn và gửi thông báo tới bệnh nhân",
                    Booking = booking.ToDTO()
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await BookingExists(bookingId))
                {
                    return NotFound("Booking not found during update");
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi chung
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Helper method to check if booking exists
        private async Task<bool> BookingExists(string bookingId)
        {
            return await _context.Bookings.AnyAsync(b => b.BookingId == bookingId);
        }

        // DTO for booking update
        public class UpdateBookingDTO
        {
            public string DoctorId { get; set; }
            public string? ServiceId { get; set; }
            public string? SlotId { get; set; }
            public DateTime? DateBooking { get; set; }
            public string? Description { get; set; }
            public string? Note { get; set; }
            public string? Status { get; set; }
        }

        // Hàm cho phép bác sĩ hủy lịch hoặc đổi lịch hẹn
        [HttpPut("booking/{bookingId}/change-status")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult> ChangeBookingStatus(string bookingId, [FromBody] ChangeBookingStatusDTO changeRequest)
        {
            if (string.IsNullOrEmpty(bookingId))
            {
                return BadRequest("Booking ID is required");
            }

            // Tìm booking theo ID
            var booking = await _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Doctor)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return NotFound($"Booking with ID {bookingId} not found");
            }

            // Kiểm tra quyền của bác sĩ
            if (booking.DoctorId != changeRequest.DoctorId)
            {
                return Forbid("You don't have permission to update this booking");
            }

            // Thực hiện thay đổi trạng thái
            booking.Status = changeRequest.Status;

            // Cập nhật note nếu có
            if (!string.IsNullOrEmpty(changeRequest.Note))
            {
                booking.Note = changeRequest.Note;
            }

            // Cập nhật lịch nếu là đổi lịch
            if (changeRequest.Status == "rescheduled" && changeRequest.NewDate.HasValue && !string.IsNullOrEmpty(changeRequest.NewSlotId))
            {
                // Kiểm tra slot tồn tại
                var slot = await _context.Slots.FindAsync(changeRequest.NewSlotId);
                if (slot == null)
                {
                    return BadRequest($"Slot with ID {changeRequest.NewSlotId} not found");
                }

                // Kiểm tra xem khung giờ đã được đặt chưa
                var isSlotTaken = await _context.Bookings
                    .AnyAsync(b =>
                        b.SlotId == changeRequest.NewSlotId &&
                        b.DoctorId == booking.DoctorId &&
                        b.DateBooking.Date == changeRequest.NewDate.Value.Date &&
                        b.BookingId != bookingId);

                if (isSlotTaken)
                {
                    return BadRequest("The selected slot is already booked for this date");
                }

                booking.SlotId = changeRequest.NewSlotId;
                booking.DateBooking = changeRequest.NewDate.Value;
            }

            _context.Entry(booking).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Tạo thông báo cho bệnh nhân và bác sĩ
                string messageForPatient;
                string messageForDoctor;

                if (changeRequest.Status == "cancelled")
                {
                    messageForPatient = $"Lịch hẹn của bạn với ID {booking.BookingId} đã bị hủy bởi bác sĩ {booking.Doctor?.DoctorName}. Lý do: {changeRequest.Note}";
                    messageForDoctor = $"Bạn đã hủy lịch hẹn với bệnh nhân {booking.Patient?.Name} (ID: {booking.PatientId}). Lý do: {changeRequest.Note}";
                }
                else if (changeRequest.Status == "rescheduled")
                {
                    messageForPatient = $"Lịch hẹn của bạn với ID {booking.BookingId} đã được đổi sang ngày {changeRequest.NewDate?.ToString("dd/MM/yyyy")}. Vui lòng kiểm tra thông tin mới.";
                    messageForDoctor = $"Bạn đã đổi lịch hẹn với bệnh nhân {booking.Patient?.Name} (ID: {booking.PatientId}) sang ngày {changeRequest.NewDate?.ToString("dd/MM/yyyy")}. Vui lòng kiểm tra thông tin mới.";
                }
                else
                {
                    messageForPatient = $"Trạng thái lịch hẹn của bạn với ID {booking.BookingId} đã được cập nhật thành: {changeRequest.Status}";
                    messageForDoctor = $"Bạn đã cập nhật trạng thái lịch hẹn với bệnh nhân {booking.Patient?.Name} (ID: {booking.PatientId}) thành: {changeRequest.Status}";
                }

                var notification = new Notification
                {
                    NotificationId = "NOTIF_" + Guid.NewGuid().ToString().Substring(0, 8),
                    PatientId = booking.PatientId,
                    DoctorId = booking.DoctorId,
                    Message = messageForPatient,
                    MessageForDoctor = messageForDoctor,
                    Time = DateTime.UtcNow, // Sử dụng UTC thay vì Local
                    Type = "Booking",
                    BookingId = booking.BookingId,
                    PatientIsRead = false,
                    DoctorIsRead = false
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Trả về kết quả
                return Ok(new
                {
                    Message = $"Booking status has been changed to {changeRequest.Status}",
                    BookingId = booking.BookingId,
                    Status = booking.Status
                });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await BookingExists(bookingId))
                {
                    return NotFound("Booking not found during update");
                }
                else
                {
                    throw;
                }
            }
        }

        // DTO mới cho thay đổi trạng thái booking
        public class ChangeBookingStatusDTO
        {
            public string DoctorId { get; set; }
            public string Status { get; set; } // "cancelled", "rescheduled", etc.
            public string? Note { get; set; }
            public DateTime? NewDate { get; set; } // Chỉ cần thiết khi đổi lịch
            public string? NewSlotId { get; set; } // Chỉ cần thiết khi đổi lịch
        }


        /// <summary>
        /// Đánh dấu thông báo đã đọc
        /// </summary>
        /// <param name="notificationId">ID của thông báo</param>
        /// <returns>Kết quả cập nhật trạng thái</returns>
        [HttpPut("notifications/{notificationId}/read")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult> MarkNotificationAsRead(string notificationId)
        {
            if (string.IsNullOrEmpty(notificationId))
            {
                return BadRequest("notificationId is required");
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

            if (notification == null)
            {
                return NotFound($"Notification with ID {notificationId} not found");
            }

            // Đánh dấu thông báo đã đọc
            notification.DoctorIsRead = true;
            _context.Entry(notification).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                NotificationId = notification.NotificationId,
                DoctorIsRead = notification.DoctorIsRead,
                Message = "Notification marked as read"
            });
        }
        /// <summary>
        /// Lấy danh sách thông báo của bác sĩ dựa trên userId
        /// </summary>
        /// <param name="userId">ID của người dùng (bác sĩ)</param>
        /// <param name="limit">Số lượng thông báo tối đa cần lấy</param>
        /// <param name="onlyUnread">Chỉ lấy các thông báo chưa đọc</param>
        /// <returns>Danh sách thông báo của bác sĩ</returns>
        [HttpGet("notifications")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<IEnumerable<object>>> GetDoctorNotifications(
            [FromQuery] string userId,
            [FromQuery] int limit = 20)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("userId is required");
            }

            // Truy xuất DoctorId từ UserId
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                return NotFound("Doctor not found for the given userId");
            }

            string doctorId = doctor.DoctorId;

            // Xây dựng truy vấn cơ bản
            var query = _context.Notifications
                .Include(n => n.Patient)
                .Include(n => n.Booking)
                .Include(n => n.TreatmentProcess)
                .Where(n => n.DoctorId == doctorId);


            // Thực hiện truy vấn
            var notificationList = await query
                .OrderByDescending(n => n.Time)
                .Take(limit)
                .ToListAsync();

            // Chuyển đổi kết quả ở bên ngoài LINQ query để tránh lỗi null
            var notifications = notificationList.Select(n => new
            {
                NotificationId = n.NotificationId,
                PatientId = n.PatientId,
                PatientName = n.Patient?.Name,
                DoctorId = n.DoctorId,
                BookingId = n.BookingId,
                TreatmentProcessId = n.TreatmentProcessId,
                Type = n.Type,
                Message = n.Message,
                MessageForDoctor = n.MessageForDoctor,
                Time = n.Time,
                DoctorIsRead = n.DoctorIsRead ?? false,
                BookingDate = n.Booking?.DateBooking,
                BookingStatus = n.Booking?.Status,
                TreatmentStatus = n.TreatmentProcess?.Status
            }).ToList();

            return Ok(notifications);
        }
        /// <summary>
        /// Đánh dấu tất cả thông báo của bác sĩ là đã đọc
        /// </summary>
        /// <param name="doctorId">ID của bác sĩ</param>
        /// <returns>Kết quả cập nhật trạng thái</returns>
        [HttpPut("notifications/read-all")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult> MarkAllNotificationsAsRead([FromQuery] string doctorId)
        {
            if (string.IsNullOrEmpty(doctorId))
            {
                return BadRequest("DoctorId is required");
            }

            // Kiểm tra bác sĩ tồn tại
            var doctorExists = await _context.Doctors.AnyAsync(d => d.DoctorId == doctorId);
            if (!doctorExists)
            {
                return NotFound($"Doctor with ID {doctorId} not found");
            }

            // Lấy tất cả thông báo chưa đọc của bác sĩ
            var notifications = await _context.Notifications
                .Where(n => n.DoctorId == doctorId && (n.DoctorIsRead == null || n.DoctorIsRead == false))
                .ToListAsync();

            if (!notifications.Any())
            {
                return Ok(new { Message = "Không có thông báo nào cần đánh dấu đã đọc" });
            }

            // Đánh dấu tất cả thông báo đã đọc
            foreach (var notification in notifications)
            {
                notification.DoctorIsRead = true;
                _context.Entry(notification).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                UpdatedCount = notifications.Count,
                Message = $"Đã đánh dấu {notifications.Count} thông báo là đã đọc"
            });
        }

        /// <summary>
        /// Lấy danh sách các buổi khám của bác sĩ theo doctorId
        /// </summary>
        /// <param name="doctorId">ID của bác sĩ</param>
        /// <returns>Danh sách các buổi khám của bác sĩ</returns>
        [HttpGet("examinations")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<IEnumerable<object>>> GetDoctorExaminations([FromQuery] string doctorId)
        {
            if (string.IsNullOrEmpty(doctorId))
            {
                return BadRequest("doctorId is required");
            }

            // Kiểm tra bác sĩ tồn tại
            var doctorExists = await _context.Doctors.AnyAsync(d => d.DoctorId == doctorId);
            if (!doctorExists)
            {
                return NotFound($"Doctor with ID {doctorId} not found");
            }

            // Lấy danh sách các buổi khám của bác sĩ
            var examinations = await _context.Examinations
                .Include(e => e.Booking)
                    .ThenInclude(b => b.Patient)
                .Include(e => e.Booking)
                    .ThenInclude(b => b.Service)
                .Where(e => e.DoctorId == doctorId)
                .OrderByDescending(e => e.ExaminationDate)
                .ToListAsync();

            // Chuyển đổi kết quả thành đối tượng phù hợp để trả về
            var result = examinations.Select(e => new
            {
                ExaminationId = e.ExaminationId,
                BookingId = e.BookingId,
                PatientId = e.PatientId,
                PatientName = e.Booking?.Patient?.Name,
                DoctorId = e.DoctorId,
                ExaminationDate = e.ExaminationDate,
                ExaminationDescription = e.ExaminationDescription,
                Result = e.Result,
                Status = e.Status,
                Note = e.Note,
                CreateAt = e.CreateAt,
                ServiceName = e.Booking?.Service?.Name,
                ServiceId = e.Booking?.ServiceId
            }).ToList();

            return Ok(result);
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
                // Fix for CS0019: Operator '??' cannot be applied to operands of type 'DateTime' and 'DateTime'
                // The issue occurs because DateTime is a value type and cannot be null. Instead, use Nullable<DateTime> (DateTime?) for null checks.
                ExaminationDate = examinationCreateDTO.ExaminationDate != default ? examinationCreateDTO.ExaminationDate : DateTime.UtcNow,
                ExaminationDescription = examinationCreateDTO.ExaminationDescription,
                Result = examinationCreateDTO.Result,
                Status = examinationCreateDTO.Status,
                Note = examinationCreateDTO.Note,
                CreateAt = DateTime.UtcNow
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
                Time = DateTime.UtcNow,
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

        [HttpGet("treatmentplans")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<IEnumerable<object>>> GetTreatmentPlansByDoctor([FromQuery] string doctorId)
        {
            if (string.IsNullOrEmpty(doctorId))
                return BadRequest("doctorId is required");

            var exists = await _context.Doctors.AnyAsync(d => d.DoctorId == doctorId);
            if (!exists)
                return NotFound($"Doctor with ID {doctorId} not found");

            var treatmentPlans = await _context.TreatmentPlans
                .Include(tp => tp.PatientDetail)
                .Where(tp => tp.DoctorId == doctorId)
                .OrderByDescending(tp => tp.StartDate)
                .Select(tp => new
                {
                    tp.TreatmentPlanId,
                    tp.DoctorId,
                    tp.ServiceId,
                    tp.Method,
                    tp.PatientDetailId,
                    StartDate = tp.StartDate != null ? DateOnly.FromDateTime(tp.StartDate.Value) : (DateOnly?)null,
                    EndDate = tp.EndDate != null ? DateOnly.FromDateTime(tp.EndDate.Value) : (DateOnly?)null,
                    tp.Status,
                    tp.TreatmentDescription,
                    PatientDetailName = tp.PatientDetail != null ? tp.PatientDetail.Name : null,
                    tp.Giaidoan,
                    tp.GhiChu

                })
                .ToListAsync();

            return Ok(treatmentPlans);
        }

        [HttpPut("treatmentplan/update")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> UpdateTreatmentPlan([FromBody] TreatmentPlanUpdateDTO dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.TreatmentPlanId))
                return BadRequest("Thiếu thông tin TreatmentPlanId");

            // Tìm treatment plan theo ID
            var plan = await _context.TreatmentPlans.FirstOrDefaultAsync(tp => tp.TreatmentPlanId == dto.TreatmentPlanId);
            if (plan == null)
                return NotFound($"Không tìm thấy TreatmentPlan với ID {dto.TreatmentPlanId}");

            // Kiểm tra quyền: chỉ cho phép bác sĩ sở hữu kế hoạch được sửa
            if (plan.DoctorId != dto.DoctorId)
                return Forbid("Bạn không có quyền cập nhật kế hoạch điều trị này");

            // Cập nhật các trường
            plan.Method = dto.Method;
            plan.PatientDetailId = dto.PatientDetailId;
            plan.StartDate = dto.StartDate?.ToDateTime(TimeOnly.MinValue);
            plan.EndDate = dto.EndDate?.ToDateTime(TimeOnly.MinValue);
            plan.Status = dto.Status;
            plan.TreatmentDescription = dto.TreatmentDescription;
            plan.Giaidoan = dto.Giaidoan; // Giai đoạn điều trị
            plan.GhiChu = dto.GhiChu; // Ghi chú nếu có

            _context.TreatmentPlans.Update(plan);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật kế hoạch điều trị thành công" });
        }

        // DTO for creating treatment plan with minimal required fields from frontend
        public class CreateTreatmentPlanDTO
        {
            // Required fields from frontend
            public string DoctorId { get; set; }
            public string PatientId { get; set; }
            public string ServiceId { get; set; }

            // Optional fields that can be set later
            public string? Method { get; set; }
            public DateOnly? StartDate { get; set; }
            public DateOnly? EndDate { get; set; }
            public string? Status { get; set; }
            public string? TreatmentDescription { get; set; }
            public string? Giaidoan { get; set; } // Treatment stage
            public string? GhiChu { get; set; } // Notes, can be null
        }

        [HttpPost("treatmentplan/create")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<object>> CreateTreatmentPlan([FromBody] CreateTreatmentPlanDTO dto)
        {
            // Validate required fields
            if (string.IsNullOrEmpty(dto.DoctorId))
                return BadRequest("DoctorId is required");

            if (string.IsNullOrEmpty(dto.PatientId))
                return BadRequest("PatientId is required");

            if (string.IsNullOrEmpty(dto.ServiceId))
                return BadRequest("ServiceId is required");

            // Verify doctor exists
            var doctor = await _context.Doctors.FindAsync(dto.DoctorId);
            if (doctor == null)
                return NotFound($"Doctor with ID {dto.DoctorId} not found");

            // Verify service exists
            var service = await _context.Services.FindAsync(dto.ServiceId);
            if (service == null)
                return NotFound($"Service with ID {dto.ServiceId} not found");

            // Find or create PatientDetail for the PatientId
            var patientDetail = await _context.PatientDetails
                .FirstOrDefaultAsync(pd => pd.PatientId == dto.PatientId);

            if (patientDetail == null)
            {
                // Verify patient exists before creating PatientDetail
                var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == dto.PatientId);
                if (!patientExists)
                    return NotFound($"Patient with ID {dto.PatientId} not found");

                // Create new PatientDetail if it doesn't exist
                patientDetail = new PatientDetail
                {
                    PatientDetailId = "PATD_" + Guid.NewGuid().ToString().Substring(0, 8),
                    PatientId = dto.PatientId,
                    TreatmentStatus = "Đang điều trị"
                };

                _context.PatientDetails.Add(patientDetail);
                await _context.SaveChangesAsync();
            }

            // Create a new treatment plan with unique ID
            var treatmentPlan = new TreatmentPlan
            {
                TreatmentPlanId = "TP_" + Guid.NewGuid().ToString().Substring(0, 8),
                DoctorId = dto.DoctorId,
                ServiceId = dto.ServiceId,
                PatientDetailId = patientDetail.PatientDetailId,

                // Set fields with default values if not provided
                Method = dto.Method ?? "Chưa xác định",
                StartDate = dto.StartDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.UtcNow,
                EndDate = dto.EndDate?.ToDateTime(TimeOnly.MinValue),
                Status = dto.Status ?? "Mới tạo",
                TreatmentDescription = dto.TreatmentDescription ?? "Chờ bác sĩ cập nhật thông tin",
                Giaidoan = dto.Giaidoan ?? "Giai đoạn 1",
                GhiChu = dto.GhiChu
            };

            try
            {
                // Add and save the treatment plan
                _context.TreatmentPlans.Add(treatmentPlan);
                await _context.SaveChangesAsync();

                // Create notification for patient
                var notification = new Notification
                {
                    NotificationId = "NOTIF_" + Guid.NewGuid().ToString().Substring(0, 8),
                    PatientId = dto.PatientId,
                    DoctorId = dto.DoctorId,
                    Message = $"Bác sĩ {doctor.DoctorName} đã tạo kế hoạch điều trị mới cho bạn. Vui lòng kiểm tra thông tin chi tiết.",
                    MessageForDoctor = $"Bạn đã tạo kế hoạch điều trị mới cho bệnh nhân {patientDetail.Patient?.Name}. Vui lòng kiểm tra thông tin chi tiết.",
                    Time = DateTime.UtcNow,
                    Type = "TreatmentPlan"
                };
                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Load related entities to return complete data
                var result = await _context.TreatmentPlans
                    .Include(tp => tp.Doctor)
                    .Include(tp => tp.PatientDetail)
                        .ThenInclude(pd => pd.Patient)
                    .Include(tp => tp.Service)
                    .FirstOrDefaultAsync(tp => tp.TreatmentPlanId == treatmentPlan.TreatmentPlanId);

                // Map to response object and return
                return Ok(new
                {
                    Message = "Tạo kế hoạch điều trị thành công",
                    TreatmentPlan = new
                    {
                        result.TreatmentPlanId,
                        result.DoctorId,
                        result.ServiceId,
                        result.PatientDetailId,
                        PatientId = result.PatientDetail?.PatientId,
                        result.Method,
                        StartDate = result.StartDate.HasValue ? DateOnly.FromDateTime(result.StartDate.Value) : (DateOnly?)null,
                        EndDate = result.EndDate.HasValue ? DateOnly.FromDateTime(result.EndDate.Value) :  (DateOnly?)null,
                        result.Status,
                        result.TreatmentDescription,
                        result.Giaidoan,
                        result.GhiChu,
                        Doctor = result.Doctor != null ? new
                        {
                            result.Doctor.DoctorId,
                            result.Doctor.DoctorName
                        } : null,
                        PatientDetail = result.PatientDetail != null ? new
                        {
                            result.PatientDetail.PatientDetailId,
                            result.PatientDetail.PatientId,
                            PatientName = result.PatientDetail.Patient?.Name
                        } : null,
                        Service = result.Service != null ? new
                        {
                            result.Service.ServiceId,
                            result.Service.Name
                        } : null
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        // DTO for flexible treatment plan update with specific fields
        public class FlexibleTreatmentPlanUpdateDTO
        {
            // Required fields
            public string TreatmentPlanId { get; set; }
            public string DoctorId { get; set; }

            // Optional fields for flexible update
            public string? Method { get; set; }
            public DateOnly? StartDate { get; set; }
            public DateOnly? EndDate { get; set; }
            public string? TreatmentDescription { get; set; }
            public string? Status { get; set; } // List of stages as semicolon-separated string
            public string? Giaidoan { get; set; } // Current stage, default: "in-progress"
            public string? GhiChu { get; set; } // Notes, can be null
        }

        /// <summary>
        /// Cập nhật kế hoạch điều trị với các trường linh hoạt
        /// </summary>
        /// <param name="dto">DTO chứa thông tin cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPut("treatmentplan/flexible-update")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> UpdateTreatmentPlanFlexible([FromBody] FlexibleTreatmentPlanUpdateDTO dto)
        {
            // Validate required fields
            if (dto == null || string.IsNullOrEmpty(dto.TreatmentPlanId))
                return BadRequest("Thiếu thông tin TreatmentPlanId");

            if (string.IsNullOrEmpty(dto.DoctorId))
                return BadRequest("Thiếu thông tin DoctorId");

            // Tìm treatment plan theo ID
            var plan = await _context.TreatmentPlans
                .Include(tp => tp.PatientDetail)
                .FirstOrDefaultAsync(tp => tp.TreatmentPlanId == dto.TreatmentPlanId);

            if (plan == null)
                return NotFound($"Không tìm thấy kế hoạch điều trị với ID {dto.TreatmentPlanId}");

            // Kiểm tra quyền: chỉ cho phép bác sĩ sở hữu kế hoạch được sửa
            if (plan.DoctorId != dto.DoctorId)
                return Forbid("Bạn không có quyền cập nhật kế hoạch điều trị này");

            // Track if any changes were made
            bool hasChanges = false;

            // Cập nhật các trường một cách linh hoạt, chỉ khi có giá trị được cung cấp
            if (dto.Method != null)
            {
                plan.Method = dto.Method;
                hasChanges = true;
            }

            if (dto.StartDate.HasValue)
            {
                plan.StartDate = dto.StartDate.Value.ToDateTime(TimeOnly.MinValue);
                hasChanges = true;
            }

            if (dto.EndDate.HasValue)
            {
                plan.EndDate = dto.EndDate.Value.ToDateTime(TimeOnly.MinValue);
                hasChanges = true;
            }

            if (dto.TreatmentDescription != null)
            {
                plan.TreatmentDescription = dto.TreatmentDescription;
                hasChanges = true;
            }

            if (dto.Status != null)
            {
                plan.Status = dto.Status; // Store status as semicolon-separated list
                hasChanges = true;
            }

            if (dto.Giaidoan != null)
            {
                plan.Giaidoan = dto.Giaidoan;
                hasChanges = true;
            }
            else if (hasChanges && string.IsNullOrEmpty(plan.Giaidoan))
            {
                // If other changes were made and Giaidoan is empty, set default value
                plan.Giaidoan = "in-progress";
            }

            if (!hasChanges)
                return BadRequest("Không có thông tin nào được cập nhật");

            try
            {
                _context.TreatmentPlans.Update(plan);
                await _context.SaveChangesAsync();

                // Create notification for patient about the update
                if (plan.PatientDetail?.PatientId != null)
                {
                    var notification = new Notification
                    {
                        NotificationId = "NOTIF_" + Guid.NewGuid().ToString().Substring(0, 8),
                        PatientId = plan.PatientDetail.PatientId,
                        DoctorId = dto.DoctorId,
                        Message = $"Kế hoạch điều trị của bạn đã được cập nhật. Vui lòng kiểm tra thông tin mới.",
                        Time = DateTime.UtcNow,
                        Type = "TreatmentPlan"
                    };
                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                }

                // Return updated plan data
                return Ok(new
                {
                    Message = "Cập nhật kế hoạch điều trị thành công",
                    TreatmentPlan = new
                    {
                        plan.TreatmentPlanId,
                        plan.DoctorId,
                        plan.ServiceId,
                        plan.PatientDetailId,
                        PatientId = plan.PatientDetail?.PatientId,
                        plan.Method,
                        StartDate = plan.StartDate.HasValue ? DateOnly.FromDateTime(plan.StartDate.Value) : (DateOnly?)null,
                        EndDate = plan.EndDate.HasValue ? DateOnly.FromDateTime(plan.EndDate.Value) : (DateOnly?)null,
                        plan.Status,
                        StatusList = plan.Status?.Split(';').Select(s => s.Trim()).ToList(),
                        plan.TreatmentDescription,
                        plan.Giaidoan,
                        plan.GhiChu
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi cập nhật kế hoạch điều trị: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy danh sách các bước điều trị (treatment steps) cho một kế hoạch điều trị cụ thể
        /// </summary>
        /// <param name="treatmentPlanId">ID của kế hoạch điều trị</param>
        /// <returns>Danh sách các bước điều trị</returns>
        [HttpGet("treatmentplan/{treatmentPlanId}/steps")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<IEnumerable<object>>> GetTreatmentStepsByPlanId(string treatmentPlanId)
        {
            if (string.IsNullOrEmpty(treatmentPlanId))
                return BadRequest("TreatmentPlanId is required");

            // Kiểm tra xem treatment plan có tồn tại không
            var treatmentPlan = await _context.TreatmentPlans
                .FirstOrDefaultAsync(tp => tp.TreatmentPlanId == treatmentPlanId);

            if (treatmentPlan == null)
                return NotFound($"Không tìm thấy kế hoạch điều trị với ID {treatmentPlanId}");

            // Lấy danh sách các bước điều trị theo thứ tự
            var steps = await _context.TreatmentSteps
                .Where(ts => ts.TreatmentPlanId == treatmentPlanId)
                .OrderBy(ts => ts.StepOrder)
                .Select(ts => new
                {
                    ts.TreatmentStepId,
                    ts.TreatmentPlanId,
                    ts.StepOrder,
                    ts.StepName,
                    // Có thể thêm các trường khác của TreatmentStep nếu cần
                })
                .ToListAsync();

            if (!steps.Any())
                return Ok(new { Message = "Kế hoạch điều trị này chưa có bước điều trị nào", Steps = new List<object>() });

            return Ok(new
            {
                TreatmentPlanId = treatmentPlanId,
                Steps = steps
            });
        }

        /// <summary>
        /// Tạo mới hoặc cập nhật danh sách các bước điều trị cho một kế hoạch điều trị
        /// </summary>
        /// <param name="treatmentPlanId">ID của kế hoạch điều trị</param>
        /// <param name="steps">Danh sách các bước điều trị cần thêm/cập nhật</param>
        /// <returns>Kết quả thực hiện</returns>
        [HttpPost("treatmentplan/{treatmentPlanId}/steps")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult> CreateOrUpdateTreatmentSteps(string treatmentPlanId, [FromBody] List<TreatmentStepDTO> steps)
        {
            if (string.IsNullOrEmpty(treatmentPlanId))
                return BadRequest("TreatmentPlanId is required");

            if (steps == null || !steps.Any())
                return BadRequest("Danh sách các bước điều trị không được trống");
            //Nếu bạn muốn cho phép xóa hết các bước cũ(tức là cập nhật về rỗng), thì sửa đoạn đó thành:
            //if (steps == null)
            //    return BadRequest("Danh sách các bước điều trị không được null");
            try
            {
                // Lấy userId từ token
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID not found in token");
                }

                // Truy xuất DoctorId từ UserId
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.UserId == userId);

                if (doctor == null)
                {
                    return NotFound("Doctor not found for the authenticated user");
                }

                string doctorId = doctor.DoctorId;

                // Kiểm tra xem treatment plan có tồn tại không
                var treatmentPlan = await _context.TreatmentPlans
                    .FirstOrDefaultAsync(tp => tp.TreatmentPlanId == treatmentPlanId);

                if (treatmentPlan == null)
                    return NotFound($"Không tìm thấy kế hoạch điều trị với ID {treatmentPlanId}");

                // Kiểm tra quyền: chỉ bác sĩ phụ trách mới được cập nhật
                if (treatmentPlan.DoctorId != doctorId)
                    return Forbid("Bạn không có quyền cập nhật các bước điều trị cho kế hoạch này");

                // Lấy danh sách các bước hiện tại
                var existingSteps = await _context.TreatmentSteps
                    .Where(ts => ts.TreatmentPlanId == treatmentPlanId)
                    .ToListAsync();

                // Tạo từ điển để tra cứu nhanh các bước hiện có theo StepOrder
                var existingStepsDict = existingSteps.ToDictionary(s => s.StepOrder);

                // Theo dõi các bước đã được xử lý
                var processedStepOrders = new HashSet<int>();
                var stepsUpdated = 0;
                var stepsCreated = 0;

                // Duyệt qua từng bước được gửi lên
                foreach (var stepDto in steps)
                {
                    if (existingStepsDict.TryGetValue(stepDto.StepOrder, out var existingStep))
                    {
                        // Nếu đã có bước với StepOrder này, cập nhật thông tin
                        existingStep.StepName = stepDto.StepName;
                        existingStep.Description = stepDto.Description ?? $"Mô tả cho bước {stepDto.StepOrder}: {stepDto.StepName}";

                        _context.Entry(existingStep).State = EntityState.Modified;
                        stepsUpdated++;
                    }
                    else
                    {
                        // Nếu chưa có bước với StepOrder này, tạo mới
                        var newStep = new TreatmentStep
                        {
                            TreatmentStepId = "TS_" + Guid.NewGuid().ToString().Substring(0, 8),
                            TreatmentPlanId = treatmentPlanId,
                            StepOrder = stepDto.StepOrder,
                            StepName = stepDto.StepName,
                            Description = stepDto.Description ?? $"Mô tả cho bước {stepDto.StepOrder}: {stepDto.StepName}"
                        };

                        _context.TreatmentSteps.Add(newStep);
                        stepsCreated++;
                    }

                    processedStepOrders.Add(stepDto.StepOrder);
                }

                // Xóa các bước không còn trong danh sách mới
                var stepsToRemove = existingSteps
                    .Where(step => !processedStepOrders.Contains(step.StepOrder))
                    .ToList();

                if (stepsToRemove.Any())
                {
                    _context.TreatmentSteps.RemoveRange(stepsToRemove);
                }

                await _context.SaveChangesAsync();

                // Tạo thông báo cho bệnh nhân
                var patient = await _context.PatientDetails
                    .FirstOrDefaultAsync(pd => pd.PatientDetailId == treatmentPlan.PatientDetailId);

                if (patient != null)
                {
                    var notification = new Notification
                    {
                        NotificationId = "NOTIF_" + Guid.NewGuid().ToString().Substring(0, 8),
                        PatientId = patient.PatientId,
                        DoctorId = doctorId,
                        Message = "Bác sĩ đã cập nhật các bước trong kế hoạch điều trị của bạn. Vui lòng kiểm tra thông tin mới.",
                        MessageForDoctor = $"Bạn đã cập nhật các bước điều trị cho bệnh nhân {patient.Name}.",
                        Time = DateTime.UtcNow,
                        Type = "TreatmentPlan",
                        PatientIsRead = false,
                        DoctorIsRead = false
                    };

                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();
                }

                // Lấy danh sách các bước sau khi cập nhật
                var updatedSteps = await _context.TreatmentSteps
                    .Where(ts => ts.TreatmentPlanId == treatmentPlanId)
                    .OrderBy(ts => ts.StepOrder)
                    .Select(ts => new
                    {
                        ts.TreatmentStepId,
                        ts.TreatmentPlanId,
                        ts.StepOrder,
                        ts.StepName,
                        ts.Description
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Message = $"Đã cập nhật các bước điều trị thành công: {stepsCreated} bước mới, {stepsUpdated} bước cập nhật, {stepsToRemove.Count} bước đã xóa",
                    DoctorId = doctorId,
                    TreatmentPlanId = treatmentPlanId,
                    Steps = updatedSteps
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi cập nhật các bước điều trị: {ex.Message}");
            }
        }

        // Cập nhật DTO để bao gồm trường Description
        public class TreatmentStepDTO
        {
            public int StepOrder { get; set; }
            public string StepName { get; set; }
            public string? Description { get; set; } // Thêm trường Description
        }

    }
}