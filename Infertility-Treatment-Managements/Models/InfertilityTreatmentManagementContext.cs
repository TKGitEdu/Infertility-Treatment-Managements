using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infertility_Treatment_Managements.Models;
using Microsoft.EntityFrameworkCore;

namespace Infertility_Treatment_Managements.Models
{
    public class InfertilityTreatmentManagementContext : DbContext
    {
        public InfertilityTreatmentManagementContext(DbContextOptions<InfertilityTreatmentManagementContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Booking> Bookings { get; set; }
        public virtual DbSet<Doctor> Doctors { get; set; }
        public virtual DbSet<Examination> Examinations { get; set; }
        public virtual DbSet<Patient> Patients { get; set; }
        public virtual DbSet<PatientDetail> PatientDetails { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Service> Services { get; set; }
        public virtual DbSet<Slot> Slots { get; set; }
        public virtual DbSet<TreatmentPlan> TreatmentPlans { get; set; }
        public virtual DbSet<TreatmentProcess> TreatmentProcesses { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Entity configurations with column naming conventions
            modelBuilder.Entity<Booking>(entity =>
            {
                entity.ToTable("Bookings");
                entity.Property(b => b.BookingId).HasColumnName("BookingID").HasMaxLength(50);
                entity.Property(b => b.PatientId).HasColumnName("PatientID").HasMaxLength(50);
                entity.Property(b => b.ServiceId).HasColumnName("ServiceID").HasMaxLength(50);
                entity.Property(b => b.DoctorId).HasColumnName("DoctorID").HasMaxLength(50);
                entity.Property(b => b.SlotId).HasColumnName("SlotID").HasMaxLength(50);
                entity.Property(b => b.Description).HasMaxLength(500);
                entity.Property(b => b.Note).HasMaxLength(500);

                // Important: Ignore PaymentId property in C# model since it doesn't exist in database
                entity.Ignore(b => b.PaymentId);
            });

            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.ToTable("Doctors");
                entity.Property(d => d.DoctorId).HasColumnName("DoctorID").HasMaxLength(50);
                entity.Property(d => d.UserId).HasColumnName("UserID").HasMaxLength(50);
                entity.Property(d => d.DoctorName).HasMaxLength(100);
                entity.Property(d => d.Specialization).HasMaxLength(100);
                entity.Property(d => d.Phone).HasMaxLength(20);
                entity.Property(d => d.Email).HasMaxLength(100);
            });

            modelBuilder.Entity<Examination>(entity =>
            {
                entity.ToTable("Examinations");
                entity.Property(e => e.ExaminationId).HasColumnName("ExaminationID").HasMaxLength(50);
                entity.Property(e => e.BookingId).HasColumnName("BookingID").HasMaxLength(50);
                entity.Property(e => e.ExaminationDescription).HasMaxLength(500);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.Result).HasMaxLength(500);
                entity.Property(e => e.Note).HasMaxLength(500);
            });

            modelBuilder.Entity<Patient>(entity =>
            {
                entity.ToTable("Patients");
                entity.Property(p => p.PatientId).HasColumnName("PatientID").HasMaxLength(50);
                entity.Property(p => p.UserId).HasColumnName("UserID").HasMaxLength(50);
                entity.Property(p => p.Name).HasMaxLength(100);
                entity.Property(p => p.Phone).HasMaxLength(20);
                entity.Property(p => p.Email).HasMaxLength(100);
                entity.Property(p => p.Address).HasMaxLength(200);
                entity.Property(p => p.Gender).HasMaxLength(10);
                entity.Property(p => p.BloodType).HasMaxLength(10);
                entity.Property(p => p.EmergencyPhoneNumber).HasMaxLength(20);
            });

            modelBuilder.Entity<PatientDetail>(entity =>
            {
                entity.ToTable("PatientDetails");
                entity.Property(pd => pd.PatientDetailId).HasColumnName("PatientDetailID").HasMaxLength(50);
                entity.Property(pd => pd.PatientId).HasColumnName("PatientID").HasMaxLength(50);
                entity.Property(pd => pd.TreatmentStatus).HasMaxLength(100);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("Payments");
                entity.Property(p => p.PaymentId).HasColumnName("PaymentID").HasMaxLength(50);
                entity.Property(p => p.BookingId).HasColumnName("BookingID").HasMaxLength(50);
                entity.Property(p => p.TotalAmount).HasColumnType("decimal(10,2)");
                entity.Property(p => p.Status).HasMaxLength(50);
                entity.Property(p => p.Method).HasMaxLength(50);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");
                entity.Property(r => r.RoleId).HasColumnName("RoleID").HasMaxLength(50); // Add MaxLength to match User.RoleId
                entity.Property(r => r.RoleName).HasMaxLength(100);
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.ToTable("Services");
                entity.Property(s => s.ServiceId).HasColumnName("ServiceID").HasMaxLength(50);
                entity.Property(s => s.Name).HasMaxLength(100);
                entity.Property(s => s.Description).HasMaxLength(500);
                entity.Property(s => s.Price).HasColumnType("decimal(10,2)");
                entity.Property(s => s.Status).HasMaxLength(50);
            });

            modelBuilder.Entity<Slot>(entity =>
            {
                entity.ToTable("Slots");
                entity.Property(s => s.SlotId).HasColumnName("SlotID").HasMaxLength(50);
                entity.Property(s => s.SlotName).HasMaxLength(100);
                entity.Property(s => s.StartTime).HasMaxLength(10);
                entity.Property(s => s.EndTime).HasMaxLength(10);
            });

            modelBuilder.Entity<TreatmentPlan>(entity =>
            {
                entity.ToTable("TreatmentPlans");
                entity.Property(tp => tp.TreatmentPlanId).HasColumnName("TreatmentPlanID").HasMaxLength(50);
                entity.Property(tp => tp.DoctorId).HasColumnName("DoctorID").HasMaxLength(50);
                entity.Property(tp => tp.PatientDetailId).HasColumnName("PatientDetailID").HasMaxLength(50);
                entity.Property(tp => tp.Method).HasMaxLength(100);
                entity.Property(tp => tp.Status).HasMaxLength(50);
                entity.Property(tp => tp.TreatmentDescription).HasMaxLength(500);
            });

            modelBuilder.Entity<TreatmentProcess>(entity =>
            {
                entity.ToTable("TreatmentProcesses");
                entity.Property(tp => tp.TreatmentProcessId).HasColumnName("TreatmentProcessID").HasMaxLength(50);
                entity.Property(tp => tp.PatientDetailId).HasColumnName("PatientDetailID").HasMaxLength(50);
                entity.Property(tp => tp.TreatmentPlanId).HasColumnName("TreatmentPlanID").HasMaxLength(50);
                entity.Property(tp => tp.Method).HasMaxLength(100);
                entity.Property(tp => tp.Result).HasMaxLength(500);
                entity.Property(tp => tp.Status).HasMaxLength(50);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.Property(u => u.UserId).HasColumnName("UserID").HasMaxLength(50).IsRequired();
                entity.Property(u => u.RoleId).HasColumnName("RoleID").HasMaxLength(50).IsRequired(false); // Explicitly mark as not required
                entity.Property(u => u.FullName).HasMaxLength(100);
                entity.Property(u => u.Email).HasMaxLength(100);
                entity.Property(u => u.Phone).HasMaxLength(20);
                entity.Property(u => u.Username).HasMaxLength(50);
                entity.Property(u => u.Password).HasMaxLength(100);
                entity.Property(u => u.Address).HasMaxLength(200);
                entity.Property(u => u.Gender).HasMaxLength(10);
            });

            // Relationships

            // Configure Doctor-User one-to-one relationship
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithOne(u => u.Doctor)
                .HasForeignKey<Doctor>(d => d.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Patient-User relationship
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithMany(u => u.Patients)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure PatientDetail-Patient relationship
            modelBuilder.Entity<PatientDetail>()
                .HasOne(pd => pd.Patient)
                .WithMany(p => p.PatientDetails)
                .HasForeignKey(pd => pd.PatientId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Booking-Patient relationship
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Patient)
                .WithMany(p => p.BookingFk)
                .HasForeignKey(b => b.PatientId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Booking-Service relationship
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Service)
                .WithMany(s => s.BookingsFk)
                .HasForeignKey(b => b.ServiceId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Booking-Doctor relationship
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Doctor)
                .WithMany(d => d.Bookings)
                .HasForeignKey(b => b.DoctorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Booking-Slot relationship
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Slot)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.SlotId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Payment-Booking relationship (1:1)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Booking)
                .WithOne(b => b.Payment)
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Examination-Booking relationship (1:1)
            modelBuilder.Entity<Examination>()
                .HasOne(e => e.Booking)
                .WithOne(b => b.Examination)
                .HasForeignKey<Examination>(e => e.BookingId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure TreatmentProcess-PatientDetail relationship
            modelBuilder.Entity<TreatmentProcess>()
                .HasOne(tp => tp.PatientDetail)
                .WithMany(pd => pd.TreatmentProcessesFk)
                .HasForeignKey(tp => tp.PatientDetailId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure TreatmentProcess-TreatmentPlan relationship
            modelBuilder.Entity<TreatmentProcess>()
                .HasOne(tp => tp.TreatmentPlan)
                .WithMany(plan => plan.TreatmentProcesses)
                .HasForeignKey(tp => tp.TreatmentPlanId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure TreatmentPlan-Doctor relationship
            modelBuilder.Entity<TreatmentPlan>()
                .HasOne(tp => tp.Doctor)
                .WithMany(d => d.TreatmentPlans)
                .HasForeignKey(tp => tp.DoctorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure TreatmentPlan-PatientDetail relationship
            modelBuilder.Entity<TreatmentPlan>()
                .HasOne(tp => tp.PatientDetail)
                .WithMany(pd => pd.TreatmentPlansFk)
                .HasForeignKey(tp => tp.PatientDetailId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure User-Role relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .IsRequired(false)  // Explicitly mark as not required
                .OnDelete(DeleteBehavior.SetNull);

            // Index configuration
            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.PatientId)
                .HasDatabaseName("IDX_Bookings_PatientID");

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.ServiceId)
                .HasDatabaseName("IDX_Bookings_ServiceID");

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.DoctorId)
                .HasDatabaseName("IDX_Bookings_DoctorID");

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.SlotId)
                .HasDatabaseName("IDX_Bookings_SlotID");
        }
    }
}