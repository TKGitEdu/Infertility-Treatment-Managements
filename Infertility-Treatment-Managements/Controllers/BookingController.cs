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

        // GET: api/Booking
        // Lấy tất cả danh sách booking - yêu cầu quyền Admin hoặc Doctor
        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]

        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetAllBookings()
        {
            try
            {
                // Lấy thông tin người dùng hiện tại
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Không thể xác định người dùng");
                }

                // Truy vấn danh sách booking
                IQueryable<Booking> query = _context.Bookings
                    .Include(b => b.Patient)
                    .Include(b => b.Service)
                    .Include(b => b.Doctor)
                    .Include(b => b.Slot)
                    .Include(b => b.Payment);

                // Nếu là Doctor, chỉ hiển thị các booking được chỉ định cho bác sĩ đó
                if (userRole == "Doctor")
                {
                    var doctor = await _context.Doctors
                        .FirstOrDefaultAsync(d => d.UserId == userId);

                    if (doctor == null)
                    {
                        return BadRequest("Không tìm thấy thông tin bác sĩ");
                    }

                    query = query.Where(b => b.DoctorId == doctor.DoctorId);
                }

                // Sắp xếp theo ngày đặt lịch, mới nhất lên đầu
                var bookings = await query.OrderByDescending(b => b.DateBooking).ToListAsync();

                // Chuyển đổi sang DTO và trả về
                return Ok(bookings.Select(b => b.ToDTO()));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy danh sách đặt lịch: {ex.Message}");
            }
        }


        // POST: api/Booking
        // Tạo mới lịch đặt khám - yêu cầu đăng nhập với vai trò Patient
        [HttpPost]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult<BookingDTO>> CreateBooking(BookingCreateDTO createDTO)
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

                // Kiểm tra tồn tại của service
                var service = await _context.Services.FindAsync(createDTO.ServiceId);
                if (service == null)
                {
                    return BadRequest($"Dịch vụ với ID {createDTO.ServiceId} không tồn tại");
                }

                // Kiểm tra tồn tại của doctor
                var doctor = await _context.Doctors.FindAsync(createDTO.DoctorId);
                if (doctor == null)
                {
                    return BadRequest($"Bác sĩ với ID {createDTO.DoctorId} không tồn tại");
                }

                // Kiểm tra tồn tại của slot
                var slot = await _context.Slots.FindAsync(createDTO.SlotId);
                if (slot == null)
                {
                    return BadRequest($"Khung giờ với ID {createDTO.SlotId} không tồn tại");
                }

                // Kiểm tra xem khung giờ đã được đặt chưa
                var isBooked = await _context.Bookings
                    .AnyAsync(b => b.DoctorId == createDTO.DoctorId &&
                             b.DateBooking.Date == createDTO.DateBooking.Date &&
                             b.SlotId == createDTO.SlotId);

                if (isBooked)
                {
                    return BadRequest("Khung giờ này đã được đặt. Vui lòng chọn khung giờ khác.");
                }

                // Tạo mới booking
                var booking = new Booking
                {
                    BookingId = Guid.NewGuid().ToString(),
                    PatientId = patient.PatientId,
                    ServiceId = createDTO.ServiceId,
                    DoctorId = createDTO.DoctorId,
                    SlotId = createDTO.SlotId,
                    DateBooking = createDTO.DateBooking,
                    Description = createDTO.Description ?? "",
                    Note = createDTO.Note ?? "",
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
                return StatusCode(500, $"Lỗi khi tạo lịch: {ex.Message}");
            }
        }

        // GET: api/Booking/mybookings
        // Lấy danh sách đặt lịch của bệnh nhân - yêu cầu đăng nhập
        [HttpGet("mybookings")]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetMyBookings()
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

                // Tải đầy đủ thông tin booking cùng với các quan hệ liên quan
                var bookings = await _context.Bookings
                    .Include(b => b.Patient)
                    .Include(b => b.Service)
                    .Include(b => b.Doctor)
                    .Include(b => b.Slot)
                    .Include(b => b.Payment)
                    .Include(b => b.Examination)
                    .Where(b => b.PatientId == patient.PatientId)
                    .OrderByDescending(b => b.DateBooking)
                    .ToListAsync();

                // Kiểm tra kết quả và trả về thông báo nếu không có booking nào
                if (!bookings.Any())
                {
                    return Ok(new List<BookingDTO>()); // Trả về danh sách rỗng
                }

                // Chuyển đổi sang DTO và trả về
                var bookingDTOs = bookings.Select(b => new BookingDTO
                {
                    BookingId = b.BookingId,
                    PatientId = b.PatientId,
                    ServiceId = b.ServiceId,
                    PaymentId = b.PaymentId,
                    DoctorId = b.DoctorId,
                    SlotId = b.SlotId,
                    DateBooking = b.DateBooking,
                    Description = b.Description,
                    Note = b.Note,
                    CreateAt = b.CreateAt,
                    Doctor = b.Doctor?.ToBasicDTO(),
                    Patient = b.Patient?.ToBasicDTO(),
                    Service = b.Service?.ToBasicDTO(),
                    Slot = b.Slot?.ToBasicDTO(),
                    Payment = b.Payment?.ToBasicDTO(),
                    Examination = b.Examination?.ToBasicDTO()
                }).ToList();

                return Ok(bookingDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy danh sách đặt lịch: {ex.Message}");
            }
        }

        // GET: api/Booking/{BookingId}
        // Lấy chi tiết đặt lịch theo ID - yêu cầu đăng nhập
        [HttpGet("{id}")]
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

        // PUT: api/Booking/{id}
        // Cập nhật thông tin đặt lịch - yêu cầu đăng nhập
        [HttpPut("{id}")]
        [Authorize(Roles = "Patient")]
        public async Task<ActionResult<BookingDTO>> UpdateBooking(string id, BookingUpdateDTO updateDTO)
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

                // Tìm booking cần cập nhật
                var booking = await _context.Bookings
                    .FirstOrDefaultAsync(b => b.BookingId == id && b.PatientId == patient.PatientId);

                if (booking == null)
                {
                    return NotFound($"Không tìm thấy lịch đặt với ID {id}");
                }

                // Kiểm tra nếu đã thanh toán thì không cho cập nhật
                if (booking.PaymentId != null)
                {
                    return BadRequest("Không thể cập nhật lịch đã thanh toán");
                }

                // Kiểm tra nếu lịch hẹn trong vòng 24 giờ thì không cho cập nhật
                if (booking.DateBooking <= DateTime.Now.AddHours(24))
                {
                    return BadRequest("Không thể cập nhật lịch hẹn trong vòng 24 giờ trước khi khám");
                }

                // Cập nhật thông tin booking
                if (!string.IsNullOrEmpty(updateDTO.Description))
                {
                    booking.Description = updateDTO.Description;
                }

                if (!string.IsNullOrEmpty(updateDTO.Note))
                {
                    booking.Note = updateDTO.Note;
                }

                // Cập nhật ngày hẹn nếu có
                if (updateDTO.DateBooking != null && updateDTO.DateBooking != default(DateTime))
                {
                    booking.DateBooking = updateDTO.DateBooking;
                }

                // Cập nhật các thông tin khác nếu cần
                if (!string.IsNullOrEmpty(updateDTO.ServiceId))
                {
                    var service = await _context.Services.FindAsync(updateDTO.ServiceId);
                    if (service == null)
                    {
                        return BadRequest($"Dịch vụ với ID {updateDTO.ServiceId} không tồn tại");
                    }
                    booking.ServiceId = updateDTO.ServiceId;
                }

                if (!string.IsNullOrEmpty(updateDTO.DoctorId))
                {
                    var doctor = await _context.Doctors.FindAsync(updateDTO.DoctorId);
                    if (doctor == null)
                    {
                        return BadRequest($"Bác sĩ với ID {updateDTO.DoctorId} không tồn tại");
                    }
                    booking.DoctorId = updateDTO.DoctorId;
                }

                if (!string.IsNullOrEmpty(updateDTO.SlotId))
                {
                    var slot = await _context.Slots.FindAsync(updateDTO.SlotId);
                    if (slot == null)
                    {
                        return BadRequest($"Khung giờ với ID {updateDTO.SlotId} không tồn tại");
                    }

                    // Kiểm tra xem khung giờ đã được đặt chưa (nếu thay đổi)
                    if (updateDTO.SlotId != booking.SlotId)
                    {
                        var isBooked = await _context.Bookings
                            .AnyAsync(b => b.DoctorId == booking.DoctorId &&
                                      b.DateBooking.Date == booking.DateBooking.Date &&
                                      b.SlotId == updateDTO.SlotId &&
                                      b.BookingId != id);

                        if (isBooked)
                        {
                            return BadRequest("Khung giờ này đã được đặt. Vui lòng chọn khung giờ khác.");
                        }
                    }

                    booking.SlotId = updateDTO.SlotId;
                }

                _context.Entry(booking).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                // Lấy booking đầy đủ thông tin
                var bookingFull = await _context.Bookings
                    .Include(b => b.Patient)
                    .Include(b => b.Service)
                    .Include(b => b.Doctor)
                    .Include(b => b.Slot)
                    .FirstOrDefaultAsync(b => b.BookingId == id);

                return Ok(bookingFull.ToDTO());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi cập nhật lịch: {ex.Message}");
            }
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

}