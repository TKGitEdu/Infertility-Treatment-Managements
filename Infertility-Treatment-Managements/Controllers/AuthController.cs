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
            // Validate credentials
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user == null || string.IsNullOrEmpty(user.Password))
            {
                // It's important not to reveal whether the username exists or not for security reasons.
                return Unauthorized("Invalid username or password");
            }

            bool passwordsMatch = false;
            bool needsPasswordUpdate = false;

            // Check if the stored password looks like a BCrypt hash
            // BCrypt hashes typically start with $2a$, $2b$, or $2y$, and are 60 characters long.
            bool isPotentiallyHashed = user.Password.Length == 60 &&
                                       (user.Password.StartsWith("$2a$") ||
                                        user.Password.StartsWith("$2b$") ||
                                        user.Password.StartsWith("$2y$"));

            if (isPotentiallyHashed)
            {
                try
                {
                    passwordsMatch = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password);
                }
                catch (BCrypt.Net.SaltParseException) // Or other BCrypt-specific exceptions if appropriate
                {
                    // This might indicate it's not a valid hash or a different kind of stored string.
                    // For this logic, we'll assume if Verify throws, it's not a match or not a valid hash we can work with.
                    passwordsMatch = false;
                }
            }
            else
            {
                // Assume plain-text password
                if (user.Password == loginDto.Password)
                {
                    passwordsMatch = true;
                    needsPasswordUpdate = true; // Mark for hashing and update
                }
            }

            if (!passwordsMatch)
            {
                return Unauthorized("Invalid username or password");
            }

            // If plain-text password matched, hash it and update the user record
            if (needsPasswordUpdate)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(loginDto.Password);
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            // Generate token
            var token = GenerateJwtToken(user);

            // Return token and user info using the existing DTO in the project
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
                Role = user.Role != null ? new RoleDTO
                {
                    RoleId = user.Role.RoleId,
                    RoleName = user.Role.RoleName
                } : null
            };

            // Add token to response
            Response.Headers.Add("Authorization", $"Bearer {token}");

            return userDto;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDTO>> Register(UserCreateDTO userCreateDto)
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

            // Hash the password before storing
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(userCreateDto.Password);

            // Create new user
            var user = new User
            {
                FullName = userCreateDto.FullName,
                Email = userCreateDto.Email,
                Phone = userCreateDto.Phone,
                Username = userCreateDto.Username,
                Password = hashedPassword,
                RoleId = userCreateDto.RoleId ?? 2, // Default to customer role if not specified
                Address = userCreateDto.Address,
                Gender = userCreateDto.Gender,
                DateOfBirth = userCreateDto.DateOfBirth
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Load the role
            await _context.Entry(user).Reference(u => u.Role).LoadAsync();

            // Generate token
            var token = GenerateJwtToken(user);

            // Return user DTO
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
                Role = user.Role != null ? new RoleDTO
                {
                    RoleId = user.Role.RoleId,
                    RoleName = user.Role.RoleName
                } : null
            };

            // Add token to response
            Response.Headers.Add("Authorization", $"Bearer {token}");

            return CreatedAtAction(nameof(Login), userDto);
        }

        [HttpGet("validate")]
        [Authorize]
        public async Task<ActionResult<UserDTO>> ValidateToken()
        {
            // Get user ID from claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Invalid token");
            }

            // Find user
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Doctor)
                .Include(u => u.Patients)
                .FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (user == null)
            {
                return Unauthorized("User not found");
            }

            // Return user as DTO
            return new UserDTO
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
                Role = user.Role != null ? new RoleDTO
                {
                    RoleId = user.Role.RoleId,
                    RoleName = user.Role.RoleName
                } : null,
                Doctor = user.Doctor != null ? new DoctorBasicDTO
                {
                    DoctorId = user.Doctor.DoctorId,
                    DoctorName = user.Doctor.DoctorName,
                    Specialization = user.Doctor.Specialization,
                    Email = user.Doctor.Email,
                    Phone = user.Doctor.Phone
                } : null,
                Patients = user.Patients?.Select(p => new PatientBasicDTO
                {
                    PatientId = p.PatientId,
                    Name = p.Name,
                    Email = p.Email,
                    Phone = p.Phone,
                    Gender = p.Gender,
                    DateOfBirth = p.DateOfBirth.HasValue ? DateOnly.FromDateTime(p.DateOfBirth.Value) : null
                }).ToList() ?? new List<PatientBasicDTO>()
            };
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] UserForgotPasswordDTO forgotPasswordDTO)
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
            
            // Lưu token và thời gian hết hạn
            user.ResetPasswordToken = resetToken;
            user.ResetPasswordExpiry = DateTime.UtcNow.AddHours(1); // Token có hiệu lực 1 giờ
            
            await _context.SaveChangesAsync();
            
            // Lấy base URL từ request
            string baseUrl = $"{Request.Scheme}://{Request.Host}";
            
            // Đối với frontend SPA, URL reset password sẽ trỏ đến route của frontend
            // Ví dụ: http://localhost:3000/reset-password?token=abc123
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
            
            // Cập nhật mật khẩu và xóa token
            user.Password = BCrypt.Net.BCrypt.HashPassword(resetPasswordDTO.NewPassword);
            user.ResetPasswordToken = null;
            user.ResetPasswordExpiry = null;
            
            await _context.SaveChangesAsync();
            
            return Ok("Mật khẩu đã được đặt lại thành công");
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add role claim if user has a role
            if (user.Role != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role.RoleName));
            }

            // Lấy thời gian tồn tại token từ cấu hình
            var expirationMinutes = Convert.ToDouble(jwtSettings["AccessTokenExpirationMinutes"] ?? "60");

            // Tạo token với thời gian tồn tại từ cấu hình
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(expirationMinutes),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(secretKey),
                    SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}