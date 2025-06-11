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
    public class PatientController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;
        private const string PATIENT_ROLE_NAME = "Patient";

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
                .Include(p => p.BookingFk)
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
                .Include(p => p.BookingFk)
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
                .Include(p => p.BookingFk)
                .Include(p => p.PatientDetails)
                .ToListAsync();

            return patients.Select(p => p.ToDTO()).ToList();
        }

        // GET: api/Patient/GetRoleId (renamed to match DoctorController naming)
        [HttpGet("GetRoleId")]
        public async Task<ActionResult<int?>> GetPatientRoleId()
        {
            var patientRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == PATIENT_ROLE_NAME);

            if (patientRole == null)
            {
                return NotFound($"Role '{PATIENT_ROLE_NAME}' not found in the system");
            }

            return Ok(patientRole.RoleId);
        }

        // POST: api/Patient/Create (new endpoint similar to DoctorController)
        [HttpPost("Create")]
        public async Task<ActionResult<PatientDTO>> CreatePatient(PatientCreateDTO patientCreateDTO)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Get patient role
                var patientRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.RoleName == PATIENT_ROLE_NAME);

                if (patientRole == null)
                {
                    return BadRequest($"Patient role not found. Please create a '{PATIENT_ROLE_NAME}' role first.");
                }

                // 2. Handle user creation or validation
                User user;

                if (patientCreateDTO.UserId.HasValue)
                {
                    // Using existing user
                    user = await _context.Users.FindAsync(patientCreateDTO.UserId.Value);
                    if (user == null)
                    {
                        return BadRequest("Invalid UserId: User does not exist");
                    }

                    // Check if patient with this user ID already exists
                    var patientExists = await _context.Patients.AnyAsync(p => p.UserId == patientCreateDTO.UserId);
                    if (patientExists)
                    {
                        return BadRequest("Patient with this UserId already exists");
                    }

                    // Update user's role if needed
                    if (user.RoleId != patientRole.RoleId)
                    {
                        user.RoleId = patientRole.RoleId;
                        _context.Entry(user).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    // Check if required fields for user creation are provided
                    if (string.IsNullOrEmpty(patientCreateDTO.Name))
                    {
                        return BadRequest("Name is required when creating a new user");
                    }
                    if (string.IsNullOrEmpty(patientCreateDTO.Email))
                    {
                        return BadRequest("Email is required when creating a new user");
                    }
                    if (string.IsNullOrEmpty(patientCreateDTO.Phone))
                    {
                        return BadRequest("Phone is required when creating a new user");
                    }

                    // Generate a username (email prefix)
                    string username = patientCreateDTO.Email.Split('@')[0];

                    // Check if username already exists
                    bool usernameExists = await _context.Users.AnyAsync(u => u.Username == username);
                    if (usernameExists)
                    {
                        // Append a random number if username exists
                        Random random = new Random();
                        username = $"{username}{random.Next(1000, 9999)}";
                    }

                    // Generate a default password
                    Random passwordRandom = new Random();
                    string namePrefix = patientCreateDTO.Name.Length >= 3
                        ? patientCreateDTO.Name.Substring(0, 3)
                        : patientCreateDTO.Name;
                    string password = $"Patient@{namePrefix}{passwordRandom.Next(1000, 9999)}";

                    // Create a new user
                    user = new User
                    {
                        FullName = patientCreateDTO.Name,
                        Email = patientCreateDTO.Email,
                        Phone = patientCreateDTO.Phone,
                        Username = username,
                        Password = password, // In production, this should be hashed
                        RoleId = patientRole.RoleId,
                        Address = patientCreateDTO.Address,
                        Gender = patientCreateDTO.Gender,
                        DateOfBirth = patientCreateDTO.DateOfBirth?.ToDateTime(new TimeOnly(0, 0))
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }

                // 3. Create patient record
                var patient = new Patient
                {
                    UserId = user.UserId,
                    Name = patientCreateDTO.Name ?? user.FullName,
                    Email = patientCreateDTO.Email ?? user.Email,
                    Phone = patientCreateDTO.Phone ?? user.Phone,
                    Address = patientCreateDTO.Address ?? user.Address,
                    Gender = patientCreateDTO.Gender ?? user.Gender,
                    DateOfBirth = patientCreateDTO.DateOfBirth?.ToDateTime(new TimeOnly(0, 0)) ?? user.DateOfBirth,
                    BloodType = patientCreateDTO.BloodType,
                    EmergencyPhoneNumber = patientCreateDTO.EmergencyPhoneNumber
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                // 4. Load the complete patient with related entities
                var createdPatient = await _context.Patients
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.PatientId == patient.PatientId);

                await transaction.CommitAsync();

                // Return the created patient
                var result = createdPatient.ToDTO();

                // If we created a new user, include the generated credentials in the response
                if (!patientCreateDTO.UserId.HasValue)
                {
                    // Add user credentials to the response
                    return Ok(new
                    {
                        Patient = result,
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

        // PUT: api/Patient/Update (renamed to match DoctorController naming)
        [HttpPut("Update")]
        public async Task<IActionResult> UpdatePatient(PatientUpdateDTO patientUpdateDTO)
        {
            var patient = await _context.Patients.FindAsync(patientUpdateDTO.PatientId);
            if (patient == null)
            {
                return NotFound($"Patient with ID {patientUpdateDTO.PatientId} not found");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Validate UserId if being changed
                if (patientUpdateDTO.UserId.HasValue && patientUpdateDTO.UserId != patient.UserId)
                {
                    var userExists = await _context.Users.AnyAsync(u => u.UserId == patientUpdateDTO.UserId);
                    if (!userExists)
                    {
                        return BadRequest("Invalid UserId: User does not exist");
                    }

                    // Check if another patient already has this UserId
                    var existingPatient = await _context.Patients
                        .FirstOrDefaultAsync(p => p.UserId == patientUpdateDTO.UserId && p.PatientId != patientUpdateDTO.PatientId);
                    if (existingPatient != null)
                    {
                        return BadRequest("Another patient is already associated with this user");
                    }

                    // Get patient role
                    var patientRole = await _context.Roles
                        .FirstOrDefaultAsync(r => r.RoleName == PATIENT_ROLE_NAME);

                    if (patientRole == null)
                    {
                        return BadRequest($"Patient role not found. Please create a '{PATIENT_ROLE_NAME}' role first.");
                    }

                    // Update new user's role
                    var newUser = await _context.Users.FindAsync(patientUpdateDTO.UserId.Value);
                    if (newUser.RoleId != patientRole.RoleId)
                    {
                        newUser.RoleId = patientRole.RoleId;
                        _context.Entry(newUser).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                    }
                }

                // Update patient entity
                patientUpdateDTO.UpdateEntity(patient);
                _context.Entry(patient).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                // Also update the associated user's basic information if available
                if (patient.UserId.HasValue)
                {
                    var user = await _context.Users.FindAsync(patient.UserId.Value);
                    if (user != null)
                    {
                        // Update user's matching fields
                        bool userModified = false;

                        if (!string.IsNullOrEmpty(patientUpdateDTO.Name) && user.FullName != patientUpdateDTO.Name)
                        {
                            user.FullName = patientUpdateDTO.Name;
                            userModified = true;
                        }

                        if (!string.IsNullOrEmpty(patientUpdateDTO.Email) && user.Email != patientUpdateDTO.Email)
                        {
                            user.Email = patientUpdateDTO.Email;
                            userModified = true;
                        }

                        if (!string.IsNullOrEmpty(patientUpdateDTO.Phone) && user.Phone != patientUpdateDTO.Phone)
                        {
                            user.Phone = patientUpdateDTO.Phone;
                            userModified = true;
                        }

                        if (!string.IsNullOrEmpty(patientUpdateDTO.Address) && user.Address != patientUpdateDTO.Address)
                        {
                            user.Address = patientUpdateDTO.Address;
                            userModified = true;
                        }

                        if (!string.IsNullOrEmpty(patientUpdateDTO.Gender) && user.Gender != patientUpdateDTO.Gender)
                        {
                            user.Gender = patientUpdateDTO.Gender;
                            userModified = true;
                        }

                        if (patientUpdateDTO.DateOfBirth.HasValue)
                        {
                            var dateTime = patientUpdateDTO.DateOfBirth.Value.ToDateTime(new TimeOnly(0, 0));
                            if (user.DateOfBirth != dateTime)
                            {
                                user.DateOfBirth = dateTime;
                                userModified = true;
                            }
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

        // DELETE: api/Patient/Delete/{userId} (renamed to match DoctorController naming)
        [HttpDelete("Delete/{userId}")]
        public async Task<IActionResult> DeletePatientByUserId(int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (patient == null)
                {
                    return NotFound($"No patient found with UserId {userId}");
                }

                // Check if this patient has associated bookings
                var hasBooking = await _context.Bookings.AnyAsync(b => b.PatientId == patient.PatientId);
                if (hasBooking)
                {
                    return BadRequest("Cannot delete patient with associated bookings");
                }

                // Check if this patient has associated patient details
                var hasPatientDetails = await _context.PatientDetails.AnyAsync(pd => pd.PatientId == patient.PatientId);
                if (hasPatientDetails)
                {
                    return BadRequest("Cannot delete patient with associated patient details");
                }

                // Remove only the patient record, not the user
                _context.Patients.Remove(patient);
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

        // Keep the old endpoints for backward compatibility, but mark as obsolete

        // POST: api/Patient
        [HttpPost]
        [Obsolete("Use POST /api/Patient/Create instead. This endpoint will be removed in a future version.")]
        public async Task<ActionResult<PatientDTO>> CreatePatientLegacy(PatientCreateDTO patientCreateDTO)
        {
            // Find Patient role ID if we're creating a user too
            int? patientRoleId = null;
            if (!patientCreateDTO.UserId.HasValue)
            {
                var roles = await _context.Roles.ToListAsync();
                foreach (var role in roles)
                {
                    if (role.RoleName == PATIENT_ROLE_NAME)
                    {
                        patientRoleId = role.RoleId;
                        break; // Stop the loop when found
                    }
                }

                if (!patientRoleId.HasValue)
                {
                    return BadRequest($"Patient role not found. Please create a '{PATIENT_ROLE_NAME}' role first.");
                }
            }

            // Validate UserId if provided
            if (patientCreateDTO.UserId.HasValue)
            {
                var userExists = await _context.Users.AnyAsync(u => u.UserId == patientCreateDTO.UserId);
                if (!userExists)
                {
                    return BadRequest("Invalid UserId: User does not exist");
                }

                // Check if patient with this user ID already exists
                var patientExists = await _context.Patients.AnyAsync(p => p.UserId == patientCreateDTO.UserId);
                if (patientExists)
                {
                    return BadRequest("Patient with this UserId already exists");
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
        [Obsolete("Use PUT /api/Patient/Update instead. This endpoint will be removed in a future version.")]
        public async Task<IActionResult> UpdatePatientLegacy(int id, PatientUpdateDTO patientUpdateDTO)
        {
            if (id != patientUpdateDTO.PatientId)
            {
                return BadRequest("ID mismatch");
            }

            return await UpdatePatient(patientUpdateDTO);
        }

        // DELETE: api/Patient/5
        [HttpDelete("{id}")]
        [Obsolete("Use DELETE /api/Patient/Delete/{userId} instead. This endpoint will be removed in a future version.")]
        public async Task<IActionResult> DeletePatientLegacy(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
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

                // Get the associated user ID
                int? userId = patient.UserId;

                // Remove the patient
                _context.Patients.Remove(patient);
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

        private async Task<bool> PatientExists(int id)
        {
            return await _context.Patients.AnyAsync(p => p.PatientId == id);
        }
    }
}