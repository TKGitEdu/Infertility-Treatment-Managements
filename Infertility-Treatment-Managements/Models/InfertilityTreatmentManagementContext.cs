using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Models
{
    public class InfertilityTreatmentManagementContext : DbContext
    {
        public InfertilityTreatmentManagementContext(DbContextOptions<InfertilityTreatmentManagementContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Booking> Booking { get; set; }
        public virtual DbSet<Doctor> Doctor { get; set; }
        public virtual DbSet<Examination> Examination { get; set; }
        public virtual DbSet<Patient> Patient { get; set; }
        public virtual DbSet<PatientDetail> PatientDetail { get; set; }
        public virtual DbSet<Payment> Payment { get; set; }
        public virtual DbSet<Role> Role { get; set; }
        public virtual DbSet<Service> Service { get; set; }
        public virtual DbSet<Slot> Slot { get; set; }
        public virtual DbSet<TreatmentProcess> TreatmentProcess { get; set; }
        public virtual DbSet<User> User { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Doctor-User one-to-one relationship
            modelBuilder.Entity<Doctor>()
                .HasOne(d => d.User)  // Changed from DoctorNavigation to User
                .WithOne(u => u.Doctor)
                .HasForeignKey<Doctor>(d => d.UserId);  // Changed from DoctorId to UserId

            // Configure Booking-Payment one-to-one relationship (from previous fix)
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Payment)
                .WithOne(p => p.Booking)
                .HasForeignKey<Booking>(b => b.PaymentId);

            // Remove UNIQUE constraints that conflict with the foreign keys
            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.PatientId)
                .IsUnique(false); // Remove UNIQUE constraint

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.ServiceId)
                .IsUnique(false); // Remove UNIQUE constraint

            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.PaymentId)
                .IsUnique(false); // Remove UNIQUE constraint

            modelBuilder.Entity<User>()
                .HasIndex(u => u.RoleId)
                .IsUnique(false); // Remove UNIQUE constraint

            modelBuilder.Entity<Examination>()
                .HasIndex(e => e.BookingId)
                .IsUnique(false); // Remove UNIQUE constraint
        }
    }
}