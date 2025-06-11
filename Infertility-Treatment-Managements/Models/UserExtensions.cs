using System;

namespace Infertility_Treatment_Managements.Models
{
    public partial class User
    {
        public string ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordExpiry { get; set; }
    }
}