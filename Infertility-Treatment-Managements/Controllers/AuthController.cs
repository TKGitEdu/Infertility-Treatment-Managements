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
        private readonly InfertilityTreatmentManagementContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthController(
            InfertilityTreatmentManagementContext context, 
            IConfiguration configuration,
            IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(UserLoginDTO loginDto)
        {
            try
            {
                if (loginDto == null)
                {
                    return BadRequest("Username and password are required");
                }

                // Truy vấn user với chỉ những trường cần thiết
                var user = await _context.Users
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
                if (user.RoleId.HasValue)
                {
                    var role = await _context.Roles
                        .Where(r => r.RoleId == user.RoleId.Value)
                        .Select(r => new { r.RoleName })
                        .FirstOrDefaultAsync();

                    roleName = role?.RoleName;
                }

                // Tạo token đơn giản
                var token = CreateJwtToken(user.UserId, user.Username, roleName);

                // Tạo response
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

                // Thêm token vào header
                Response.Headers.Add("Authorization", $"Bearer {token}");

                return userDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // Hàm tạo token đơn giản
        private string CreateJwtToken(int userId, string username, string roleName)
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
                expires: DateTime.Now.AddHours(1),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(secretKey),
                    SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(UserCreateDTO userCreateDto)
        {
            try
            {
                // Check if username already exists
                if (await _context.Users.AnyAsync(u => u.Username == userCreateDto.Username))
                {
                    return BadRequest("Username already exists");
                }

                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == userCreateDto.Email))
                {
                    return BadRequest("Email already exists");
                }

                // Store password directly without hashing
                string password = userCreateDto.Password;

                // Create new user
                var user = new User
                {
                    FullName = userCreateDto.FullName ?? "",
                    Email = userCreateDto.Email ?? "",
                    Phone = userCreateDto.Phone ?? "",
                    Username = userCreateDto.Username ?? "",
                    Password = password, // Plain text password
                    RoleId = userCreateDto.RoleId ?? 2, // Default to Patient role if not specified
                    Address = userCreateDto.Address ?? "",
                    Gender = userCreateDto.Gender ?? "",
                    DateOfBirth = userCreateDto.DateOfBirth
                };

                // Use the execution strategy provided by the DbContext
                var strategy = _context.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    // Begin transaction
                    using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            _context.Users.Add(user);
                            await _context.SaveChangesAsync();

                            // Check user's role
                            if (user.RoleId.HasValue)
                            {
                                var Role = await _context.Roles.FindAsync(user.RoleId.Value);

                                // If the user is a Doctor (RoleId = 1)
                                if (Role?.RoleName?.ToLower() == "doctor")
                                {
                                    // Create a doctor record
                                    var doctor = new Doctor
                                    {
                                        UserId = user.UserId,
                                        DoctorName = user.FullName ?? "",
                                        Email = user.Email ?? "",
                                        Phone = user.Phone ?? "",
                                        Specialization = userCreateDto.Specialization ?? "General" // Mặc định
                                    };

                                    _context.Doctors.Add(doctor);
                                    await _context.SaveChangesAsync();
                                }
                                // If the user is a Patient (RoleId = 2)
                                else if (Role?.RoleName?.ToLower() == "patient")
                                {
                                    // Create a patient record
                                    var patient = new Patient
                                    {
                                        UserId = user.UserId,
                                        Name = user.FullName ?? "",
                                        Email = user.Email ?? "",
                                        Phone = user.Phone ?? "",
                                        Address = user.Address ?? "",
                                        Gender = user.Gender ?? "",
                                        DateOfBirth = user.DateOfBirth,
                                        BloodType = userCreateDto.BloodType,
                                        EmergencyPhoneNumber = userCreateDto.EmergencyPhoneNumber
                                    };

                                    _context.Patients.Add(patient);
                                    await _context.SaveChangesAsync();
                                }
                            }

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
                if (user.RoleId.HasValue)
                {
                    role = await _context.Roles.FindAsync(user.RoleId.Value);
                }

                // Generate token
                var token = GenerateJwtToken(user, role);

                // Return user DTO
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
                    DateOfBirth = user.DateOfBirth,
                    Role = role != null ? new RoleDTO
                    {
                        RoleId = role.RoleId,
                        RoleName = role.RoleName ?? ""
                    } : null
                };

                // Add token to response
                Response.Headers.Add("Authorization", $"Bearer {token}");

                return CreatedAtAction(nameof(Login), userDto);
            }
            catch (Exception ex)
            {
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
                var user = await _context.Users
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

                Console.WriteLine($"Tìm thấy user: {user.Username}");

                // Lấy role nếu có
                string roleName = null;
                int? roleId = null;

                if (user.RoleId.HasValue)
                {
                    var role = await _context.Roles
                        .AsNoTracking()
                        .Where(r => r.RoleId == user.RoleId.Value)
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
                };

                // Thêm role nếu có
                if (roleId.HasValue && !string.IsNullOrEmpty(roleName))
                {
                    userDto.Role = new RoleDTO
                    {
                        RoleId = roleId.Value,
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
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == forgotPasswordDTO.Email);

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

                    await _context.SaveChangesAsync();
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
                {
                    // Inject EmailService và gửi email
                    await _emailService.SendEmailAsync(user.Email, "Đặt lại mật khẩu", emailBody);

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
            var user = await _context.Users.FirstOrDefaultAsync(u =>
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

            await _context.SaveChangesAsync();

            return Ok("Mật khẩu đã được đặt lại thành công");
        }

        [HttpPost("fix-patient-records")]
        [Authorize(Roles = "Admin")] // Chỉ admin mới có thể chạy
        public async Task<IActionResult> FixPatientRecords()
        {
            try
            {
                // Tìm tất cả người dùng với RoleId = 2 (Patient)
                var patientUsers = await _context.Users
                    .Where(u => u.RoleId == 2)
                    .ToListAsync();

                int created = 0;

                foreach (var user in patientUsers)
                {
                    // Kiểm tra xem đã có bản ghi patient chưa
                    var existingPatient = await _context.Patients
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
                            // Các trường BloodType và EmergencyPhoneNumber sẽ là null
                        };

                        _context.Patients.Add(patient);
                        created++;
                    }
                }

                if (created > 0)
                {
                    await _context.SaveChangesAsync();
                }

                return Ok($"Fixed {created} patient records");
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
                // Tìm tất cả người dùng với RoleId = 1 (Doctor)
                var doctorUsers = await _context.Users
                    .Where(u => u.RoleId == 1)
                    .ToListAsync();

                int created = 0;

                foreach (var user in doctorUsers)
                {
                    // Kiểm tra xem đã có bản ghi doctor chưa
                    var existingDoctor = await _context.Doctors
                        .FirstOrDefaultAsync(d => d.UserId == user.UserId);

                    if (existingDoctor == null)
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

                        _context.Doctors.Add(doctor);
                        created++;
                    }
                }

                if (created > 0)
                {
                    await _context.SaveChangesAsync();
                }

                return Ok($"Fixed {created} doctor records");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        private string GenerateJwtToken(User user, Role role = null)
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
                    expires: DateTime.Now.AddMinutes(expirationMinutes),
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