// Tạo file SeedData.cs trong thư mục Data hoặc Helpers
using Infertility_Treatment_Managements.Models;
using Microsoft.EntityFrameworkCore;

namespace Infertility_Treatment_Managements.Helpers
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = new InfertilityTreatmentManagementContext(
                serviceProvider.GetRequiredService<DbContextOptions<InfertilityTreatmentManagementContext>>());

            // Thêm roles
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { RoleId = "ROLE_1", RoleName = "Admin" },
                    new Role { RoleId = "ROLE_2", RoleName = "Doctor" },
                    new Role { RoleId = "ROLE_3", RoleName = "Patient" }
                );
                context.SaveChanges();
            }

            // Thêm tài khoản admin
            if (!context.Users.Any(u => u.Username == "admin"))
            {
                var adminUser = new User
                {
                    UserId = "USR_1",
                    Username = "admin",
                    Password = "admin123", // Trong thực tế nên mã hóa mật khẩu
                    FullName = "Administrator",
                    Email = "admin@example.com",
                    RoleId = "ROLE_1" // Admin role
                };
                context.Users.Add(adminUser);
                context.SaveChanges();
            }

            // Thêm các dữ liệu mẫu khác nếu cần
        }
    }
}
