using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repositories.Models;

namespace Repositories.Models
{
    public class InfertilityTreatmentManagementContext : DbContext
    {
        public InfertilityTreatmentManagementContext(DbContextOptions<InfertilityTreatmentManagementContext> options)
            : base(options)
        {
        }

        // Updated to use plural form to match SWP391_DATABASEContext naming
        public virtual DbSet<Booking> Bookings { get; set; }
        public virtual DbSet<Doctor> Doctors { get; set; }
        public virtual DbSet<Examination> Examinations { get; set; }
        public virtual DbSet<Patient> Patients { get; set; }
        public virtual DbSet<PatientDetail> PatientDetails { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Service> Services { get; set; }
        public virtual DbSet<Slot> Slots { get; set; }
        public virtual DbSet<TreatmentProcess> TreatmentProcesses { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Doctor-User one-to-one relationship
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.User)
                .WithOne(u => u.Doctor)
                .HasForeignKey<Doctor>(d => d.UserId)
                .HasConstraintName("FK__Doctor__UserID__2A4B4B5E");

            // Configure Booking-Payment one-to-one relationship
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Payment)
                .WithOne(p => p.Booking)
                .HasForeignKey<Booking>(b => b.PaymentId)
                .HasConstraintName("FK__Booking__Payment__3D5E1FD2");

            // Configure Patient-User relationship
            modelBuilder.Entity<Patient>()
                .HasOne(p => p.User)
                .WithMany(u => u.Patients)
                .HasForeignKey(p => p.UserId)
                .HasConstraintName("FK__Patient__UserID__2F10007B");

            // Configure PatientDetail-Patient relationship
            modelBuilder.Entity<PatientDetail>()
                .HasOne(pd => pd.Patient)
                .WithMany(p => p.PatientDetails)
                .HasForeignKey(pd => pd.PatientId)
                .HasConstraintName("FK__PatientDe__Patie__31EC6D26");

            // Configure TreatmentProcess-PatientDetail relationship
            modelBuilder.Entity<TreatmentProcess>()
                .HasOne(tp => tp.PatientDetail)
                .WithMany(pd => pd.TreatmentProcess)
                .HasForeignKey(tp => tp.PatientDetailId)
                .HasConstraintName("FK__Treatment__Patie__45F365D3");

            // Configure Booking-Patient relationship
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Patient)
                .WithOne(p => p.Booking)
                .HasForeignKey<Booking>(b => b.PatientId)
                .HasConstraintName("FK__Booking__Patient__3B75D760");

            // Configure Booking-Service relationship
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Service)
                .WithOne(s => s.Booking)
                .HasForeignKey<Booking>(b => b.ServiceId)
                .HasConstraintName("FK__Booking__Service__3C69FB99");

            // Configure Booking-Doctor relationship
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Doctor)
                .WithMany(d => d.Bookings)
                .HasForeignKey(b => b.DoctorId)
                .HasConstraintName("FK__Booking__DoctorI__3E52440B");

            // Configure Booking-Slot relationship
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Slot)
                .WithMany(s => s.Bookings)
                .HasForeignKey(b => b.SlotId)
                .HasConstraintName("FK__Booking__SlotID__3F466844");

            // Configure Examination-Booking relationship
            modelBuilder.Entity<Examination>()
                .HasOne(e => e.Booking)
                .WithOne(b => b.Examination)
                .HasForeignKey<Examination>(e => e.BookingId)
                .HasConstraintName("FK__Examinati__Booki__4316F928");

            // Remove UNIQUE constraints that conflict with the foreign keys
            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.PatientId)
                .IsUnique(false);

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.ServiceId)
                .IsUnique(false);

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.PaymentId)
                .IsUnique(false);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.RoleId)
                .IsUnique(false);

            modelBuilder.Entity<Examination>()
                .HasIndex(e => e.BookingId)
                .IsUnique(false);
        }
    }
}