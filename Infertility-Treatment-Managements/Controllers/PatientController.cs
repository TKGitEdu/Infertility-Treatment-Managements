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
using Microsoft.AspNetCore.Authorization;

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
        public async Task<ActionResult<PatientDTO>> GetPatient(string id)
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
        public async Task<ActionResult<IEnumerable<PatientDTO>>> GetPatientsByUser(string userId)
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
        public async Task<ActionResult<string>> GetPatientRoleId()
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
        [AllowAnonymous]
        public async Task<ActionResult<PatientDTO>> CreatePatient(PatientCreateDTO patientCreateDTO)
        {
            try
            {
                // Sử dụng execution strategy cho transaction
                var result = await _context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        // 1. Get patient role
                        var patientRole = await _context.Roles
                            .FirstOrDefaultAsync(r => r.RoleName == PATIENT_ROLE_NAME);

                        if (patientRole == null)
                        {
                            throw new Exception($"Patient role not found. Please create a '{PATIENT_ROLE_NAME}' role first.");
                        }

                        // 2. Handle user creation or validation
                        User user;
                        int maxRetries = 3; // Số lần thử lại tối đa
                        int retryCount = 0;
                        string generatedUsername = null;
                        string generatedPassword = null;

                        if (!string.IsNullOrEmpty(patientCreateDTO.UserId))
                        {
                            // Using existing user
                            user = await _context.Users.FindAsync(patientCreateDTO.UserId);
                            if (user == null)
                            {
                                throw new Exception("Invalid UserId: User does not exist");
                            }

                            // Check if patient with this user ID already exists
                            var patientExists = await _context.Patients.AnyAsync(p => p.UserId == patientCreateDTO.UserId);
                            if (patientExists)
                            {
                                throw new Exception("Patient with this UserId already exists");
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
                                throw new Exception("Name is required when creating a new user");
                            }
                            if (string.IsNullOrEmpty(patientCreateDTO.Email))
                            {
                                throw new Exception("Email is required when creating a new user");
                            }
                            if (string.IsNullOrEmpty(patientCreateDTO.Phone))
                            {
                                throw new Exception("Phone is required when creating a new user");
                            }

                            // Use provided username or generate one from email
                            string username = patientCreateDTO.Username;
                            if (string.IsNullOrEmpty(username))
                            {
                                // Generate username from email
                                username = patientCreateDTO.Email.Split('@')[0];

                                // Check if username already exists
                                bool usernameExists = await _context.Users.AnyAsync(u => u.Username == username);
                                if (usernameExists)
                                {
                                    // Append a random number if username exists
                                    Random random = new Random();
                                    username = $"{username}{random.Next(1000, 9999)}";
                                }
                            }
                            generatedUsername = username;

                            // Use provided password or generate one
                            string password = patientCreateDTO.Password;
                            if (string.IsNullOrEmpty(password))
                            {
                                // Generate a default password
                                Random passwordRandom = new Random();
                                string namePrefix = patientCreateDTO.Name.Length >= 3
                                    ? patientCreateDTO.Name.Substring(0, 3)
                                    : patientCreateDTO.Name;
                                password = $"Patient@{namePrefix}{passwordRandom.Next(1000, 9999)}";
                            }
                            generatedPassword = password;

                            // Create a new user with generated UserId
                            user = new User
                            {
                                UserId = Guid.NewGuid().ToString(), // Tạo UserId mới
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

                            // Đảm bảo user đã được lưu và có UserId
                            if (string.IsNullOrEmpty(user.UserId))
                            {
                                throw new Exception("Failed to create user: UserId not generated");
                            }
                        }

                        // 3. Tạo patient với vòng lặp kiểm tra
                        Patient patient = null;
                        bool isPatientCreated = false;

                        while (!isPatientCreated && retryCount < maxRetries)
                        {
                            try
                            {
                                // Tải lại user từ database để đảm bảo có UserId chính xác
                                user = await _context.Users.FindAsync(user.UserId);
                                if (user == null || string.IsNullOrEmpty(user.UserId))
                                {
                                    throw new Exception($"User not found or invalid after creation. UserId: {user?.UserId}");
                                }

                                patient = new Patient
                                {
                                    PatientId = Guid.NewGuid().ToString(), // Tạo PatientId mới
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

                                // Kiểm tra xem có thể lấy lại patient từ database không
                                var createdPatient = await _context.Patients.FindAsync(patient.PatientId);
                                if (createdPatient == null)
                                {
                                    throw new Exception("Failed to retrieve patient after creation");
                                }

                                // Kiểm tra các trường thông tin bắt buộc
                                if (string.IsNullOrEmpty(createdPatient.Name) ||
                                    string.IsNullOrEmpty(createdPatient.Email) ||
                                    string.IsNullOrEmpty(createdPatient.Phone))
                                {
                                    throw new Exception("Patient created but missing required fields");
                                }

                                isPatientCreated = true;
                            }
                            catch (Exception ex)
                            {
                                retryCount++;
                                if (retryCount >= maxRetries)
                                {
                                    // Nếu đã thử lại nhiều lần nhưng không thành công
                                    throw new Exception($"Failed to create patient after {maxRetries} attempts: {ex.Message}");
                                }

                                // Log lỗi và thử lại
                                Console.WriteLine($"Retry {retryCount}/{maxRetries}: {ex.Message}");
                                await Task.Delay(500); // Đợi 0.5 giây trước khi thử lại
                            }
                        }

                        // 4. Load the complete patient with related entities
                        var completePatient = await _context.Patients
                            .Include(p => p.User)
                            .Include(p => p.BookingFk)
                            .Include(p => p.PatientDetails)
                            .FirstOrDefaultAsync(p => p.PatientId == patient.PatientId);

                        await transaction.CommitAsync();

                        // Return user credentials along with patient data if we created a new user
                        if (!string.IsNullOrEmpty(generatedUsername))
                        {
                            // Add user credentials to response headers
                            Response.Headers.Add("X-Username", System.Text.RegularExpressions.Regex.Replace(generatedUsername, @"[^\u0000-\u007F]", ""));
                            Response.Headers.Add("X-UserId", user.UserId);

                            // For development environments, include password in headers
#if DEBUG
                            if (!string.IsNullOrEmpty(generatedPassword))
                            {
                                Response.Headers.Add("X-Password", generatedPassword);
                            }
#endif
                        }

                        return completePatient.ToDTO();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception($"Transaction rolled back: {ex.Message}", ex);
                    }
                });

                // Return the result
                var response = new OkObjectResult(result);
                return response;
            }
            catch (Exception ex)
            {
                // Log chi tiết lỗi cho debugging
                Console.WriteLine($"CreatePatient failed: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

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
                if (!string.IsNullOrEmpty(patientUpdateDTO.UserId) && patientUpdateDTO.UserId != patient.UserId)
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
                    var newUser = await _context.Users.FindAsync(patientUpdateDTO.UserId);
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
                if (!string.IsNullOrEmpty(patient.UserId))
                {
                    var user = await _context.Users.FindAsync(patient.UserId);
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
        public async Task<IActionResult> DeletePatientByUserId(string userId)
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
            string patientRoleId = null;
            if (string.IsNullOrEmpty(patientCreateDTO.UserId))
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

                if (string.IsNullOrEmpty(patientRoleId))
                {
                    return BadRequest($"Patient role not found. Please create a '{PATIENT_ROLE_NAME}' role first.");
                }
            }

            // Validate UserId if provided
            if (!string.IsNullOrEmpty(patientCreateDTO.UserId))
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
        public async Task<IActionResult> UpdatePatientLegacy(string id, PatientUpdateDTO patientUpdateDTO)
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
        public async Task<IActionResult> DeletePatientLegacy(string id)
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
                string userId = patient.UserId;

                // Remove the patient
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();

                // If there's an associated user, delete them too
                if (!string.IsNullOrEmpty(userId))
                {
                    var user = await _context.Users.FindAsync(userId);
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

        private async Task<bool> PatientExists(string id)
        {
            return await _context.Patients.AnyAsync(p => p.PatientId == id);
        }
    }
}