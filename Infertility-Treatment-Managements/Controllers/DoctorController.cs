using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infertility_Treatment_Managements.Models;
using Infertility_Treatment_Managements.DTOs;
using Infertility_Treatment_Managements.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Infertility_Treatment_Managements.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;
        private const string DOCTOR_ROLE_NAME = "Doctor";

        public DoctorController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        // GET: api/Doctor
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DoctorDTO>>> GetDoctors()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Bookings)
                .ToListAsync();

            return doctors.Select(d => d.ToDTO()).ToList();
        }

        // GET: api/Doctor/ByUser/{userId}
        [HttpGet("ByUser/{userId}")]
        public async Task<ActionResult<DoctorDTO>> GetDoctorByUser(int userId)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Bookings)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                return NotFound("No doctor record found for this user");
            }

            return doctor.ToDTO();
        }

        // GET: api/Doctor/BySpecialization/{specialization}
        [HttpGet("BySpecialization/{specialization}")]
        public async Task<ActionResult<IEnumerable<DoctorDTO>>> GetDoctorsBySpecialization(string specialization)
        {
            var doctors = await _context.Doctors
                .Where(d => d.Specialization == specialization)
                .Include(d => d.User)
                .Include(d => d.Bookings)
                .ToListAsync();

            return doctors.Select(d => d.ToDTO()).ToList();
        }

        // GET: api/Doctor/GetRoleId
        [HttpGet("GetRoleId")]
        public async Task<ActionResult<int?>> GetDoctorRoleId()
        {
            var doctorRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == DOCTOR_ROLE_NAME);

            if (doctorRole == null)
            {
                return NotFound($"Role '{DOCTOR_ROLE_NAME}' not found in the system");
            }

            return Ok(doctorRole.RoleId);
        }

        // POST: api/Doctor/Create
        [HttpPost("Create")]
        public async Task<ActionResult<DoctorDTO>> CreateDoctor(DoctorCreateDTO doctorCreateDTO)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Get doctor role
                var doctorRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.RoleName == DOCTOR_ROLE_NAME);

                if (doctorRole == null)
                {
                    return BadRequest($"Doctor role not found. Please create a '{DOCTOR_ROLE_NAME}' role first.");
                }

                // 2. Handle user creation or validation
                User user;

                if (doctorCreateDTO.UserId.HasValue)
                {
                    // Using existing user
                    user = await _context.Users.FindAsync(doctorCreateDTO.UserId.Value);
                    if (user == null)
                    {
                        return BadRequest("Invalid UserId: User does not exist");
                    }

                    // Check if doctor with this user ID already exists
                    var doctorExists = await _context.Doctors.AnyAsync(d => d.UserId == doctorCreateDTO.UserId);
                    if (doctorExists)
                    {
                        return BadRequest("Doctor with this UserId already exists");
                    }

                    // Update user's role if needed
                    if (user.RoleId != doctorRole.RoleId)
                    {
                        user.RoleId = doctorRole.RoleId;
                        _context.Entry(user).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    // Check if required fields for user creation are provided
                    if (string.IsNullOrEmpty(doctorCreateDTO.DoctorName))
                    {
                        return BadRequest("DoctorName is required when creating a new user");
                    }
                    if (string.IsNullOrEmpty(doctorCreateDTO.Email))
                    {
                        return BadRequest("Email is required when creating a new user");
                    }
                    if (string.IsNullOrEmpty(doctorCreateDTO.Phone))
                    {
                        return BadRequest("Phone is required when creating a new user");
                    }

                    // Generate a username if not provided
                    string username = doctorCreateDTO.Username;
                    if (string.IsNullOrEmpty(username))
                    {
                        // Generate username from email or name
                        username = doctorCreateDTO.Email.Split('@')[0];

                        // Check if username already exists
                        bool usernameExists = await _context.Users.AnyAsync(u => u.Username == username);
                        if (usernameExists)
                        {
                            // Append a random number if username exists
                            Random random = new Random();
                            username = $"{username}{random.Next(1000, 9999)}";
                        }
                    }

                    // Generate a default password if not provided
                    string password = doctorCreateDTO.Password;
                    if (string.IsNullOrEmpty(password))
                    {
                        // Default password is "Doctor@" + first 3 letters of name + 4 random digits
                        Random random = new Random();
                        string namePrefix = doctorCreateDTO.DoctorName.Length >= 3
                            ? doctorCreateDTO.DoctorName.Substring(0, 3)
                            : doctorCreateDTO.DoctorName;
                        password = $"Doctor@{namePrefix}{random.Next(1000, 9999)}";
                    }

                    // Create a new user
                    user = new User
                    {
                        FullName = doctorCreateDTO.DoctorName,
                        Email = doctorCreateDTO.Email,
                        Phone = doctorCreateDTO.Phone,
                        Username = username,
                        Password = password, // In production, this should be hashed
                        RoleId = doctorRole.RoleId,
                        // Optional fields can be set if available
                        Address = doctorCreateDTO.Address,
                        Gender = doctorCreateDTO.Gender
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                // 3. Create doctor record
                var doctor = new Doctor
                {
                    UserId = user.UserId,
                    DoctorName = doctorCreateDTO.DoctorName ?? user.FullName,
                    Email = doctorCreateDTO.Email ?? user.Email,
                    Phone = doctorCreateDTO.Phone ?? user.Phone,
                    Specialization = doctorCreateDTO.Specialization ?? "General"
                };

                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                // 4. Load the complete doctor with related entities
                var createdDoctor = await _context.Doctors
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.DoctorId == doctor.DoctorId);

                await transaction.CommitAsync();

                // Return the created doctor
                var result = createdDoctor.ToDTO();

                // If we created a new user, include the generated credentials in the response
                if (!doctorCreateDTO.UserId.HasValue)
                {
                    // Add user credentials to the response
                    // You might want to create a custom response object for this
                    return Ok(new
                    {
                        Doctor = result,
                        UserCredentials = new
                        {
                            Username = user.Username,
                            Password = user.Password, // Note: In production, don't return passwords
                            UserId = user.UserId
                        }
                    });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // PUT: api/Doctor/Update
        [HttpPut("Update")]
        public async Task<IActionResult> UpdateDoctor(DoctorUpdateDTO doctorUpdateDTO)
        {
            var doctor = await _context.Doctors.FindAsync(doctorUpdateDTO.DoctorId);
            if (doctor == null)
            {
                return NotFound($"Doctor with ID {doctorUpdateDTO.DoctorId} not found");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Validate UserId if being changed
                if (doctorUpdateDTO.UserId.HasValue && doctorUpdateDTO.UserId != doctor.UserId)
                {
                    var userExists = await _context.Users.AnyAsync(u => u.UserId == doctorUpdateDTO.UserId);
                    if (!userExists)
                    {
                        return BadRequest("Invalid UserId: User does not exist");
                    }

                    // Check if another doctor already has this UserId
                    var existingDoctor = await _context.Doctors
                        .FirstOrDefaultAsync(d => d.UserId == doctorUpdateDTO.UserId && d.DoctorId != doctorUpdateDTO.DoctorId);
                    if (existingDoctor != null)
                    {
                        return BadRequest("Another doctor is already associated with this user");
                    }

                    // Get doctor role
                    var doctorRole = await _context.Roles
                        .FirstOrDefaultAsync(r => r.RoleName == DOCTOR_ROLE_NAME);

                    if (doctorRole == null)
                    {
                        return BadRequest($"Doctor role not found. Please create a '{DOCTOR_ROLE_NAME}' role first.");
                    }

                    // Update new user's role
                    var newUser = await _context.Users.FindAsync(doctorUpdateDTO.UserId.Value);
                    if (newUser.RoleId != doctorRole.RoleId)
                    {
                        newUser.RoleId = doctorRole.RoleId;
                        _context.Entry(newUser).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                    }
                }

                // Update doctor entity
                doctorUpdateDTO.UpdateEntity(doctor);
                _context.Entry(doctor).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                // Also update the associated user's basic information if available
                if (doctor.UserId.HasValue)
                {
                    var user = await _context.Users.FindAsync(doctor.UserId.Value);
                    if (user != null)
                    {
                        // Update user's matching fields
                        bool userModified = false;

                        if (!string.IsNullOrEmpty(doctorUpdateDTO.DoctorName) && user.FullName != doctorUpdateDTO.DoctorName)
                        {
                            user.FullName = doctorUpdateDTO.DoctorName;
                            userModified = true;
                        }

                        if (!string.IsNullOrEmpty(doctorUpdateDTO.Email) && user.Email != doctorUpdateDTO.Email)
                        {
                            user.Email = doctorUpdateDTO.Email;
                            userModified = true;
                        }

                        if (!string.IsNullOrEmpty(doctorUpdateDTO.Phone) && user.Phone != doctorUpdateDTO.Phone)
                        {
                            user.Phone = doctorUpdateDTO.Phone;
                            userModified = true;
                        }

                        if (userModified)
                        {
                            _context.Entry(user).State = EntityState.Modified;
                            await _context.SaveChangesAsync();
                        }
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

        // DELETE: api/Doctor/Delete/{userId}
        [HttpDelete("Delete/{userId}")]
        public async Task<IActionResult> DeleteDoctorByUserId(int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.UserId == userId);

                if (doctor == null)
                {
                    return NotFound($"No doctor found with UserId {userId}");
                }

                // Check if this doctor has associated bookings
                var hasBookings = await _context.Bookings.AnyAsync(b => b.DoctorId == doctor.DoctorId);
                if (hasBookings)
                {
                    return BadRequest("Cannot delete doctor with associated bookings");
                }

                // Check if doctor has treatment plans
                var hasTreatmentPlans = await _context.TreatmentPlans.AnyAsync(tp => tp.DoctorId == doctor.DoctorId);
                if (hasTreatmentPlans)
                {
                    return BadRequest("Cannot delete doctor with associated treatment plans");
                }

                // Remove only the doctor record, not the user
                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();

                // Set user role to default if needed
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    // You may want to reset the user's role to a default role
                    // This is optional and depends on your application logic
                    // user.RoleId = defaultRoleId;
                    // _context.Entry(user).State = EntityState.Modified;
                    // await _context.SaveChangesAsync();
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

        // POST: api/Doctor/UpdateSpecialization
        [HttpPost("UpdateSpecialization")]
        public async Task<IActionResult> UpdateSpecialization([FromBody] DoctorSpecializationDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Specialization))
            {
                return BadRequest("Specialization cannot be empty");
            }

            var doctor = await _context.Doctors.FindAsync(dto.DoctorId);
            if (doctor == null)
            {
                return NotFound($"Doctor with ID {dto.DoctorId} not found");
            }

            doctor.Specialization = dto.Specialization;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // Additional DTOs for the controller
    public class DoctorSpecializationDTO
    {
        public int DoctorId { get; set; }
        public string Specialization { get; set; }
    }
}