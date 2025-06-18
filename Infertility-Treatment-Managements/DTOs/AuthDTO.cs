using System;

namespace Infertility_Treatment_Managements.DTOs
{
    public class UserLoginDTO
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    
    public class AuthResponseDTO
    {
        public string Token { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; } // Just the role name as string
    }
}
