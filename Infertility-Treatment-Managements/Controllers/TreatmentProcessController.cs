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
        [Authorize(Roles = "Admin,Doctor")]
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
        }        // GET: api/TreatmentProcess/ByPatientDetail/{patientDetailId}
        [HttpGet("ByPatientDetail/{patientDetailId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TreatmentProcessDTO>>> GetTreatmentProcessesByPatientDetail(string patientDetailId)
        {
            // Validate patient detail
            var patientDetail = await _context.PatientDetails
                .Include(pd => pd.Patient)
                .FirstOrDefaultAsync(pd => pd.PatientDetailId == patientDetailId);

            if (patientDetail == null)
            {
                return NotFound($"Patient detail with ID {patientDetailId} not found");
            }

            // Kiểm tra quyền truy cập
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            if (currentUserRole != "Admin" && currentUserRole != "Doctor")
            {
                // Nếu không phải Admin hoặc Doctor, kiểm tra xem người dùng có phải là bệnh nhân này không
                var patient = patientDetail.Patient;
                if (patient == null || patient.UserId != currentUserId)
                {
                    return Forbid("You do not have permission to view these treatment processes");
                }
            }

            var treatmentProcesses = await _context.TreatmentProcesses
                .Where(tp => tp.PatientDetailId == patientDetailId)
                .Include(tp => tp.PatientDetail)
                    .ThenInclude(pd => pd.Patient)
                .Include(tp => tp.TreatmentPlan)
                .ToListAsync();

            var result = treatmentProcesses.Select(tp => MapToDTO(tp)).ToList();
            return Ok(result);
        }        // GET: api/TreatmentProcess/ByTreatmentPlan/{treatmentPlanId}
        [HttpGet("ByTreatmentPlan/{treatmentPlanId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TreatmentProcessDTO>>> GetTreatmentProcessesByTreatmentPlan(string treatmentPlanId)
        {
            // Validate treatment plan
            var treatmentPlan = await _context.TreatmentPlans
                .Include(tp => tp.PatientDetail)
                    .ThenInclude(pd => pd.Patient)
                .FirstOrDefaultAsync(tp => tp.TreatmentPlanId == treatmentPlanId);

            if (treatmentPlan == null)
            {
                return NotFound($"Treatment plan with ID {treatmentPlanId} not found");
            }

            // Kiểm tra quyền truy cập
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            if (currentUserRole != "Admin" && currentUserRole != "Doctor")
            {
                // Nếu không phải Admin hoặc Doctor, kiểm tra xem người dùng có phải là bệnh nhân của kế hoạch này không
                var patient = treatmentPlan.PatientDetail?.Patient;
                if (patient == null || patient.UserId != currentUserId)
                {
                    return Forbid("You do not have permission to view these treatment processes");
                }
            }

            var treatmentProcesses = await _context.TreatmentProcesses
                .Where(tp => tp.TreatmentPlanId == treatmentPlanId)
                .Include(tp => tp.PatientDetail)
                    .ThenInclude(pd => pd.Patient)
                .Include(tp => tp.TreatmentPlan)
                .ToListAsync();

            var result = treatmentProcesses.Select(tp => MapToDTO(tp)).ToList();
            return Ok(result);
        }

        // POST: api/TreatmentProcess
        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<TreatmentProcessDTO>> CreateTreatmentProcess(TreatmentProcessCreateDTO treatmentProcessCreateDTO)
        {
            try
            {
                // Validate patient detail
                var patientDetail = await _context.PatientDetails.FindAsync(treatmentProcessCreateDTO.PatientDetailId);
                if (patientDetail == null)
                {
                    return BadRequest($"Patient detail with ID {treatmentProcessCreateDTO.PatientDetailId} not found");
                }

                // Create treatment process
                var treatmentProcess = new TreatmentProcess
                {
                    TreatmentProcessId = "TPR_" + Guid.NewGuid().ToString().Substring(0, 8),
                    Method = treatmentProcessCreateDTO.Method,
                    PatientDetailId = treatmentProcessCreateDTO.PatientDetailId,
                    ScheduledDate = treatmentProcessCreateDTO.ScheduledDate.HasValue ? 
                        new DateTime(treatmentProcessCreateDTO.ScheduledDate.Value.Year, 
                                    treatmentProcessCreateDTO.ScheduledDate.Value.Month, 
                                    treatmentProcessCreateDTO.ScheduledDate.Value.Day) : null,
                    ActualDate = treatmentProcessCreateDTO.ActualDate.HasValue ? 
                        new DateTime(treatmentProcessCreateDTO.ActualDate.Value.Year, 
                                    treatmentProcessCreateDTO.ActualDate.Value.Month, 
                                    treatmentProcessCreateDTO.ActualDate.Value.Day) : null,
                    Result = treatmentProcessCreateDTO.Result,
                    Status = treatmentProcessCreateDTO.Status ?? "Scheduled"
                };

                _context.TreatmentProcesses.Add(treatmentProcess);
                await _context.SaveChangesAsync();

                // Kiểm tra có TreatmentPlan nào active cho PatientDetail này không
                var activeTreatmentPlan = await _context.TreatmentPlans
                    .FirstOrDefaultAsync(tp => tp.PatientDetailId == patientDetail.PatientDetailId && tp.Status == "Active");

                // Nếu có, liên kết TreatmentProcess với TreatmentPlan
                if (activeTreatmentPlan != null)
                {
                    treatmentProcess.TreatmentPlanId = activeTreatmentPlan.TreatmentPlanId;
                    _context.Entry(treatmentProcess).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }

                // Return the created treatment process
                var result = new TreatmentProcessDTO
                {
                    TreatmentProcessId = treatmentProcess.TreatmentProcessId,
                    Method = treatmentProcess.Method,
                    PatientDetailId = treatmentProcess.PatientDetailId,
                    ScheduledDate = treatmentProcess.ScheduledDate.HasValue ? DateOnly.FromDateTime(treatmentProcess.ScheduledDate.Value) : null,
                    ActualDate = treatmentProcess.ActualDate.HasValue ? DateOnly.FromDateTime(treatmentProcess.ActualDate.Value) : null,
                    Result = treatmentProcess.Result,
                    Status = treatmentProcess.Status
                };

                return CreatedAtAction(nameof(GetTreatmentProcess), new { id = treatmentProcess.TreatmentProcessId }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred while creating the treatment process: {ex.Message}" });
            }
        }

        // PUT: api/TreatmentProcess/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> UpdateTreatmentProcess(string id, TreatmentProcessUpdateDTO treatmentProcessUpdateDTO)
        {
            if (id != treatmentProcessUpdateDTO.TreatmentProcessId)
            {
                return BadRequest("ID mismatch");
            }

            // Validate patient detail
            var patientDetail = await _context.PatientDetails.FindAsync(treatmentProcessUpdateDTO.PatientDetailId);
            if (patientDetail == null)
            {
                return BadRequest($"Patient detail with ID {treatmentProcessUpdateDTO.PatientDetailId} not found");
            }

            // Get existing treatment process
            var treatmentProcess = await _context.TreatmentProcesses.FindAsync(id);
            if (treatmentProcess == null)
            {
                return NotFound($"Treatment process with ID {id} not found");
            }

            // Update treatment process
            treatmentProcess.Method = treatmentProcessUpdateDTO.Method;
            treatmentProcess.PatientDetailId = treatmentProcessUpdateDTO.PatientDetailId;
            treatmentProcess.ScheduledDate = treatmentProcessUpdateDTO.ScheduledDate.HasValue ? 
                new DateTime(treatmentProcessUpdateDTO.ScheduledDate.Value.Year, 
                            treatmentProcessUpdateDTO.ScheduledDate.Value.Month, 
                            treatmentProcessUpdateDTO.ScheduledDate.Value.Day) : null;
            treatmentProcess.ActualDate = treatmentProcessUpdateDTO.ActualDate.HasValue ? 
                new DateTime(treatmentProcessUpdateDTO.ActualDate.Value.Year, 
                            treatmentProcessUpdateDTO.ActualDate.Value.Month, 
                            treatmentProcessUpdateDTO.ActualDate.Value.Day) : null;
            treatmentProcess.Result = treatmentProcessUpdateDTO.Result;
            treatmentProcess.Status = treatmentProcessUpdateDTO.Status;

            _context.Entry(treatmentProcess).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TreatmentProcessExists(id))
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
                return StatusCode(500, new { message = $"An error occurred while updating the treatment process: {ex.Message}" });
            }
        }

        // DELETE: api/TreatmentProcess/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> DeleteTreatmentProcess(string id)
        {
            var treatmentProcess = await _context.TreatmentProcesses.FindAsync(id);
            if (treatmentProcess == null)
            {
                return NotFound($"Treatment process with ID {id} not found");
            }

            // Kiểm tra nếu quy trình đã bắt đầu hoặc hoàn thành
            if (treatmentProcess.ActualDate != null || treatmentProcess.Status == "Completed")
            {
                return BadRequest("Cannot delete a treatment process that has already started or completed");
            }

            _context.TreatmentProcesses.Remove(treatmentProcess);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/TreatmentProcess/{id}/UpdateStatus
        [HttpPatch("{id}/UpdateStatus")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> UpdateTreatmentProcessStatus(string id, [FromBody] string status)
        {
            var treatmentProcess = await _context.TreatmentProcesses.FindAsync(id);
            if (treatmentProcess == null)
            {
                return NotFound($"Treatment process with ID {id} not found");
            }

            treatmentProcess.Status = status;
            
            // Nếu status là "Completed", cập nhật ActualDate là ngày hiện tại nếu chưa có
            if (status == "Completed" && treatmentProcess.ActualDate == null)
            {
                treatmentProcess.ActualDate = DateTime.Now;
            }

            _context.Entry(treatmentProcess).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TreatmentProcessExists(id))
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
                return StatusCode(500, new { message = $"An error occurred while updating the treatment process status: {ex.Message}" });
            }
        }        private async Task<bool> TreatmentProcessExists(string id)
        {
            return await _context.TreatmentProcesses.AnyAsync(tp => tp.TreatmentProcessId == id);
        }

        // Helper method to map TreatmentProcess entity to TreatmentProcessDTO
        private TreatmentProcessDTO MapToDTO(TreatmentProcess tp)
        {
            var dto = new TreatmentProcessDTO
            {
                TreatmentProcessId = tp.TreatmentProcessId,
                Method = tp.Method,
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
