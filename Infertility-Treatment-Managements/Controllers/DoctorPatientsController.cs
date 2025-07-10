using Infertility_Treatment_Managements.DTOs;
using Infertility_Treatment_Managements.Helpers;
using Infertility_Treatment_Managements.Models;
using Infertility_Treatment_Managements.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    [Authorize(Roles = "Doctor")]
    public class DoctorPatientsController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;
        private readonly IEmailService _emailService;

        public DoctorPatientsController(InfertilityTreatmentManagementContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        /// <summary>
        /// Lấy danh sách bệnh nhân của bác sĩ dựa trên userId
        /// </summary>
        /// <param name="userId">ID của user (bác sĩ)</param>
        /// <returns>Danh sách bệnh nhân đã có lịch hẹn với bác sĩ</returns>
        [HttpGet("patients")]
        public async Task<ActionResult<IEnumerable<PatientDTO>>> GetPatientsByUserId([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("UserId is required");
            }

            // Lấy thông tin bác sĩ từ UserId
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                return NotFound($"Doctor with User ID {userId} not found");
            }

            string doctorId = doctor.DoctorId;

            // Lấy danh sách bệnh nhân dựa trên lịch hẹn với bác sĩ
            var patients = await _context.Bookings
                .Where(b => b.DoctorId == doctorId)
                .Select(b => b.Patient)
                .Distinct()
                .Include(p => p.User)
                .Include(p => p.PatientDetails)
                .ToListAsync();

            return Ok(patients.Select(p => p.ToDTO()));
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một bệnh nhân
        /// </summary>
        /// <param name="patientId">ID của bệnh nhân</param>
        /// <returns>Thông tin chi tiết của bệnh nhân</returns>
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<PatientDTO>> GetPatientDetails(string patientId)
        {
            if (string.IsNullOrEmpty(patientId))
            {
                return BadRequest("PatientId is required");
            }

            var patient = await _context.Patients
                .Include(p => p.User)
                .Include(p => p.PatientDetails)
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (patient == null)
            {
                return NotFound($"Patient with ID {patientId} not found");
            }

            return Ok(patient.ToDTO());
        }

        /// <summary>
        /// Lấy lịch sử điều trị của bệnh nhân
        /// </summary>
        /// <param name="patientId">ID của bệnh nhân</param>
        /// <returns>Danh sách các bản ghi điều trị và kế hoạch điều trị của bệnh nhân</returns>
        [HttpGet("patient/{patientId}/treatment-history")]
        public async Task<ActionResult<object>> GetPatientTreatmentHistory(string patientId)
        {
            if (string.IsNullOrEmpty(patientId))
            {
                return BadRequest("PatientId is required");
            }

            // Kiểm tra bệnh nhân tồn tại
            var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == patientId);
            if (!patientExists)
            {
                return NotFound($"Patient with ID {patientId} not found");
            }

            // Lấy danh sách PatientDetail của bệnh nhân
            var patientDetails = await _context.PatientDetails
                .Where(pd => pd.PatientId == patientId)
                .ToListAsync();

            if (!patientDetails.Any())
            {
                return Ok(new { TreatmentPlans = new List<object>(), TreatmentProcesses = new List<object>() });
            }

            // Lấy ID của các PatientDetail
            var patientDetailIds = patientDetails.Select(pd => pd.PatientDetailId).ToList();

            // Lấy danh sách TreatmentPlan dựa trên PatientDetailId
            var treatmentPlans = await _context.TreatmentPlans
                .Include(tp => tp.Doctor)
                .Where(tp => patientDetailIds.Contains(tp.PatientDetailId))
                .OrderByDescending(tp => tp.StartDate)
                .ToListAsync();

            // Lấy ID của các TreatmentPlan
            var treatmentPlanIds = treatmentPlans.Select(tp => tp.TreatmentPlanId).ToList();

            // Lấy danh sách TreatmentProcess dựa trên TreatmentPlanId
            var treatmentProcesses = await _context.TreatmentProcesses
                .Include(tp => tp.Doctor)
                .Include(tp => tp.TreatmentPlan)
                .Where(tp => treatmentPlanIds.Contains(tp.TreatmentPlanId))
                .OrderByDescending(tp => tp.ScheduledDate)
                .ToListAsync();

            // Trả về kết quả
            return Ok(new
            {
                TreatmentPlans = treatmentPlans.Select(tp => new
                {
                    TreatmentPlanId = tp.TreatmentPlanId,
                    DoctorId = tp.DoctorId,
                    DoctorName = tp.Doctor?.DoctorName,
                    PatientDetailId = tp.PatientDetailId,
                    StartDate = tp.StartDate,
                    EndDate = tp.EndDate,
                    Status = tp.Status,
                    TreatmentDescription = tp.TreatmentDescription,
                    Method = tp.Method
                }),
                TreatmentProcesses = treatmentProcesses.Select(tp => new
                {
                    TreatmentProcessId = tp.TreatmentProcessId,
                    TreatmentPlanId = tp.TreatmentPlanId,
                    DoctorId = tp.DoctorId,
                    DoctorName = tp.Doctor?.DoctorName,
                    ProcessDate = tp.ScheduledDate,
                    Result = tp.Result,
                    Status = tp.Status,
                    TreatmentPlanDescription = tp.TreatmentPlan?.TreatmentDescription
                })
            });
        }

        /// <summary>
        /// Lấy kết quả xét nghiệm của bệnh nhân
        /// </summary>
        /// <param name="patientId">ID của bệnh nhân</param>
        /// <returns>Danh sách các kết quả xét nghiệm của bệnh nhân</returns>
        [HttpGet("patient/{patientId}/test-results")]
        public async Task<ActionResult<IEnumerable<object>>> GetPatientTestResults(string patientId)
        {
            if (string.IsNullOrEmpty(patientId))
            {
                return BadRequest("PatientId is required");
            }

            // Kiểm tra bệnh nhân tồn tại
            var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == patientId);
            if (!patientExists)
            {
                return NotFound($"Patient with ID {patientId} not found");
            }

            // Lấy danh sách kết quả xét nghiệm của bệnh nhân
            var examinations = await _context.Examinations
                .Where(e => e.PatientId == patientId)
                .OrderByDescending(e => e.ExaminationDate)
                .Select(e => new
                {
                    ExaminationId = e.ExaminationId,
                    BookingId = e.BookingId,
                    PatientId = e.PatientId,
                    DoctorId = e.DoctorId,
                    DoctorName = e.Doctor != null ? e.Doctor.DoctorName : null,
                    ExaminationDate = e.ExaminationDate,
                    ExaminationDescription = e.ExaminationDescription,
                    Result = e.Result,
                    Status = e.Status,
                    Note = e.Note,
                    ServiceName = e.Booking != null && e.Booking.Service != null ? e.Booking.Service.Name : null
                })
                .ToListAsync();

            return Ok(examinations);
        }


        /// <summary>
        /// Cập nhật ghi chú cho bệnh nhân trong phiên khám
        /// </summary>
        /// <param name="patientId">ID của bệnh nhân</param>
        /// <param name="updateNoteDTO">Thông tin ghi chú cần cập nhật</param>
        /// <returns>Thông tin kiểm tra đã cập nhật</returns>
        [HttpPut("patient/{patientId}/note")]
        public async Task<ActionResult<object>> UpdateExaminationNote(string patientId, [FromBody] UpdateExaminationNoteDTO updateNoteDTO)
        {
            if (string.IsNullOrEmpty(patientId))
            {
                return BadRequest("PatientId is required");
            }

            if (string.IsNullOrEmpty(updateNoteDTO.UserId))
            {
                return BadRequest("UserId is required");
            }

            if (string.IsNullOrEmpty(updateNoteDTO.BookingId))
            {
                return BadRequest("BookingId is required");
            }

            // Tìm doctorId từ userId
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == updateNoteDTO.UserId);

            if (doctor == null)
            {
                return NotFound($"Doctor with User ID {updateNoteDTO.UserId} not found");
            }

            string doctorId = doctor.DoctorId;

            // Tìm phiên khám liên quan đến bệnh nhân và lịch hẹn
            var examination = await _context.Examinations
                .FirstOrDefaultAsync(e => e.PatientId == patientId &&
                                        e.BookingId == updateNoteDTO.BookingId &&
                                        e.DoctorId == doctorId);

            if (examination == null)
            {
                // Nếu không có phiên khám, tạo mới
                var booking = await _context.Bookings
                    .FirstOrDefaultAsync(b => b.BookingId == updateNoteDTO.BookingId &&
                                            b.PatientId == patientId &&
                                            b.DoctorId == doctorId);

                if (booking == null)
                {
                    return NotFound("Booking not found");
                }

                examination = new Examination
                {
                    ExaminationId = "EXAM_" + Guid.NewGuid().ToString().Substring(0, 8),
                    BookingId = booking.BookingId,
                    PatientId = patientId,
                    DoctorId = doctorId, // Sử dụng doctorId đã tìm được
                    ExaminationDate = DateTime.Now,
                    ExaminationDescription = "Examination note",
                    Note = updateNoteDTO.Note,
                    Status = "Completed",
                    CreateAt = DateTime.Now
                };

                _context.Examinations.Add(examination);
            }
            else
            {
                // Cập nhật ghi chú cho phiên khám hiện có
                examination.Note = updateNoteDTO.Note;
                examination.CreateAt = DateTime.Now;
                _context.Entry(examination).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                ExaminationId = examination.ExaminationId,
                BookingId = examination.BookingId,
                PatientId = examination.PatientId,
                DoctorId = examination.DoctorId,
                ExaminationDate = examination.ExaminationDate,
                ExaminationDescription = examination.ExaminationDescription,
                Note = examination.Note,
                Status = examination.Status
            });
        }

        /// <summary>
        /// Thêm bản ghi điều trị mới
        /// </summary>
        /// <param name="treatmentProcessDTO">Thông tin bản ghi điều trị mới</param>
        /// <returns>Bản ghi điều trị đã được tạo</returns>
        [HttpPost("treatment-record")]
        public async Task<ActionResult<object>> AddNewTreatmentProcess([FromBody] AddTreatmentProcessDTO treatmentProcessDTO)
        {
            if (string.IsNullOrEmpty(treatmentProcessDTO.DoctorId))
            {
                return BadRequest("DoctorId is required");
            }

            if (string.IsNullOrEmpty(treatmentProcessDTO.PatientId))
            {
                return BadRequest("PatientId is required");
            }

            if (string.IsNullOrEmpty(treatmentProcessDTO.TreatmentPlanId))
            {
                return BadRequest("TreatmentPlanId is required");
            }

            // Kiểm tra kế hoạch điều trị tồn tại và lấy thông tin quy trình
            var treatmentPlan = await _context.TreatmentPlans
                .FirstOrDefaultAsync(tp => tp.TreatmentPlanId == treatmentProcessDTO.TreatmentPlanId);

            if (treatmentPlan == null)
            {
                return NotFound("Treatment plan not found");
            }

            // Lấy TreatmentDescription từ TreatmentPlan
            string treatmentDescription = treatmentPlan.TreatmentDescription;

            // Kiểm tra bệnh nhân tồn tại
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == treatmentProcessDTO.PatientId);

            if (patient == null)
            {
                return NotFound("Patient not found");
            }

            // Tìm PatientDetail
            var patientDetail = await _context.PatientDetails
                .FirstOrDefaultAsync(pd => pd.PatientId == treatmentProcessDTO.PatientId);

            if (patientDetail == null)
            {
                // Tạo mới PatientDetail nếu không tồn tại
                patientDetail = new PatientDetail
                {
                    PatientDetailId = "PATD_" + Guid.NewGuid().ToString().Substring(0, 8),
                    PatientId = treatmentProcessDTO.PatientId,
                    TreatmentStatus = "In Treatment"
                };

                _context.PatientDetails.Add(patientDetail);
                await _context.SaveChangesAsync();
            }

            // Tạo bản ghi điều trị mới
            var treatmentProcess = new TreatmentProcess
            {
                TreatmentProcessId = "TPR_" + Guid.NewGuid().ToString().Substring(0, 8),
                TreatmentPlanId = treatmentProcessDTO.TreatmentPlanId,
                DoctorId = treatmentProcessDTO.DoctorId,
                PatientDetailId = patientDetail.PatientDetailId,
                Result = treatmentProcessDTO.Result,
                Status = treatmentProcessDTO.Status ?? "Pending",
                ScheduledDate = treatmentProcessDTO.ProcessDate ?? DateTime.Now
            };

            _context.TreatmentProcesses.Add(treatmentProcess);
            await _context.SaveChangesAsync();

            // Tạo thông báo cho bệnh nhân
            var notification = new Notification
            {
                NotificationId = "NOTIF_" + Guid.NewGuid().ToString().Substring(0, 8),
                PatientId = treatmentProcessDTO.PatientId,
                DoctorId = treatmentProcessDTO.DoctorId,
                Message = $"Quá trình điều trị mới đã được thêm vào: {treatmentDescription}",
                Time = DateTime.Now,
                Type = "Treatment",
                TreatmentProcessId = treatmentProcess.TreatmentProcessId,
                PatientIsRead = false,
                DoctorIsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Lấy thông tin đầy đủ của quá trình điều trị bao gồm thông tin từ kế hoạch điều trị
            var treatmentPlanDetails = await _context.TreatmentPlans
                .Where(tp => tp.TreatmentPlanId == treatmentProcess.TreatmentPlanId)
                .Select(tp => new { tp.TreatmentDescription, tp.Method })
                .FirstOrDefaultAsync();

            return Ok(new
            {
                TreatmentProcessId = treatmentProcess.TreatmentProcessId,
                TreatmentPlanId = treatmentProcess.TreatmentPlanId,
                DoctorId = treatmentProcess.DoctorId,
                PatientDetailId = treatmentProcess.PatientDetailId,
                ScheduledDate = treatmentProcess.ScheduledDate,
                Result = treatmentProcess.Result,
                Status = treatmentProcess.Status,
                TreatmentDescription = treatmentPlanDetails?.TreatmentDescription,
                Method = treatmentPlanDetails?.Method
            });
        }

        /// <summary>
        /// Lấy danh sách lịch hẹn của một bệnh nhân
        /// </summary>
        /// <param name="patientId">ID của bệnh nhân</param>
        /// <returns>Danh sách lịch hẹn của bệnh nhân</returns>
        [HttpGet("patient/{patientId}/appointments")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetAppointmentsByPatientId(string patientId)
        {
            if (string.IsNullOrEmpty(patientId))
            {
                return BadRequest("PatientId is required");
            }

            // Kiểm tra bệnh nhân tồn tại
            var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == patientId);
            if (!patientExists)
            {
                return NotFound($"Patient with ID {patientId} not found");
            }

            // Lấy danh sách lịch hẹn của bệnh nhân
            var bookings = await _context.Bookings
                .Include(b => b.Service)
                .Include(b => b.Doctor)
                .Include(b => b.Slot)
                .Where(b => b.PatientId == patientId)
                .OrderByDescending(b => b.DateBooking)
                .ToListAsync();

            return Ok(bookings.Select(b => b.ToDTO()));
        }

        /// <summary>
        /// Tạo lịch hẹn mới cho bệnh nhân
        /// </summary>
        /// <param name="bookingDTO">Thông tin lịch hẹn mới</param>
        /// <returns>Thông tin lịch hẹn đã được tạo</returns>
        [HttpPost("booking")]
        public async Task<ActionResult<BookingDTO>> CreateAppointmentForPatient([FromBody] CreateBookingDTO bookingDTO)
        {
            if (string.IsNullOrEmpty(bookingDTO.PatientId))
            {
                return BadRequest("PatientId is required");
            }

            if (string.IsNullOrEmpty(bookingDTO.UserId))
            {
                return BadRequest("UserId is required");
            }

            if (string.IsNullOrEmpty(bookingDTO.ServiceId))
            {
                return BadRequest("ServiceId is required");
            }

            if (string.IsNullOrEmpty(bookingDTO.SlotId))
            {
                return BadRequest("SlotId is required");
            }

            // Kiểm tra bệnh nhân tồn tại
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == bookingDTO.PatientId);

            if (patient == null)
            {
                return NotFound("Patient not found");
            }

            // Tìm bác sĩ từ UserId
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == bookingDTO.UserId);

            if (doctor == null)
            {
                return NotFound($"Doctor with User ID {bookingDTO.UserId} not found");
            }

            string doctorId = doctor.DoctorId;

            // Kiểm tra dịch vụ tồn tại
            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.ServiceId == bookingDTO.ServiceId);

            if (service == null)
            {
                return NotFound("Service not found");
            }

            // Kiểm tra slot tồn tại
            var slot = await _context.Slots
                .FirstOrDefaultAsync(s => s.SlotId == bookingDTO.SlotId);

            if (slot == null)
            {
                return NotFound("Slot not found");
            }

            // Kiểm tra xem slot đã được đặt chưa
            var isSlotTaken = await _context.Bookings
                .AnyAsync(b => b.DoctorId == doctorId &&
                              b.SlotId == bookingDTO.SlotId &&
                              b.DateBooking.Date == bookingDTO.DateBooking.Date);

            if (isSlotTaken)
            {
                return BadRequest("This slot is already booked for the selected date");
            }

            // Tạo booking mới
            var booking = new Booking
            {
                BookingId = "BKG_" + Guid.NewGuid().ToString().Substring(0, 8),
                PatientId = bookingDTO.PatientId,
                DoctorId = doctorId, // Sử dụng doctorId đã tìm được từ userId
                ServiceId = bookingDTO.ServiceId,
                SlotId = bookingDTO.SlotId,
                DateBooking = bookingDTO.DateBooking,
                Description = bookingDTO.Description,
                Note = bookingDTO.Note,
                Status = "confirmed", // Bác sĩ tạo lịch hẹn nên mặc định là đã xác nhận
                CreateAt = DateTime.Now
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Tạo thông báo cho bệnh nhân
            var notification = new Notification
            {
                NotificationId = "NOTIF_" + Guid.NewGuid().ToString().Substring(0, 8),
                PatientId = bookingDTO.PatientId,
                DoctorId = doctorId, // Sử dụng doctorId đã tìm được
                Message = $"Bác sĩ {doctor.DoctorName} đã tạo lịch hẹn mới cho bạn vào ngày {bookingDTO.DateBooking.ToString("dd/MM/yyyy")}",
                Time = DateTime.Now,
                Type = "Booking",
                BookingId = booking.BookingId,
                PatientIsRead = false,
                DoctorIsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Gửi email thông báo cho bệnh nhân
            var emailSubject = "Thông báo: Lịch hẹn mới được tạo";
            var emailBody = $@"
                <h2>Thông báo lịch hẹn mới</h2>
                <p>Kính gửi <b>{patient.Name}</b>,</p>
                <p>Bác sĩ <b>{doctor.DoctorName}</b> đã tạo một lịch hẹn mới cho bạn với thông tin như sau:</p>
                <ul>
                    <li><b>Mã lịch hẹn:</b> {booking.BookingId}</li>
                    <li><b>Dịch vụ:</b> {service.Name}</li>
                    <li><b>Ngày hẹn:</b> {booking.DateBooking:dd/MM/yyyy}</li>
                    <li><b>Thời gian:</b> {slot.StartTime} - {slot.EndTime}</li>
                    <li><b>Mô tả:</b> {booking.Description}</li>
                    <li><b>Ghi chú:</b> {booking.Note ?? "Không có"}</li>
                </ul>
                <p>Vui lòng đến đúng giờ. Nếu bạn cần thay đổi lịch hẹn, vui lòng liên hệ với chúng tôi ít nhất 24 giờ trước thời gian hẹn.</p>
                <p>Trân trọng,<br><b>Phòng khám của chúng tôi</b></p>
            ";

            await _emailService.SendEmailAsync(patient.Email, emailSubject, emailBody);

            // Lấy thông tin đầy đủ của booking
            var createdBooking = await _context.Bookings
                .Include(b => b.Service)
                .Include(b => b.Doctor)
                .Include(b => b.Patient)
                .Include(b => b.Slot)
                .FirstOrDefaultAsync(b => b.BookingId == booking.BookingId);

            return Ok(createdBooking.ToDTO());
        }

        // DTOs for the controller
        public class UpdateExaminationNoteDTO
        {
            public string UserId { get; set; }
            public string BookingId { get; set; }
            public string Note { get; set; }
        }

        public class AddTreatmentProcessDTO
        {
            public string DoctorId { get; set; }
            public string PatientId { get; set; }
            public string TreatmentPlanId { get; set; }
            public DateTime? ProcessDate { get; set; }
            public string Result { get; set; }
            public string Status { get; set; }
        }

        public class CreateBookingDTO
        {
            public string PatientId { get; set; }
            public string UserId { get; set; }
            public string ServiceId { get; set; }
            public string SlotId { get; set; }
            public DateTime DateBooking { get; set; }
            public string Description { get; set; }
            public string Note { get; set; }
        }
    }
}