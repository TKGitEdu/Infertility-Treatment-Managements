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

            // Thêm bác sĩ mẫu
            if (!context.Doctors.Any())
            {
                // Tạo tài khoản user cho bác sĩ
                var doctorUser = new User
                {
                    UserId = "USR_DOC1",
                    Username = "doctor1",
                    Password = "Doctor@123", // Trong thực tế nên mã hóa mật khẩu
                    FullName = "Dr. John Smith",
                    Email = "john.smith@example.com",
                    Phone = "0901234567",
                    Address = "123 Medical Center, City",
                    Gender = "Male",
                    RoleId = "ROLE_2" // Doctor role
                };

                if (!context.Users.Any(u => u.UserId == doctorUser.UserId))
                {
                    context.Users.Add(doctorUser);
                    context.SaveChanges();
                }

                // Tạo thông tin bác sĩ
                var doctor = new Doctor
                {
                    DoctorId = "DOC_1",
                    UserId = doctorUser.UserId,
                    DoctorName = doctorUser.FullName,
                    Specialization = "Infertility Specialist",
                    Phone = doctorUser.Phone,
                    Email = doctorUser.Email
                };

                context.Doctors.Add(doctor);
                context.SaveChanges();
            }

            // Thêm bệnh nhân mẫu
            if (!context.Patients.Any())
            {
                // Tạo tài khoản user cho bệnh nhân
                var patientUser = new User
                {
                    UserId = "USR_PAT1",
                    Username = "patient1",
                    Password = "Patient@123", // Trong thực tế nên mã hóa mật khẩu
                    FullName = "Jane Doe",
                    Email = "jane.doe@example.com",
                    Phone = "0909876543",
                    Address = "456 Residential St, City",
                    Gender = "Female",
                    DateOfBirth = new DateTime(1985, 5, 15),
                    RoleId = "ROLE_3" // Patient role
                };

                if (!context.Users.Any(u => u.UserId == patientUser.UserId))
                {
                    context.Users.Add(patientUser);
                    context.SaveChanges();
                }

                // Tạo thông tin bệnh nhân
                var patient = new Patient
                {
                    PatientId = "PAT_1",
                    UserId = patientUser.UserId,
                    Name = patientUser.FullName,
                    Email = patientUser.Email,
                    Phone = patientUser.Phone,
                    Address = patientUser.Address,
                    Gender = patientUser.Gender,
                    DateOfBirth = patientUser.DateOfBirth,
                    BloodType = "A+",
                    EmergencyPhoneNumber = "0901122334"
                };

                context.Patients.Add(patient);

                // Tạo thông tin chi tiết bệnh nhân
                var patientDetail = new PatientDetail
                {
                    PatientDetailId = "PATD_1",
                    PatientId = patient.PatientId,
                    TreatmentStatus = "New"
                };

                context.PatientDetails.Add(patientDetail);
                context.SaveChanges();
            }

            // Thêm dịch vụ
            if (!context.Services.Any())
            {
                context.Services.AddRange(
                    new Service
                    {
                        ServiceId = "SRV_1",
                        Name = "Initial Consultation",
                        Description = "First consultation with fertility specialist"
                    },
                    new Service
                    {
                        ServiceId = "SRV_2",
                        Name = "IVF Treatment",
                        Description = "In Vitro Fertilization treatment cycle"
                    }
                );
                context.SaveChanges();
            }

            // Thêm slots
            if (!context.Slots.Any())
            {
                context.Slots.AddRange(
                    new Slot
                    {
                        SlotId = "SLOT_1",
                        SlotName = "Morning 9:00-10:00"
                    },
                    new Slot
                    {
                        SlotId = "SLOT_2",
                        SlotName = "Morning 10:00-11:00"
                    },
                    new Slot
                    {
                        SlotId = "SLOT_3",
                        SlotName = "Afternoon 14:00-15:00"
                    }
                );
                context.SaveChanges();
            }
        }
    }
}