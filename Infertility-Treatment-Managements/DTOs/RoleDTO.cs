using System;
using System.Collections.Generic;

namespace Infertility_Treatment_Managements.DTOs
{
    public class RoleDTO
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }

    public class RoleCreateDTO
    {
        public string RoleName { get; set; }
    }

    public class RoleUpdateDTO
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
}