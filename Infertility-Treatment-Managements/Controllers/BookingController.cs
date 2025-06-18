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
                .Include(b => b.Doctor)
                .Include(b => b.Patient)
                .Include(b => b.Service)
                .Include(b => b.Slot)
                .Include(b => b.Payment)
                .Include(b => b.Examination)
                .ToListAsync();

            if (bookings == null || !bookings.Any())
            {
                return NotFound("No bookings found");
            }

            var bookingDTOs = bookings.Select(b => new BookingDTO
            {
                BookingId = b.BookingId,
                PatientId = b.PatientId,
                ServiceId = b.ServiceId,
                PaymentId = b.Payment?.PaymentId,
                DoctorId = b.DoctorId,
                SlotId = b.SlotId,
                DateBooking = b.DateBooking,
                Description = b.Description,
                Note = b.Note,
                CreateAt = b.CreateAt,
                Doctor = b.Doctor != null ? new DoctorBasicDTO
                {
                    DoctorId = b.Doctor.DoctorId,
                    DoctorName = b.Doctor.DoctorName,
                    Specialization = b.Doctor.Specialization
                } : null,
                Patient = b.Patient != null ? new PatientBasicDTO
                {
                    PatientId = b.Patient.PatientId,
                    Name = b.Patient.Name,
                    Phone = b.Patient.Phone,
                    Email = b.Patient.Email
                } : null,
                Payment = b.Payment != null ? new PaymentBasicDTO
                {
                    PaymentId = b.Payment.PaymentId,
                    TotalAmount = b.Payment.TotalAmount,
                    Status = b.Payment.Status
                } : null,
                Service = b.Service != null ? new ServiceBasicDTO
                {
                    ServiceId = b.Service.ServiceId,
                    Name = b.Service.Name,
                    Price = b.Service.Price
                } : null,
                Slot = b.Slot != null ? new SlotBasicDTO
                {
                    SlotId = b.Slot.SlotId,
                    SlotName = b.Slot.SlotName,
                    StartTime = b.Slot.StartTime,
                    EndTime = b.Slot.EndTime
                } : null,
                Examination = b.Examination != null ? new ExaminationBasicDTO
                {
                    ExaminationId = b.Examination.ExaminationId,
                    ExaminationDate = b.Examination.ExaminationDate,
                    Status = b.Examination.Status,
                    Result = b.Examination.Result
                } : null
            }).ToList();

            return Ok(bookingDTOs);
        }

        // GET: api/Booking/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDTO>> GetBooking(string id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Doctor)
                .Include(b => b.Patient)
                .Include(b => b.Service)
                .Include(b => b.Slot)
                .Include(b => b.Payment)
                .Include(b => b.Examination)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound($"Booking with ID {id} not found");
            }

            var bookingDTO = new BookingDTO
            {
                BookingId = booking.BookingId,
                PatientId = booking.PatientId,
                ServiceId = booking.ServiceId,
                PaymentId = booking.Payment?.PaymentId,
                DoctorId = booking.DoctorId,
                SlotId = booking.SlotId,
                DateBooking = booking.DateBooking,
                Description = booking.Description,
                Note = booking.Note,
                CreateAt = booking.CreateAt,
                Doctor = booking.Doctor != null ? new DoctorBasicDTO
                {
                    DoctorId = booking.Doctor.DoctorId,
                    DoctorName = booking.Doctor.DoctorName,
                    Specialization = booking.Doctor.Specialization
                } : null,
                Patient = booking.Patient != null ? new PatientBasicDTO
                {
                    PatientId = booking.Patient.PatientId,
                    Name = booking.Patient.Name,
                    Phone = booking.Patient.Phone,
                    Email = booking.Patient.Email
                } : null,
                Payment = booking.Payment != null ? new PaymentBasicDTO
                {
                    PaymentId = booking.Payment.PaymentId,
                    TotalAmount = booking.Payment.TotalAmount,
                    Status = booking.Payment.Status
                } : null,
                Service = booking.Service != null ? new ServiceBasicDTO
                {
                    ServiceId = booking.Service.ServiceId,
                    Name = booking.Service.Name,
                    Price = booking.Service.Price
                } : null,
                Slot = booking.Slot != null ? new SlotBasicDTO
                {
                    SlotId = booking.Slot.SlotId,
                    SlotName = booking.Slot.SlotName,
                    StartTime = booking.Slot.StartTime,
                    EndTime = booking.Slot.EndTime
                } : null,
                Examination = booking.Examination != null ? new ExaminationBasicDTO
                {
                    ExaminationId = booking.Examination.ExaminationId,
                    ExaminationDate = booking.Examination.ExaminationDate,
                    Status = booking.Examination.Status,
                    Result = booking.Examination.Result
                } : null
            };

            return Ok(bookingDTO);
        }

        // POST: api/Booking
        [HttpPost]
        public async Task<ActionResult<BookingDTO>> CreateBooking(BookingCreateDTO bookingDTO)
        {
            // Validate foreign keys exist
            var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == bookingDTO.PatientId);
            var serviceExists = await _context.Services.AnyAsync(s => s.ServiceId == bookingDTO.ServiceId);
            var doctorExists = await _context.Doctors.AnyAsync(d => d.DoctorId == bookingDTO.DoctorId);
            var slotExists = await _context.Slots.AnyAsync(s => s.SlotId == bookingDTO.SlotId);

            if (!patientExists)
                return BadRequest($"Patient with ID {bookingDTO.PatientId} does not exist");
            if (!serviceExists)
                return BadRequest($"Service with ID {bookingDTO.ServiceId} does not exist");
            if (!doctorExists)
                return BadRequest($"Doctor with ID {bookingDTO.DoctorId} does not exist");
            if (!slotExists)
                return BadRequest($"Slot with ID {bookingDTO.SlotId} does not exist");

            // Check if slot is already booked for the doctor on the specified date
            var slotBooked = await _context.Bookings
                .AnyAsync(b => b.DoctorId == bookingDTO.DoctorId &&
                               b.SlotId == bookingDTO.SlotId &&
                               b.DateBooking.Date == bookingDTO.DateBooking.Date);

            if (slotBooked)
            {
                return BadRequest("The selected slot is already booked for this doctor on the specified date");
            }

            // Create a new booking with a unique ID
            var booking = new Booking
            {
                BookingId = "BK" + Guid.NewGuid().ToString().Substring(0, 8),
                PatientId = bookingDTO.PatientId,
                ServiceId = bookingDTO.ServiceId,
                DoctorId = bookingDTO.DoctorId,
                SlotId = bookingDTO.SlotId,
                DateBooking = bookingDTO.DateBooking,
                Description = bookingDTO.Description,
                Note = bookingDTO.Note,
                CreateAt = DateTime.Now
            };

            _context.Bookings.Add(booking);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"An error occurred while creating the booking: {ex.Message}");
            }

            // Return the created booking with its ID
            var createdBookingDTO = new BookingDTO
            {
                BookingId = booking.BookingId,
                PatientId = booking.PatientId,
                ServiceId = booking.ServiceId,
                DoctorId = booking.DoctorId,
                SlotId = booking.SlotId,
                DateBooking = booking.DateBooking,
                Description = booking.Description,
                Note = booking.Note,
                CreateAt = booking.CreateAt
            };

            return CreatedAtAction(nameof(GetBooking), new { id = booking.BookingId }, createdBookingDTO);
        }

        // PUT: api/Booking/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBooking(string id, BookingUpdateDTO bookingDTO)
        {
            if (id != bookingDTO.BookingId)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound($"Booking with ID {id} not found");
            }

            // Validate foreign keys exist
            var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == bookingDTO.PatientId);
            var serviceExists = await _context.Services.AnyAsync(s => s.ServiceId == bookingDTO.ServiceId);
            var doctorExists = await _context.Doctors.AnyAsync(d => d.DoctorId == bookingDTO.DoctorId);
            var slotExists = await _context.Slots.AnyAsync(s => s.SlotId == bookingDTO.SlotId);

            if (!patientExists)
                return BadRequest($"Patient with ID {bookingDTO.PatientId} does not exist");
            if (!serviceExists)
                return BadRequest($"Service with ID {bookingDTO.ServiceId} does not exist");
            if (!doctorExists)
                return BadRequest($"Doctor with ID {bookingDTO.DoctorId} does not exist");
            if (!slotExists)
                return BadRequest($"Slot with ID {bookingDTO.SlotId} does not exist");

            // Check if slot is already booked for the doctor on the specified date (excluding this booking)
            var slotBooked = await _context.Bookings
                .AnyAsync(b => b.BookingId != id &&
                               b.DoctorId == bookingDTO.DoctorId &&
                               b.SlotId == bookingDTO.SlotId &&
                               b.DateBooking.Date == bookingDTO.DateBooking.Date);

            if (slotBooked)
            {
                return BadRequest("The selected slot is already booked for this doctor on the specified date");
            }

            // Update booking properties
            booking.PatientId = bookingDTO.PatientId;
            booking.ServiceId = bookingDTO.ServiceId;
            booking.DoctorId = bookingDTO.DoctorId;
            booking.SlotId = bookingDTO.SlotId;
            booking.DateBooking = bookingDTO.DateBooking;
            booking.Description = bookingDTO.Description;
            booking.Note = bookingDTO.Note;

            _context.Entry(booking).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(id))
                {
                    return NotFound($"Booking with ID {id} not found");
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
            var booking = await _context.Bookings
                .Include(b => b.Payment)
                .Include(b => b.Examination)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound($"Booking with ID {id} not found");
            }

            // Check if related entities exist and handle them according to your business rules
            // For example, you might want to prevent deletion if there's a payment or examination
            if (booking.Payment != null)
            {
                return BadRequest("Cannot delete booking with an associated payment. Please delete the payment first.");
            }

            if (booking.Examination != null)
            {
                return BadRequest("Cannot delete booking with an associated examination. Please delete the examination first.");
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Booking/Patient/{patientId}
        [HttpGet("Patient/{patientId}")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetBookingsByPatient(string patientId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Doctor)
                .Include(b => b.Service)
                .Include(b => b.Slot)
                .Include(b => b.Payment)
                .Include(b => b.Examination)
                .Where(b => b.PatientId == patientId)
                .ToListAsync();

            if (bookings == null || !bookings.Any())
            {
                return NotFound($"No bookings found for patient with ID {patientId}");
            }

            var bookingDTOs = bookings.Select(b => new BookingDTO
            {
                BookingId = b.BookingId,
                PatientId = b.PatientId,
                ServiceId = b.ServiceId,
                PaymentId = b.Payment?.PaymentId,
                DoctorId = b.DoctorId,
                SlotId = b.SlotId,
                DateBooking = b.DateBooking,
                Description = b.Description,
                Note = b.Note,
                CreateAt = b.CreateAt,
                Doctor = b.Doctor != null ? new DoctorBasicDTO
                {
                    DoctorId = b.Doctor.DoctorId,
                    DoctorName = b.Doctor.DoctorName,
                    Specialization = b.Doctor.Specialization
                } : null,
                Service = b.Service != null ? new ServiceBasicDTO
                {
                    ServiceId = b.Service.ServiceId,
                    Name = b.Service.Name,
                    Price = b.Service.Price
                } : null,
                Slot = b.Slot != null ? new SlotBasicDTO
                {
                    SlotId = b.Slot.SlotId,
                    SlotName = b.Slot.SlotName,
                    StartTime = b.Slot.StartTime,
                    EndTime = b.Slot.EndTime
                } : null,
                Payment = b.Payment != null ? new PaymentBasicDTO
                {
                    PaymentId = b.Payment.PaymentId,
                    TotalAmount = b.Payment.TotalAmount,
                    Status = b.Payment.Status
                } : null,
                Examination = b.Examination != null ? new ExaminationBasicDTO
                {
                    ExaminationId = b.Examination.ExaminationId,
                    ExaminationDate = b.Examination.ExaminationDate,
                    Status = b.Examination.Status,
                    Result = b.Examination.Result
                } : null
            }).ToList();

            return Ok(bookingDTOs);
        }

        // GET: api/Booking/Doctor/{doctorId}
        [HttpGet("Doctor/{doctorId}")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetBookingsByDoctor(string doctorId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Service)
                .Include(b => b.Slot)
                .Include(b => b.Payment)
                .Include(b => b.Examination)
                .Where(b => b.DoctorId == doctorId)
                .ToListAsync();

            if (bookings == null || !bookings.Any())
            {
                return NotFound($"No bookings found for doctor with ID {doctorId}");
            }

            var bookingDTOs = bookings.Select(b => new BookingDTO
            {
                BookingId = b.BookingId,
                PatientId = b.PatientId,
                ServiceId = b.ServiceId,
                PaymentId = b.Payment?.PaymentId,
                DoctorId = b.DoctorId,
                SlotId = b.SlotId,
                DateBooking = b.DateBooking,
                Description = b.Description,
                Note = b.Note,
                CreateAt = b.CreateAt,
                Patient = b.Patient != null ? new PatientBasicDTO
                {
                    PatientId = b.Patient.PatientId,
                    Name = b.Patient.Name,
                    Phone = b.Patient.Phone,
                    Email = b.Patient.Email
                } : null,
                Service = b.Service != null ? new ServiceBasicDTO
                {
                    ServiceId = b.Service.ServiceId,
                    Name = b.Service.Name,
                    Price = b.Service.Price
                } : null,
                Slot = b.Slot != null ? new SlotBasicDTO
                {
                    SlotId = b.Slot.SlotId,
                    SlotName = b.Slot.SlotName,
                    StartTime = b.Slot.StartTime,
                    EndTime = b.Slot.EndTime
                } : null,
                Payment = b.Payment != null ? new PaymentBasicDTO
                {
                    PaymentId = b.Payment.PaymentId,
                    TotalAmount = b.Payment.TotalAmount,
                    Status = b.Payment.Status
                } : null,
                Examination = b.Examination != null ? new ExaminationBasicDTO
                {
                    ExaminationId = b.Examination.ExaminationId,
                    ExaminationDate = b.Examination.ExaminationDate,
                    Status = b.Examination.Status,
                    Result = b.Examination.Result
                } : null
            }).ToList();

            return Ok(bookingDTOs);
        }

        // GET: api/Booking/Date/{date}
        [HttpGet("Date/{date}")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetBookingsByDate(DateTime date)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Doctor)
                .Include(b => b.Patient)
                .Include(b => b.Service)
                .Include(b => b.Slot)
                .Include(b => b.Payment)
                .Include(b => b.Examination)
                .Where(b => b.DateBooking.Date == date.Date)
                .ToListAsync();

            if (bookings == null || !bookings.Any())
            {
                return NotFound($"No bookings found for date {date.ToShortDateString()}");
            }

            var bookingDTOs = bookings.Select(b => new BookingDTO
            {
                BookingId = b.BookingId,
                PatientId = b.PatientId,
                ServiceId = b.ServiceId,
                PaymentId = b.Payment?.PaymentId,
                DoctorId = b.DoctorId,
                SlotId = b.SlotId,
                DateBooking = b.DateBooking,
                Description = b.Description,
                Note = b.Note,
                CreateAt = b.CreateAt,
                Doctor = b.Doctor != null ? new DoctorBasicDTO
                {
                    DoctorId = b.Doctor.DoctorId,
                    DoctorName = b.Doctor.DoctorName,
                    Specialization = b.Doctor.Specialization
                } : null,
                Patient = b.Patient != null ? new PatientBasicDTO
                {
                    PatientId = b.Patient.PatientId,
                    Name = b.Patient.Name,
                    Phone = b.Patient.Phone,
                    Email = b.Patient.Email
                } : null,
                Service = b.Service != null ? new ServiceBasicDTO
                {
                    ServiceId = b.Service.ServiceId,
                    Name = b.Service.Name,
                    Price = b.Service.Price
                } : null,
                Slot = b.Slot != null ? new SlotBasicDTO
                {
                    SlotId = b.Slot.SlotId,
                    SlotName = b.Slot.SlotName,
                    StartTime = b.Slot.StartTime,
                    EndTime = b.Slot.EndTime
                } : null,
                Payment = b.Payment != null ? new PaymentBasicDTO
                {
                    PaymentId = b.Payment.PaymentId,
                    TotalAmount = b.Payment.TotalAmount,
                    Status = b.Payment.Status
                } : null,
                Examination = b.Examination != null ? new ExaminationBasicDTO
                {
                    ExaminationId = b.Examination.ExaminationId,
                    ExaminationDate = b.Examination.ExaminationDate,
                    Status = b.Examination.Status,
                    Result = b.Examination.Result
                } : null
            }).ToList();

            return Ok(bookingDTOs);
        }

        private bool BookingExists(string id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}