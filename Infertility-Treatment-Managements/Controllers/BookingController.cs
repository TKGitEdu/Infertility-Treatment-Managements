using Infertility_Treatment_Managements.DTOs;
using Infertility_Treatment_Managements.Helpers;
using Infertility_Treatment_Managements.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ServiceDTO = Infertility_Treatment_Managements.DTOs.ServiceDTO;

namespace Infertility_Treatment_Managements.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public BookingController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        // GET: api/Booking/services
        // Lấy danh sách dịch vụ điều trị
        [HttpGet("services")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ServiceDTO>>> GetTreatmentServices()
        {
            var services = await _context.Services
                .Where(s => s.Status == "Active")
                .ToListAsync();

            return Ok(services.Select(s => s.ToDTO()));
        }

        // GET: api/Booking/doctors
        // Lấy danh sách bác sĩ - chỉ trả về ID và tên
        [HttpGet("doctors")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetDoctors()
        {
            try
            {
                // Truy vấn chỉ lấy các trường cần thiết
                var doctors = await _context.Doctors
                    .Select(d => new
                    {
                        doctorId = d.DoctorId,
                        doctorName = d.DoctorName ?? "Không có tên",
                        specialization = d.Specialization ?? "Chuyên khoa chung",
                        phone = d.Phone ?? ""
                    })
                    .ToListAsync();

                return Ok(doctors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy danh sách bác sĩ: {ex.Message}");
            }
        }

        // GET: api/Booking/slots
        // Lấy tất cả các khung giờ
        [HttpGet("slots")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SlotBasicDTO>>> GetAllSlots()
        {
            var slots = await _context.Slots
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            return Ok(slots.Select(s => new SlotBasicDTO
            {
                SlotId = s.SlotId,
                SlotName = s.SlotName,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            }));
        }

        // GET: api/Booking/available-slots
        // Lấy các khung giờ còn trống cho bác sĩ và ngày cụ thể
        [HttpGet("available-slots")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SlotBasicDTO>>> GetAvailableSlots(
            [FromQuery] string doctorId,
            [FromQuery] DateTime date)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(doctorId))
            {
                return BadRequest("Vui lòng chọn bác sĩ");
            }

            // Kiểm tra bác sĩ tồn tại
            var doctor = await _context.Doctors.FindAsync(doctorId);
            if (doctor == null)
            {
                return NotFound($"Không tìm thấy bác sĩ với ID {doctorId}");
            }

            // Lấy danh sách tất cả các khung giờ
            var allSlots = await _context.Slots
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            // Lấy danh sách các khung giờ đã được đặt cho bác sĩ trong ngày đó
            var bookedSlots = await _context.Bookings
                .Where(b => b.DoctorId == doctorId &&
                       b.DateBooking.Date == date.Date)
                .Select(b => b.SlotId)
                .ToListAsync();

            // Lọc ra các khung giờ còn trống
            var availableSlots = allSlots
                .Where(s => !bookedSlots.Contains(s.SlotId))
                .Select(s => new SlotBasicDTO
                {
                    SlotId = s.SlotId,
                    SlotName = s.SlotName,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime
                })
                .ToList();

            return Ok(availableSlots);
        }

        // POST: api/Booking/check-availability
        // Kiểm tra xem một khung giờ cụ thể có sẵn sàng không
        [HttpPost("check-availability")]
        [AllowAnonymous]
        public async Task<ActionResult<bool>> CheckSlotAvailability(CheckSlotAvailabilityDTO checkDTO)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(checkDTO.DoctorId) ||
                string.IsNullOrEmpty(checkDTO.SlotId))
            {
                return BadRequest("Vui lòng cung cấp đầy đủ thông tin bác sĩ và khung giờ");
            }

            // Kiểm tra bác sĩ tồn tại
            var doctor = await _context.Doctors.FindAsync(checkDTO.DoctorId);
            if (doctor == null)
            {
                return NotFound($"Không tìm thấy bác sĩ với ID {checkDTO.DoctorId}");
            }

            // Kiểm tra khung giờ tồn tại
            var slot = await _context.Slots.FindAsync(checkDTO.SlotId);
            if (slot == null)
            {
                return NotFound($"Không tìm thấy khung giờ với ID {checkDTO.SlotId}");
            }

            // Kiểm tra xem khung giờ đã được đặt chưa
            var isBooked = await _context.Bookings
                .AnyAsync(b => b.DoctorId == checkDTO.DoctorId &&
                          b.DateBooking.Date == checkDTO.Date.Date &&
                          b.SlotId == checkDTO.SlotId);

            // Trả về kết quả: true nếu khung giờ còn trống, false nếu đã được đặt
            return Ok(new { isAvailable = !isBooked });
        }

        // POST: api/Booking/create
        // Tạo lịch hẹn mới - yêu cầu đăng nhập
        [HttpPost("create")]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult<BookingDTO>> CreateBooking(SimpleBookingCreateDTO bookingDTO)
        {
            try
            {
                // Lấy UserId từ token
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Không thể xác định người dùng");
                }

                // Tìm Patient từ UserId
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (patient == null)
                {
                    return BadRequest("Không tìm thấy thông tin bệnh nhân");
                }

                // Kiểm tra dịch vụ
                var service = await _context.Services
                    .FirstOrDefaultAsync(s => s.ServiceId == bookingDTO.ServiceId && s.Status == "Active");

                if (service == null)
                {
                    return BadRequest($"Dịch vụ với ID {bookingDTO.ServiceId} không tồn tại hoặc không hoạt động");
                }

                // Kiểm tra bác sĩ
                var doctor = await _context.Doctors.FindAsync(bookingDTO.DoctorId);
                if (doctor == null)
                {
                    return BadRequest($"Bác sĩ với ID {bookingDTO.DoctorId} không tồn tại");
                }

                // Kiểm tra khung giờ
                var slot = await _context.Slots.FindAsync(bookingDTO.SlotId);
                if (slot == null)
                {
                    return BadRequest($"Khung giờ với ID {bookingDTO.SlotId} không tồn tại");
                }

                // Kiểm tra xem khung giờ đã được đặt chưa
                var isBooked = await _context.Bookings
                    .AnyAsync(b => b.DoctorId == bookingDTO.DoctorId &&
                              b.DateBooking.Date == bookingDTO.DateBooking.Date &&
                              b.SlotId == bookingDTO.SlotId);

                if (isBooked)
                {
                    return BadRequest("Khung giờ này đã được đặt. Vui lòng chọn khung giờ khác.");
                }

                // Tạo booking mới
                var booking = new Booking
                {
                    BookingId = "BK_" + Guid.NewGuid().ToString().Substring(0, 8),
                    PatientId = patient.PatientId,
                    ServiceId = bookingDTO.ServiceId,
                    DoctorId = bookingDTO.DoctorId,
                    SlotId = bookingDTO.SlotId,
                    DateBooking = bookingDTO.DateBooking,
                    Description = bookingDTO.Description ?? $"Đặt lịch sử dụng dịch vụ {service.Name}",
                    Note = bookingDTO.Note,
                    CreateAt = DateTime.Now
                };

                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Lấy booking đầy đủ thông tin
                var bookingFull = await _context.Bookings
                    .Include(b => b.Patient)
                    .Include(b => b.Service)
                    .Include(b => b.Doctor)
                    .Include(b => b.Slot)
                    .FirstOrDefaultAsync(b => b.BookingId == booking.BookingId);

                return Ok(bookingFull.ToDTO());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi đặt lịch: {ex.Message}");
            }
        }

        // GET: api/Booking/mybookings
        // Lấy danh sách đặt lịch của bệnh nhân - yêu cầu đăng nhập
        [HttpGet("mybookings")]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetMyBookings()
        {
            // Lấy UserId từ token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định người dùng");
            }

            // Tìm Patient từ UserId
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return BadRequest("Không tìm thấy thông tin bệnh nhân");
            }

            var bookings = await _context.Bookings
                .Include(b => b.Service)
                .Include(b => b.Doctor)
                .Include(b => b.Slot)
                .Where(b => b.PatientId == patient.PatientId)
                .OrderByDescending(b => b.DateBooking)
                .ToListAsync();

            return Ok(bookings.Select(b => b.ToDTO()));
        }

        // GET: api/Booking/details/{id}
        // Lấy chi tiết đặt lịch
        [HttpGet("details/{id}")]
        [Authorize(Roles = "Patient,Doctor,Admin")]
        public async Task<ActionResult<BookingDTO>> GetBookingDetails(string id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Service)
                .Include(b => b.Doctor)
                .Include(b => b.Slot)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound($"Không tìm thấy lịch đặt với ID {id}");
            }

            // Kiểm tra quyền truy cập
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Nếu không phải admin thì kiểm tra quyền
            if (userRole != "Admin")
            {
                // Nếu là patient, chỉ cho phép xem booking của chính mình
                if (userRole == "Patient")
                {
                    var patient = await _context.Patients
                        .FirstOrDefaultAsync(p => p.UserId == userId);

                    if (patient == null || booking.PatientId != patient.PatientId)
                    {
                        return Forbid("Bạn không có quyền xem lịch đặt này");
                    }
                }
                // Nếu là doctor, chỉ cho phép xem booking được chỉ định cho mình
                else if (userRole == "Doctor")
                {
                    var doctor = await _context.Doctors
                        .FirstOrDefaultAsync(d => d.UserId == userId);

                    if (doctor == null || booking.DoctorId != doctor.DoctorId)
                    {
                        return Forbid("Bạn không có quyền xem lịch đặt này");
                    }
                }
            }

            return Ok(booking.ToDTO());
        }

        // DELETE: api/Booking/cancel/{id}
        // Hủy đặt lịch - yêu cầu đăng nhập
        [HttpDelete("cancel/{id}")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> CancelBooking(string id)
        {
            // Lấy UserId từ token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Không thể xác định người dùng");
            }

            // Tìm Patient từ UserId
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return BadRequest("Không tìm thấy thông tin bệnh nhân");
            }

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == id && b.PatientId == patient.PatientId);

            if (booking == null)
            {
                return NotFound($"Không tìm thấy lịch đặt với ID {id}");
            }

            // Kiểm tra nếu đã thanh toán thì không cho hủy
            if (booking.PaymentId != null)
            {
                return BadRequest("Không thể hủy lịch đã thanh toán");
            }

            // Kiểm tra nếu lịch hẹn trong vòng 24 giờ thì không cho hủy
            if (booking.DateBooking <= DateTime.Now.AddHours(24))
            {
                return BadRequest("Không thể hủy lịch hẹn trong vòng 24 giờ trước khi khám");
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã hủy lịch đặt thành công" });
        }
    }

    // DTO classes
    public class SimpleBookingCreateDTO
    {
        public string ServiceId { get; set; }
        public string DoctorId { get; set; }
        public string SlotId { get; set; }
        public DateTime DateBooking { get; set; }
        public string? Description { get; set; }
        public string? Note { get; set; }
    }

    public class CheckSlotAvailabilityDTO
    {
        public string DoctorId { get; set; }
        public string SlotId { get; set; }
        public DateTime Date { get; set; }
    }
}