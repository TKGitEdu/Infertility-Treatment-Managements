using Microsoft.AspNetCore.Http;
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
    public class ExaminationController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public ExaminationController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        // GET: api/Examination
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ExaminationDTO>>> GetExaminations()
        {
            var examinations = await _context.Examinations
                .Include(e => e.Booking)
                    .ThenInclude(b => b.Patient)
                .Include(e => e.Booking)
                    .ThenInclude(b => b.Doctor)
                .ToListAsync();

            return examinations.Select(e => e.ToDTO()).ToList();
        }

        // GET: api/Examination/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ExaminationDTO>> GetExamination(string id)
        {
            var examination = await _context.Examinations
                .Include(e => e.Booking)
                    .ThenInclude(b => b.Patient)
                .Include(e => e.Booking)
                    .ThenInclude(b => b.Doctor)
                .FirstOrDefaultAsync(e => e.ExaminationId == id);

            if (examination == null)
            {
                return NotFound();
            }

            return examination.ToDTO();
        }

        // GET: api/Examination/Booking/5
        [HttpGet("Booking/{bookingId}")]
        public async Task<ActionResult<ExaminationDTO>> GetExaminationByBooking(string bookingId)
        {
            var bookingExists = await _context.Bookings.AnyAsync(b => b.BookingId == bookingId);
            if (!bookingExists)
            {
                return NotFound("Booking not found");
            }

            var examination = await _context.Examinations
                .Include(e => e.Booking)
                    .ThenInclude(b => b.Patient)
                .Include(e => e.Booking)
                    .ThenInclude(b => b.Doctor)
                .FirstOrDefaultAsync(e => e.BookingId == bookingId);

            if (examination == null)
            {
                return NotFound("Examination not found for this booking");
            }

            return examination.ToDTO();
        }

        // GET: api/Examination/Status/Completed
        [HttpGet("Status/{status}")]
        public async Task<ActionResult<IEnumerable<ExaminationDTO>>> GetExaminationsByStatus(string status)
        {
            var examinations = await _context.Examinations
                .Where(e => e.Status == status)
                .Include(e => e.Booking)
                    .ThenInclude(b => b.Patient)
                .Include(e => e.Booking)
                    .ThenInclude(b => b.Doctor)
                .ToListAsync();

            return examinations.Select(e => e.ToDTO()).ToList();
        }

        // POST: api/Examination
        [HttpPost]
        public async Task<ActionResult<ExaminationDTO>> CreateExamination(ExaminationCreateDTO examinationCreateDTO)
        {
            // Validate booking exists
            var booking = await _context.Bookings.FindAsync(examinationCreateDTO.BookingId);
            if (booking == null)
            {
                return BadRequest("Invalid BookingId: Booking does not exist");
            }

            // Check if examination already exists for this booking
            var examinationExists = await _context.Examinations.AnyAsync(e => e.BookingId == examinationCreateDTO.BookingId);
            if (examinationExists)
            {
                return BadRequest("An examination already exists for this booking");
            }

            var examination = examinationCreateDTO.ToEntity();
            _context.Examinations.Add(examination);
            await _context.SaveChangesAsync();

            // Reload with related data for return
            var createdExamination = await _context.Examinations
                .Include(e => e.Booking)
                    .ThenInclude(b => b.Patient)
                .Include(e => e.Booking)
                    .ThenInclude(b => b.Doctor)
                .FirstOrDefaultAsync(e => e.ExaminationId == examination.ExaminationId);

            return CreatedAtAction(nameof(GetExamination), new { id = createdExamination.ExaminationId }, createdExamination.ToDTO());
        }

        // PUT: api/Examination/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExamination(string id, ExaminationUpdateDTO examinationUpdateDTO)
        {
            if (id != examinationUpdateDTO.ExaminationId)
            {
                return BadRequest("ID mismatch");
            }

            var examination = await _context.Examinations.FindAsync(id);
            if (examination == null)
            {
                return NotFound();
            }

            // Validate booking exists
            var bookingExists = await _context.Bookings.AnyAsync(b => b.BookingId == examinationUpdateDTO.BookingId);
            if (!bookingExists)
            {
                return BadRequest("Invalid BookingId: Booking does not exist");
            }

            // Check if another examination exists for this booking (excluding this one)
            var duplicateExamination = await _context.Examinations
                .AnyAsync(e => e.ExaminationId != id && e.BookingId == examinationUpdateDTO.BookingId);
            if (duplicateExamination)
            {
                return BadRequest("Another examination already exists for this booking");
            }

            examinationUpdateDTO.UpdateEntity(examination);
            _context.Entry(examination).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ExaminationExists(id))
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

        // PATCH: api/Examination/5/UpdateStatus
        [HttpPatch("{id}/UpdateStatus")]
        public async Task<IActionResult> UpdateExaminationStatus(string id, [FromBody] string status)
        {
            var examination = await _context.Examinations.FindAsync(id);
            if (examination == null)
            {
                return NotFound();
            }

            examination.Status = status;
            _context.Entry(examination).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ExaminationExists(id))
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

        // DELETE: api/Examination/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExamination(string id)
        {
            var examination = await _context.Examinations.FindAsync(id);
            if (examination == null)
            {
                return NotFound();
            }

            _context.Examinations.Remove(examination);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> ExaminationExists(string id)
        {
            return await _context.Examinations.AnyAsync(e => e.ExaminationId == id);
        }
    }
}