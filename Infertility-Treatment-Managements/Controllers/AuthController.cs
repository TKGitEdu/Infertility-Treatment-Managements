using Infertility_Treatment_Management.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repositories.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace Infertility_Treatment_Management.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(InfertilityTreatmentManagementContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDTO>> Login(DTOs.UserLoginDTO loginDto)
        {
            // Validate credentials
            var user = await _context.User
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
                    passwordsMatch = BCrypt.BCrypt.Verify(loginDto.Password, user.Password);
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
                user.Password = BCrypt.BCrypt.HashPassword(loginDto.Password);
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            // Generate token
            var token = GenerateJwtToken(user);

            // Return token and user info
            return new UserDTO
            {
                Token = token,
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role?.RoleName
            };
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

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(secretKey),
                    SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
