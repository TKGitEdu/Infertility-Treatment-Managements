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
    public class DoctorController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public DoctorController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        // GET: api/Doctor
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DoctorDTO>>> GetDoctors()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)  // Change from DoctorNavigation to User
                .Include(d => d.Bookings)
                .ToListAsync();

            return doctors.Select(d => d.ToDTO()).ToList();
        }

        // GET: api/Doctor/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DoctorDTO>> GetDoctor(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)  // Change from DoctorNavigation to User
                .Include(d => d.Bookings)
                .FirstOrDefaultAsync(d => d.DoctorId == id);

            if (doctor == null)
            {
                return NotFound();
            }

            return doctor.ToDTO();
        }

        // GET: api/Doctor/User/5
        [HttpGet("User/{userId}")]
        public async Task<ActionResult<IEnumerable<DoctorDTO>>> GetDoctorsByUser(int userId)
        {
            var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
            if (!userExists)
            {
                return NotFound("User not found");
            }

            var doctors = await _context.Doctors
                .Where(d => d.UserId == userId)
                .Include(d => d.User)  // Change from DoctorNavigation to User
                .Include(d => d.Bookings)
                .ToListAsync();

            return doctors.Select(d => d.ToDTO()).ToList();
        }

        // GET: api/Doctor/Specialization/{specialization}
        [HttpGet("Specialization/{specialization}")]
        public async Task<ActionResult<IEnumerable<DoctorDTO>>> GetDoctorsBySpecialization(string specialization)
        {
            var doctors = await _context.Doctors
                .Where(d => d.Specialization == specialization)
                .Include(d => d.User)  // Change from DoctorNavigation to User
                .Include(d => d.Bookings)
                .ToListAsync();

            return doctors.Select(d => d.ToDTO()).ToList();
        }

        // POST: api/Doctor
        [HttpPost]
        public async Task<ActionResult<DoctorDTO>> CreateDoctor(DoctorCreateDTO doctorCreateDTO)
        {
            // Validate UserId if provided
            if (doctorCreateDTO.UserId.HasValue)
            {
                var userExists = await _context.Users.AnyAsync(u => u.UserId == doctorCreateDTO.UserId);
                if (!userExists)
                {
                    return BadRequest("Invalid UserId: User does not exist");
                }
                
                // Check if doctor with this user ID already exists
                var doctorExists = await _context.Doctors.AnyAsync(d => d.UserId == doctorCreateDTO.UserId);
                if (doctorExists)
                {
                    return BadRequest("Doctor with this UserId already exists");
                }
            }

            var doctor = doctorCreateDTO.ToEntity();
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            // Reload with related data for return
            var createdDoctor = await _context.Doctors
                .Include(d => d.User)  // Change from DoctorNavigation to User
                .FirstOrDefaultAsync(d => d.DoctorId == doctor.DoctorId);

            return CreatedAtAction(nameof(GetDoctor), new { id = createdDoctor.DoctorId }, createdDoctor.ToDTO());
        }

        // PUT: api/Doctor/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDoctor(int id, DoctorUpdateDTO doctorUpdateDTO)
        {
            if (id != doctorUpdateDTO.DoctorId)
            {
                return BadRequest("ID mismatch");
            }

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }

            // Validate UserId if provided
            if (doctorUpdateDTO.UserId.HasValue)
            {
                var userExists = await _context.Users.AnyAsync(u => u.UserId == doctorUpdateDTO.UserId);
                if (!userExists)
                {
                    return BadRequest("Invalid UserId: User does not exist");
                }
                
                // Check if another doctor already has this UserId
                var existingDoctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.UserId == doctorUpdateDTO.UserId && d.DoctorId != id);
                if (existingDoctor != null)
                {
                    return BadRequest("Another doctor is already associated with this user");
                }
            }

            doctorUpdateDTO.UpdateEntity(doctor);
            _context.Entry(doctor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await DoctorExists(id))
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

        // DELETE: api/Doctor/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var doctor = await _context.Doctors.FindAsync(id);
                    if (doctor == null)
                    {
                        return NotFound();
                    }

                    // Check if this doctor has associated bookings
                    var hasBookings = await _context.Bookings.AnyAsync(b => b.DoctorId == id);
                    if (hasBookings)
                    {
                        return BadRequest("Cannot delete doctor with associated bookings");
                    }

                    // Get the associated user ID
                    int? userId = doctor.UserId;

                    // Remove the doctor
                    _context.Doctors.Remove(doctor);
                    await _context.SaveChangesAsync();

                    // If there's an associated user, delete them too
                    if (userId.HasValue)
                    {
                        var user = await _context.Users.FindAsync(userId.Value);
                        if (user != null)
                        {
                            _context.Users.Remove(user);
                            await _context.SaveChangesAsync();
                        }
                    }

                    await transaction.CommitAsync();
                    return NoContent();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }

        private async Task<bool> DoctorExists(int id)
        {
            return await _context.Doctors.AnyAsync(d => d.DoctorId == id);
        }
    }
}