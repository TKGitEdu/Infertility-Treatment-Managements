using Infertility_Treatment_Managements.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infertility_Treatment_Managements.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infertility_Treatment_Managements.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;
        private const string PATIENT_ROLE_NAME = "Patient";
        private const string DOCTOR_ROLE_NAME = "Doctor";

        public UserController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .ToListAsync();

            var userDTOs = users.Select(u => new UserDTO
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                Username = u.Username,
                RoleId = u.RoleId,
                Address = u.Address,
                Gender = u.Gender,
                DateOfBirth = u.DateOfBirth,
                Role = u.Role != null ? new RoleDTO
                {
                    RoleId = u.Role.RoleId,
                    RoleName = u.Role.RoleName
                } : null
            }).ToList();

            return userDTOs;
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(string id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null) return NotFound();

            var userDTO = new UserDTO
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                Username = user.Username,
                RoleId = user.RoleId,
                Address = user.Address,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                Role = user.Role != null ? new RoleDTO
                {
                    RoleId = user.Role.RoleId,
                    RoleName = user.Role.RoleName
                } : null
            };

            return userDTO;
        }

        // POST: api/User
        [HttpPost]
        public async Task<ActionResult<UserDTO>> PostUser(UserCreateDTO userCreateDTO)
        {
            // Start a transaction to ensure data consistency
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Hash the password
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(userCreateDTO.Password);

                    // Create a new User entity from the DTO
                    var user = new User
                    {
                        FullName = userCreateDTO.FullName,
                        Email = userCreateDTO.Email,
                        Phone = userCreateDTO.Phone,
                        Username = userCreateDTO.Username,
                        Password = hashedPassword, // Use the hashed password
                        RoleId = userCreateDTO.RoleId,
                        Address = userCreateDTO.Address,
                        Gender = userCreateDTO.Gender,
                        DateOfBirth = userCreateDTO.DateOfBirth
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // Check user's role
                    if (!string.IsNullOrEmpty(userCreateDTO.RoleId))
                    {
                        var role = await _context.Roles.FindAsync(userCreateDTO.RoleId);

                        // If the user has the Patient role, create a Patient record
                        if (role != null && role.RoleName.ToLower() == PATIENT_ROLE_NAME.ToLower())
                        {
                            // Create a new Patient record
                            var patient = new Patient
                            {
                                UserId = user.UserId,
                                Name = user.FullName,
                                Email = user.Email,
                                Phone = user.Phone,
                                Address = user.Address,
                                Gender = user.Gender,
                                DateOfBirth = user.DateOfBirth
                            };

                            _context.Patients.Add(patient);
                            await _context.SaveChangesAsync();
                        }
                        // If the user has the Doctor role, create a Doctor record
                        else if (role != null && role.RoleName.ToLower() == DOCTOR_ROLE_NAME.ToLower())
                        {
                            // Create a new Doctor record
                            var doctor = new Doctor
                            {
                                UserId = user.UserId,
                                DoctorName = user.FullName,
                                Email = user.Email,
                                Phone = user.Phone,
                                Specialization = userCreateDTO.Specialization ?? "General" // Default specialization
                            };

                            _context.Doctors.Add(doctor);
                            await _context.SaveChangesAsync();
                        }
                    }

                    await transaction.CommitAsync();

                    // Return user data without patient information
                    var userDTO = new UserDTO
                    {
                        UserId = user.UserId,
                        FullName = user.FullName,
                        Email = user.Email,
                        Phone = user.Phone,
                        Username = user.Username,
                        RoleId = user.RoleId,
                        Address = user.Address,
                        Gender = user.Gender,
                        DateOfBirth = user.DateOfBirth
                    };

                    return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, userDTO);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }

        // PUT: api/User/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, UserUpdateDTO userUpdateDTO)
        {
            if (id != userUpdateDTO.UserId) return BadRequest();

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var user = await _context.Users
                        .Include(u => u.Role)
                        .FirstOrDefaultAsync(u => u.UserId == id);

                    if (user == null) return NotFound();

                    // Check if the role is changing
                    string currentRoleName = user.Role?.RoleName?.ToLower() ?? "";
                    bool wasPatient = currentRoleName == PATIENT_ROLE_NAME.ToLower();
                    bool wasDoctor = currentRoleName == DOCTOR_ROLE_NAME.ToLower();

                    // Update user properties
                    user.FullName = userUpdateDTO.FullName;
                    user.Email = userUpdateDTO.Email;
                    user.Phone = userUpdateDTO.Phone;
                    user.Username = userUpdateDTO.Username;
                    user.RoleId = userUpdateDTO.RoleId;
                    user.Address = userUpdateDTO.Address;
                    user.Gender = userUpdateDTO.Gender;
                    user.DateOfBirth = userUpdateDTO.DateOfBirth;

                    _context.Entry(user).State = EntityState.Modified;
                    await _context.SaveChangesAsync();

                    // Check user's new role
                    string newRoleName = "";
                    if (!string.IsNullOrEmpty(userUpdateDTO.RoleId))
                    {
                        var newRole = await _context.Roles.FindAsync(userUpdateDTO.RoleId);
                        newRoleName = newRole?.RoleName?.ToLower() ?? "";
                    }

                    bool isPatientNow = newRoleName == PATIENT_ROLE_NAME.ToLower();
                    bool isDoctorNow = newRoleName == DOCTOR_ROLE_NAME.ToLower();

                    // Handle Patient role changes
                    if (isPatientNow && !wasPatient)
                    {
                        // User is becoming a Patient
                        var existingPatient = await _context.Patients
                            .FirstOrDefaultAsync(p => p.UserId == id);

                        if (existingPatient == null)
                        {
                            // Create a new Patient record
                            var patient = new Patient
                            {
                                UserId = user.UserId,
                                Name = user.FullName,
                                Email = user.Email,
                                Phone = user.Phone,
                                Address = user.Address,
                                Gender = user.Gender,
                                DateOfBirth = user.DateOfBirth,
                                BloodType = userUpdateDTO.BloodType,
                                EmergencyPhoneNumber = userUpdateDTO.EmergencyPhoneNumber
                            };

                            _context.Patients.Add(patient);
                            await _context.SaveChangesAsync();
                        }
                    }
                    else if (isPatientNow && wasPatient)
                    {
                        // User is still a Patient, update their Patient record
                        var existingPatient = await _context.Patients
                            .FirstOrDefaultAsync(p => p.UserId == id);

                        if (existingPatient != null)
                        {
                            existingPatient.Name = user.FullName;
                            existingPatient.Email = user.Email;
                            existingPatient.Phone = user.Phone;
                            existingPatient.Address = user.Address;
                            existingPatient.Gender = user.Gender;
                            existingPatient.DateOfBirth = user.DateOfBirth;

                            if (!string.IsNullOrEmpty(userUpdateDTO.BloodType))
                                existingPatient.BloodType = userUpdateDTO.BloodType;

                            if (!string.IsNullOrEmpty(userUpdateDTO.EmergencyPhoneNumber))
                                existingPatient.EmergencyPhoneNumber = userUpdateDTO.EmergencyPhoneNumber;

                            _context.Entry(existingPatient).State = EntityState.Modified;
                            await _context.SaveChangesAsync();
                        }
                    }

                    // Handle Doctor role changes
                    if (isDoctorNow && !wasDoctor)
                    {
                        // User is becoming a Doctor
                        var existingDoctor = await _context.Doctors
                            .FirstOrDefaultAsync(d => d.UserId == id);

                        if (existingDoctor == null)
                        {
                            // Create a new Doctor record
                            var doctor = new Doctor
                            {
                                UserId = user.UserId,
                                DoctorName = user.FullName,
                                Email = user.Email,
                                Phone = user.Phone,
                                Specialization = userUpdateDTO.Specialization ?? "General" // Default specialization
                            };

                            _context.Doctors.Add(doctor);
                            await _context.SaveChangesAsync();
                        }
                    }
                    else if (isDoctorNow && wasDoctor)
                    {
                        // User is still a Doctor, update their Doctor record
                        var existingDoctor = await _context.Doctors
                            .FirstOrDefaultAsync(d => d.UserId == id);

                        if (existingDoctor != null)
                        {
                            existingDoctor.DoctorName = user.FullName;
                            existingDoctor.Email = user.Email;
                            existingDoctor.Phone = user.Phone;

                            if (!string.IsNullOrEmpty(userUpdateDTO.Specialization))
                                existingDoctor.Specialization = userUpdateDTO.Specialization;

                            _context.Entry(existingDoctor).State = EntityState.Modified;
                            await _context.SaveChangesAsync();
                        }
                    }

                    await transaction.CommitAsync();
                    return NoContent();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await UserExistsAsync(id)) return NotFound();
                    await transaction.RollbackAsync();
                    throw;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Check if user exists
                    var user = await _context.Users
                        .Include(u => u.Role)
                        .FirstOrDefaultAsync(u => u.UserId == id);

                    if (user == null) return NotFound();

                    string roleName = user.Role?.RoleName?.ToLower() ?? "";

                    // Check if there's an associated patient record
                    if (roleName == PATIENT_ROLE_NAME.ToLower())
                    {
                        var patient = await _context.Patients
                            .FirstOrDefaultAsync(p => p.UserId == id);

                        if (patient != null)
                        {
                            // Check if this patient has any related records that would block deletion
                            bool hasPatientDetails = await _context.PatientDetails
                                .AnyAsync(pd => pd.PatientId == patient.PatientId);

                            bool hasBookings = await _context.Bookings
                                .AnyAsync(b => b.PatientId == patient.PatientId);

                            if (hasPatientDetails || hasBookings)
                            {
                                return BadRequest("Cannot delete user because the associated patient has related records.");
                            }

                            // Remove the patient record first
                            _context.Patients.Remove(patient);
                            await _context.SaveChangesAsync();
                        }
                    }

                    // Check if there's an associated doctor record
                    if (roleName == DOCTOR_ROLE_NAME.ToLower())
                    {
                        var doctor = await _context.Doctors
                            .FirstOrDefaultAsync(d => d.UserId == id);

                        if (doctor != null)
                        {
                            // Check if this doctor has any related records that would block deletion
                            bool hasBookings = await _context.Bookings
                                .AnyAsync(b => b.DoctorId == doctor.DoctorId);

                            if (hasBookings)
                            {
                                return BadRequest("Cannot delete user because the associated doctor has bookings.");
                            }

                            // Remove the doctor record first
                            _context.Doctors.Remove(doctor);
                            await _context.SaveChangesAsync();
                        }
                    }

                    // Remove the user
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();

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

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(UserPasswordUpdateDTO passwordUpdateDTO)
        {
            var user = await _context.Users.FindAsync(passwordUpdateDTO.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Verify current password
            bool currentPasswordValid = false;
            bool isPotentiallyHashed = user.Password.Length == 60 &&
                                   (user.Password.StartsWith("$2a$") ||
                                    user.Password.StartsWith("$2b$") ||
                                    user.Password.StartsWith("$2y$"));

            if (isPotentiallyHashed)
            {
                try
                {
                    currentPasswordValid = BCrypt.Net.BCrypt.Verify(passwordUpdateDTO.CurrentPassword, user.Password);
                }
                catch
                {
                    currentPasswordValid = false;
                }
            }
            else
            {
                // Plain text comparison as fallback
                currentPasswordValid = (user.Password == passwordUpdateDTO.CurrentPassword);
            }

            if (!currentPasswordValid)
            {
                return BadRequest("Current password is incorrect");
            }

            // Verify that new password and confirm password match
            if (passwordUpdateDTO.NewPassword != passwordUpdateDTO.ConfirmPassword)
            {
                return BadRequest("New password and confirmation do not match");
            }

            // Update password with hashed version
            user.Password = BCrypt.Net.BCrypt.HashPassword(passwordUpdateDTO.NewPassword);
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok("Password changed successfully");
        }

        private async Task<bool> UserExistsAsync(string id)
        {
            return await _context.Users.AnyAsync(u => u.UserId == id);
        }
    }
}