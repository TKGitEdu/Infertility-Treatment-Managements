using Infertility_Treatment_Managements.DTOs;
using Infertility_Treatment_Managements.Helpers;
using Infertility_Treatment_Managements.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infertility_Treatment_Managements.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infertility_Treatment_Managements.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReminderController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;
        private readonly IEmailService _emailService;

        public ReminderController(InfertilityTreatmentManagementContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: api/Reminder
        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<IEnumerable<ReminderDTO>>> GetReminders()
        {
            var reminders = await _context.Reminders
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Include(r => r.Booking)
                .Include(r => r.TreatmentProcess)
                .ToListAsync();

            return Ok(reminders.Select(r => MapToDTO(r)));
        }

        // GET: api/Reminder/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ReminderDTO>> GetReminder(string id)
        {
            var reminder = await _context.Reminders
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Include(r => r.Booking)
                .Include(r => r.TreatmentProcess)
                .FirstOrDefaultAsync(r => r.ReminderId == id);

            if (reminder == null)
            {
                return NotFound($"Reminder with ID {id} not found");
            }

            // Kiểm tra quyền truy cập
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            if (currentUserRole != "Admin" && currentUserRole != "Doctor")
            {
                // Nếu không phải Admin hoặc Doctor, kiểm tra xem người dùng có phải là bệnh nhân của nhắc nhở này không
                var patient = reminder.Patient;
                if (patient == null || patient.UserId != currentUserId)
                {
                    // Nếu không phải bệnh nhân, kiểm tra xem có phải là bác sĩ của nhắc nhở này không
                    var doctor = reminder.Doctor;
                    if (doctor == null || doctor.UserId != currentUserId)
                    {
                        return Forbid("You do not have permission to view this reminder");
                    }
                }
            }

            return Ok(MapToDTO(reminder));
        }

        // GET: api/Reminder/ByPatient/{patientId}
        [HttpGet("ByPatient/{patientId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReminderDTO>>> GetRemindersByPatient(string patientId)
        {
            // Kiểm tra quyền truy cập
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            if (currentUserRole != "Admin" && currentUserRole != "Doctor")
            {
                // Nếu không phải Admin hoặc Doctor, kiểm tra xem người dùng có phải là bệnh nhân này không
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.PatientId == patientId);
                if (patient == null || patient.UserId != currentUserId)
                {
                    return Forbid("You do not have permission to view these reminders");
                }
            }

            var reminders = await _context.Reminders
                .Where(r => r.PatientId == patientId)
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Include(r => r.Booking)
                .Include(r => r.TreatmentProcess)
                .ToListAsync();

            return Ok(reminders.Select(r => MapToDTO(r)));
        }

        // GET: api/Reminder/ByDoctor/{doctorId}
        [HttpGet("ByDoctor/{doctorId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReminderDTO>>> GetRemindersByDoctor(string doctorId)
        {
            // Kiểm tra quyền truy cập
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            if (currentUserRole != "Admin")
            {
                // Nếu không phải Admin, kiểm tra xem người dùng có phải là bác sĩ này không
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.DoctorId == doctorId);
                if (doctor == null || doctor.UserId != currentUserId)
                {
                    return Forbid("You do not have permission to view these reminders");
                }
            }

            var reminders = await _context.Reminders
                .Where(r => r.DoctorId == doctorId)
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Include(r => r.Booking)
                .Include(r => r.TreatmentProcess)
                .ToListAsync();

            return Ok(reminders.Select(r => MapToDTO(r)));
        }

        // GET: api/Reminder/ByBooking/{bookingId}
        [HttpGet("ByBooking/{bookingId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReminderDTO>>> GetRemindersByBooking(string bookingId)
        {
            // Validate booking
            var booking = await _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Doctor)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return NotFound($"Booking with ID {bookingId} not found");
            }

            // Kiểm tra quyền truy cập
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            if (currentUserRole != "Admin")
            {
                // Nếu không phải Admin, kiểm tra xem người dùng có phải là bệnh nhân hoặc bác sĩ của booking này không
                var patient = booking.Patient;
                var doctor = booking.Doctor;

                if ((patient == null || patient.UserId != currentUserId) && 
                    (doctor == null || doctor.UserId != currentUserId))
                {
                    return Forbid("You do not have permission to view these reminders");
                }
            }

            var reminders = await _context.Reminders
                .Where(r => r.BookingId == bookingId)
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Include(r => r.Booking)
                .Include(r => r.TreatmentProcess)
                .ToListAsync();

            return Ok(reminders.Select(r => MapToDTO(r)));
        }

        // POST: api/Reminder
        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<ReminderDTO>> CreateReminder(ReminderCreateDTO reminderCreateDTO)
        {
            try
            {
                // Validate references if provided
                if (!string.IsNullOrEmpty(reminderCreateDTO.PatientId))
                {
                    var patient = await _context.Patients.FindAsync(reminderCreateDTO.PatientId);
                    if (patient == null)
                    {
                        return BadRequest($"Patient with ID {reminderCreateDTO.PatientId} not found");
                    }
                }

                if (!string.IsNullOrEmpty(reminderCreateDTO.DoctorId))
                {
                    var doctor = await _context.Doctors.FindAsync(reminderCreateDTO.DoctorId);
                    if (doctor == null)
                    {
                        return BadRequest($"Doctor with ID {reminderCreateDTO.DoctorId} not found");
                    }
                }

                if (!string.IsNullOrEmpty(reminderCreateDTO.BookingId))
                {
                    var booking = await _context.Bookings.FindAsync(reminderCreateDTO.BookingId);
                    if (booking == null)
                    {
                        return BadRequest($"Booking with ID {reminderCreateDTO.BookingId} not found");
                    }
                }

                if (!string.IsNullOrEmpty(reminderCreateDTO.TreatmentProcessId))
                {
                    var treatmentProcess = await _context.TreatmentProcesses.FindAsync(reminderCreateDTO.TreatmentProcessId);
                    if (treatmentProcess == null)
                    {
                        return BadRequest($"Treatment process with ID {reminderCreateDTO.TreatmentProcessId} not found");
                    }
                }

                // Create reminder
                var reminder = new Reminder
                {
                    ReminderId = "REM_" + Guid.NewGuid().ToString().Substring(0, 8),
                    Title = reminderCreateDTO.Title,
                    Description = reminderCreateDTO.Description,
                    PatientId = reminderCreateDTO.PatientId,
                    DoctorId = reminderCreateDTO.DoctorId,
                    BookingId = reminderCreateDTO.BookingId,
                    TreatmentProcessId = reminderCreateDTO.TreatmentProcessId,
                    ScheduledTime = reminderCreateDTO.ScheduledTime,
                    ReminderType = reminderCreateDTO.ReminderType,
                    Status = "Pending",
                    IsRepeating = reminderCreateDTO.IsRepeating,
                    RepeatPattern = reminderCreateDTO.RepeatPattern,
                    IsEmailNotification = reminderCreateDTO.IsEmailNotification,
                    IsSmsNotification = reminderCreateDTO.IsSmsNotification,
                    CreateDate = DateTime.Now
                };

                _context.Reminders.Add(reminder);
                await _context.SaveChangesAsync();

                // Send email notification immediately if scheduled for now or in the past
                if (reminderCreateDTO.IsEmailNotification && reminder.ScheduledTime <= DateTime.Now)
                {
                    await SendReminderEmailAsync(reminder);
                }

                return CreatedAtAction(nameof(GetReminder), new { id = reminder.ReminderId }, MapToDTO(reminder));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred while creating the reminder: {ex.Message}" });
            }
        }

        // PUT: api/Reminder/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> UpdateReminder(string id, ReminderUpdateDTO reminderUpdateDTO)
        {
            if (id != reminderUpdateDTO.ReminderId)
            {
                return BadRequest("ID mismatch");
            }

            var reminder = await _context.Reminders.FindAsync(id);
            if (reminder == null)
            {
                return NotFound($"Reminder with ID {id} not found");
            }

            // Validate references if provided
            if (!string.IsNullOrEmpty(reminderUpdateDTO.PatientId))
            {
                var patient = await _context.Patients.FindAsync(reminderUpdateDTO.PatientId);
                if (patient == null)
                {
                    return BadRequest($"Patient with ID {reminderUpdateDTO.PatientId} not found");
                }
            }

            if (!string.IsNullOrEmpty(reminderUpdateDTO.DoctorId))
            {
                var doctor = await _context.Doctors.FindAsync(reminderUpdateDTO.DoctorId);
                if (doctor == null)
                {
                    return BadRequest($"Doctor with ID {reminderUpdateDTO.DoctorId} not found");
                }
            }

            if (!string.IsNullOrEmpty(reminderUpdateDTO.BookingId))
            {
                var booking = await _context.Bookings.FindAsync(reminderUpdateDTO.BookingId);
                if (booking == null)
                {
                    return BadRequest($"Booking with ID {reminderUpdateDTO.BookingId} not found");
                }
            }

            if (!string.IsNullOrEmpty(reminderUpdateDTO.TreatmentProcessId))
            {
                var treatmentProcess = await _context.TreatmentProcesses.FindAsync(reminderUpdateDTO.TreatmentProcessId);
                if (treatmentProcess == null)
                {
                    return BadRequest($"Treatment process with ID {reminderUpdateDTO.TreatmentProcessId} not found");
                }
            }

            // Update reminder
            reminder.Title = reminderUpdateDTO.Title;
            reminder.Description = reminderUpdateDTO.Description;
            reminder.PatientId = reminderUpdateDTO.PatientId;
            reminder.DoctorId = reminderUpdateDTO.DoctorId;
            reminder.BookingId = reminderUpdateDTO.BookingId;
            reminder.TreatmentProcessId = reminderUpdateDTO.TreatmentProcessId;
            reminder.ScheduledTime = reminderUpdateDTO.ScheduledTime;
            reminder.ReminderType = reminderUpdateDTO.ReminderType;
            reminder.Status = reminderUpdateDTO.Status;
            reminder.IsRepeating = reminderUpdateDTO.IsRepeating;
            reminder.RepeatPattern = reminderUpdateDTO.RepeatPattern;
            reminder.IsEmailNotification = reminderUpdateDTO.IsEmailNotification;
            reminder.IsSmsNotification = reminderUpdateDTO.IsSmsNotification;

            _context.Entry(reminder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ReminderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred while updating the reminder: {ex.Message}" });
            }
        }

        // DELETE: api/Reminder/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> DeleteReminder(string id)
        {
            var reminder = await _context.Reminders.FindAsync(id);
            if (reminder == null)
            {
                return NotFound($"Reminder with ID {id} not found");
            }

            _context.Reminders.Remove(reminder);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/Reminder/{id}/SendNow
        [HttpPatch("{id}/SendNow")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> SendReminderNow(string id)
        {
            var reminder = await _context.Reminders
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Include(r => r.Booking)
                .Include(r => r.TreatmentProcess)
                .FirstOrDefaultAsync(r => r.ReminderId == id);

            if (reminder == null)
            {
                return NotFound($"Reminder with ID {id} not found");
            }

            // Gửi email ngay lập tức nếu có cấu hình email
            if (reminder.IsEmailNotification)
            {
                await SendReminderEmailAsync(reminder);
            }

            // Cập nhật trạng thái và thời gian gửi
            reminder.Status = "Sent";
            reminder.SentTime = DateTime.Now;
            _context.Entry(reminder).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Reminder sent successfully" });
        }

        // GET: api/Reminder/Upcoming
        [HttpGet("Upcoming")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReminderDTO>>> GetUpcomingReminders([FromQuery] int days = 7)
        {
            var endDate = DateTime.Now.AddDays(days);
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            IQueryable<Reminder> query = _context.Reminders
                .Where(r => r.ScheduledTime >= DateTime.Now && r.ScheduledTime <= endDate && r.Status == "Pending")
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Include(r => r.Booking)
                .Include(r => r.TreatmentProcess);

            // Filter based on user role
            if (currentUserRole == "Patient")
            {
                var patientIds = await _context.Patients
                    .Where(p => p.UserId == currentUserId)
                    .Select(p => p.PatientId)
                    .ToListAsync();

                query = query.Where(r => patientIds.Contains(r.PatientId));
            }
            else if (currentUserRole == "Doctor")
            {
                var doctorIds = await _context.Doctors
                    .Where(d => d.UserId == currentUserId)
                    .Select(d => d.DoctorId)
                    .ToListAsync();

                query = query.Where(r => doctorIds.Contains(r.DoctorId));
            }

            var reminders = await query.ToListAsync();
            return Ok(reminders.Select(r => MapToDTO(r)));
        }

        private async Task<bool> SendReminderEmailAsync(Reminder reminder)
        {
            try
            {
                // Chuẩn bị thông tin người nhận
                string recipientEmail = null;
                string recipientName = null;

                if (reminder.Patient != null)
                {
                    recipientEmail = reminder.Patient.Email;
                    recipientName = reminder.Patient.Name;
                }
                else if (!string.IsNullOrEmpty(reminder.PatientId))
                {
                    var patient = await _context.Patients.FindAsync(reminder.PatientId);
                    if (patient != null)
                    {
                        recipientEmail = patient.Email;
                        recipientName = patient.Name;
                    }
                }

                if (string.IsNullOrEmpty(recipientEmail))
                {
                    return false;
                }

                // Tạo nội dung email
                string subject = $"Reminder: {reminder.Title}";
                string body = $@"
                    <h2>Reminder from Infertility Treatment Management System</h2>
                    <p>Dear {recipientName},</p>
                    <p>This is a reminder about: <strong>{reminder.Title}</strong></p>
                    <p>{reminder.Description}</p>
                    <p>Scheduled Time: {reminder.ScheduledTime.ToString("dddd, dd MMMM yyyy HH:mm")}</p>
                ";

                // Thêm thông tin booking nếu có
                if (reminder.Booking != null || !string.IsNullOrEmpty(reminder.BookingId))
                {
                    var booking = reminder.Booking;
                    if (booking == null && !string.IsNullOrEmpty(reminder.BookingId))
                    {
                        booking = await _context.Bookings
                            .Include(b => b.Doctor)
                            .Include(b => b.Service)
                            .Include(b => b.Slot)
                            .FirstOrDefaultAsync(b => b.BookingId == reminder.BookingId);
                    }

                    if (booking != null)
                    {
                        body += $@"
                            <h3>Appointment Details:</h3>
                            <p>Date: {booking.DateBooking.ToString("dddd, dd MMMM yyyy")}</p>
                            <p>Time: {booking.Slot?.StartTime} - {booking.Slot?.EndTime}</p>
                            <p>Doctor: {booking.Doctor?.DoctorName}</p>
                            <p>Service: {booking.Service?.Name}</p>
                        ";
                    }
                }

                body += $@"
                    <p>If you have any questions, please contact our support team.</p>
                    <p>Best regards,<br>Infertility Treatment Management Team</p>
                ";

                // Gửi email
                await _emailService.SendEmailAsync(recipientEmail, subject, body);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending reminder email: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> ReminderExists(string id)
        {
            return await _context.Reminders.AnyAsync(r => r.ReminderId == id);
        }

        // Helper method to map Reminder entity to ReminderDTO
        private ReminderDTO MapToDTO(Reminder reminder)
        {
            var dto = new ReminderDTO
            {
                ReminderId = reminder.ReminderId,
                Title = reminder.Title,
                Description = reminder.Description,
                PatientId = reminder.PatientId,
                DoctorId = reminder.DoctorId,
                BookingId = reminder.BookingId,
                TreatmentProcessId = reminder.TreatmentProcessId,
                ScheduledTime = reminder.ScheduledTime,
                SentTime = reminder.SentTime,
                ReminderType = reminder.ReminderType,
                Status = reminder.Status,
                IsRepeating = reminder.IsRepeating,
                RepeatPattern = reminder.RepeatPattern,
                IsEmailNotification = reminder.IsEmailNotification,
                IsSmsNotification = reminder.IsSmsNotification,
                CreateDate = reminder.CreateDate
            };

            // Map related entities if loaded
            if (reminder.Patient != null)
            {
                dto.Patient = new PatientBasicDTO
                {
                    PatientId = reminder.Patient.PatientId,
                    Name = reminder.Patient.Name,
                    Phone = reminder.Patient.Phone,
                    Email = reminder.Patient.Email,
                    Gender = reminder.Patient.Gender,
                    DateOfBirth = reminder.Patient.DateOfBirth.HasValue ? 
                        DateOnly.FromDateTime(reminder.Patient.DateOfBirth.Value) : null
                };
            }

            if (reminder.Doctor != null)
            {
                dto.Doctor = new DoctorBasicDTO
                {
                    DoctorId = reminder.Doctor.DoctorId,
                    DoctorName = reminder.Doctor.DoctorName,
                    Specialization = reminder.Doctor.Specialization,
                    Phone = reminder.Doctor.Phone,
                    Email = reminder.Doctor.Email
                };
            }

            if (reminder.Booking != null)
            {
                dto.Booking = new BookingBasicDTO
                {
                    BookingId = reminder.Booking.BookingId,
                    DateBooking = reminder.Booking.DateBooking,
                    Description = reminder.Booking.Description,
                    Note = reminder.Booking.Note
                };
            }

            if (reminder.TreatmentProcess != null)
            {
                dto.TreatmentProcess = new TreatmentProcessBasicDTO
                {
                    TreatmentProcessId = reminder.TreatmentProcess.TreatmentProcessId,
                    Method = reminder.TreatmentProcess.Method,
                    ScheduledDate = reminder.TreatmentProcess.ScheduledDate.HasValue ? 
                        DateOnly.FromDateTime(reminder.TreatmentProcess.ScheduledDate.Value) : null,
                    ActualDate = reminder.TreatmentProcess.ActualDate.HasValue ? 
                        DateOnly.FromDateTime(reminder.TreatmentProcess.ActualDate.Value) : null,
                    Result = reminder.TreatmentProcess.Result,
                    Status = reminder.TreatmentProcess.Status
                };
            }

            return dto;
        }
    }
}
