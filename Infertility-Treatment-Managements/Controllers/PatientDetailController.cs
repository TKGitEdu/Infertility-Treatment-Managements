using Infertility_Treatment_Managements.DTOs;
using Infertility_Treatment_Managements.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infertility_Treatment_Managements.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infertility_Treatment_Managements.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientDetailController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public PatientDetailController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }
        //Tuy nhiên, nếu bạn muốn một phương thức mới để lấy tất cả kế hoạch điều trị của 
        //một bệnh nhân qua tất cả chi tiết bệnh nhân của họ, 
        //    bạn có thể thêm phương thức này:

        // GET: api/PatientDetail/Patient/{patientId}/TreatmentPlans
        [HttpGet("Patient/{patientId}/TreatmentPlans")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<TreatmentPlanDTO>>> GetAllTreatmentPlansByPatient(string patientId)
        {
            var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == patientId);
            if (!patientExists)
            {
                return NotFound($"Patient with ID {patientId} not found");
            }

            // Lấy tất cả chi tiết bệnh nhân của bệnh nhân
            var patientDetailIds = await _context.PatientDetails
                .Where(pd => pd.PatientId == patientId)
                .Select(pd => pd.PatientDetailId)
                .ToListAsync();

            if (!patientDetailIds.Any())
            {
                return Ok(new List<TreatmentPlanDTO>());
            }

            // Lấy tất cả kế hoạch điều trị cho các chi tiết bệnh nhân này
            var treatmentPlans = await _context.TreatmentPlans
                .Where(tp => patientDetailIds.Contains(tp.PatientDetailId))
                .Include(tp => tp.Doctor)
                .Include(tp => tp.PatientDetail)
                    .ThenInclude(pd => pd.Patient)
                .Include(tp => tp.TreatmentProcesses)
                .ToListAsync();

            var result = treatmentPlans.Select(tp => new TreatmentPlanDTO
            {
                TreatmentPlanId = tp.TreatmentPlanId,
                DoctorId = tp.DoctorId,
                ServiceId = tp.ServiceId,
                Method = tp.Method,
                PatientDetailId = tp.PatientDetailId,
                StartDate = tp.StartDate.HasValue ? DateOnly.FromDateTime(tp.StartDate.Value) : null,
                EndDate = tp.EndDate.HasValue ? DateOnly.FromDateTime(tp.EndDate.Value) : null,
                Status = tp.Status,
                TreatmentDescription = tp.TreatmentDescription,
                Giaidoan = tp.Giaidoan, // Thêm trường Giaidoan
                Doctor = tp.Doctor != null ? new DoctorBasicDTO
                {
                    DoctorId = tp.Doctor.DoctorId,
                    DoctorName = tp.Doctor.DoctorName,
                    Specialization = tp.Doctor.Specialization,
                    Phone = tp.Doctor.Phone,
                    Email = tp.Doctor.Email
                } : null,
                PatientDetail = tp.PatientDetail != null ? new PatientDetailBasicDTO
                {
                    PatientDetailId = tp.PatientDetail.PatientDetailId,
                    PatientId = tp.PatientDetail.PatientId,
                    TreatmentStatus = tp.PatientDetail.TreatmentStatus,
                    Patient = tp.PatientDetail.Patient != null ? new PatientBasicDTO
                    {
                        PatientId = tp.PatientDetail.Patient.PatientId,
                        Name = tp.PatientDetail.Patient.Name,
                        Email = tp.PatientDetail.Patient.Email,
                        Phone = tp.PatientDetail.Patient.Phone,
                        Gender = tp.PatientDetail.Patient.Gender,
                        DateOfBirth = tp.PatientDetail.Patient.DateOfBirth.HasValue ?
                            DateOnly.FromDateTime(tp.PatientDetail.Patient.DateOfBirth.Value) : null
                    } : null
                } : null,
                TreatmentProcesses = tp.TreatmentProcesses.Select(tpr => new TreatmentProcessBasicDTO
                {
                    TreatmentProcessId = tpr.TreatmentProcessId,
                    Method = tp.Method,
                    ScheduledDate = tpr.ScheduledDate.HasValue ? DateOnly.FromDateTime(tpr.ScheduledDate.Value) : null,
                    ActualDate = tpr.ScheduledDate.HasValue ? DateOnly.FromDateTime(tpr.ScheduledDate.Value) : null,
                    Result = tpr.Result,
                    Status = tpr.Status
                }).ToList()
            }).ToList();

            return Ok(result);
        }
        // GET: api/PatientDetail/User/{userId}/PatientId
        [HttpGet("User/{userId}/PatientId")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> GetPatientIdByUserId(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("UserId is required");
            }

            var patient = await _context.Patients
                .Where(p => p.UserId == userId)
                .Select(p => new { p.PatientId })
                .FirstOrDefaultAsync();

            if (patient == null)
            {
                return NotFound($"No patient found with UserId: {userId}");
            }

            return Ok(patient.PatientId);
        }

        // GET: api/PatientDetail/Patient/{patientId}/PatientDetailId
        // Lấy PatientDetailId đầu tiên theo PatientId
        [HttpGet("Patient/{patientId}/PatientDetailId")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> GetPatientDetailIdByPatientId(string patientId)
        {
            if (string.IsNullOrEmpty(patientId))
            {
                return BadRequest("PatientId is required");
            }

            var patientDetail = await _context.PatientDetails
                .Where(pd => pd.PatientId == patientId)
                .Select(pd => pd.PatientDetailId)
                .FirstOrDefaultAsync();

            if (patientDetail == null)
            {
                return NotFound($"No PatientDetail found with PatientId: {patientId}");
            }

            return Ok(patientDetail);
        }


    }
}