using Infertility_Treatment_Managements.DTOs;
using Infertility_Treatment_Managements.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infertility_Treatment_Managements.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientDashBoardController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public PatientDashBoardController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy tất cả thông báo của bệnh nhân dựa trên UserId
        /// </summary>
        /// <param name="userId">ID của người dùng</param>
        /// <param name="limit">Số lượng thông báo tối đa cần lấy (mặc định: 20)</param>
        /// <param name="onlyUnread">Chỉ lấy thông báo chưa đọc (mặc định: false)</param>
        /// <returns>Danh sách thông báo của bệnh nhân</returns>
        [HttpGet("notifications")]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult<IEnumerable<object>>> GetPatientNotifications(
            [FromQuery] string userId,
            [FromQuery] int limit = 20,
            [FromQuery] bool onlyUnread = false)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("userId is required");
            }

            // Truy xuất PatientId từ UserId
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return NotFound("Patient not found for the given userId");
            }

            string patientId = patient.PatientId;

            // Xây dựng truy vấn cơ bản
            var query = _context.Notifications
                .Include(n => n.Doctor)
                .Include(n => n.Booking)
                .Include(n => n.TreatmentProcess)
                .Where(n => n.PatientId == patientId);

            // Lọc theo trạng thái đã đọc nếu cần
            if (onlyUnread)
            {
                query = query.Where(n => n.PatientIsRead == null || n.PatientIsRead == false);
            }

            // Thực hiện truy vấn
            var notificationList = await query
                .OrderByDescending(n => n.Time)
                .Take(limit)
                .ToListAsync();

            // Chuyển đổi kết quả thành đối tượng vô danh để trả về
            var notifications = notificationList.Select(n => new
            {
                NotificationId = n.NotificationId,
                PatientId = n.PatientId,
                DoctorId = n.DoctorId,
                DoctorName = n.Doctor?.DoctorName,
                BookingId = n.BookingId,
                TreatmentProcessId = n.TreatmentProcessId,
                Type = n.Type,
                Message = n.Message,
                Time = n.Time,
                PatientIsRead = n.PatientIsRead ?? false,
                BookingDate = n.Booking?.DateBooking,
                BookingStatus = n.Booking?.Status,
                TreatmentStatus = n.TreatmentProcess?.Status
            }).ToList();

            return Ok(notifications);
        }

        /// <summary>
        /// Đánh dấu thông báo đã đọc
        /// </summary>
        /// <param name="notificationId">ID của thông báo</param>
        /// <returns>Kết quả cập nhật trạng thái</returns>
        [HttpPut("notifications/{notificationId}/read")]
        [Authorize(Roles = "Patient")]
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
            notification.PatientIsRead = true;
            _context.Entry(notification).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                NotificationId = notification.NotificationId,
                PatientIsRead = notification.PatientIsRead,
                Message = "Notification marked as read"
            });
        }

        /// <summary>
        /// Đánh dấu tất cả thông báo của bệnh nhân là đã đọc
        /// </summary>
        /// <param name="userId">ID của người dùng (bệnh nhân)</param>
        /// <returns>Số lượng thông báo đã được cập nhật</returns>
        [HttpPut("notifications/read-all")]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult> MarkAllNotificationsAsRead([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("userId is required");
            }

            // Truy xuất PatientId từ UserId
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return NotFound("Patient not found for the given userId");
            }

            string patientId = patient.PatientId;

            // Lấy tất cả thông báo chưa đọc của bệnh nhân
            var notifications = await _context.Notifications
                .Where(n => n.PatientId == patientId && (n.PatientIsRead == null || n.PatientIsRead == false))
                .ToListAsync();

            if (!notifications.Any())
            {
                return Ok(new
                {
                    UpdatedCount = 0,
                    Message = "No unread notifications found for this patient"
                });
            }

            // Đánh dấu tất cả thông báo đã đọc
            foreach (var notification in notifications)
            {
                notification.PatientIsRead = true;
                _context.Entry(notification).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                UpdatedCount = notifications.Count,
                Message = $"{notifications.Count} notifications marked as read"
            });
        }

        /// <summary>
        /// Lấy danh sách kết quả khám bệnh (examinations) dựa trên BookingId và UserId
        /// </summary>
        /// <param name="bookingId">ID của lịch hẹn</param>
        /// <param name="userId">ID của người dùng (bệnh nhân)</param>
        /// <returns>Danh sách kết quả khám bệnh của bệnh nhân cho lịch hẹn cụ thể</returns>
        [HttpGet("examinations")]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult<IEnumerable<object>>> GetExaminationsByBookingAndUser(
            [FromQuery] string bookingId,
            [FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(bookingId))
            {
                return BadRequest("BookingId is required");
            }

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("UserId is required");
            }

            // Truy xuất PatientId từ UserId
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return NotFound("Patient not found for the given userId");
            }

            string patientId = patient.PatientId;

            // Lấy danh sách kết quả khám bệnh khớp với BookingId và PatientId
            var examinations = await _context.Examinations
                .Include(e => e.Doctor)
                .Include(e => e.Booking)
                    .ThenInclude(b => b.Service)
                .Where(e => e.BookingId == bookingId && e.PatientId == patientId)
                .OrderByDescending(e => e.ExaminationDate)
                .ToListAsync();

            if (!examinations.Any())
            {
                return Ok(new List<object>());
            }

            // Chuyển đổi kết quả thành đối tượng vô danh để trả về
            var result = examinations.Select(e => new
            {
                ExaminationId = e.ExaminationId,
                BookingId = e.BookingId,
                PatientId = e.PatientId,
                DoctorId = e.DoctorId,
                DoctorName = e.Doctor?.DoctorName,
                ExaminationDate = e.ExaminationDate,
                ExaminationDescription = e.ExaminationDescription,
                Result = e.Result,
                Status = e.Status,
                Note = e.Note,
                CreateAt = e.CreateAt,
                ServiceName = e.Booking?.Service?.Name
            }).ToList();

            return Ok(result);
        }

    }
}
