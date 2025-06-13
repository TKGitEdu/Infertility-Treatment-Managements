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
        public async Task<ActionResult<BookingDTO>> GetBooking(string id)
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
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetBookingsByPatient(string patientId)
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
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetBookingsByDoctor(string doctorId)
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
            var booking = bookingCreateDTO.ToEntity();
            booking.CreateAt = DateTime.Now;
            
            // Generate a unique ID (GUID-based)
            booking.BookingId = Guid.NewGuid().ToString();

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Reload the booking with its relations for returning
            var createdBooking = await _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Service)
                .Include(b => b.Payment)
                .Include(b => b.Doctor)
                .Include(b => b.Slot)
                .FirstOrDefaultAsync(b => b.BookingId == booking.BookingId);

            return CreatedAtAction(nameof(GetBooking), new { id = booking.BookingId }, createdBooking.ToDTO());
        }

        // PUT: api/Booking/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(string id, BookingUpdateDTO bookingUpdateDTO)
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
        public async Task<IActionResult> DeleteBooking(string id)
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

        private bool BookingExists(string id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}