using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Infertility_Treatment_Managements.DTOs;
using Infertility_Treatment_Managements.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infertility_Treatment_Managements.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public PatientController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        // GET: api/Patient
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PatientDTO>>> GetPatients()
        {
            var patients = await _context.Patients
                .Include(p => p.User)
                .Include(p => p.Booking)
                .Include(p => p.PatientDetails)
                .ToListAsync();

            return patients.Select(p => p.ToDTO()).ToList();
        }

        // GET: api/Patient/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PatientDTO>> GetPatient(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .Include(p => p.Booking)
                .Include(p => p.PatientDetails)
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null)
            {
                return NotFound();
            }

            return patient.ToDTO();
        }

        // GET: api/Patient/User/5
        [HttpGet("User/{userId}")]
        public async Task<ActionResult<IEnumerable<PatientDTO>>> GetPatientsByUser(int userId)
        {
            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
            {
                return NotFound("User not found");
            }

            var patients = await _context.Patients
                .Where(p => p.UserId == userId)
                .Include(p => p.User)
                .Include(p => p.Booking)
                .Include(p => p.PatientDetails)
                .ToListAsync();

            return patients.Select(p => p.ToDTO()).ToList();
        }

        // POST: api/Patient
        [HttpPost]
        public async Task<ActionResult<PatientDTO>> CreatePatient(PatientCreateDTO patientCreateDTO)
        {
            // Validate UserId if provided
            if (patientCreateDTO.UserId.HasValue)
            {
                var userExists = await _context.Users.AnyAsync(u => u.UserId == patientCreateDTO.UserId);
                if (!userExists)
                {
                    return BadRequest("Invalid UserId: User does not exist");
                }
            }

            var patient = patientCreateDTO.ToEntity();
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            // Reload with related data for return
            var createdPatient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PatientId == patient.PatientId);

            return CreatedAtAction(nameof(GetPatient), new { id = createdPatient.PatientId }, createdPatient.ToDTO());
        }

        // PUT: api/Patient/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(int id, PatientUpdateDTO patientUpdateDTO)
        {
            if (id != patientUpdateDTO.PatientId)
            {
                return BadRequest("ID mismatch");
            }

            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            // Validate UserId if provided
            if (patientUpdateDTO.UserId.HasValue)
            {
                var userExists = await _context.Users.AnyAsync(u => u.UserId == patientUpdateDTO.UserId);
                if (!userExists)
                {
                    return BadRequest("Invalid UserId: User does not exist");
                }
            }

            patientUpdateDTO.UpdateEntity(patient);
            _context.Entry(patient).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PatientExists(id))
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

        // DELETE: api/Patient/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            // Check if this patient has associated bookings
            var hasBooking = await _context.Bookings.AnyAsync(b => b.PatientId == id);
            if (hasBooking)
            {
                return BadRequest("Cannot delete patient with associated bookings");
            }

            // Check if this patient has associated patient details
            var hasPatientDetails = await _context.PatientDetails.AnyAsync(pd => pd.PatientId == id);
            if (hasPatientDetails)
            {
                return BadRequest("Cannot delete patient with associated patient details");
            }

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> PatientExists(int id)
        {
            return await _context.Patients.AnyAsync(p => p.PatientId == id);
        }
    }
}