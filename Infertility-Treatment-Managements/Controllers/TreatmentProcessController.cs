using Infertility_Treatment_Managements.DTOs;
using Infertility_Treatment_Managements.Helpers;
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
    public class TreatmentProcessController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public TreatmentProcessController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        // GET: api/TreatmentProcess
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<TreatmentProcessDTO>>> GetTreatmentProcesses()
        {
            var treatmentProcesses = await _context.TreatmentProcesses
                .Include(tp => tp.PatientDetail)
                    .ThenInclude(pd => pd.Patient)
                .Include(tp => tp.TreatmentPlan)
                .ToListAsync();

            var result = treatmentProcesses.Select(tp => MapToDTO(tp)).ToList();
            return Ok(result);
        }

        // GET: api/TreatmentProcess/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<TreatmentProcessDTO>> GetTreatmentProcess(string id)
        {
            var treatmentProcess = await _context.TreatmentProcesses
                .Include(tp => tp.PatientDetail)
                    .ThenInclude(pd => pd.Patient)
                .Include(tp => tp.TreatmentPlan)
                .FirstOrDefaultAsync(tp => tp.TreatmentProcessId == id);

            if (treatmentProcess == null)
            {
                return NotFound($"Treatment process with ID {id} not found");
            }

            // Kiểm tra quyền truy cập
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            if (currentUserRole != "Admin" && currentUserRole != "Doctor")
            {
                // Nếu không phải Admin hoặc Doctor, kiểm tra xem người dùng có phải là bệnh nhân của quy trình này không
                var patient = treatmentProcess.PatientDetail?.Patient;
                if (patient == null || patient.UserId != currentUserId)
                {
                    return Forbid("You do not have permission to view this treatment process");
                }
            }

            var result = MapToDTO(treatmentProcess);
            return Ok(result);
        }

        // NEW: GET: api/TreatmentProcess/patient/{patientId}
        [HttpGet("patient/{patientId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TreatmentProcessDTO>>> GetTreatmentProcessesByPatient(string patientId)
        {
            // Validate patient
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == patientId);

            if (patient == null)
            {
                return NotFound($"Patient with ID {patientId} not found");
            }

            // Kiểm tra quyền truy cập
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            if (currentUserRole != "Admin" && currentUserRole != "Doctor")
            {
                // Nếu không phải Admin hoặc Doctor, kiểm tra xem người dùng có phải là bệnh nhân này không
                if (patient.UserId != currentUserId)
                {
                    return Forbid("You do not have permission to view these treatment processes");
                }
            }

            // Lấy PatientDetail của bệnh nhân
            var patientDetails = await _context.PatientDetails
                .Where(pd => pd.PatientId == patientId)
                .Select(pd => pd.PatientDetailId)
                .ToListAsync();

            // Lấy các TreatmentProcess liên quan đến PatientDetail
            var treatmentProcesses = await _context.TreatmentProcesses
                .Where(tp => patientDetails.Contains(tp.PatientDetailId))
                .Include(tp => tp.PatientDetail)
                    .ThenInclude(pd => pd.Patient)
                .Include(tp => tp.TreatmentPlan)
                .ToListAsync();

            var result = treatmentProcesses.Select(tp => MapToDTO(tp)).ToList();
            return Ok(result);
        }

        // Helper method to check if a treatment process exists
        private async Task<bool> TreatmentProcessExists(string id)
        {
            return await _context.TreatmentProcesses.AnyAsync(tp => tp.TreatmentProcessId == id);
        }

        // Helper method to map TreatmentProcess entity to TreatmentProcessDTO
        private TreatmentProcessDTO MapToDTO(TreatmentProcess tp)
        {
            var dto = new TreatmentProcessDTO
            {
                TreatmentProcessId = tp.TreatmentProcessId,
                PatientDetailId = tp.PatientDetailId,
                ScheduledDate = tp.ScheduledDate.HasValue ? DateOnly.FromDateTime(tp.ScheduledDate.Value) : null,
                ActualDate = tp.ActualDate.HasValue ? DateOnly.FromDateTime(tp.ActualDate.Value) : null,
                Result = tp.Result,
                Status = tp.Status
            };

            if (tp.PatientDetail != null)
            {
                var patientDetailDto = new PatientDetailBasicDTO
                {
                    PatientDetailId = tp.PatientDetail.PatientDetailId,
                    PatientId = tp.PatientDetail.PatientId,
                    TreatmentStatus = tp.PatientDetail.TreatmentStatus
                };

                if (tp.PatientDetail.Patient != null)
                {
                    patientDetailDto.Patient = new PatientBasicDTO
                    {
                        PatientId = tp.PatientDetail.Patient.PatientId,
                        Name = tp.PatientDetail.Patient.Name,
                        Phone = tp.PatientDetail.Patient.Phone,
                        Email = tp.PatientDetail.Patient.Email,
                        Gender = tp.PatientDetail.Patient.Gender,
                        DateOfBirth = tp.PatientDetail.Patient.DateOfBirth.HasValue ? 
                            DateOnly.FromDateTime(tp.PatientDetail.Patient.DateOfBirth.Value) : null
                    };
                }

                dto.PatientDetail = patientDetailDto;
            }

            return dto;
        }
    }
}