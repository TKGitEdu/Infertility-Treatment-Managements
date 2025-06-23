// Tạo file SeedData.cs trong thư mục Data hoặc Helpers
using Infertility_Treatment_Managements.Models;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

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
                // Danh sách bác sĩ mẫu
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
                            DateOfBirth = new DateTime(1980, 5, 15),
                            RoleId = "ROLE_2" // Doctor role
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
                            DateOfBirth = new DateTime(1982, 8, 20),
                            RoleId = "ROLE_2" // Doctor role
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
                            DateOfBirth = new DateTime(1975, 3, 10),
                            RoleId = "ROLE_2" // Doctor role
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

                // Thêm các bác sĩ vào database
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
                // Danh sách bệnh nhân mẫu
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
                            DateOfBirth = new DateTime(1990, 5, 15),
                            RoleId = "ROLE_3" // Patient role
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
                            DateOfBirth = new DateTime(1990, 5, 15),
                            BloodType = "A+",
                            EmergencyPhoneNumber = "0945678901"
                        },
                        new PatientDetail
                        {
                            PatientDetailId = "PATD_1",
                            PatientId = "PAT_1",
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
                            DateOfBirth = new DateTime(1988, 7, 25),
                            RoleId = "ROLE_3" // Patient role
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
                            DateOfBirth = new DateTime(1988, 7, 25),
                            BloodType = "B+",
                            EmergencyPhoneNumber = "0956789012"
                        },
                        new PatientDetail
                        {
                            PatientDetailId = "PATD_2",
                            PatientId = "PAT_2",
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
                            DateOfBirth = new DateTime(1985, 11, 10),
                            RoleId = "ROLE_3" // Patient role
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
                            DateOfBirth = new DateTime(1985, 11, 10),
                            BloodType = "O+",
                            EmergencyPhoneNumber = "0967890123"
                        },
                        new PatientDetail
                        {
                            PatientDetailId = "PATD_3",
                            PatientId = "PAT_3",
                            TreatmentStatus = "Đang điều trị"
                        }
                    )
                };

                // Thêm các bệnh nhân vào database
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

            // Thêm dịch vụ
            if (!context.Services.Any())
            {
                var services = new List<Service>
                {
                    // Dịch vụ tư vấn
                    new Service
                    {
                        ServiceId = "SRV_1",
                        Name = "Tư vấn ban đầu",
                        Description = "Buổi tư vấn đầu tiên với chuyên gia điều trị hiếm muộn",
                        Category = "Consultation",
                        Price = 500000M,
                        Status = "Active"
                    },
                    new Service
                    {
                        ServiceId = "SRV_2",
                        Name = "Tư vấn chuyên sâu",
                        Description = "Tư vấn chuyên sâu về các phương pháp điều trị hiếm muộn",
                        Category = "Consultation",
                        Price = 800000M,
                        Status = "Active"
                    },

                    // Dịch vụ điều trị hiếm muộn
                    new Service
                    {
                        ServiceId = "SRV_3",
                        Name = "Điều trị IVF (Thụ tinh trong ống nghiệm)",
                        Description = "Một chu kỳ điều trị thụ tinh trong ống nghiệm (IVF) bao gồm kích trứng, lấy trứng, thụ tinh và chuyển phôi",
                        Category = "InfertilityTreatment",
                        Price = 50000000M,
                        Status = "Active"
                    },
                    new Service
                    {
                        ServiceId = "SRV_4",
                        Name = "Điều trị IUI (Bơm tinh trùng vào buồng tử cung)",
                        Description = "Phương pháp điều trị bằng cách bơm tinh trùng đã được xử lý vào buồng tử cung",
                        Category = "InfertilityTreatment",
                        Price = 15000000M,
                        Status = "Active"
                    },
                    new Service
                    {
                        ServiceId = "SRV_5",
                        Name = "Trữ đông trứng",
                        Description = "Dịch vụ trữ đông trứng để sử dụng trong tương lai",
                        Category = "InfertilityTreatment",
                        Price = 25000000M,
                        Status = "Active"
                    },
                    new Service
                    {
                        ServiceId = "SRV_6",
                        Name = "Trữ đông tinh trùng",
                        Description = "Dịch vụ trữ đông tinh trùng để sử dụng trong tương lai",
                        Category = "InfertilityTreatment",
                        Price = 10000000M,
                        Status = "Active"
                    },
                    new Service
                    {
                        ServiceId = "SRV_7",
                        Name = "Trữ đông phôi",
                        Description = "Dịch vụ trữ đông phôi sau quá trình thụ tinh trong ống nghiệm",
                        Category = "InfertilityTreatment",
                        Price = 30000000M,
                        Status = "Active"
                    },
                    new Service
                    {
                        ServiceId = "SRV_8",
                        Name = "ICSI (Tiêm tinh trùng vào bào tương trứng)",
                        Description = "Phương pháp tiêm một tinh trùng trực tiếp vào trứng để thụ tinh",
                        Category = "InfertilityTreatment",
                        Price = 60000000M,
                        Status = "Active"
                    },

                    // Dịch vụ xét nghiệm
                    new Service
                    {
                        ServiceId = "SRV_9",
                        Name = "Xét nghiệm hormone",
                        Description = "Xét nghiệm các hormone liên quan đến sinh sản",
                        Category = "Testing",
                        Price = 2000000M,
                        Status = "Active"
                    },
                    new Service
                    {
                        ServiceId = "SRV_10",
                        Name = "Siêu âm theo dõi nang trứng",
                        Description = "Siêu âm để theo dõi sự phát triển của nang trứng trong quá trình kích trứng",
                        Category = "Testing",
                        Price = 800000M,
                        Status = "Active"
                    }
                };

                context.Services.AddRange(services);
                context.SaveChanges();
            }

            // Thêm slots
            // Thêm slots (24 khung giờ trong ngày)
            if (!context.Slots.Any())
            {
                var slots = new List<Slot>();

                // Khung giờ sáng sớm (06:00 - 07:59)
                slots.Add(new Slot { SlotId = "SLOT_01", SlotName = "Sáng sớm 06:00-07:00", StartTime = "06:00", EndTime = "07:00" });
                slots.Add(new Slot { SlotId = "SLOT_02", SlotName = "Sáng sớm 07:00-08:00", StartTime = "07:00", EndTime = "08:00" });

                // Khung giờ sáng (08:00 - 11:59)
                slots.Add(new Slot { SlotId = "SLOT_03", SlotName = "Sáng 08:00-09:00", StartTime = "08:00", EndTime = "09:00" });
                slots.Add(new Slot { SlotId = "SLOT_04", SlotName = "Sáng 09:00-10:00", StartTime = "09:00", EndTime = "10:00" });
                slots.Add(new Slot { SlotId = "SLOT_05", SlotName = "Sáng 10:00-11:00", StartTime = "10:00", EndTime = "11:00" });
                slots.Add(new Slot { SlotId = "SLOT_06", SlotName = "Sáng 11:00-12:00", StartTime = "11:00", EndTime = "12:00" });

                // Khung giờ trưa (12:00 - 13:59)
                slots.Add(new Slot { SlotId = "SLOT_07", SlotName = "Trưa 12:00-13:00", StartTime = "12:00", EndTime = "13:00" });
                slots.Add(new Slot { SlotId = "SLOT_08", SlotName = "Trưa 13:00-14:00", StartTime = "13:00", EndTime = "14:00" });

                // Khung giờ chiều (14:00 - 17:59)
                slots.Add(new Slot { SlotId = "SLOT_09", SlotName = "Chiều 14:00-15:00", StartTime = "14:00", EndTime = "15:00" });
                slots.Add(new Slot { SlotId = "SLOT_10", SlotName = "Chiều 15:00-16:00", StartTime = "15:00", EndTime = "16:00" });
                slots.Add(new Slot { SlotId = "SLOT_11", SlotName = "Chiều 16:00-17:00", StartTime = "16:00", EndTime = "17:00" });
                slots.Add(new Slot { SlotId = "SLOT_12", SlotName = "Chiều 17:00-18:00", StartTime = "17:00", EndTime = "18:00" });

                // Khung giờ tối (18:00 - 20:59)
                slots.Add(new Slot { SlotId = "SLOT_13", SlotName = "Tối 18:00-19:00", StartTime = "18:00", EndTime = "19:00" });
                slots.Add(new Slot { SlotId = "SLOT_14", SlotName = "Tối 19:00-20:00", StartTime = "19:00", EndTime = "20:00" });
                slots.Add(new Slot { SlotId = "SLOT_15", SlotName = "Tối 20:00-21:00", StartTime = "20:00", EndTime = "21:00" });

                context.Slots.AddRange(slots);
                context.SaveChanges();
            }

            // Thêm đăng ký dịch vụ hiếm muộn mẫu
            if (!context.Bookings.Any())
            {
                var bookings = new List<Booking>
                {
                    new Booking
                    {
                        BookingId = "BKG_1",
                        PatientId = "PAT_1",
                        DoctorId = "DOC_1",
                        ServiceId = "SRV_3", // IVF
                        SlotId = "SLOT_04", // Sáng 09:00-10:00
                        DateBooking = DateTime.Now.AddDays(1),
                        Description = "Đăng ký điều trị IVF",
                        CreateAt = DateTime.Now.AddDays(-2),
                        Note = "Đã xác nhận qua điện thoại"
                    },
                    new Booking
                    {
                        BookingId = "BKG_2",
                        PatientId = "PAT_2",
                        DoctorId = "DOC_2",
                        ServiceId = "SRV_4", // IUI
                        SlotId = "SLOT_10", // Chiều 15:00-16:00
                        DateBooking = DateTime.Now.AddDays(3),
                        Description = "Đăng ký điều trị IUI",
                        CreateAt = DateTime.Now.AddDays(-1),
                        Note = "Lần đầu tiên thực hiện IUI"
                    },
                    new Booking
                    {
                        BookingId = "BKG_3",
                        PatientId = "PAT_3",
                        DoctorId = "DOC_3",
                        ServiceId = "SRV_1", // Tư vấn ban đầu
                        SlotId = "SLOT_06", // Sáng 11:00-12:00
                        DateBooking = DateTime.Now.AddDays(2),
                        Description = "Tư vấn về điều trị hiếm muộn",
                        CreateAt = DateTime.Now.AddDays(-3),
                        Note = "Cần tư vấn các phương pháp điều trị phù hợp"
                    }
                };

                context.Bookings.AddRange(bookings);
                context.SaveChanges();
            }

            // Thêm kế hoạch điều trị mẫu với ServiceId
            if (!context.TreatmentPlans.Any())
            {
                var treatmentPlans = new List<TreatmentPlan>
                {
                    new TreatmentPlan
                    {
                        TreatmentPlanId = "TP_1",
                        DoctorId = "DOC_1",
                        PatientDetailId = "PATD_1",
                        ServiceId = "SRV_3", // IVF service
                        Method = "IVF",
                        StartDate = DateTime.Now.AddDays(5),
                        EndDate = DateTime.Now.AddDays(35),
                        Status = "Đã lên lịch",
                        TreatmentDescription = "Kế hoạch điều trị IVF đầy đủ bao gồm kích trứng, lấy trứng, thụ tinh và chuyển phôi"
                    },
                    new TreatmentPlan
                    {
                        TreatmentPlanId = "TP_2",
                        DoctorId = "DOC_2",
                        PatientDetailId = "PATD_2",
                        ServiceId = "SRV_4", // IUI service
                        Method = "IUI",
                        StartDate = DateTime.Now.AddDays(7),
                        EndDate = DateTime.Now.AddDays(21),
                        Status = "Chờ xác nhận",
                        TreatmentDescription = "Kế hoạch điều trị IUI bao gồm kích trứng nhẹ và bơm tinh trùng vào buồng tử cung"
                    },
                    new TreatmentPlan
                    {
                        TreatmentPlanId = "TP_3",
                        DoctorId = "DOC_3",
                        PatientDetailId = "PATD_3",
                        ServiceId = "SRV_6", // Trữ đông tinh trùng
                        Method = "Trữ đông tinh trùng",
                        StartDate = DateTime.Now.AddDays(3),
                        EndDate = DateTime.Now.AddDays(4),
                        Status = "Đã lên lịch",
                        TreatmentDescription = "Trữ đông tinh trùng để sử dụng trong tương lai"
                    }
                };

                context.TreatmentPlans.AddRange(treatmentPlans);
                context.SaveChanges();
            }

            // Thêm quy trình điều trị mẫu với các trường đã cập nhật
            if (!context.TreatmentProcesses.Any())
            {
                var treatmentProcesses = new List<TreatmentProcess>
                {
                    new TreatmentProcess
                    {
                        TreatmentProcessId = "TPR_1",
                        PatientDetailId = "PATD_1",
                        TreatmentPlanId = "TP_1",
                        DoctorId = "DOC_1",
                        ScheduledDate = DateTime.Now.AddDays(5),
                        ActualDate = null,
                        Status = "Đã lên lịch",
                        Result = "Chưa thực hiện"
                    },
                    new TreatmentProcess
                    {
                        TreatmentProcessId = "TPR_2",
                        PatientDetailId = "PATD_1",
                        TreatmentPlanId = "TP_1",
                        DoctorId = "DOC_1",
                        ScheduledDate = DateTime.Now.AddDays(15),
                        ActualDate = null,
                        Status = "Đã lên lịch",
                        Result = "Chưa thực hiện"
                    },
                    new TreatmentProcess
                    {
                        TreatmentProcessId = "TPR_3",
                        PatientDetailId = "PATD_1",
                        TreatmentPlanId = "TP_1",
                        DoctorId = "DOC_1",
                        ScheduledDate = DateTime.Now.AddDays(25),
                        ActualDate = null,
                        Status = "Đã lên lịch",
                        Result = "Chưa thực hiện"
                    },
                    new TreatmentProcess
                    {
                        TreatmentProcessId = "TPR_4",
                        PatientDetailId = "PATD_2",
                        TreatmentPlanId = "TP_2",
                        DoctorId = "DOC_2",
                        ScheduledDate = DateTime.Now.AddDays(7),
                        ActualDate = null,
                        Status = "Chờ xác nhận",
                        Result = "Chưa thực hiện"
                    },
                    new TreatmentProcess
                    {
                        TreatmentProcessId = "TPR_5",
                        PatientDetailId = "PATD_2",
                        TreatmentPlanId = "TP_2",
                        DoctorId = "DOC_2",
                        ScheduledDate = DateTime.Now.AddDays(14),
                        ActualDate = null,
                        Status = "Chờ xác nhận",
                        Result = "Chưa thực hiện"
                    },
                    new TreatmentProcess
                    {
                        TreatmentProcessId = "TPR_6",
                        PatientDetailId = "PATD_3",
                        TreatmentPlanId = "TP_3",
                        DoctorId = "DOC_3",
                        ScheduledDate = DateTime.Now.AddDays(3),
                        ActualDate = null,
                        Status = "Đã lên lịch",
                        Result = "Chưa thực hiện"
                    }
                };

                context.TreatmentProcesses.AddRange(treatmentProcesses);
                context.SaveChanges();
            }

            // Thêm nhắc nhở mẫu
            if (!context.Reminders.Any())
            {
                var reminders = new List<Reminder>
                {
                    new Reminder
                    {
                        ReminderId = "RMD_1",
                        PatientId = "PAT_1",
                        DoctorId = "DOC_1",
                        BookingId = "BKG_1",
                        TreatmentProcessId = "TPR_1",
                        Title = "Nhắc lịch kích trứng",
                        Description = "Vui lòng đến phòng khám vào ngày mai lúc 09:00 để bắt đầu quy trình kích trứng",
                        ScheduledTime = DateTime.Now.AddDays(4),
                        Status = "Pending",
                        ReminderType = "Appointment",
                        IsEmailNotification = true,
                        IsSmsNotification = false,
                        IsRepeating = false,
                        CreateDate = DateTime.Now
                    },
                    new Reminder
                    {
                        ReminderId = "RMD_2",
                        PatientId = "PAT_2",
                        DoctorId = "DOC_2",
                        BookingId = "BKG_2",
                        Title = "Nhắc lịch tư vấn IUI",
                        Description = "Vui lòng đến phòng khám vào ngày mai lúc 15:00 để được tư vấn về quy trình IUI",
                        ScheduledTime = DateTime.Now.AddDays(2),
                        Status = "Pending",
                        ReminderType = "Appointment",
                        IsEmailNotification = true,
                        IsSmsNotification = true,
                        IsRepeating = false,
                        CreateDate = DateTime.Now
                    },
                    new Reminder
                    {
                        ReminderId = "RMD_3",
                        PatientId = "PAT_3",
                        DoctorId = "DOC_3",
                        BookingId = "BKG_3",
                        TreatmentProcessId = "TPR_6",
                        Title = "Nhắc lịch trữ đông tinh trùng",
                        Description = "Vui lòng đến phòng khám vào ngày mai lúc 11:00 để thực hiện quy trình trữ đông tinh trùng",
                        ScheduledTime = DateTime.Now.AddDays(2),
                        Status = "Pending",
                        ReminderType = "Appointment",
                        IsEmailNotification = true,
                        IsSmsNotification = false,
                        IsRepeating = false,
                        CreateDate = DateTime.Now
                    }
                };

                context.Reminders.AddRange(reminders);
                context.SaveChanges();
            }

            // Thêm các đánh giá mẫu
            if (!context.Ratings.Any())
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
                        RatingDate = DateTime.Now.AddDays(-10),
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
                        RatingDate = DateTime.Now.AddDays(-5),
                        RatingType = "Service",
                        Status = "Approved",
                        IsAnonymous = false
                    }
                };

                context.Ratings.AddRange(ratings);
                context.SaveChanges();
            }

            // Thêm phản hồi mẫu
            if (!context.Feedbacks.Any())
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
                        CreateDate = DateTime.Now.AddDays(-15),
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
                        CreateDate = DateTime.Now.AddDays(-7),
                        Status = "New",
                        FeedbackType = "General",
                        IsPublic = false
                    }
                };

                context.Feedbacks.AddRange(feedbacks);
                context.SaveChanges();
            }

            // Thêm thanh toán mẫu
            if (!context.Payments.Any())
            {
                var payments = new List<Payment>
                {
                    new Payment
                    {
                        PaymentId = "PAY_1",
                        BookingId = "BKG_1",
                        TotalAmount = 50000000M, // 50 triệu VND cho IVF
                        Method = "Chuyển khoản",
                        Status = "Đã thanh toán"
                    },
                    new Payment
                    {
                        PaymentId = "PAY_2",
                        BookingId = "BKG_2",
                        TotalAmount = 15000000M, // 15 triệu VND cho IUI
                        Method = "Tiền mặt",
                        Status = "Đã thanh toán"
                    },
                    new Payment
                    {
                        PaymentId = "PAY_3",
                        BookingId = "BKG_3",
                        TotalAmount = 500000M, // 500k VND cho tư vấn ban đầu
                        Method = "Thẻ tín dụng",
                        Status = "Đã thanh toán"
                    }
                };

                context.Payments.AddRange(payments);
                context.SaveChanges();
            }

            // Thêm kiểm tra mẫu
            if (!context.Examinations.Any())
            {
                var examinations = new List<Examination>
                {
                    new Examination
                    {
                        ExaminationId = "EXM_1",
                        BookingId = "BKG_1",
                        PatientId = "PAT_1",
                        DoctorId = "DOC_1",
                        ExaminationDate = DateTime.Now.AddDays(-1),
                        ExaminationDescription = "Vô sinh nguyên phát do tắc ống dẫn trứng. Điều trị bằng phương pháp IVF.",
                        Result = "Kết quả kiểm tra sức khỏe tốt, đủ điều kiện để bắt đầu chu trình IVF",
                        Status = "Hoàn thành",
                        Note = "Cần tiến hành kích trứng theo lịch đã đề ra",
                        CreateAt = DateTime.Now.AddDays(-1)
                    },
                    new Examination
                    {
                        ExaminationId = "EXM_2",
                        BookingId = "BKG_2",
                        PatientId = "PAT_2",
                        DoctorId = "DOC_2",
                        ExaminationDate = DateTime.Now,
                        ExaminationDescription = "Vô sinh do không rụng trứng. Điều trị bằng phương pháp IUI.",
                        Result = "Kết quả siêu âm cho thấy có 3 nang trứng phát triển tốt",
                        Status = "Hoàn thành",
                        Note = "Cần theo dõi thêm 3 ngày nữa trước khi tiến hành IUI",
                        CreateAt = DateTime.Now
                    }
                };

                context.Examinations.AddRange(examinations);
                context.SaveChanges();
            }
        }
    }
}