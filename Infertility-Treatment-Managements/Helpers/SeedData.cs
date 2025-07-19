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

            // Đảm bảo database tồn tại
            try
            {
                context.Database.CanConnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể kết nối tới database: {ex.Message}");
                return;
            }

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
                    Password = "admin123",
                    FullName = "Administrator",
                    Email = "admin@example.com",
                    RoleId = "ROLE_1"
                };
                context.Users.Add(adminUser);
                context.SaveChanges();
            }

            // Thêm slots
            if (!context.Slots.Any())
            {
                var slots = new List<Slot>
                {
                    new Slot { SlotId = "SLOT_01", SlotName = "Sáng sớm 06:00-07:00", StartTime = "06:00", EndTime = "07:00" },
                    new Slot { SlotId = "SLOT_02", SlotName = "Sáng sớm 07:00-08:00", StartTime = "07:00", EndTime = "08:00" },
                    new Slot { SlotId = "SLOT_03", SlotName = "Sáng 08:00-09:00", StartTime = "08:00", EndTime = "09:00" },
                    new Slot { SlotId = "SLOT_04", SlotName = "Sáng 09:00-10:00", StartTime = "09:00", EndTime = "10:00" },
                    new Slot { SlotId = "SLOT_05", SlotName = "Sáng 10:00-11:00", StartTime = "10:00", EndTime = "11:00" },
                    new Slot { SlotId = "SLOT_06", SlotName = "Sáng 11:00-12:00", StartTime = "11:00", EndTime = "12:00" },
                    new Slot { SlotId = "SLOT_07", SlotName = "Trưa 12:00-13:00", StartTime = "12:00", EndTime = "13:00" },
                    new Slot { SlotId = "SLOT_08", SlotName = "Trưa 13:00-14:00", StartTime = "13:00", EndTime = "14:00" },
                    new Slot { SlotId = "SLOT_09", SlotName = "Chiều 14:00-15:00", StartTime = "14:00", EndTime = "15:00" },
                    new Slot { SlotId = "SLOT_10", SlotName = "Chiều 15:00-16:00", StartTime = "15:00", EndTime = "16:00" },
                    new Slot { SlotId = "SLOT_11", SlotName = "Chiều 16:00-17:00", StartTime = "16:00", EndTime = "17:00" },
                    new Slot { SlotId = "SLOT_12", SlotName = "Chiều 17:00-18:00", StartTime = "17:00", EndTime = "18:00" },
                    new Slot { SlotId = "SLOT_13", SlotName = "Tối 18:00-19:00", StartTime = "18:00", EndTime = "19:00" },
                    new Slot { SlotId = "SLOT_14", SlotName = "Tối 19:00-20:00", StartTime = "19:00", EndTime = "20:00" },
                    new Slot { SlotId = "SLOT_15", SlotName = "Tối 20:00-21:00", StartTime = "20:00", EndTime = "21:00" }
                };
                context.Slots.AddRange(slots);
                context.SaveChanges();
            }

            // Thêm dịch vụ
            if (!context.Services.Any())
            {
                var services = new List<Service>
                {
                    new Service
                    {
                        ServiceId = "SRV_1",
                        Name = "Tư vấn ban đầu",
                        Description = "Buổi tư vấn đầu tiên với chuyên gia điều trị hiếm muộn",
                        Category = "Consultation",
                        Price = 1000M,
                        Status = "Active"
                    },
                    new Service
                    {
                        ServiceId = "SRV_2",
                        Name = "Tư vấn chuyên sâu",
                        Description = "Tư vấn chuyên sâu về các phương pháp điều trị hiếm muộn",
                        Category = "Consultation",
                        Price = 1000M,
                        Status = "Active"
                    },
                    new Service
                    {
                        ServiceId = "SRV_3",
                        Name = "Điều trị IVF (Thụ tinh trong ống nghiệm)",
                        Description = "Một chu kỳ điều trị thụ tinh trong ống nghiệm (IVF) bao gồm kích trứng, lấy trứng, thụ tinh và chuyển phôi",
                        Category = "InfertilityTreatment",
                        Price = 1000M,
                        Status = "Active"
                    },
                    new Service
                    {
                        ServiceId = "SRV_4",
                        Name = "Điều trị IUI (Bơm tinh trùng vào buồng tử cung)",
                        Description = "Phương pháp điều trị bằng cách bơm tinh trùng đã được xử lý vào buồng tử cung",
                        Category = "InfertilityTreatment",
                        Price = 1000M,
                        Status = "Active"
                    },
                    new Service
                    {
                        ServiceId = "SRV_5",
                        Name = "Trữ đông trứng",
                        Description = "Dịch vụ trữ đông trứng để sử dụng trong tương lai",
                        Category = "InfertilityTreatment",
                        Price = 1000M,
                        Status = "Active"
                    },
                    new Service
                    {
                        ServiceId = "SRV_6",
                        Name = "Trữ đông tinh trùng",
                        Description = "Dịch vụ trữ đông tinh trùng để sử dụng trong tương lai",
                        Category = "InfertilityTreatment",
                        Price = 1000M,
                        Status = "Active"
                    },
                    new Service
                    {
                        ServiceId = "SRV_7",
                        Name = "Trữ đông phôi",
                        Description = "Dịch vụ trữ đông phôi sau quá trình thụ tinh trong ống nghiệm",
                        Category = "InfertilityTreatment",
                        Price = 1000M,
                        Status = "Active"
                    },
                    new Service
                    {
                        ServiceId = "SRV_8",
                        Name = "ICSI (Tiêm tinh trùng vào bào tương trứng)",
                        Description = "Phương pháp tiêm một tinh trùng trực tiếp vào trứng để thụ tinh",
                        Category = "InfertilityTreatment",
                        Price = 1000M,
                        Status = "Active"
                    },
                    new Service
                    {
                        ServiceId = "SRV_9",
                        Name = "Xét nghiệm hormone",
                        Description = "Xét nghiệm các hormone liên quan đến sinh sản",
                        Category = "Testing",
                        Price = 1000M,
                        Status = "Active"
                    },
                    new Service
                    {
                        ServiceId = "SRV_10",
                        Name = "Siêu âm theo dõi nang trứng",
                        Description = "Siêu âm để theo dõi sự phát triển của nang trứng trong quá trình kích trứng",
                        Category = "Testing",
                        Price = 1000M,
                        Status = "Active"
                    },
                    new Service
                    {
                        ServiceId = "SRV_FET",
                        Name = "Chuyển phôi đông lạnh (FET)",
                        Description = "Quy trình rã đông và chuyển phôi đã trữ đông vào tử cung của bệnh nhân",
                        Category = "InfertilityTreatment",
                        Price = 1000M,
                        Status = "Active"
                    }
                };
                context.Services.AddRange(services);
                context.SaveChanges();
            }

            // Thêm bác sĩ mẫu
            if (!context.Doctors.Any())
            {
                var doctorUsers = new List<(User User, Doctor Doctor)>
                {
                    (
                        new User
                        {
                            UserId = "USR_DOC1",
                            Username = "doctor1",
                            Password = "Doctor@123",
                            FullName = "Nguyễn Văn An",
                            Email = "nguyenvanan@example.com",
                            Phone = "0901234567",
                            Address = "123 Lê Lợi, Q.1, TP.HCM",
                            Gender = "Nam",
                            DateOfBirth = DateTime.SpecifyKind(new DateTime(1980, 5, 15), DateTimeKind.Utc),
                            RoleId = "ROLE_2"
                        },
                        new Doctor
                        {
                            DoctorId = "DOC_1",
                            UserId = "USR_DOC1",
                            DoctorName = "Nguyễn Văn An",
                            Specialization = "Chuyên gia điều trị hiếm muộn",
                            Phone = "0901234567",
                            Email = "nguyenvanan@example.com"
                        }
                    ),
                    (
                        new User
                        {
                            UserId = "USR_DOC2",
                            Username = "doctor2",
                            Password = "Doctor@123",
                            FullName = "Trần Thị Bình",
                            Email = "tranthibinh@example.com",
                            Phone = "0912345678",
                            Address = "456 Nguyễn Huệ, Q.1, TP.HCM",
                            Gender = "Nữ",
                            DateOfBirth = DateTime.SpecifyKind(new DateTime(1982, 8, 20), DateTimeKind.Utc),
                            RoleId = "ROLE_2"
                        },
                        new Doctor
                        {
                            DoctorId = "DOC_2",
                            UserId = "USR_DOC2",
                            DoctorName = "Trần Thị Bình",
                            Specialization = "Chuyên gia IVF",
                            Phone = "0912345678",
                            Email = "tranthibinh@example.com"
                        }
                    ),
                    (
                        new User
                        {
                            UserId = "USR_DOC3",
                            Username = "doctor3",
                            Password = "Doctor@123",
                            FullName = "Lê Văn Cường",
                            Email = "levancuong@example.com",
                            Phone = "0923456789",
                            Address = "789 Võ Văn Tần, Q.3, TP.HCM",
                            Gender = "Nam",
                            DateOfBirth = DateTime.SpecifyKind(new DateTime(1975, 3, 10), DateTimeKind.Utc),
                            RoleId = "ROLE_2"
                        },
                        new Doctor
                        {
                            DoctorId = "DOC_3",
                            UserId = "USR_DOC3",
                            DoctorName = "Lê Văn Cường",
                            Specialization = "Chuyên gia IUI",
                            Phone = "0923456789",
                            Email = "levancuong@example.com"
                        }
                    )
                };

                foreach (var doctorPair in doctorUsers)
                {
                    if (!context.Users.Any(u => u.UserId == doctorPair.User.UserId))
                    {
                        context.Users.Add(doctorPair.User);
                        context.SaveChanges();
                        context.Doctors.Add(doctorPair.Doctor);
                        context.SaveChanges();
                    }
                }
            }

            // Thêm bệnh nhân mẫu
            if (!context.Patients.Any())
            {
                var patientUsers = new List<(User User, Patient Patient, PatientDetail PatientDetail)>
                {
                    (
                        new User
                        {
                            UserId = "USR_PAT1",
                            Username = "patient1",
                            Password = "Patient@123",
                            FullName = "Phạm Thị Dung",
                            Email = "phamthidung@example.com",
                            Phone = "0934567890",
                            Address = "123 Lý Tự Trọng, Q.1, TP.HCM",
                            Gender = "Nữ",
                            DateOfBirth = DateTime.SpecifyKind(new DateTime(1990, 5, 15), DateTimeKind.Utc),
                            RoleId = "ROLE_3"
                        },
                        new Patient
                        {
                            PatientId = "PAT_1",
                            UserId = "USR_PAT1",
                            Name = "Phạm Thị Dung",
                            Email = "phamthidung@example.com",
                            Phone = "0934567890",
                            Address = "123 Lý Tự Trọng, Q.1, TP.HCM",
                            Gender = "Nữ",
                            DateOfBirth = DateTime.SpecifyKind(new DateTime(1990, 5, 15), DateTimeKind.Utc),
                            BloodType = "A+",
                            EmergencyPhoneNumber = "0945678901"
                        },
                        new PatientDetail
                        {
                            PatientDetailId = "PATD_1",
                            PatientId = "PAT_1",
                            Name = "Phạm Thị Dung",
                            TreatmentStatus = "Đang điều trị"
                        }
                    ),
                    (
                        new User
                        {
                            UserId = "USR_PAT2",
                            Username = "patient2",
                            Password = "Patient@123",
                            FullName = "Nguyễn Thị Em",
                            Email = "nguyenthiem@example.com",
                            Phone = "0945678901",
                            Address = "456 Điện Biên Phủ, Q.3, TP.HCM",
                            Gender = "Nữ",
                            DateOfBirth = DateTime.SpecifyKind(new DateTime(1988, 7, 25), DateTimeKind.Utc),
                            RoleId = "ROLE_3"
                        },
                        new Patient
                        {
                            PatientId = "PAT_2",
                            UserId = "USR_PAT2",
                            Name = "Nguyễn Thị Em",
                            Email = "nguyenthiem@example.com",
                            Phone = "0945678901",
                            Address = "456 Điện Biên Phủ, Q.3, TP.HCM",
                            Gender = "Nữ",
                            DateOfBirth = DateTime.SpecifyKind(new DateTime(1988, 7, 25), DateTimeKind.Utc),
                            BloodType = "B+",
                            EmergencyPhoneNumber = "0956789012"
                        },
                        new PatientDetail
                        {
                            PatientDetailId = "PATD_2",
                            PatientId = "PAT_2",
                            Name = "Nguyễn Thị Em",
                            TreatmentStatus = "Mới đăng ký"
                        }
                    ),
                    (
                        new User
                        {
                            UserId = "USR_PAT3",
                            Username = "patient3",
                            Password = "Patient@123",
                            FullName = "Trần Văn Phong",
                            Email = "tranvanphong@example.com",
                            Phone = "0956789012",
                            Address = "789 Cách Mạng Tháng 8, Q.10, TP.HCM",
                            Gender = "Nam",
                            DateOfBirth = DateTime.SpecifyKind(new DateTime(1985, 11, 10), DateTimeKind.Utc),
                            RoleId = "ROLE_3"
                        },
                        new Patient
                        {
                            PatientId = "PAT_3",
                            UserId = "USR_PAT3",
                            Name = "Trần Văn Phong",
                            Email = "tranvanphong@example.com",
                            Phone = "0956789012",
                            Address = "789 Cách Mạng Tháng 8, Q.10, TP.HCM",
                            Gender = "Nam",
                            DateOfBirth = DateTime.SpecifyKind(new DateTime(1985, 11, 10), DateTimeKind.Utc),
                            BloodType = "O+",
                            EmergencyPhoneNumber = "0967890123"
                        },
                        new PatientDetail
                        {
                            PatientDetailId = "PATD_3",
                            PatientId = "PAT_3",
                            Name = "Trần Văn Phong",
                            TreatmentStatus = "Đang điều trị"
                        }
                    )
                };

                foreach (var patientPair in patientUsers)
                {
                    if (!context.Users.Any(u => u.UserId == patientPair.User.UserId))
                    {
                        context.Users.Add(patientPair.User);
                        context.SaveChanges();
                        context.Patients.Add(patientPair.Patient);
                        context.SaveChanges();
                        context.PatientDetails.Add(patientPair.PatientDetail);
                        context.SaveChanges();
                    }
                }
            }

            // Thêm đăng ký dịch vụ hiếm muộn mẫu
            if (!context.Bookings.Any() && context.Doctors.Any() && context.Patients.Any() && context.Services.Any() && context.Slots.Any())
            {
                try
                {
                    var bookings = new List<Booking>
                    {
                        new Booking
                        {
                            BookingId = "BKG_1",
                            PatientId = "PAT_1",
                            DoctorId = "DOC_1",
                            ServiceId = "SRV_3",
                            SlotId = "SLOT_04",
                            DateBooking = DateTime.UtcNow.AddDays(1),
                            Description = "Đăng ký điều trị IVF",
                            CreateAt = DateTime.UtcNow.AddDays(-2),
                            Status = "pending",
                            Note = "Đã xác nhận qua điện thoại"
                        },
                        new Booking
                        {
                            BookingId = "BKG_2",
                            PatientId = "PAT_2",
                            DoctorId = "DOC_2",
                            ServiceId = "SRV_4",
                            SlotId = "SLOT_10",
                            DateBooking = DateTime.UtcNow.AddDays(3),
                            Description = "Đăng ký điều trị IUI",
                            CreateAt = DateTime.UtcNow.AddDays(-1),
                            Status = "pending",
                            Note = "Lần đầu tiên thực hiện IUI"
                        },
                        new Booking
                        {
                            BookingId = "BKG_3",
                            PatientId = "PAT_3",
                            DoctorId = "DOC_3",
                            ServiceId = "SRV_6",
                            SlotId = "SLOT_06",
                            DateBooking = DateTime.UtcNow.AddDays(2),
                            Description = "Tư vấn về điều trị hiếm muộn",
                            CreateAt = DateTime.UtcNow.AddDays(-3),
                            Status = "pending",
                            Note = "Cần tư vấn các phương pháp điều trị phù hợp"
                        },
                        new Booking
                        {
                            BookingId = "BKG_4",
                            PatientId = "PAT_2",
                            DoctorId = "DOC_2",
                            ServiceId = "SRV_FET",
                            SlotId = "SLOT_07",
                            DateBooking = DateTime.UtcNow.AddDays(2),
                            Description = "Trữ đông tinh trùng",
                            CreateAt = DateTime.UtcNow.AddDays(-3),
                            Status = "pending",
                            Note = "Cần tư vấn các phương pháp điều trị phù hợp"
                        }
                    };

                    context.Bookings.AddRange(bookings);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi thêm bookings: {ex.Message}");
                }
            }

            // Thêm kế hoạch điều trị mẫu
            if (!context.TreatmentPlans.Any() && context.Doctors.Any() && context.PatientDetails.Any() && context.Services.Any())
            {
                try
                {
                    var treatmentPlans = new List<TreatmentPlan>
                    {
                        new TreatmentPlan
                        {
                            TreatmentPlanId = "TP_1",
                            DoctorId = "DOC_1",
                            PatientDetailId = "PATD_1",
                            ServiceId = "SRV_3",
                            Method = "IVF",
                            StartDate = DateTime.UtcNow.AddDays(5),
                            EndDate = DateTime.UtcNow.AddDays(35),
                            Status = "Thụ tinh trong ống nghiệm",
                            TreatmentDescription = "Khám tổng quát; Kích thích buồng trứng; Chọc hút trứng; Thụ tinh trong ống nghiệm; Nuôi phôi; Chuyển phôi vào tử cung",
                            Giaidoan = "in-progress",
                            GhiChu = "Bác sĩ thêm ghi chú cho kê hoạch điều trị ở đây"
                        },
                        new TreatmentPlan
                        {
                            TreatmentPlanId = "TP_2",
                            DoctorId = "DOC_2",
                            PatientDetailId = "PATD_2",
                            ServiceId = "SRV_4",
                            Method = "IUI",
                            StartDate = DateTime.UtcNow.AddDays(7),
                            EndDate = DateTime.UtcNow.AddDays(21),
                            Status = "Kích thích nhẹ buồng trứng",
                            TreatmentDescription = "Khám tổng quát; Kích thích nhẹ buồng trứng; Theo dõi nang noãn; Lọc rửa tinh trùng; Bơm tinh trùng vào buồng tử cung",
                            Giaidoan = "in-progress",
                            GhiChu = "Bác sĩ thêm ghi chú cho kê hoạch điều trị ở đây"
                        },
                        new TreatmentPlan
                        {
                            TreatmentPlanId = "TP_3",
                            DoctorId = "DOC_3",
                            PatientDetailId = "PATD_3",
                            ServiceId = "SRV_6",
                            Method = "Trữ đông tinh trùng",
                            StartDate = DateTime.UtcNow.AddDays(3),
                            EndDate = DateTime.UtcNow.AddDays(4),
                            Status = "Lấy mẫu tinh dịch",
                            TreatmentDescription = "Lấy mẫu tinh dịch; Đánh giá chất lượng; Tiến hành trữ đông; Lưu trữ mẫu tại ngân hàng tinh trùng",
                            Giaidoan = "in-progress",
                            GhiChu = "Bác sĩ thêm ghi chú cho kê hoạch điều trị ở đây"
                        },
                        new TreatmentPlan
                        {
                            TreatmentPlanId = "TP_FET_1",
                            DoctorId = "DOC_2",
                            PatientDetailId = "PATD_2",
                            ServiceId = "SRV_FET",
                            Method = "FET",
                            StartDate = DateTime.UtcNow.AddDays(5),
                            EndDate = DateTime.UtcNow.AddDays(10),
                            Status = "Chuẩn bị nội mạc tử cung",
                            TreatmentDescription = "Chuẩn bị nội mạc tử cung; Rã đông phôi; Chuyển phôi vào tử cung",
                            Giaidoan = "completed",
                            GhiChu = "Bác sĩ thêm ghi chú cho kê hoạch điều trị ở đây"
                        }
                    };

                    context.TreatmentPlans.AddRange(treatmentPlans);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi thêm treatment plans: {ex.Message}");
                }
            }

            // Thêm TreatmentSteps
            if (!context.TreatmentSteps.Any() && context.TreatmentPlans.Any())
            {
                try
                {
                    var plans = context.TreatmentPlans.ToList();
                    var treatmentSteps = new List<TreatmentStep>();

                    foreach (var plan in plans)
                    {
                        var steps = plan.TreatmentDescription.Split(';');
                        for (int i = 0; i < steps.Length; i++)
                        {
                            var step = new TreatmentStep
                            {
                                TreatmentStepId = "TS_" + Guid.NewGuid().ToString().Substring(0, 8),
                                TreatmentPlanId = plan.TreatmentPlanId,
                                StepOrder = i + 1,
                                StepName = steps[i].Trim(),
                                Description = "placeholder(500)"
                            };
                            treatmentSteps.Add(step);
                        }
                    }

                    context.TreatmentSteps.AddRange(treatmentSteps);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi thêm treatment steps: {ex.Message}");
                }
            }

            // Thêm TreatmentMedications
            if (!context.TreatmentMedications.Any() && context.TreatmentPlans.Any())
            {
                try
                {
                    var medications = new List<TreatmentMedication>
                    {
                        new TreatmentMedication
                        {
                            MedicationId = "M_" + Guid.NewGuid().ToString().Substring(0, 8),
                            TreatmentPlanId = "TP_1",
                            DrugType = "Kích thích buồng trứng",
                            DrugName = "Gonal-F",
                            Description = "placeholder(500)"
                        },
                        new TreatmentMedication
                        {
                            MedicationId = "M_" + Guid.NewGuid().ToString().Substring(0, 8),
                            TreatmentPlanId = "TP_2",
                            DrugType = "Hỗ trợ rụng trứng",
                            DrugName = "Clomid",
                            Description = "placeholder(500)"
                        },
                        new TreatmentMedication
                        {
                            MedicationId = "M_" + Guid.NewGuid().ToString().Substring(0, 8),
                            TreatmentPlanId = "TP_3",
                            DrugType = "Kháng sinh",
                            DrugName = "Doxycycline",
                            Description = "placeholder(500)"
                        },
                        new TreatmentMedication
                        {
                            MedicationId = "M_" + Guid.NewGuid().ToString().Substring(0, 8),
                            TreatmentPlanId = "TP_FET_1",
                            DrugType = "Hỗ trợ nội mạc",
                            DrugName = "Estrogen",
                            Description = "placeholder(500)"
                        }
                    };

                    context.TreatmentMedications.AddRange(medications);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi thêm medications: {ex.Message}");
                }
            }

            // Thêm quy trình điều trị
            if (!context.TreatmentProcesses.Any() && context.TreatmentPlans.Any() && context.PatientDetails.Any() && context.Doctors.Any())
            {
                try
                {
                    var treatmentProcesses = new List<TreatmentProcess>
                    {
                        new TreatmentProcess
                        {
                            TreatmentProcessId = "TPR_1",
                            PatientDetailId = "PATD_1",
                            TreatmentPlanId = "TP_1",
                            DoctorId = "DOC_1",
                            ScheduledDate = DateTime.UtcNow.AddDays(5),
                            Status = "Đã lên lịch",
                            Result = "Chưa thực hiện"
                        },
                        new TreatmentProcess
                        {
                            TreatmentProcessId = "TPR_2",
                            PatientDetailId = "PATD_1",
                            TreatmentPlanId = "TP_1",
                            DoctorId = "DOC_1",
                            ScheduledDate = DateTime.UtcNow.AddDays(15),
                            Status = "Đã lên lịch",
                            Result = "Chưa thực hiện"
                        },
                        new TreatmentProcess
                        {
                            TreatmentProcessId = "TPR_3",
                            PatientDetailId = "PATD_1",
                            TreatmentPlanId = "TP_1",
                            DoctorId = "DOC_1",
                            ScheduledDate = DateTime.UtcNow.AddDays(25),
                            Status = "Đã lên lịch",
                            Result = "Chưa thực hiện"
                        },
                        new TreatmentProcess
                        {
                            TreatmentProcessId = "TPR_4",
                            PatientDetailId = "PATD_2",
                            TreatmentPlanId = "TP_2",
                            DoctorId = "DOC_2",
                            ScheduledDate = DateTime.UtcNow.AddDays(7),
                            Status = "Chờ xác nhận",
                            Result = "Chưa thực hiện"
                        },
                        new TreatmentProcess
                        {
                            TreatmentProcessId = "TPR_5",
                            PatientDetailId = "PATD_2",
                            TreatmentPlanId = "TP_2",
                            DoctorId = "DOC_2",
                            ScheduledDate = DateTime.UtcNow.AddDays(14),
                            Status = "Chờ xác nhận",
                            Result = "Chưa thực hiện"
                        },
                        new TreatmentProcess
                        {
                            TreatmentProcessId = "TPR_6",
                            PatientDetailId = "PATD_3",
                            TreatmentPlanId = "TP_3",
                            DoctorId = "DOC_3",
                            ScheduledDate = DateTime.UtcNow.AddDays(3),
                            Status = "Đã lên lịch",
                            Result = "Chưa thực hiện"
                        }
                    };

                    context.TreatmentProcesses.AddRange(treatmentProcesses);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi thêm treatment processes: {ex.Message}");
                }
            }

            // Thêm notifications
            if (!context.Notifications.Any() && context.Patients.Any() && context.Doctors.Any())
            {
                try
                {
                    var notifications = new List<Notification>
                    {
                        new Notification
                        {
                            NotificationId = "NTF_1",
                            PatientId = "PAT_1",
                            DoctorId = "DOC_1",
                            BookingId = "BKG_1",
                            TreatmentProcessId = "TPR_1",
                            Type = "appointment",
                            Message = "Vui lòng đến phòng khám để bắt đầu quy trình kích trứng",
                            MessageForDoctor = "Bạn vui lòng chuẩn bị để tiến hành quy trình kích trứng với lịch được đặt",
                            Time = DateTime.UtcNow.AddDays(4)
                        },
                        new Notification
                        {
                            NotificationId = "NTF_2",
                            PatientId = "PAT_2",
                            DoctorId = "DOC_2",
                            BookingId = "BKG_2",
                            Type = "appointment",
                            Message = "Vui lòng đến phòng khám để được tư vấn về quy trình IUI",
                            MessageForDoctor = "Bệnh nhân đã đặt lịch khám với bạn để tư vấn về quy trình IUI",
                            Time = DateTime.UtcNow.AddDays(2)
                        },
                        new Notification
                        {
                            NotificationId = "NTF_3",
                            PatientId = "PAT_3",
                            DoctorId = "DOC_3",
                            BookingId = "BKG_3",
                            TreatmentProcessId = "TPR_6",
                            Type = "treatment",
                            Message = "Vui lòng đến phòng khám để thực hiện quy trình trữ đông tinh trùng",
                            MessageForDoctor = "Bệnh nhân đã đặt lịch khám dịch vụ thực hiện quy trình trữ đông tinh trùng",
                            Time = DateTime.UtcNow.AddDays(2)
                        }
                    };

                    context.Notifications.AddRange(notifications);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi thêm notifications: {ex.Message}");
                }
            }

            // Thêm ratings
            if (!context.Ratings.Any() && context.Patients.Any() && context.Doctors.Any())
            {
                try
                {
                    var ratings = new List<Rating>
                    {
                        new Rating
                        {
                            RatingId = "RTG_1",
                            PatientId = "PAT_1",
                            DoctorId = "DOC_1",
                            ServiceId = "SRV_1",
                            BookingId = "BKG_1",
                            Score = 5,
                            Comment = "Bác sĩ tư vấn rất nhiệt tình và chuyên nghiệp",
                            RatingDate = DateTime.UtcNow.AddDays(-10),
                            RatingType = "Doctor",
                            Status = "Approved",
                            IsAnonymous = false
                        },
                        new Rating
                        {
                            RatingId = "RTG_2",
                            PatientId = "PAT_2",
                            DoctorId = "DOC_2",
                            ServiceId = "SRV_4",
                            BookingId = "BKG_2",
                            Score = 4,
                            Comment = "Dịch vụ tốt, nhân viên phòng khám thân thiện",
                            RatingDate = DateTime.UtcNow.AddDays(-5),
                            RatingType = "Service",
                            Status = "Approved",
                            IsAnonymous = false
                        }
                    };

                    context.Ratings.AddRange(ratings);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi thêm ratings: {ex.Message}");
                }
            }

            // Thêm feedbacks
            if (!context.Feedbacks.Any() && context.Patients.Any())
            {
                try
                {
                    var feedbacks = new List<Feedback>
                    {
                        new Feedback
                        {
                            FeedbackId = "FBK_1",
                            PatientId = "PAT_1",
                            ServiceId = "SRV_3",
                            Title = "Phản hồi về dịch vụ IVF",
                            Content = "Tôi rất hài lòng với quy trình điều trị IVF tại phòng khám. Đội ngũ y bác sĩ rất tận tâm và chuyên nghiệp.",
                            CreateDate = DateTime.UtcNow.AddDays(-15),
                            Status = "Read",
                            FeedbackType = "Service",
                            IsPublic = true
                        },
                        new Feedback
                        {
                            FeedbackId = "FBK_2",
                            PatientId = "PAT_3",
                            Title = "Góp ý về cơ sở vật chất",
                            Content = "Phòng khám nên bổ sung thêm khu vực chờ rộng rãi hơn cho bệnh nhân và người nhà.",
                            CreateDate = DateTime.UtcNow.AddDays(-7),
                            Status = "New",
                            FeedbackType = "General",
                            IsPublic = false
                        }
                    };

                    context.Feedbacks.AddRange(feedbacks);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi thêm feedbacks: {ex.Message}");
                }
            }

            // Thêm thanh toán mẫu
            if (!context.Payments.Any() && context.Bookings.Any())
            {
                try
                {
                    var payments = new List<Payment>
                    {
                        new Payment
                        {
                            PaymentId = "PAY_1",
                            BookingId = "BKG_1",
                            TotalAmount = 50000000M,
                            Method = "Chuyển khoản",
                            Status = "Đã thanh toán",
                            Confirmed = false
                        },
                        new Payment
                        {
                            PaymentId = "PAY_2",
                            BookingId = "BKG_2",
                            TotalAmount = 1000,//15000000M
                            Method = "Tiền mặt",
                            Status = "Đã thanh toán",
                            Confirmed = false
                        },
                        new Payment
                        {
                            PaymentId = "PAY_3",
                            BookingId = "BKG_3",
                            TotalAmount = 500000M,
                            Method = "Thẻ tín dụng",
                            Status = "Đã thanh toán",
                            Confirmed = false
                        }
                    };

                    context.Payments.AddRange(payments);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi thêm payments: {ex.Message}");
                }
            }

            // Thêm kiểm tra mẫu
            if (!context.Examinations.Any() && context.Bookings.Any() && context.Patients.Any() && context.Doctors.Any())
            {
                try
                {
                    var examinations = new List<Examination>
                    {
                        new Examination
                        {
                            ExaminationId = "EXM_1",
                            BookingId = "BKG_1",
                            PatientId = "PAT_1",
                            DoctorId = "DOC_1",
                            ExaminationDate = DateTime.UtcNow.AddDays(-1),
                            ExaminationDescription = "Vô sinh nguyên phát do tắc ống dẫn trứng. Điều trị bằng phương pháp IVF.",
                            Result = "Kết quả kiểm tra sức khỏe tốt, đủ điều kiện để bắt đầu chu trình IVF",
                            Status = "completed",
                            Note = "Cần tiến hành kích trứng theo lịch đã đề ra",
                            CreateAt = DateTime.UtcNow.AddDays(-1)
                        },
                        new Examination
                        {
                            ExaminationId = "EXM_2",
                            BookingId = "BKG_2",
                            PatientId = "PAT_2",
                            DoctorId = "DOC_2",
                            ExaminationDate = DateTime.UtcNow,
                            ExaminationDescription = "Vô sinh do không rụng trứng. Điều trị bằng phương pháp IUI.",
                            Result = "Kết quả siêu âm cho thấy có 3 nang trứng phát triển tốt",
                            Status = "in-progress",
                            Note = "Cần theo dõi thêm 3 ngày nữa trước khi tiến hành IUI",
                            CreateAt = DateTime.UtcNow
                        }
                    };

                    context.Examinations.AddRange(examinations);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi thêm examinations: {ex.Message}");
                }
            }

            Console.WriteLine("Seed data đã hoàn thành");
        }
    }
}