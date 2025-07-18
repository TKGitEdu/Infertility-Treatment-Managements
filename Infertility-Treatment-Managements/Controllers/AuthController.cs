using Infertility_Treatment_Managements.DTOs;
using Infertility_Treatment_Managements.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Infertility_Treatment_Managements.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Infertility_Treatment_Managements.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _Context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthController(
            InfertilityTreatmentManagementContext context, 
            IConfiguration configuration,
            IEmailService emailService)
        {
            _Context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<object>> Login(UserLoginDTO loginDto)
        {
            try
            {
                if (loginDto == null)
                {
                    return BadRequest("Username and password are required");
                }

                // Truy vấn user với chỉ những trường cần thiết
                var user = await _Context.Users
                    .AsNoTracking()
                    .Where(u => u.Username == loginDto.Username)
                    .Select(u => new {
                        u.UserId,
                        u.Username,
                        u.Password,
                        u.FullName,
                        u.Email,
                        u.Phone,
                        u.RoleId,
                        u.Address,
                        u.Gender,
                        u.DateOfBirth
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return Unauthorized("Invalid username or password");
                }

                // Kiểm tra mật khẩu
                if (user.Password != loginDto.Password)
                {
                    return Unauthorized("Invalid username or password");
                }

                // Lấy role
                string roleName = null;
                if (!string.IsNullOrEmpty(user.RoleId) && !user.RoleId.Equals("null", StringComparison.OrdinalIgnoreCase))
                {
                    var role = await _Context.Roles
                        .Where(r => r.RoleId == user.RoleId.Trim())
                        .Select(r => new { r.RoleName })
                        .FirstOrDefaultAsync();

                    roleName = role?.RoleName;
                }

                // Tạo token đơn giản
                var token = CreateJwtToken(user.UserId, user.Username, roleName);

                // Tạo user DTO
                var userDto = new UserDTO
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    RoleId = user.RoleId,
                    Address = user.Address,
                    Gender = user.Gender,
                    DateOfBirth = user.DateOfBirth,
                    Role = roleName != null ? new RoleDTO { RoleName = roleName } : null
                };

                // Lấy DoctorId nếu user có vai trò là Doctor
                string doctorId = null;
                if (roleName == "Doctor")
                {
                    var doctor = await _Context.Doctors
                        .AsNoTracking()
                        .Where(d => d.UserId == user.UserId)
                        .Select(d => new { d.DoctorId })
                        .FirstOrDefaultAsync();

                    doctorId = doctor?.DoctorId;

                    if (doctor != null)
                    {
                        userDto.Doctor = new DoctorBasicDTO
                        {
                            DoctorId = doctor.DoctorId,
                            UserId = user.UserId,
                            Email = user.Email,
                            Phone = user.Phone
                        };
                    }
                }

                // Lấy PatientId nếu user có vai trò là Patient
                string patientId = null;
                if (roleName == "Patient")
                {
                    var patient = await _Context.Patients
                        .AsNoTracking()
                        .Where(p => p.UserId == user.UserId)
                        .Select(p => new { p.PatientId, p.Name, p.Email, p.Phone, p.Gender, p.DateOfBirth })
                        .FirstOrDefaultAsync();

                    patientId = patient?.PatientId;

                    if (patient != null)
                    {
                        userDto.Patients = new List<PatientBasicDTO>
                {
                    new PatientBasicDTO
                    {
                        PatientId = patient.PatientId,
                        Name = patient.Name,
                        Email = patient.Email,
                        Phone = patient.Phone,
                        Gender = patient.Gender,
                        DateOfBirth = patient.DateOfBirth.HasValue
                            ? DateOnly.FromDateTime(patient.DateOfBirth.Value)
                            : null
                    }
                };
                    }
                }

                // Vẫn giữ nguyên cách gửi token qua header để tương thích ngược
                Response.Headers.Append("Authorization", $"Bearer {token}");

                // Trả về cả user, token, doctorId và patientId trong body response
                return Ok(new
                {
                    user = userDto,
                    token = token,
                    doctorId = doctorId,
                    patientId = patientId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("logout")]
        [Authorize] // Có thể yêu cầu token hợp lệ, hoặc bỏ nếu không cần
        public IActionResult Logout()
        {
            // Không cần xử lý gì vì JWT là stateless
            // Có thể ghi log nếu muốn
            return Ok(new { message = "Logged out successfully (client-side token removed)." });
        }

        // Hàm tạo token đơn giản
        private string CreateJwtToken(string userId, string username, string roleName)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"] ?? "defaultsecretkey12345678901234567890");

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, username),
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            if (!string.IsNullOrEmpty(roleName))
            {
                claims.Add(new Claim(ClaimTypes.Role, roleName));
            }

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"] ?? "InfertilityTreatmentManagement",
                audience: jwtSettings["Audience"] ?? "InfertilityTreatmentManagementClient",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(secretKey),
                    SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> Register(UserCreateDTO dto)
        {
            try
            {
                // 1. Kiểm tra trùng username/email
                if (await _Context.Users.AnyAsync(u => u.Username == dto.Username))
                    return BadRequest("Username already exists");
                if (await _Context.Users.AnyAsync(u => u.Email == dto.Email))
                    return BadRequest("Email already exists");

                // 2. Lấy Role "Patient"
                var patientRole = await _Context.Roles
                    .FirstOrDefaultAsync(r => r.RoleName.ToLower() == "patient");

                if (patientRole == null)
                {
                    return StatusCode(500, "Patient role not found in system");
                }

                // 3. Tạo User
                var user = new User
                {
                    UserId = Guid.NewGuid().ToString(),
                    Username = dto.Username,
                    FullName = dto.FullName ?? "",
                    Email = dto.Email,
                    Phone = dto.Phone ?? "",
                    Password = dto.Password, // Lưu mật khẩu nguyên bản theo yêu cầu
                    RoleId = patientRole.RoleId,
                    Address = dto.Address ?? "",
                    Gender = dto.Gender ?? "",
                    DateOfBirth = dto.DateOfBirth
                };

                // 4. Tạo Patient
                var patient = new Patient
                {
                    PatientId = "PAT_" + Guid.NewGuid().ToString().Substring(0, 8),
                    UserId = user.UserId,
                    Name = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone ?? "",
                    Address = user.Address ?? "",
                    Gender = user.Gender ?? "",
                    DateOfBirth = user.DateOfBirth,
                    BloodType = dto.BloodType ?? "Unknown",
                    EmergencyPhoneNumber = dto.EmergencyPhoneNumber ?? user.Phone ?? ""
                };

                // 5. Tạo PatientDetail
                var patientDetail = new PatientDetail
                {
                    PatientDetailId = "PATD_" + Guid.NewGuid().ToString().Substring(0, 8),
                    PatientId = patient.PatientId,
                    TreatmentStatus = "New" // Trạng thái mặc định khi mới đăng ký
                };

                // 6. Thêm cả User, Patient và PatientDetail trong một transaction
                _Context.Users.Add(user);
                _Context.Patients.Add(patient);
                _Context.PatientDetails.Add(patientDetail);

                // 7. Lưu các thay đổi vào database
                await _Context.SaveChangesAsync();

                // 8. Tạo token và trả về
                var token = GenerateJwtToken(user, patientRole);
                Response.Headers.Append("Authorization", $"Bearer {token}");

                // 9. Chuẩn bị DTO để trả về
                var userDto = new UserDTO
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    RoleId = user.RoleId,
                    Address = user.Address,
                    Gender = user.Gender,
                    DateOfBirth = user.DateOfBirth,
                    Role = new RoleDTO
                    {
                        RoleId = patientRole.RoleId,
                        RoleName = patientRole.RoleName ?? ""
                    }
                };

                return Ok("Đăng ký thành công");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Registration error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpGet("validate")]
        [Authorize]
        public async Task<ActionResult<UserDTO>> ValidateToken()
        {
            try
            {
                // Lấy userId từ claims
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"Validating token for userId: {userId}");

                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("UserId không tìm thấy trong token");
                    return Unauthorized("Invalid token");
                }

                // Tìm user với projection để tránh lỗi null
                var user = await _Context.Users
                    .AsNoTracking()
                    .Where(u => u.UserId.ToString() == userId)
                    .Select(u => new {
                        u.UserId,
                        u.Username,
                        u.FullName,
                        u.Email,
                        u.Phone,
                        u.RoleId,
                        u.Address,
                        u.Gender,
                        u.DateOfBirth
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    Console.WriteLine($"Không tìm thấy user với id: {userId}");
                    return Unauthorized("User not found");
                }

                Console.WriteLine($"Tìm thấy user: {user.Username}");                // Lấy role nếu có
                string? roleName = null;
                string? roleId = null;

                if (!string.IsNullOrEmpty(user.RoleId) && !user.RoleId.Equals("null", StringComparison.OrdinalIgnoreCase))
                {
                    var role = await _Context.Roles
                        .AsNoTracking()
                        .Where(r => r.RoleId == user.RoleId)
                        .Select(r => new { r.RoleId, r.RoleName })
                        .FirstOrDefaultAsync();

                    if (role != null)
                    {
                        roleId = role.RoleId;
                        roleName = role.RoleName;
                        Console.WriteLine($"Vai trò của user: {roleName}");
                    }
                }

                // Tạo UserDTO an toàn
                var userDto = new UserDTO
                {
                    UserId = user.UserId,
                    Username = user.Username ?? "",
                    FullName = user.FullName ?? "",
                    Email = user.Email ?? "",
                    Phone = user.Phone ?? "",
                    RoleId = user.RoleId,
                    Address = user.Address ?? "",
                    Gender = user.Gender ?? "",
                    DateOfBirth = user.DateOfBirth
                };                // Thêm role nếu có
                if (roleId?.Length > 0 && !string.IsNullOrEmpty(roleName))
                {
                    userDto.Role = new RoleDTO
                    {
                        RoleId = roleId,
                        RoleName = roleName
                    };
                }

                // Tạm thời bỏ qua các thông tin Doctor và Patient
                // Những thông tin này có thể được lấy trong các API riêng biệt

                Console.WriteLine("Xác thực token thành công");
                return userDto;
            }
            catch (Exception ex)
            {
                // Log đầy đủ thông tin lỗi
                Console.WriteLine($"Token validation error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }

                return StatusCode(500, new { error = true, message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] UserForgotPasswordDTO forgotPasswordDTO)
        {
            try
            {
                // Tìm user với email được cung cấp
                var user = await _Context.Users.FirstOrDefaultAsync(u => u.Email == forgotPasswordDTO.Email);

                if (user == null)
                {
                    // Không nên tiết lộ rằng email không tồn tại, vì lý do bảo mật
                    return Ok("Nếu email tồn tại trong hệ thống, một email hướng dẫn đặt lại mật khẩu sẽ được gửi.");
                }

                // Tạo token reset password (có thể dùng GUID)
                string resetToken = Guid.NewGuid().ToString();

                // Bắt lỗi nếu các trường không tồn tại trong cơ sở dữ liệu
                try
                {
                    // Lưu token và thời gian hết hạn
                    user.ResetPasswordToken = resetToken;
                    user.ResetPasswordExpiry = DateTime.UtcNow.AddHours(1); // Token có hiệu lực 1 giờ

                    await _Context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi lưu token: {ex.Message}");
                    // Vẫn tiếp tục gửi email, nhưng sẽ không lưu token trong DB
                    // Có thể lưu token trong cache hoặc chấp nhận không có chức năng reset password
                }

                // Lấy base URL từ request
                string baseUrl = $"{Request.Scheme}://{Request.Host}";

                // Đối với frontend SPA, URL reset password sẽ trỏ đến route của frontend
                string resetLink = $"{baseUrl}/reset-password?token={resetToken}";

                // Tạo nội dung email
                string emailBody = $@"
            <h2>Yêu cầu đặt lại mật khẩu</h2>
            <p>Xin chào {user.FullName},</p>
            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
            <p>Vui lòng nhấp vào liên kết dưới đây để đặt lại mật khẩu:</p>
            <p><a href='{resetLink}'>Đặt lại mật khẩu</a></p>
            <p>Liên kết này sẽ hết hạn sau 1 giờ.</p>
            <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
            <p>Trân trọng,<br>Phòng khám điều trị vô sinh</p>
        ";

                try
                {                    // Inject EmailService và gửi email
                    if (!string.IsNullOrEmpty(user.Email))
                    {
                        await _emailService.SendEmailAsync(user.Email, "Đặt lại mật khẩu", emailBody);
                    }

                    return Ok("Nếu email tồn tại trong hệ thống, một email hướng dẫn đặt lại mật khẩu sẽ được gửi.");
                }
                catch (Exception ex)
                {
                    // Log lỗi
                    return StatusCode(500, $"Lỗi khi gửi email: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi forgot password: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, $"Lỗi hệ thống: {ex.Message}");
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] UserResetPasswordDTO resetPasswordDTO)
        {
            // Tìm user với token và kiểm tra thời hạn
            var user = await _Context.Users.FirstOrDefaultAsync(u =>
                u.ResetPasswordToken == resetPasswordDTO.Token &&
                u.ResetPasswordExpiry > DateTime.UtcNow);

            if (user == null)
            {
                return BadRequest("Token không hợp lệ hoặc đã hết hạn");
            }

            // Kiểm tra mật khẩu mới và xác nhận mật khẩu
            if (resetPasswordDTO.NewPassword != resetPasswordDTO.ConfirmPassword)
            {
                return BadRequest("Mật khẩu mới và xác nhận mật khẩu không khớp");
            }

            // Cập nhật mật khẩu không mã hóa và xóa token
            user.Password = resetPasswordDTO.NewPassword;
            user.ResetPasswordToken = null;
            user.ResetPasswordExpiry = null;

            await _Context.SaveChangesAsync();

            return Ok("Mật khẩu đã được đặt lại thành công");
        }

        [HttpPost("fix-patient-records")]
        [Authorize(Roles = "Admin")] // Chỉ admin mới có thể chạy
        public async Task<IActionResult> FixPatientRecords()
        {
            try
            {
                // Tìm Patient role trong database
                var patientRole = await _Context.Roles
                    .FirstOrDefaultAsync(r => r.RoleName.ToLower() == "patient");

                if (patientRole == null)
                {
                    return BadRequest("Patient role not found in the system");
                }

                // Lấy ID của Patient role
                string patientRoleId = patientRole.RoleId;

                // Tìm tất cả người dùng có role là Patient
                var patientUsers = await _Context.Users
                    .Where(u => u.RoleId == patientRoleId)
                    .ToListAsync();

                int created = 0;
                int existing = 0;
                int failed = 0;

                // Tạo transaction để đảm bảo tính nhất quán
                using var transaction = await _Context.Database.BeginTransactionAsync();

                try
                {
                    foreach (var user in patientUsers)
                    {
                        // Kiểm tra xem đã có bản ghi patient chưa
                        var existingPatient = await _Context.Patients
                            .FirstOrDefaultAsync(p => p.UserId == user.UserId);

                        if (existingPatient == null)
                        {
                            // Tạo bản ghi patient mới
                            var patient = new Patient
                            {
                                UserId = user.UserId,
                                Name = user.FullName ?? "",
                                Email = user.Email ?? "",
                                Phone = user.Phone ?? "",
                                Address = user.Address ?? "",
                                Gender = user.Gender ?? "",
                                DateOfBirth = user.DateOfBirth
                            };

                            _Context.Patients.Add(patient);
                            created++;
                        }
                        else
                        {
                            existing++;
                        }
                    }

                    // Lưu các thay đổi vào database
                    await _Context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Trả về thông tin chi tiết về kết quả
                    return Ok(new
                    {
                        Message = $"Fixed {created} patient records",
                        Created = created,
                        AlreadyExisting = existing,
                        Failed = failed,
                        TotalUsers = patientUsers.Count
                    });
                }
                catch (Exception ex)
                {
                    // Rollback transaction nếu có lỗi
                    await transaction.RollbackAsync();
                    return StatusCode(500, new
                    {
                        Message = $"An error occurred while creating patient records",
                        Error = ex.Message
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An error occurred while processing request",
                    Error = ex.Message
                });
            }
        }
        [HttpPost("register-simple")]
        [AllowAnonymous] // Cho phép đăng ký mà không cần xác thực
        public async Task<ActionResult<object>> RegisterSimple(SimpleUserRegisterDTO registerDto)
        {
            try
            {
                // Check if username already exists
                if (await _Context.Users.AnyAsync(u => u.Username == registerDto.Username))
                {
                    return BadRequest("Username already exists");
                }

                // Check if email already exists
                if (await _Context.Users.AnyAsync(u => u.Email == registerDto.Email))
                {
                    return BadRequest("Email already exists");
                }

                // Find the Patient role
                String roldIdPatient = null;
                var roles = await _Context.Roles.ToListAsync();
                if (roles != null && roles.Count > 0)
                {
                    var patientRole = roles.FirstOrDefault(r => r.RoleName?.ToLower() == "patient");
                    if (patientRole != null)
                    {
                        roldIdPatient = patientRole.RoleId.ToString();
                    }
                }

                // Create new user with minimal information
                var user = new User
                {
                    UserId = Guid.NewGuid().ToString(),
                    FullName = registerDto.FullName ?? "",
                    Email = registerDto.Email,
                    Phone = registerDto.Phone ?? "",
                    Username = registerDto.Username,
                    Password = registerDto.Password, // Plain text password
                    RoleId = roldIdPatient, // Default to Patient role
                    Address = "",
                    Gender = "",
                    DateOfBirth = null
                };

                // Use the execution strategy provided by the DbContext
                var strategy = _Context.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    // Begin transaction
                    using (var transaction = await _Context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            _Context.Users.Add(user);
                            await _Context.SaveChangesAsync();

                            // Create a patient record with minimal info
                            var patient = new Patient
                            {
                                PatientId = "PAT_" + Guid.NewGuid().ToString().Substring(0, 8),
                                UserId = user.UserId,
                                Name = user.FullName ?? "",
                                Email = user.Email,
                                Phone = user.Phone ?? "",
                                Address = "",
                                Gender = "",
                                DateOfBirth = null,
                                BloodType = null,
                                EmergencyPhoneNumber = null
                            };

                            _Context.Patients.Add(patient);
                            await _Context.SaveChangesAsync();

                            // Commit transaction
                            await transaction.CommitAsync();
                        }
                        catch (Exception)
                        {
                            // Rollback transaction if there's an error
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
                });

                // Load role safely
                Role role = null;
                if (!string.IsNullOrEmpty(user.RoleId) && !user.RoleId.Equals("null", StringComparison.OrdinalIgnoreCase))
                {
                    role = await _Context.Roles.FindAsync(user.RoleId);
                }

                // Generate token
                var token = GenerateJwtToken(user, role);

                // Return user DTO
                var userDto = new UserDTO
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    FullName = user.FullName ?? "",
                    Email = user.Email,
                    Phone = user.Phone ?? "",
                    RoleId = user.RoleId,
                    Address = "",
                    Gender = "",
                    DateOfBirth = null,
                    Role = role != null ? new RoleDTO
                    {
                        RoleId = role.RoleId,
                        RoleName = role.RoleName ?? ""
                    } : null
                };

                // Add token to response
                Response.Headers.Append("Authorization", $"Bearer {token}");

                // Return both user and token in the response body
                return Ok(new
                {
                    user = userDto,
                    token = token
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost("fix-doctor-records")]
        [Authorize(Roles = "Admin")] // Chỉ admin mới có thể chạy
        public async Task<IActionResult> FixDoctorRecords()
        {
            try
            {
                // Tìm Doctor role trong database
                var doctorRole = await _Context.Roles
                    .FirstOrDefaultAsync(r => r.RoleName.ToLower() == "doctor");

                if (doctorRole == null)
                {
                    return BadRequest("Doctor role not found in the system");
                }

                // Lấy ID của Doctor role
                string doctorRoleId = doctorRole.RoleId;

                // Tìm tất cả người dùng có role là Doctor
                var doctorUsers = await _Context.Users
                    .Where(u => u.RoleId == doctorRoleId)
                    .ToListAsync();

                int created = 0;
                int existing = 0;
                int failed = 0;

                // Tạo transaction để đảm bảo tính nhất quán
                using var transaction = await _Context.Database.BeginTransactionAsync();

                try
                {
                    foreach (var user in doctorUsers)
                    {
                        // Kiểm tra xem đã có bản ghi doctor chưa
                        var existingDoctor = await _Context.Doctors
                            .FirstOrDefaultAsync(d => d.UserId == user.UserId);

                        if (existingDoctor == null)
                        {
                            try
                            {
                                // Tạo bản ghi doctor mới
                                var doctor = new Doctor
                                {
                                    UserId = user.UserId,
                                    DoctorName = user.FullName ?? "",
                                    Email = user.Email ?? "",
                                    Phone = user.Phone ?? "",
                                    Specialization = "General" // Mặc định
                                };

                                _Context.Doctors.Add(doctor);
                                await _Context.SaveChangesAsync();
                                created++;
                            }
                            catch (Exception)
                            {
                                failed++;
                            }
                        }
                        else
                        {
                            existing++;
                        }
                    }

                    await transaction.CommitAsync();

                    // Trả về thông tin chi tiết về kết quả
                    return Ok(new
                    {
                        Message = $"Fixed {created} doctor records",
                        Created = created,
                        AlreadyExisting = existing,
                        Failed = failed,
                        TotalUsers = doctorUsers.Count
                    });
                }
                catch (Exception ex)
                {
                    // Rollback transaction nếu có lỗi
                    await transaction.RollbackAsync();
                    return StatusCode(500, new
                    {
                        Message = $"An error occurred while creating doctor records",
                        Error = ex.Message
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An error occurred while processing request",
                    Error = ex.Message
                });
            }
        }


        private string GenerateJwtToken(User user, Role? role = null)
        {
            try
            {
                if (user == null)
                {
                    Console.WriteLine("User is null in GenerateJwtToken");
                    throw new ArgumentNullException(nameof(user), "User cannot be null when generating JWT token");
                }

                var jwtSettings = _configuration.GetSection("JwtSettings");

                // Check if configuration exists
                if (jwtSettings == null)
                {
                    Console.WriteLine("JwtSettings section is missing from configuration");
                    // Use default values
                }

                var secretKey = jwtSettings?["SecretKey"];
                if (string.IsNullOrEmpty(secretKey))
                {
                    Console.WriteLine("Using default secret key");
                    secretKey = "defaultsecretkey12345678901234567890";
                }

                var key = Encoding.UTF8.GetBytes(secretKey);

                var claims = new List<Claim>();

                // Add claims with null checks
                claims.Add(new Claim(ClaimTypes.Name, user.Username ?? ""));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()));
                claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

                // Add role claim if user has a role
                if (role != null && !string.IsNullOrEmpty(role.RoleName))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.RoleName));
                }

                // Get token expiration time
                double expirationMinutes;
                if (!double.TryParse(jwtSettings?["AccessTokenExpirationMinutes"], out expirationMinutes))
                {
                    Console.WriteLine("Using default expiration time");
                    expirationMinutes = 60; // Default to 60 minutes
                }

                // Create token with default values if config values are missing
                var token = new JwtSecurityToken(
                    issuer: jwtSettings?["Issuer"] ?? "InfertilityTreatmentManagement",
                    audience: jwtSettings?["Audience"] ?? "InfertilityTreatmentManagementClient",
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
                    signingCredentials: new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256)
                );

                var tokenHandler = new JwtSecurityTokenHandler();
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating JWT token: {ex.Message}");
                throw; // Rethrow to be caught by the calling method
            }
        }
    }
}