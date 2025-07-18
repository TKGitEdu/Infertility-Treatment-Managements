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
    public class PatientServiceController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public PatientServiceController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        // GET: api/PatientService
        // Danh sách tất cả dịch vụ đang hoạt động - không yêu cầu đăng nhập
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ServiceDTO>>> GetActiveServices()
        {
            var services = await _context.Services
                .Where(s => s.Status == "Active")
                .ToListAsync();

            return Ok(services.Select(s => s.ToDTO()));
        }

        // GET: api/PatientService/5
        // Chi tiết dịch vụ - không yêu cầu đăng nhập
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ServiceDTO>> GetServiceById(string id)
        {
            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.ServiceId == id && s.Status == "Active");

            if (service == null)
            {
                return NotFound($"Không tìm thấy dịch vụ với ID {id}");
            }

            return Ok(service.ToDTO());
        }

        // GET: api/PatientService/category/{category}
        // Lọc dịch vụ theo danh mục - không yêu cầu đăng nhập
        [HttpGet("category/{category}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ServiceDTO>>> GetServicesByCategory(string category)
        {
            var services = await _context.Services
                .Where(s => s.Category == category && s.Status == "Active")
                .ToListAsync();

            return Ok(services.Select(s => s.ToDTO()));
        }

        // POST: api/PatientService/booking
        // Đặt lịch sử dụng dịch vụ - yêu cầu đăng nhập
        [HttpPost("booking")]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult<BookingDTO>> BookService(BookingCreateDTO bookingCreateDTO)
        {
            try
            {
                // Lấy PatientId từ token
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
                    .FirstOrDefaultAsync(s => s.ServiceId == bookingCreateDTO.ServiceId && s.Status == "Active");

                if (service == null)
                {
                    return BadRequest($"Dịch vụ với ID {bookingCreateDTO.ServiceId} không tồn tại hoặc không hoạt động");
                }

                // Kiểm tra bác sĩ nếu có chọn
                if (!string.IsNullOrEmpty(bookingCreateDTO.DoctorId))
                {
                    var doctor = await _context.Doctors.FindAsync(bookingCreateDTO.DoctorId);
                    if (doctor == null)
                    {
                        return BadRequest($"Bác sĩ với ID {bookingCreateDTO.DoctorId} không tồn tại");
                    }
                }

                // Kiểm tra khung giờ nếu có chọn
                if (!string.IsNullOrEmpty(bookingCreateDTO.SlotId))
                {
                    var slot = await _context.Slots.FindAsync(bookingCreateDTO.SlotId);
                    if (slot == null)
                    {
                        return BadRequest($"Khung giờ với ID {bookingCreateDTO.SlotId} không tồn tại");
                    }
                }

                // Tạo booking mới
                var booking = new Booking
                {
                    BookingId = "BK_" + Guid.NewGuid().ToString().Substring(0, 8),
                    PatientId = patient.PatientId,
                    ServiceId = bookingCreateDTO.ServiceId,
                    DoctorId = bookingCreateDTO.DoctorId,
                    SlotId = bookingCreateDTO.SlotId,
                    DateBooking = bookingCreateDTO.DateBooking,
                    Description = bookingCreateDTO.Description ?? $"Đặt lịch sử dụng dịch vụ {service.Name}",
                    Note = bookingCreateDTO.Note,
                    CreateAt = DateTime.UtcNow
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

                return CreatedAtAction(nameof(GetServiceById), new { id = service.ServiceId }, bookingFull.ToDTO());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi đặt lịch: {ex.Message}");
            }
        }

        // GET: api/PatientService/mybookings
        // Lấy danh sách đặt lịch của bệnh nhân - yêu cầu đăng nhập
        [HttpGet("mybookings")]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetMyBookings()
        {
            // Lấy PatientId từ token
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
                .OrderByDescending(b => b.CreateAt)
                .ToListAsync();

            return Ok(bookings.Select(b => b.ToDTO()));
        }

        // GET: api/PatientService/doctors
        // Lấy danh sách bác sĩ - không yêu cầu đăng nhập
        [HttpGet("doctors")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DoctorBasicDTO>>> GetDoctors()
        {
            var doctors = await _context.Doctors.ToListAsync();

            return Ok(doctors.Select(d => new DoctorBasicDTO
            {
                DoctorId = d.DoctorId,
                DoctorName = d.DoctorName,
                Email = d.Email,
                Phone = d.Phone,
                Specialization = d.Specialization
            }));
        }

        // GET: api/PatientService/slots
        // Lấy danh sách khung giờ - không yêu cầu đăng nhập
        [HttpGet("slots")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SlotBasicDTO>>> GetSlots()
        {
            var slots = await _context.Slots.ToListAsync();

            return Ok(slots.Select(s => new SlotBasicDTO
            {
                SlotId = s.SlotId,
                SlotName = s.SlotName,
                StartTime = s.StartTime,
                EndTime = s.EndTime
            }));
        }
    }
}