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
                        Description = "First consultation with fertility specialist",
                        Category = "Consultation", // Thêm Category
                        Status = "Active"
                    },
                    new Service
                    {
                        ServiceId = "SRV_2",
                        Name = "IVF Treatment",
                        Description = "In Vitro Fertilization treatment cycle",
                        Category = "InfertilityTreatment", // Thêm Category
                        Status = "Active"
                    },
                    new Service
                    {
                        ServiceId = "SRV_3",
                        Name = "IUI Treatment",
                        Description = "Intrauterine Insemination treatment",
                        Category = "InfertilityTreatment", // Thêm Category
                        Status = "Active"
                    }
                );
                context.SaveChanges();
            }

            // Thêm slots
            // Thêm slots (24 khung giờ trong ngày)
            if (!context.Slots.Any())
            {
                var slots = new List<Slot>();

                // Khung giờ sáng sớm (00:00 - 05:59)
                slots.Add(new Slot { SlotId = "SLOT_01", SlotName = "Early Morning 00:00-01:00", StartTime = "00:00", EndTime = "01:00" });
                slots.Add(new Slot { SlotId = "SLOT_02", SlotName = "Early Morning 01:00-02:00", StartTime = "01:00", EndTime = "02:00" });
                slots.Add(new Slot { SlotId = "SLOT_03", SlotName = "Early Morning 02:00-03:00", StartTime = "02:00", EndTime = "03:00" });
                slots.Add(new Slot { SlotId = "SLOT_04", SlotName = "Early Morning 03:00-04:00", StartTime = "03:00", EndTime = "04:00" });
                slots.Add(new Slot { SlotId = "SLOT_05", SlotName = "Early Morning 04:00-05:00", StartTime = "04:00", EndTime = "05:00" });
                slots.Add(new Slot { SlotId = "SLOT_06", SlotName = "Early Morning 05:00-06:00", StartTime = "05:00", EndTime = "06:00" });

                // Khung giờ sáng (06:00 - 11:59)
                slots.Add(new Slot { SlotId = "SLOT_07", SlotName = "Morning 06:00-07:00", StartTime = "06:00", EndTime = "07:00" });
                slots.Add(new Slot { SlotId = "SLOT_08", SlotName = "Morning 07:00-08:00", StartTime = "07:00", EndTime = "08:00" });
                slots.Add(new Slot { SlotId = "SLOT_09", SlotName = "Morning 08:00-09:00", StartTime = "08:00", EndTime = "09:00" });
                slots.Add(new Slot { SlotId = "SLOT_10", SlotName = "Morning 09:00-10:00", StartTime = "09:00", EndTime = "10:00" });
                slots.Add(new Slot { SlotId = "SLOT_11", SlotName = "Morning 10:00-11:00", StartTime = "10:00", EndTime = "11:00" });
                slots.Add(new Slot { SlotId = "SLOT_12", SlotName = "Morning 11:00-12:00", StartTime = "11:00", EndTime = "12:00" });

                // Khung giờ chiều (12:00 - 17:59)
                slots.Add(new Slot { SlotId = "SLOT_13", SlotName = "Afternoon 12:00-13:00", StartTime = "12:00", EndTime = "13:00" });
                slots.Add(new Slot { SlotId = "SLOT_14", SlotName = "Afternoon 13:00-14:00", StartTime = "13:00", EndTime = "14:00" });
                slots.Add(new Slot { SlotId = "SLOT_15", SlotName = "Afternoon 14:00-15:00", StartTime = "14:00", EndTime = "15:00" });
                slots.Add(new Slot { SlotId = "SLOT_16", SlotName = "Afternoon 15:00-16:00", StartTime = "15:00", EndTime = "16:00" });
                slots.Add(new Slot { SlotId = "SLOT_17", SlotName = "Afternoon 16:00-17:00", StartTime = "16:00", EndTime = "17:00" });
                slots.Add(new Slot { SlotId = "SLOT_18", SlotName = "Afternoon 17:00-18:00", StartTime = "17:00", EndTime = "18:00" });

                // Khung giờ tối (18:00 - 23:59)
                slots.Add(new Slot { SlotId = "SLOT_19", SlotName = "Evening 18:00-19:00", StartTime = "18:00", EndTime = "19:00" });
                slots.Add(new Slot { SlotId = "SLOT_20", SlotName = "Evening 19:00-20:00", StartTime = "19:00", EndTime = "20:00" });
                slots.Add(new Slot { SlotId = "SLOT_21", SlotName = "Evening 20:00-21:00", StartTime = "20:00", EndTime = "21:00" });
                slots.Add(new Slot { SlotId = "SLOT_22", SlotName = "Evening 21:00-22:00", StartTime = "21:00", EndTime = "22:00" });
                slots.Add(new Slot { SlotId = "SLOT_23", SlotName = "Evening 22:00-23:00", StartTime = "22:00", EndTime = "23:00" });
                slots.Add(new Slot { SlotId = "SLOT_24", SlotName = "Evening 23:00-00:00", StartTime = "23:00", EndTime = "00:00" });

                context.Slots.AddRange(slots);
                context.SaveChanges();
            }
        }
    }
}