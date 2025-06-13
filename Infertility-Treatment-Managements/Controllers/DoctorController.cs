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
using System.ComponentModel.DataAnnotations;

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
        public async Task<ActionResult<DoctorDTO>> GetDoctorByUser(string userId)
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

        // POST: api/Doctor/Create
        [HttpPost("Create")]
        public async Task<ActionResult<DoctorDTO>> CreateDoctor(DoctorCreateDTO doctorCreateDTO)
        {
            try
            {
                // Sử dụng execution strategy cho transaction
                var result = await _context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        // 1. Get doctor role
                        var doctorRole = await _context.Roles
                            .FirstOrDefaultAsync(r => r.RoleName == DOCTOR_ROLE_NAME);

                        if (doctorRole == null)
                        {
                            throw new Exception($"Doctor role not found. Please create a '{DOCTOR_ROLE_NAME}' role first.");
                        }

                        // 2. Handle user creation or validation
                        User user;
                        int maxRetries = 3; // Số lần thử lại tối đa
                        int retryCount = 0;

                        if (doctorCreateDTO.UserId.Length>0)
                        {
                            // Using existing user
                            user = await _context.Users.FindAsync(doctorCreateDTO.UserId);
                            if (user == null)
                            {
                                throw new Exception("Invalid UserId: User does not exist");
                            }

                            // Check if doctor with this user ID already exists
                            var doctorExists = await _context.Doctors.AnyAsync(d => d.UserId == doctorCreateDTO.UserId);
                            if (doctorExists)
                            {
                                throw new Exception("Doctor with this UserId already exists");
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
                            // Validate required fields for creating a new user
                            if (string.IsNullOrEmpty(doctorCreateDTO.DoctorName))
                            {
                                throw new Exception("DoctorName is required when creating a new user");
                            }
                            if (string.IsNullOrEmpty(doctorCreateDTO.Email))
                            {
                                throw new Exception("Email is required when creating a new user");
                            }
                            if (string.IsNullOrEmpty(doctorCreateDTO.Phone))
                            {
                                throw new Exception("Phone is required when creating a new user");
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

                            // Đảm bảo user đã được lưu và có UserId
                            if (user.UserId == "")
                            {
                                throw new Exception("Failed to create user: UserId not generated");
                            }
                        }

                        // 3. Tạo doctor với vòng lặp kiểm tra
                        Doctor doctor = null;
                        bool isDoctorCreated = false;

                        while (!isDoctorCreated && retryCount < maxRetries)
                        {
                            try
                            {
                                // Tải lại user từ database để đảm bảo có UserId chính xác
                                user = await _context.Users.FindAsync(user.UserId);
                                if (user == null || user.UserId == "")
                                {
                                    throw new Exception($"User not found or invalid after creation. UserId: {user?.UserId}");
                                }

                                doctor = new Doctor
                                {
                                    UserId = user.UserId,
                                    DoctorName = doctorCreateDTO.DoctorName ?? user.FullName,
                                    Email = doctorCreateDTO.Email ?? user.Email,
                                    Phone = doctorCreateDTO.Phone ?? user.Phone,
                                    Specialization = doctorCreateDTO.Specialization ?? "General"
                                };

                                _context.Doctors.Add(doctor);
                                await _context.SaveChangesAsync();


                                // Kiểm tra xem có thể lấy lại doctor từ database không
                                var createdDoctor = await _context.Doctors.FindAsync(doctor.DoctorId);
                                if (createdDoctor == null)
                                {
                                    throw new Exception("Failed to retrieve doctor after creation");
                                }

                                // Kiểm tra các trường thông tin bắt buộc
                                if (string.IsNullOrEmpty(createdDoctor.DoctorName) ||
                                    string.IsNullOrEmpty(createdDoctor.Email) ||
                                    string.IsNullOrEmpty(createdDoctor.Phone))
                                {
                                    throw new Exception("Doctor created but missing required fields");
                                }

                                isDoctorCreated = true;
                            }
                            catch (Exception ex)
                            {
                                retryCount++;
                                if (retryCount >= maxRetries)
                                {
                                    // Nếu đã thử lại nhiều lần nhưng không thành công
                                    throw new Exception($"Failed to create doctor after {maxRetries} attempts: {ex.Message}");
                                }

                                // Log lỗi và thử lại
                                Console.WriteLine($"Retry {retryCount}/{maxRetries}: {ex.Message}");
                                await Task.Delay(500); // Đợi 0.5 giây trước khi thử lại
                            }
                        }

                        // 4. Load the complete doctor with related entities
                        var completeDoctor = await _context.Doctors
                            .Include(d => d.User)
                            .FirstOrDefaultAsync(d => d.DoctorId == doctor.DoctorId);

                        await transaction.CommitAsync();

                        // Return the created doctor
                        var result = completeDoctor.ToDTO();

                        // Định nghĩa kết quả trả về
                        var response = string.IsNullOrEmpty(doctorCreateDTO.UserId)
                            ? new
                            {
                                Doctor = result,
                                UserCredentials = new
                                {
                                    Username = user.Username,
                                    Password = user.Password,
                                    UserId = user.UserId
                                }
                            }
                            : (object)result;

                        return response;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception($"Transaction rolled back: {ex.Message}", ex);
                    }
                });

                // Xử lý kết quả để trả về ActionResult phù hợp
                if (result is DoctorDTO doctorDTO)
                {
                    return Ok(doctorDTO);
                }
                else
                {
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                // Log chi tiết lỗi cho debugging
                Console.WriteLine($"CreateDoctor failed: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

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
                if (!string.IsNullOrEmpty(doctorUpdateDTO.UserId) && doctorUpdateDTO.UserId != doctor.UserId)
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
                    var newUser = await _context.Users.FindAsync(doctorUpdateDTO.UserId);
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
                if (!string.IsNullOrEmpty(doctor.UserId))
                {
                    var user = await _context.Users.FindAsync(doctor.UserId);
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
        public async Task<IActionResult> DeleteDoctorByUserId(string userId)
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



        // POST: api/Doctor/RegisterDoctor
        [HttpPost("RegisterDoctor")]
        public async Task<ActionResult<object>> RegisterDoctor(DoctorRegistrationDTO registrationDTO)
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

                // 2. Create new user with doctor role
                var user = new User
                {
                    Username = registrationDTO.Username,
                    Password = registrationDTO.Password, // Note: In production, hash this password
                    Email = registrationDTO.Email,
                    Phone = registrationDTO.Phone,
                    FullName = registrationDTO.DoctorName,
                    Address = registrationDTO.Address,
                    Gender = registrationDTO.Gender,
                    RoleId = doctorRole.RoleId
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // 3. Create doctor profile
                var doctor = new Doctor
                {
                    UserId = user.UserId,
                    DoctorName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Specialization = registrationDTO.Specialization ?? "General"
                };

                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                // Return combined data
                return Ok(new
                {
                    UserId = user.UserId,
                    DoctorId = doctor.DoctorId,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    Specialization = doctor.Specialization,
                    Role = DOCTOR_ROLE_NAME
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }

    // Additional DTOs for the controller
    public class DoctorSpecializationDTO
    {
        public int DoctorId { get; set; }
        public string Specialization { get; set; }
    }
}