using Infertility_Treatment_Managements.DTOs;
using Infertility_Treatment_Managements.Models;
using Infertility_Treatment_Managements.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public ReminderController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        // GET: api/Reminder
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReminderDTO>>> GetReminders()
        {
            var reminders = await _context.Reminders
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Include(r => r.Booking)
                .Include(r => r.TreatmentProcess)
                .OrderBy(r => r.ScheduledTime)
                .ToListAsync();

            if (reminders == null || !reminders.Any())
            {
                return NotFound("No reminders found");
            }

            var reminderDTOs = reminders.Select(r => new ReminderDTO
            {
                ReminderId = r.ReminderId,
                Title = r.Title,
                Description = r.Description,
                PatientId = r.PatientId,
                DoctorId = r.DoctorId,
                BookingId = r.BookingId,
                TreatmentProcessId = r.TreatmentProcessId,
                ScheduledTime = r.ScheduledTime,
                SentTime = r.SentTime,
                ReminderType = r.ReminderType,
                Status = r.Status,
                IsRepeating = r.IsRepeating,
                RepeatPattern = r.RepeatPattern,
                IsEmailNotification = r.IsEmailNotification,
                IsSmsNotification = r.IsSmsNotification,
                CreateDate = r.CreateDate,
                Patient = r.Patient != null ? new PatientBasicDTO
                {
                    PatientId = r.Patient.PatientId,
                    Name = r.Patient.Name,
                    Phone = r.Patient.Phone,
                    Email = r.Patient.Email
                } : null,
                Doctor = r.Doctor != null ? new DoctorBasicDTO
                {
                    DoctorId = r.Doctor.DoctorId,
                    DoctorName = r.Doctor.DoctorName,
                    Specialization = r.Doctor.Specialization
                } : null,
                Booking = r.Booking != null ? new BookingBasicDTO
                {
                    BookingId = r.Booking.BookingId,
                    DateBooking = r.Booking.DateBooking,
                    Description = r.Booking.Description,
                    Note = r.Booking.Note
                } : null,
                TreatmentProcess = r.TreatmentProcess != null ? new TreatmentProcessBasicDTO
                {
                    TreatmentProcessId = r.TreatmentProcess.TreatmentProcessId,
                    Method = r.TreatmentProcess.Method,
                    Status = r.TreatmentProcess.Status
                } : null
            }).ToList();

            return Ok(reminderDTOs);
        }

        // GET: api/Reminder/5
        [HttpGet("{id}")]
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

            var reminderDTO = new ReminderDTO
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
                CreateDate = reminder.CreateDate,
                Patient = reminder.Patient != null ? new PatientBasicDTO
                {
                    PatientId = reminder.Patient.PatientId,
                    Name = reminder.Patient.Name,
                    Phone = reminder.Patient.Phone,
                    Email = reminder.Patient.Email
                } : null,
                Doctor = reminder.Doctor != null ? new DoctorBasicDTO
                {
                    DoctorId = reminder.Doctor.DoctorId,
                    DoctorName = reminder.Doctor.DoctorName,
                    Specialization = reminder.Doctor.Specialization
                } : null,
                Booking = reminder.Booking != null ? new BookingBasicDTO
                {
                    BookingId = reminder.Booking.BookingId,
                    DateBooking = reminder.Booking.DateBooking,
                    Description = reminder.Booking.Description,
                    Note = reminder.Booking.Note
                } : null,
                TreatmentProcess = reminder.TreatmentProcess != null ? new TreatmentProcessBasicDTO
                {
                    TreatmentProcessId = reminder.TreatmentProcess.TreatmentProcessId,
                    Method = reminder.TreatmentProcess.Method,
                    Status = reminder.TreatmentProcess.Status
                } : null
            };

            return Ok(reminderDTO);
        }

        // GET: api/Reminder/Patient/{patientId}
        [HttpGet("Patient/{patientId}")]
        public async Task<ActionResult<IEnumerable<ReminderDTO>>> GetRemindersByPatient(string patientId)
        {
            var reminders = await _context.Reminders
                .Include(r => r.Doctor)
                .Include(r => r.Booking)
                .Include(r => r.TreatmentProcess)
                .Where(r => r.PatientId == patientId)
                .OrderBy(r => r.ScheduledTime)
                .ToListAsync();

            if (reminders == null || !reminders.Any())
            {
                return NotFound($"No reminders found for patient with ID {patientId}");
            }

            var reminderDTOs = reminders.Select(r => new ReminderDTO
            {
                ReminderId = r.ReminderId,
                Title = r.Title,
                Description = r.Description,
                PatientId = r.PatientId,
                DoctorId = r.DoctorId,
                BookingId = r.BookingId,
                TreatmentProcessId = r.TreatmentProcessId,
                ScheduledTime = r.ScheduledTime,
                SentTime = r.SentTime,
                ReminderType = r.ReminderType,
                Status = r.Status,
                IsRepeating = r.IsRepeating,
                RepeatPattern = r.RepeatPattern,
                IsEmailNotification = r.IsEmailNotification,
                IsSmsNotification = r.IsSmsNotification,
                CreateDate = r.CreateDate,
                Doctor = r.Doctor != null ? new DoctorBasicDTO
                {
                    DoctorId = r.Doctor.DoctorId,
                    DoctorName = r.Doctor.DoctorName,
                    Specialization = r.Doctor.Specialization
                } : null,
                Booking = r.Booking != null ? new BookingBasicDTO
                {
                    BookingId = r.Booking.BookingId,
                    DateBooking = r.Booking.DateBooking,
                    Description = r.Booking.Description,
                    Note = r.Booking.Note
                } : null,
                TreatmentProcess = r.TreatmentProcess != null ? new TreatmentProcessBasicDTO
                {
                    TreatmentProcessId = r.TreatmentProcess.TreatmentProcessId,
                    Method = r.TreatmentProcess.Method,
                    Status = r.TreatmentProcess.Status
                } : null
            }).ToList();

            return Ok(reminderDTOs);
        }

        // GET: api/Reminder/Upcoming
        [HttpGet("Upcoming")]
        public async Task<ActionResult<IEnumerable<ReminderDTO>>> GetUpcomingReminders()
        {
            var now = DateTime.Now;
            var reminders = await _context.Reminders
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Include(r => r.Booking)
                .Include(r => r.TreatmentProcess)
                .Where(r => r.ScheduledTime > now && r.Status == "Pending")
                .OrderBy(r => r.ScheduledTime)
                .ToListAsync();

            if (reminders == null || !reminders.Any())
            {
                return NotFound("No upcoming reminders found");
            }

            var reminderDTOs = reminders.Select(r => new ReminderDTO
            {
                ReminderId = r.ReminderId,
                Title = r.Title,
                Description = r.Description,
                PatientId = r.PatientId,
                DoctorId = r.DoctorId,
                BookingId = r.BookingId,
                TreatmentProcessId = r.TreatmentProcessId,
                ScheduledTime = r.ScheduledTime,
                SentTime = r.SentTime,
                ReminderType = r.ReminderType,
                Status = r.Status,
                IsRepeating = r.IsRepeating,
                RepeatPattern = r.RepeatPattern,
                IsEmailNotification = r.IsEmailNotification,
                IsSmsNotification = r.IsSmsNotification,
                CreateDate = r.CreateDate,
                Patient = r.Patient != null ? new PatientBasicDTO
                {
                    PatientId = r.Patient.PatientId,
                    Name = r.Patient.Name,
                    Phone = r.Patient.Phone,
                    Email = r.Patient.Email
                } : null,
                Doctor = r.Doctor != null ? new DoctorBasicDTO
                {
                    DoctorId = r.Doctor.DoctorId,
                    DoctorName = r.Doctor.DoctorName,
                    Specialization = r.Doctor.Specialization
                } : null,
                Booking = r.Booking != null ? new BookingBasicDTO
                {
                    BookingId = r.Booking.BookingId,
                    DateBooking = r.Booking.DateBooking,
                    Description = r.Booking.Description,
                    Note = r.Booking.Note
                } : null,
                TreatmentProcess = r.TreatmentProcess != null ? new TreatmentProcessBasicDTO
                {
                    TreatmentProcessId = r.TreatmentProcess.TreatmentProcessId,
                    Method = r.TreatmentProcess.Method,
                    Status = r.TreatmentProcess.Status
                } : null
            }).ToList();

            return Ok(reminderDTOs);
        }

        // POST: api/Reminder
        [HttpPost]
        public async Task<ActionResult<ReminderDTO>> CreateReminder(ReminderCreateDTO reminderDTO)
        {
            // Validate foreign keys if provided
            if (!string.IsNullOrEmpty(reminderDTO.PatientId))
            {
                var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == reminderDTO.PatientId);
                if (!patientExists)
                {
                    return BadRequest($"Patient with ID {reminderDTO.PatientId} does not exist");
                }
            }

            if (!string.IsNullOrEmpty(reminderDTO.DoctorId))
            {
                var doctorExists = await _context.Doctors.AnyAsync(d => d.DoctorId == reminderDTO.DoctorId);
                if (!doctorExists)
                {
                    return BadRequest($"Doctor with ID {reminderDTO.DoctorId} does not exist");
                }
            }

            if (!string.IsNullOrEmpty(reminderDTO.BookingId))
            {
                var bookingExists = await _context.Bookings.AnyAsync(b => b.BookingId == reminderDTO.BookingId);
                if (!bookingExists)
                {
                    return BadRequest($"Booking with ID {reminderDTO.BookingId} does not exist");
                }
            }

            if (!string.IsNullOrEmpty(reminderDTO.TreatmentProcessId))
            {
                var treatmentProcessExists = await _context.TreatmentProcesses.AnyAsync(tp => tp.TreatmentProcessId == reminderDTO.TreatmentProcessId);
                if (!treatmentProcessExists)
                {
                    return BadRequest($"Treatment Process with ID {reminderDTO.TreatmentProcessId} does not exist");
                }
            }

            var reminder = new Reminder
            {
                ReminderId = "RM" + Guid.NewGuid().ToString().Substring(0, 8),
                Title = reminderDTO.Title,
                Description = reminderDTO.Description,
                PatientId = reminderDTO.PatientId,
                DoctorId = reminderDTO.DoctorId,
                BookingId = reminderDTO.BookingId,
                TreatmentProcessId = reminderDTO.TreatmentProcessId,
                ScheduledTime = reminderDTO.ScheduledTime,
                SentTime = null,
                ReminderType = reminderDTO.ReminderType,
                Status = "Pending",
                IsRepeating = reminderDTO.IsRepeating,
                RepeatPattern = reminderDTO.RepeatPattern,
                IsEmailNotification = reminderDTO.IsEmailNotification,
                IsSmsNotification = reminderDTO.IsSmsNotification,
                CreateDate = DateTime.Now
            };

            _context.Reminders.Add(reminder);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"An error occurred while creating the reminder: {ex.Message}");
            }

            var createdReminderDTO = new ReminderDTO
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

            return CreatedAtAction(nameof(GetReminder), new { id = reminder.ReminderId }, createdReminderDTO);
        }

        // PUT: api/Reminder/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReminder(string id, ReminderUpdateDTO reminderDTO)
        {
            if (id != reminderDTO.ReminderId)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            var reminder = await _context.Reminders.FindAsync(id);
            if (reminder == null)
            {
                return NotFound($"Reminder with ID {id} not found");
            }

            // Update properties
            reminder.Title = reminderDTO.Title;
            reminder.Description = reminderDTO.Description;
            reminder.ScheduledTime = reminderDTO.ScheduledTime;
            reminder.Status = reminderDTO.Status;
            reminder.IsRepeating = reminderDTO.IsRepeating;
            reminder.RepeatPattern = reminderDTO.RepeatPattern;
            reminder.IsEmailNotification = reminderDTO.IsEmailNotification;
            reminder.IsSmsNotification = reminderDTO.IsSmsNotification;

            _context.Entry(reminder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReminderExists(id))
                {
                    return NotFound($"Reminder with ID {id} not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PATCH: api/Reminder/5/Status
        [HttpPatch("{id}/Status")]
        public async Task<IActionResult> UpdateReminderStatus(string id, [FromBody] string status)
        {
            var reminder = await _context.Reminders.FindAsync(id);
            if (reminder == null)
            {
                return NotFound($"Reminder with ID {id} not found");
            }

            reminder.Status = status;
            if (status == "Sent")
            {
                reminder.SentTime = DateTime.Now;
            }

            _context.Entry(reminder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReminderExists(id))
                {
                    return NotFound($"Reminder with ID {id} not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Reminder/5
        [HttpDelete("{id}")]
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

        // POST: api/Reminder/Send/{id}
        [HttpPost("Send/{id}")]
        public async Task<IActionResult> SendReminder(string id, [FromServices] IEmailService emailService)
        {
            var reminder = await _context.Reminders
                .Include(r => r.Patient)
                .ThenInclude(p => p.User)
                .Include(r => r.Doctor)
                .Include(r => r.Booking)
                .FirstOrDefaultAsync(r => r.ReminderId == id);

            if (reminder == null)
            {
                return NotFound($"Reminder with ID {id} not found");
            }

            if (reminder.Status != "Pending")
            {
                return BadRequest($"Reminder is not in 'Pending' status. Current status: {reminder.Status}");
            }

            try
            {
                // Only send email if email notification is enabled and patient has an email
                if (reminder.IsEmailNotification &&
                    reminder.Patient?.User?.Email != null)
                {
                    string emailSubject = $"Reminder: {reminder.Title}";
                    string emailBody = $@"
                <h2>Reminder from Infertility Treatment Management</h2>
                <p>Dear {reminder.Patient.Name},</p>
                <p><strong>{reminder.Title}</strong></p>
                <p>{reminder.Description}</p>";

                    if (reminder.Booking != null)
                    {
                        emailBody += $@"
                    <p>Date: {reminder.Booking.DateBooking.ToShortDateString()}</p>";

                        if (reminder.Booking.Doctor != null)
                        {
                            emailBody += $@"<p>Doctor: {reminder.Booking.Doctor.DoctorName}</p>";
                        }
                    }

                    emailBody += @"
                <p>Please contact us if you have any questions.</p>
                <p>Best regards,<br>Infertility Treatment Management Team</p>";

                    await emailService.SendEmailAsync(reminder.Patient.User.Email, emailSubject, emailBody);
                }

                // Update reminder status
                reminder.Status = "Sent";
                reminder.SentTime = DateTime.Now;
                _context.Entry(reminder).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                // Create a new reminder if this is a repeating reminder
                if (reminder.IsRepeating && !string.IsNullOrEmpty(reminder.RepeatPattern))
                {
                    DateTime nextScheduledTime = CalculateNextScheduledTime(reminder.ScheduledTime, reminder.RepeatPattern);

                    var newReminder = new Reminder
                    {
                        ReminderId = "RM" + Guid.NewGuid().ToString().Substring(0, 8),
                        Title = reminder.Title,
                        Description = reminder.Description,
                        PatientId = reminder.PatientId,
                        DoctorId = reminder.DoctorId,
                        BookingId = reminder.BookingId,
                        TreatmentProcessId = reminder.TreatmentProcessId,
                        ScheduledTime = nextScheduledTime,
                        ReminderType = reminder.ReminderType,
                        Status = "Pending",
                        IsRepeating = reminder.IsRepeating,
                        RepeatPattern = reminder.RepeatPattern,
                        IsEmailNotification = reminder.IsEmailNotification,
                        IsSmsNotification = reminder.IsSmsNotification,
                        CreateDate = DateTime.Now
                    };

                    _context.Reminders.Add(newReminder);
                    await _context.SaveChangesAsync();
                }

                return Ok(new { message = "Reminder sent successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while sending the reminder: {ex.Message}");
            }
        }

        private DateTime CalculateNextScheduledTime(DateTime currentScheduledTime, string repeatPattern)
        {
            // Logic to calculate the next scheduled time based on the repeat pattern
            // Example implementation:
            switch (repeatPattern.ToLower())
            {
                case "daily":
                    return currentScheduledTime.AddDays(1);
                case "weekly":
                    return currentScheduledTime.AddDays(7);
                case "monthly":
                    return currentScheduledTime.AddMonths(1);
                default:
                    return currentScheduledTime.AddDays(1); // Default to daily
            }
        }

        private bool ReminderExists(string id)
        {
            return _context.Reminders.Any(e => e.ReminderId == id);
        }
    }
}