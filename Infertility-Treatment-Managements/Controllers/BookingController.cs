using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infertility_Treatment_Managements.Models;
using Infertility_Treatment_Managements.DTOs;
using Infertility_Treatment_Managements.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetBookings()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Service)
                .Include(b => b.Payment)
                .Include(b => b.Doctor)
                .Include(b => b.Slot)
                .Include(b => b.Examination)
                .ToListAsync();

            return bookings.Select(b => b.ToDTO()).ToList();
        }

        // GET: api/Booking/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDTO>> GetBooking(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Service)
                .Include(b => b.Payment)
                .Include(b => b.Doctor)
                .Include(b => b.Slot)
                .Include(b => b.Examination)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            return booking.ToDTO();
        }

        // GET: api/Booking/Patient/5
        [HttpGet("Patient/{patientId}")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetBookingsByPatient(int patientId)
        {
            var bookings = await _context.Bookings
                .Where(b => b.PatientId == patientId)
                .Include(b => b.Service)
                .Include(b => b.Payment)
                .Include(b => b.Doctor)
                .Include(b => b.Slot)
                .Include(b => b.Examination)
                .ToListAsync();

            return bookings.Select(b => b.ToDTO()).ToList();
        }

        // GET: api/Booking/Doctor/5
        [HttpGet("Doctor/{doctorId}")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetBookingsByDoctor(int doctorId)
        {
            var bookings = await _context.Bookings
                .Where(b => b.DoctorId == doctorId)
                .Include(b => b.Patient)
                .Include(b => b.Service)
                .Include(b => b.Payment)
                .Include(b => b.Slot)
                .Include(b => b.Examination)
                .ToListAsync();

            return bookings.Select(b => b.ToDTO()).ToList();
        }

        // POST: api/Booking
        [HttpPost]
        public async Task<ActionResult<BookingDTO>> CreateBooking(BookingCreateDTO bookingCreateDTO)
        {
            // I. KIỂM TRA CƠ BẢN CÁC THUỘC TÍNH

            // 1. KIỂM TRA PHIẾU ĐẶT LỊCH
            if (bookingCreateDTO == null)
            {
                return BadRequest("Booking data is required.");
            }

            // 2. KIỂM TRA ID CỦA CÁC THUỘC TÍNH
            if (bookingCreateDTO.PatientId <= 0 || bookingCreateDTO.ServiceId <= 0 ||
                bookingCreateDTO.DoctorId <= 0 || bookingCreateDTO.SlotId <= 0)
            {
                return BadRequest("PatientId, ServiceId, DoctorId, and SlotId are required and must be valid.");
            }

            // KIỂM TRA THỜI GIAN ĐẶT (01/01/0001 00:00:00)
            if (bookingCreateDTO.DateBooking == default)
            {
                return BadRequest("Booking date is required.");
            }

            // II. BUSINESS RULE

            // 1. KIỂM TRA THỜI GIAN HỢP LỆ (TRƯỚC THỜI GIAN ĐẶT)
            if (bookingCreateDTO.DateBooking <= DateTime.Now)
            {
                return BadRequest("Booking date must be in the future.");
            }

            // 2. KIỂM TRA Patient, Service, Doctor, Slot TỒN TẠI
            var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == bookingCreateDTO.PatientId);
            if (!patientExists)
            {
                return BadRequest("Invalid PatientId: Patient does not exist.");
            }

            var serviceExists = await _context.Services.AnyAsync(s => s.ServiceId == bookingCreateDTO.ServiceId);
            if (!serviceExists)
            {
                return BadRequest("Invalid ServiceId: Service does not exist.");
            }

            var doctorExists = await _context.Doctors.AnyAsync(d => d.DoctorId == bookingCreateDTO.DoctorId);
            if (!doctorExists)
            {
                return BadRequest("Invalid DoctorId: Doctor does not exist.");
            }

            var slotExists = await _context.Slots.AnyAsync(s => s.SlotId == bookingCreateDTO.SlotId);
            if (!slotExists)
            {
                return BadRequest("Invalid SlotId: Slot does not exist.");
            }

            // 3. KIỂM TRA Slot VÀ Doctor TRONG NGÀY ĐẶT CÓ TRỐNG KHÔNG
            var isSlotDoctorBooked = await _context.Bookings
                .AnyAsync(b => b.SlotId == bookingCreateDTO.SlotId &&
                               b.DoctorId == bookingCreateDTO.DoctorId &&
                               b.DateBooking.Date == bookingCreateDTO.DateBooking.Date);
            if (isSlotDoctorBooked)
            {
                return Conflict("The selected slot is already booked for this doctor on the specified date.");
            }

            // III. TẠO MỚI PHIẾU ĐẶT LỊCH
            var booking = bookingCreateDTO.ToEntity();
            booking.CreateAt = DateTime.Now;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            var createdBooking = await _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Service)
                .Include(b => b.Payment)
                .Include(b => b.Doctor)
                .Include(b => b.Slot)
                .FirstOrDefaultAsync(b => b.BookingId == booking.BookingId);

            if (createdBooking == null)
            {
                return StatusCode(500, "Failed to retrieve the created booking.");
            }

            return CreatedAtAction(nameof(GetBooking), new { id = booking.BookingId }, createdBooking.ToDTO());
        }

        // PUT: api/Booking/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(int id, BookingUpdateDTO bookingUpdateDTO)
        {
            if (id != bookingUpdateDTO.BookingId)
            {
                return BadRequest();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            bookingUpdateDTO.UpdateEntity(booking);
            _context.Entry(booking).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Booking/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}