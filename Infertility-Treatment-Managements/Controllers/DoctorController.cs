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
using Microsoft.AspNetCore.Authorization;

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

        // GET: api/Doctor/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<DoctorDTO>> GetDoctorById(string id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Bookings)
                .FirstOrDefaultAsync(d => d.DoctorId == id);

            if (doctor == null)
            {
                return NotFound($"Doctor with ID {id} not found");
            }

            return doctor.ToDTO();
        }


        // GET: api/Doctor/BySpecialization/{specialization}
        [HttpGet("BySpecialization/{specialization}")]
        [AllowAnonymous]
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
        [AllowAnonymous]
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
                        string generatedPassword = null; // Lưu password nếu tạo mới

                        if (!string.IsNullOrEmpty(doctorCreateDTO.UserId))
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

                            // Lưu password được tạo để trả về trong headers
                            generatedPassword = password;

                            // Create a new user with generated UserId
                            user = new User
                            {
                                UserId = Guid.NewGuid().ToString(), // Thêm dòng này - tạo UserId mới
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
                            if (string.IsNullOrEmpty(user.UserId))
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
                                if (user == null || string.IsNullOrEmpty(user.UserId))
                                {
                                    throw new Exception($"User not found or invalid after creation. UserId: {user?.UserId}");
                                }

                                doctor = new Doctor
                                {
                                    DoctorId = Guid.NewGuid().ToString(), // Thêm dòng này - tạo DoctorId mới
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
                            .Include(d => d.Bookings)
                            .FirstOrDefaultAsync(d => d.DoctorId == doctor.DoctorId);

                        await transaction.CommitAsync();

                        // Luôn trả về DoctorDTO (phù hợp với schema của frontend)
                        return completeDoctor.ToDTO();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception($"Transaction rolled back: {ex.Message}", ex);
                    }
                });

                // Tạo response và thêm thông tin đăng nhập vào header nếu có
                var response = new OkObjectResult(result);

                // Nếu đã tạo tài khoản mới và có credentials, thêm vào headers
                // Nếu đã tạo tài khoản mới và có credentials, thêm vào headers
                if (result.User != null && !string.IsNullOrEmpty(result.User.Username))
                {
                    Response.Headers.Add("X-Username", System.Text.RegularExpressions.Regex.Replace(result.User.Username, @"[^\u0000-\u007F]", ""));
                    Response.Headers.Add("X-UserId", result.User.UserId);
                }

                return response;
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
        public async Task<ActionResult<DoctorDTO>> UpdateDoctor(DoctorUpdateDTO doctorUpdateDTO)
        {
            if (string.IsNullOrEmpty(doctorUpdateDTO.DoctorId))
            {
                return BadRequest("DoctorId is required for update");
            }

            try
            {
                // Tìm bác sĩ theo DoctorId
                var doctor = await _context.Doctors
                    .Include(d => d.User)
                    .FirstOrDefaultAsync(d => d.DoctorId == doctorUpdateDTO.DoctorId);

                if (doctor == null)
                {
                    return NotFound($"Doctor with ID {doctorUpdateDTO.DoctorId} not found");
                }

                // Lấy UserId từ DTO hoặc từ doctor hiện tại
                string userId = doctorUpdateDTO.UserId ?? doctor.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest("UserId is required for update");
                }

                // Thực hiện cập nhật trong transaction
                await _context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        // 1. Cập nhật thông tin User
                        var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
                        if (user == null)
                        {
                            throw new Exception($"User with ID {userId} not found");
                        }

                        // Cập nhật các trường của User nếu có giá trị
                        if (!string.IsNullOrEmpty(doctorUpdateDTO.DoctorName))
                            user.FullName = doctorUpdateDTO.DoctorName;

                        if (!string.IsNullOrEmpty(doctorUpdateDTO.Email))
                            user.Email = doctorUpdateDTO.Email;

                        if (!string.IsNullOrEmpty(doctorUpdateDTO.Phone))
                            user.Phone = doctorUpdateDTO.Phone;

                        if (!string.IsNullOrEmpty(doctorUpdateDTO.Address))
                            user.Address = doctorUpdateDTO.Address;

                        if (!string.IsNullOrEmpty(doctorUpdateDTO.Gender))
                            user.Gender = doctorUpdateDTO.Gender;

                        if (doctorUpdateDTO.DateOfBirth.HasValue)
                            user.DateOfBirth = doctorUpdateDTO.DateOfBirth.Value;

                        // Đảm bảo có role Doctor
                        var doctorRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == DOCTOR_ROLE_NAME);
                        if (doctorRole != null && user.RoleId != doctorRole.RoleId)
                        {
                            user.RoleId = doctorRole.RoleId;
                        }

                        _context.Entry(user).State = EntityState.Modified;
                        await _context.SaveChangesAsync();

                        // 2. Cập nhật thông tin Doctor
                        // Nếu UserId khác với hiện tại, kiểm tra và cập nhật
                        if (userId != doctor.UserId)
                        {
                            var existingDoctor = await _context.Doctors
                                .FirstOrDefaultAsync(d => d.UserId == userId && d.DoctorId != doctorUpdateDTO.DoctorId);

                            if (existingDoctor != null)
                            {
                                throw new Exception($"Another doctor is already associated with this user");
                            }

                            doctor.UserId = userId;
                        }

                        // Cập nhật các trường khác của Doctor nếu có giá trị
                        if (!string.IsNullOrEmpty(doctorUpdateDTO.DoctorName))
                            doctor.DoctorName = doctorUpdateDTO.DoctorName;

                        if (!string.IsNullOrEmpty(doctorUpdateDTO.Email))
                            doctor.Email = doctorUpdateDTO.Email;

                        if (!string.IsNullOrEmpty(doctorUpdateDTO.Phone))
                            doctor.Phone = doctorUpdateDTO.Phone;

                        if (!string.IsNullOrEmpty(doctorUpdateDTO.Specialization))
                            doctor.Specialization = doctorUpdateDTO.Specialization;

                        _context.Entry(doctor).State = EntityState.Modified;
                        await _context.SaveChangesAsync();

                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                // Lấy thông tin bác sĩ đã cập nhật để trả về
                var updatedDoctor = await _context.Doctors
                    .Include(d => d.User)
                    .Include(d => d.Bookings)
                    .FirstOrDefaultAsync(d => d.DoctorId == doctorUpdateDTO.DoctorId);

                // Trả về DoctorDTO (đã được chuyển đổi thông qua extension method ToDTO)
                return Ok(updatedDoctor.ToDTO());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to update doctor: {ex.Message}");
            }
        }


        // DELETE: api/Doctor/Delete/{userId}
        [HttpDelete("Delete/{userId}")]
        public async Task<IActionResult> DeleteDoctorByUserId(string userId)
        {
            try
            {
                // First find the doctor by userId outside the execution strategy
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);

                if (doctor == null)
                {
                    return NotFound($"No doctor found with UserId {userId}");
                }

                // Check dependencies outside the execution strategy
                var hasBookings = await _context.Bookings.AnyAsync(b => b.DoctorId == doctor.DoctorId);
                if (hasBookings)
                {
                    return BadRequest("Cannot delete doctor with associated bookings");
                }

                var hasTreatmentPlans = await _context.TreatmentPlans.AnyAsync(tp => tp.DoctorId == doctor.DoctorId);
                if (hasTreatmentPlans)
                {
                    return BadRequest("Cannot delete doctor with associated treatment plans");
                }

                var hasExaminations = await _context.Examinations.AnyAsync(e => e.DoctorId == doctor.DoctorId);
                if (hasExaminations)
                {
                    return BadRequest("Cannot delete doctor with associated examinations");
                }

                var hasTreatmentProcesses = await _context.TreatmentProcesses.AnyAsync(tp => tp.DoctorId == doctor.DoctorId);
                if (hasTreatmentProcesses)
                {
                    return BadRequest("Cannot delete doctor with associated treatment processes");
                }

                // Store the doctorId for later use
                string doctorId = doctor.DoctorId;

                // Now use execution strategy only for the database operations that need to be retried
                await _context.Database.CreateExecutionStrategy().ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        // Find the user
                        var user = await _context.Users.FindAsync(userId);
                        if (user != null)
                        {
                            // Delete the user - this will automatically set doctor.UserId to null due to OnDelete(DeleteBehavior.SetNull)
                            _context.Users.Remove(user);
                            await _context.SaveChangesAsync();
                        }

                        // Now find the doctor with null UserId (which should be our doctor after user deletion)
                        var doctorToDelete = await _context.Doctors.FindAsync(doctorId);
                        if (doctorToDelete != null)
                        {
                            // Doctor should have null UserId now due to the foreign key constraint
                            _context.Doctors.Remove(doctorToDelete);
                            await _context.SaveChangesAsync();
                        }

                        await transaction.CommitAsync();
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                return NoContent();
            }
            catch (Exception ex)
            {
                // Log error details
                Console.WriteLine($"DeleteDoctorByUserId failed: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

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