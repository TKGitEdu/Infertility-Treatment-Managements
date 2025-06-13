namespace Infertility_Treatment_Managements.Models
{
    public partial class User
    {
        public string UserId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? RoleId { get; set; }
        public string? Address { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordExpiry { get; set; }
        public virtual Doctor Doctor { get; set; }
        public virtual ICollection<Patient> Patients { get; set; } = new List<Patient>();
        public virtual Role Role { get; set; }
    }
}