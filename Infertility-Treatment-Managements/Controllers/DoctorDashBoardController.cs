using Infertility_Treatment_Managements.DTOs;
using Infertility_Treatment_Managements.Helpers;
using Infertility_Treatment_Managements.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
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

            // Update the record
            _context.Entry(booking).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Tìm PatientDetail tương ứng với PatientId
                var patientDetail = await _context.PatientDetails
                    .FirstOrDefaultAsync(pd => pd.PatientId == booking.PatientId);

                if (patientDetail == null)
                {
                    // Nếu không tồn tại, tạo mới PatientDetail
                    patientDetail = new PatientDetail
                    {
                        PatientDetailId = "PATD_" + Guid.NewGuid().ToString().Substring(0, 8),
                        PatientId = booking.PatientId,
                        TreatmentStatus = "Đang điều trị"
                    };

                    _context.PatientDetails.Add(patientDetail);
                    await _context.SaveChangesAsync();
                }

                // Create TreatmentPlan first
                var treatmentPlan = new TreatmentPlan
                {
                    TreatmentPlanId = "TP_" + Guid.NewGuid().ToString().Substring(0, 8),
                    DoctorId = booking.DoctorId,
                    PatientDetailId = patientDetail.PatientDetailId,
                    StartDate = booking.DateBooking,
                    Status = "chờ bác sĩ lên lịch",
                    TreatmentDescription = "chờ bác sĩ lên lịch",
                    Method = "Chưa xác định"
                };
                _context.TreatmentPlans.Add(treatmentPlan);
                await _context.SaveChangesAsync();

                // Then create TreatmentProcess
                var treatmentProcess = new TreatmentProcess
                {
                    TreatmentProcessId = "TP_" + Guid.NewGuid().ToString().Substring(0, 8),
                    DoctorId = booking.DoctorId,
                    PatientDetailId = patientDetail.PatientDetailId,
                    ScheduledDate = booking.DateBooking,
                    Result = "Updated",
                    Status = booking.Status,
                    TreatmentPlanId = treatmentPlan.TreatmentPlanId
                };
                _context.TreatmentProcesses.Add(treatmentProcess);
                await _context.SaveChangesAsync();

                // Finally create Notification
                var notification = new Notification
                {
                    NotificationId = "NOTIF_" + Guid.NewGuid().ToString().Substring(0, 8),
                    PatientId = booking.PatientId,
                    DoctorId = booking.DoctorId,
                    Message = $"Lịch hẹn của bạn với ID {booking.BookingId} đã được bác sĩ cập nhật. Vui lòng kiểm tra thông tin mới.",
                    Time = DateTime.Now,
                    Type = "Booking",
                    BookingId = booking.BookingId,
                    TreatmentProcessId = treatmentProcess.TreatmentProcessId // Reference the created process
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

                return Ok("bác sĩ đã nhận lịch, đang gửi thông báo tới bệnh nhân");
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

                // Tạo thông báo cho bệnh nhân
                string message;
                if (changeRequest.Status == "cancelled")
                {
                    message = $"Lịch hẹn của bạn với ID {booking.BookingId} đã bị hủy bởi bác sĩ {booking.Doctor?.DoctorName}. Lý do: {changeRequest.Note}";
                }
                else if (changeRequest.Status == "rescheduled")
                {
                    message = $"Lịch hẹn của bạn với ID {booking.BookingId} đã được đổi sang ngày {changeRequest.NewDate?.ToString("dd/MM/yyyy")}. Vui lòng kiểm tra thông tin mới.";
                }
                else
                {
                    message = $"Trạng thái lịch hẹn của bạn với ID {booking.BookingId} đã được cập nhật thành: {changeRequest.Status}";
                }

                var notification = new Notification
                {
                    NotificationId = "NOTIF_" + Guid.NewGuid().ToString().Substring(0, 8),
                    PatientId = booking.PatientId,
                    DoctorId = booking.DoctorId,
                    Message = message,
                    Time = DateTime.Now,
                    Type = "Booking",
                    BookingId = booking.BookingId
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
            [FromQuery] int limit = 20,
            [FromQuery] bool docvachuadoc = false || true)
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

            // Lọc theo trạng thái đã đọc nếu cần
            if (docvachuadoc)
            {
                query = query.Where(n => n.DoctorIsRead == null || n.DoctorIsRead == false);
            }

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
        /// <param name="notificationIds">Danh sách ID của các thông báo cần đánh dấu</param>
        /// <returns>Số lượng thông báo đã được cập nhật</returns>
        [HttpPut("notifications/read-all")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult> MarkAllNotificationsAsRead([FromBody] List<string> notificationIds)
        {
            if (notificationIds == null || !notificationIds.Any())
            {
                return BadRequest("At least one notificationId is required");
            }

            // Lấy tất cả thông báo cần đánh dấu
            var notifications = await _context.Notifications
                .Where(n => notificationIds.Contains(n.NotificationId))
                .ToListAsync();

            if (!notifications.Any())
            {
                return NotFound("No notifications found with the provided IDs");
            }

            // Đánh dấu tất cả thông báo đã đọc
            foreach (var notification in notifications)
            {
                notification.DoctorIsRead = true;
                _context.Entry(notification).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            // Chuyển đổi thành NotificationDTO để trả về
            var notificationDTOs = notifications.Select(n => new NotificationDTO
            {
                NotificationId = n.NotificationId,
                PatientId = n.PatientId,
                DoctorId = n.DoctorId,
                BookingId = n.BookingId,
                TreatmentProcessId = n.TreatmentProcessId,
                Type = n.Type,
                Message = n.Message,
                Time = n.Time
            }).ToList();

            return Ok(new
            {
                UpdatedCount = notifications.Count,
                Message = $"{notifications.Count} notifications marked as read",
                Notifications = notificationDTOs
            });
        }
        [HttpGet("examinations")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<IEnumerable<ExaminationDTO>>> GetExaminationsByDoctor([FromQuery] string doctorId)
        {
            if (string.IsNullOrEmpty(doctorId))
                return BadRequest("doctorId is required");

            var exists = await _context.Doctors.AnyAsync(d => d.DoctorId == doctorId);
            if (!exists)
                return NotFound($"Doctor with ID {doctorId} not found");

             var examinations = await (
                 from e in _context.Examinations
                 where e.DoctorId == doctorId
                 orderby e.ExaminationDate descending
                 select new
                 {
                        e.ExaminationId,
                        e.BookingId,
                        e.ExaminationDate,
                        e.ExaminationDescription,
                        e.Result,
                        e.Status,
                        e.Note,
                        e.CreateAt,
                        PatientName = _context.Patients
                            .Where(p => p.PatientId == e.PatientId)
                            .Select(p => p.Name)
                            .FirstOrDefault(),
                        // Nếu muốn trả về ExaminationDTO thì map các trường cần thiết
                 }
             ).ToListAsync();

            return Ok(examinations);
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
                StartDate = dto.StartDate?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now,
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
                    Time = DateTime.Now,
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
                        Time = DateTime.Now,
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

            // Kiểm tra xem treatment plan có tồn tại và bác sĩ có quyền truy cập không
            var treatmentPlan = await _context.TreatmentPlans
                .FirstOrDefaultAsync(tp => tp.TreatmentPlanId == treatmentPlanId);

            if (treatmentPlan == null)
                return NotFound($"Không tìm thấy kế hoạch điều trị với ID {treatmentPlanId}");

            // Lấy DoctorId từ request (có thể sử dụng từ claims token hoặc từ body request)
            var doctorId = HttpContext.User.FindFirst("DoctorId")?.Value;

            // Nếu không có trong token, cần yêu cầu truyền vào
            if (string.IsNullOrEmpty(doctorId))
            {
                // Nếu không có trong token, kiểm tra xem có trong query parameter không
                if (Request.Query.ContainsKey("doctorId"))
                {
                    doctorId = Request.Query["doctorId"];
                }
                else
                {
                    return BadRequest("DoctorId is required");
                }
            }

            // Kiểm tra quyền: chỉ bác sĩ phụ trách mới được cập nhật
            if (treatmentPlan.DoctorId != doctorId)
                return Forbid("Bạn không có quyền cập nhật các bước điều trị cho kế hoạch này");

            try
            {
                // Lấy danh sách các bước hiện tại (nếu có)
                var existingSteps = await _context.TreatmentSteps
                    .Where(ts => ts.TreatmentPlanId == treatmentPlanId)
                    .ToListAsync();

                // Xóa các bước cũ nếu yêu cầu thay thế hoàn toàn
                _context.TreatmentSteps.RemoveRange(existingSteps);

                // Thêm các bước mới
                foreach (var stepDto in steps)
                {
                    var newStep = new TreatmentStep
                    {
                        TreatmentStepId = "TS_" + Guid.NewGuid().ToString().Substring(0, 8),
                        TreatmentPlanId = treatmentPlanId,
                        StepOrder = stepDto.StepOrder,
                        StepName = stepDto.StepName
                    };

                    _context.TreatmentSteps.Add(newStep);
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
                        Time = DateTime.Now,
                        Type = "TreatmentPlan"
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
                        ts.StepName
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Message = "Cập nhật các bước điều trị thành công",
                    TreatmentPlanId = treatmentPlanId,
                    Steps = updatedSteps
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi cập nhật các bước điều trị: {ex.Message}");
            }
        }

        // DTO for treatment steps
        public class TreatmentStepDTO
        {
            public int StepOrder { get; set; }
            public string StepName { get; set; }
        }

    }
}