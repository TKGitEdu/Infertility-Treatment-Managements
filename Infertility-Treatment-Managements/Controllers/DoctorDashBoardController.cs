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
            [FromQuery] bool docvachuadoc = false||true)
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

    }
}