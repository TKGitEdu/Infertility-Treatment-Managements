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
    public class TreatmentPlanController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public TreatmentPlanController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        // GET: api/TreatmentPlan
        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<IEnumerable<TreatmentPlanDTO>>> GetTreatmentPlans()
        {
            var treatmentPlans = await _context.TreatmentPlans
                .Include(tp => tp.Doctor)
                .Include(tp => tp.PatientDetail)
                    .ThenInclude(pd => pd.Patient)
                .Include(tp => tp.TreatmentProcesses)
                .ToListAsync();

            var result = treatmentPlans.Select(tp => new TreatmentPlanDTO
            {
                TreatmentPlanId = tp.TreatmentPlanId,
                DoctorId = tp.DoctorId,
                Method = tp.Method,
                PatientDetailId = tp.PatientDetailId,
                StartDate = tp.StartDate.HasValue ? DateOnly.FromDateTime(tp.StartDate.Value) : null,
                EndDate = tp.EndDate.HasValue ? DateOnly.FromDateTime(tp.EndDate.Value) : null,
                Status = tp.Status,
                TreatmentDescription = tp.TreatmentDescription,
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
                    TreatmentStatus = tp.PatientDetail.TreatmentStatus
                } : null,
                TreatmentProcesses = tp.TreatmentProcesses.Select(tpr => new TreatmentProcessBasicDTO
                {
                    TreatmentProcessId = tpr.TreatmentProcessId,
                    Method = tpr.Method,
                    ScheduledDate = tpr.ScheduledDate.HasValue ? DateOnly.FromDateTime(tpr.ScheduledDate.Value) : null,
                    ActualDate = tpr.ActualDate.HasValue ? DateOnly.FromDateTime(tpr.ActualDate.Value) : null,
                    Result = tpr.Result,
                    Status = tpr.Status
                }).ToList()
            }).ToList();

            return Ok(result);
        }

        // GET: api/TreatmentPlan/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<TreatmentPlanDTO>> GetTreatmentPlan(string id)
        {
            var treatmentPlan = await _context.TreatmentPlans
                .Include(tp => tp.Doctor)
                .Include(tp => tp.PatientDetail)
                    .ThenInclude(pd => pd.Patient)
                .Include(tp => tp.TreatmentProcesses)
                .FirstOrDefaultAsync(tp => tp.TreatmentPlanId == id);

            if (treatmentPlan == null)
            {
                return NotFound($"Treatment plan with ID {id} not found");
            }

            // Kiểm tra quyền truy cập
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            var currentUserRole = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            if (currentUserRole != "Admin" && currentUserRole != "Doctor")
            {
                // Nếu không phải Admin hoặc Doctor, kiểm tra xem người dùng có phải là bệnh nhân của kế hoạch này không
                if (treatmentPlan.PatientDetail?.Patient?.UserId != currentUserId)
                {
                    return Forbid("You do not have permission to view this treatment plan");
                }
            }

            var result = new TreatmentPlanDTO
            {
                TreatmentPlanId = treatmentPlan.TreatmentPlanId,
                DoctorId = treatmentPlan.DoctorId,
                Method = treatmentPlan.Method,
                PatientDetailId = treatmentPlan.PatientDetailId,
                StartDate = treatmentPlan.StartDate.HasValue ? DateOnly.FromDateTime(treatmentPlan.StartDate.Value) : null,
                EndDate = treatmentPlan.EndDate.HasValue ? DateOnly.FromDateTime(treatmentPlan.EndDate.Value) : null,
                Status = treatmentPlan.Status,
                TreatmentDescription = treatmentPlan.TreatmentDescription,
                Doctor = treatmentPlan.Doctor != null ? new DoctorBasicDTO
                {
                    DoctorId = treatmentPlan.Doctor.DoctorId,
                    DoctorName = treatmentPlan.Doctor.DoctorName,
                    Specialization = treatmentPlan.Doctor.Specialization,
                    Phone = treatmentPlan.Doctor.Phone,
                    Email = treatmentPlan.Doctor.Email
                } : null,
                PatientDetail = treatmentPlan.PatientDetail != null ? new PatientDetailBasicDTO
                {
                    PatientDetailId = treatmentPlan.PatientDetail.PatientDetailId,
                    PatientId = treatmentPlan.PatientDetail.PatientId,
                    TreatmentStatus = treatmentPlan.PatientDetail.TreatmentStatus
                } : null,
                TreatmentProcesses = treatmentPlan.TreatmentProcesses.Select(tpr => new TreatmentProcessBasicDTO
                {
                    TreatmentProcessId = tpr.TreatmentProcessId,
                    Method = tpr.Method,
                    ScheduledDate = tpr.ScheduledDate.HasValue ? DateOnly.FromDateTime(tpr.ScheduledDate.Value) : null,
                    ActualDate = tpr.ActualDate.HasValue ? DateOnly.FromDateTime(tpr.ActualDate.Value) : null,
                    Result = tpr.Result,
                    Status = tpr.Status
                }).ToList()
            };

            return Ok(result);
        }

        // GET: api/TreatmentPlan/ByPatient/{patientId}
        [HttpGet("ByPatient/{patientId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TreatmentPlanDTO>>> GetTreatmentPlansByPatient(string patientId)
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
                    return Forbid("You do not have permission to view these treatment plans");
                }
            }

            // Lấy PatientDetail của bệnh nhân
            var patientDetails = await _context.PatientDetails
                .Where(pd => pd.PatientId == patientId)
                .Select(pd => pd.PatientDetailId)
                .ToListAsync();

            if (!patientDetails.Any())
            {
                return NotFound($"No patient details found for patient ID {patientId}");
            }

            // Lấy các kế hoạch điều trị liên quan đến PatientDetail
            var treatmentPlans = await _context.TreatmentPlans
                .Where(tp => patientDetails.Contains(tp.PatientDetailId))
                .Include(tp => tp.Doctor)
                .Include(tp => tp.PatientDetail)
                    .ThenInclude(pd => pd.Patient)
                .Include(tp => tp.TreatmentProcesses)
                .ToListAsync();

            var result = treatmentPlans.Select(tp => new TreatmentPlanDTO
            {
                TreatmentPlanId = tp.TreatmentPlanId,
                DoctorId = tp.DoctorId,
                Method = tp.Method,
                PatientDetailId = tp.PatientDetailId,
                StartDate = tp.StartDate.HasValue ? DateOnly.FromDateTime(tp.StartDate.Value) : null,
                EndDate = tp.EndDate.HasValue ? DateOnly.FromDateTime(tp.EndDate.Value) : null,
                Status = tp.Status,
                TreatmentDescription = tp.TreatmentDescription,
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
                    TreatmentStatus = tp.PatientDetail.TreatmentStatus
                } : null,
                TreatmentProcesses = tp.TreatmentProcesses.Select(tpr => new TreatmentProcessBasicDTO
                {
                    TreatmentProcessId = tpr.TreatmentProcessId,
                    Method = tpr.Method,
                    ScheduledDate = tpr.ScheduledDate.HasValue ? DateOnly.FromDateTime(tpr.ScheduledDate.Value) : null,
                    ActualDate = tpr.ActualDate.HasValue ? DateOnly.FromDateTime(tpr.ActualDate.Value) : null,
                    Result = tpr.Result,
                    Status = tpr.Status
                }).ToList()
            }).ToList();

            return Ok(result);
        }

        // GET: api/TreatmentPlan/ByDoctor/{doctorId}
        [HttpGet("ByDoctor/{doctorId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TreatmentPlanDTO>>> GetTreatmentPlansByDoctor(string doctorId)
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
                    return Forbid("You do not have permission to view these treatment plans");
                }
            }

            var treatmentPlans = await _context.TreatmentPlans
                .Where(tp => tp.DoctorId == doctorId)
                .Include(tp => tp.Doctor)
                .Include(tp => tp.PatientDetail)
                    .ThenInclude(pd => pd.Patient)
                .Include(tp => tp.TreatmentProcesses)
                .ToListAsync();

            var result = treatmentPlans.Select(tp => new TreatmentPlanDTO
            {
                TreatmentPlanId = tp.TreatmentPlanId,
                DoctorId = tp.DoctorId,
                Method = tp.Method,
                PatientDetailId = tp.PatientDetailId,
                StartDate = tp.StartDate.HasValue ? DateOnly.FromDateTime(tp.StartDate.Value) : null,
                EndDate = tp.EndDate.HasValue ? DateOnly.FromDateTime(tp.EndDate.Value) : null,
                Status = tp.Status,
                TreatmentDescription = tp.TreatmentDescription,
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
                    TreatmentStatus = tp.PatientDetail.TreatmentStatus
                } : null,
                TreatmentProcesses = tp.TreatmentProcesses.Select(tpr => new TreatmentProcessBasicDTO
                {
                    TreatmentProcessId = tpr.TreatmentProcessId,
                    Method = tpr.Method,
                    ScheduledDate = tpr.ScheduledDate.HasValue ? DateOnly.FromDateTime(tpr.ScheduledDate.Value) : null,
                    ActualDate = tpr.ActualDate.HasValue ? DateOnly.FromDateTime(tpr.ActualDate.Value) : null,
                    Result = tpr.Result,
                    Status = tpr.Status
                }).ToList()
            }).ToList();

            return Ok(result);
        }

        // POST: api/TreatmentPlan
        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<TreatmentPlanDTO>> CreateTreatmentPlan(TreatmentPlanCreateDTO treatmentPlanCreateDTO)
        {
            try
            {
                // Validate doctor
                var doctor = await _context.Doctors.FindAsync(treatmentPlanCreateDTO.DoctorId);
                if (doctor == null)
                {
                    return BadRequest($"Doctor with ID {treatmentPlanCreateDTO.DoctorId} not found");
                }

                // Validate patient detail
                var patientDetail = await _context.PatientDetails.FindAsync(treatmentPlanCreateDTO.PatientDetailId);
                if (patientDetail == null)
                {
                    return BadRequest($"Patient detail with ID {treatmentPlanCreateDTO.PatientDetailId} not found");
                }

                // Create treatment plan
                var treatmentPlan = new TreatmentPlan
                {
                    TreatmentPlanId = "TP_" + Guid.NewGuid().ToString().Substring(0, 8),
                    DoctorId = treatmentPlanCreateDTO.DoctorId,
                    Method = treatmentPlanCreateDTO.Method,
                    PatientDetailId = treatmentPlanCreateDTO.PatientDetailId,
                    StartDate = treatmentPlanCreateDTO.StartDate.HasValue ? 
                        new DateTime(treatmentPlanCreateDTO.StartDate.Value.Year, 
                                    treatmentPlanCreateDTO.StartDate.Value.Month, 
                                    treatmentPlanCreateDTO.StartDate.Value.Day) : null,
                    EndDate = treatmentPlanCreateDTO.EndDate.HasValue ? 
                        new DateTime(treatmentPlanCreateDTO.EndDate.Value.Year, 
                                    treatmentPlanCreateDTO.EndDate.Value.Month, 
                                    treatmentPlanCreateDTO.EndDate.Value.Day) : null,
                    Status = treatmentPlanCreateDTO.Status ?? "Active",
                    TreatmentDescription = treatmentPlanCreateDTO.TreatmentDescription
                };

                _context.TreatmentPlans.Add(treatmentPlan);
                await _context.SaveChangesAsync();

                // Update PatientDetail status
                patientDetail.TreatmentStatus = "InTreatment";
                _context.Entry(patientDetail).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                // Return the created treatment plan
                var result = new TreatmentPlanDTO
                {
                    TreatmentPlanId = treatmentPlan.TreatmentPlanId,
                    DoctorId = treatmentPlan.DoctorId,
                    Method = treatmentPlan.Method,
                    PatientDetailId = treatmentPlan.PatientDetailId,
                    StartDate = treatmentPlan.StartDate.HasValue ? DateOnly.FromDateTime(treatmentPlan.StartDate.Value) : null,
                    EndDate = treatmentPlan.EndDate.HasValue ? DateOnly.FromDateTime(treatmentPlan.EndDate.Value) : null,
                    Status = treatmentPlan.Status,
                    TreatmentDescription = treatmentPlan.TreatmentDescription
                };

                return CreatedAtAction(nameof(GetTreatmentPlan), new { id = treatmentPlan.TreatmentPlanId }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred while creating the treatment plan: {ex.Message}" });
            }
        }

        // PUT: api/TreatmentPlan/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> UpdateTreatmentPlan(string id, TreatmentPlanUpdateDTO treatmentPlanUpdateDTO)
        {
            if (id != treatmentPlanUpdateDTO.TreatmentPlanId)
            {
                return BadRequest("ID mismatch");
            }

            // Validate doctor
            var doctor = await _context.Doctors.FindAsync(treatmentPlanUpdateDTO.DoctorId);
            if (doctor == null)
            {
                return BadRequest($"Doctor with ID {treatmentPlanUpdateDTO.DoctorId} not found");
            }

            // Validate patient detail
            var patientDetail = await _context.PatientDetails.FindAsync(treatmentPlanUpdateDTO.PatientDetailId);
            if (patientDetail == null)
            {
                return BadRequest($"Patient detail with ID {treatmentPlanUpdateDTO.PatientDetailId} not found");
            }

            // Get existing treatment plan
            var treatmentPlan = await _context.TreatmentPlans.FindAsync(id);
            if (treatmentPlan == null)
            {
                return NotFound($"Treatment plan with ID {id} not found");
            }

            // Update treatment plan
            treatmentPlan.DoctorId = treatmentPlanUpdateDTO.DoctorId;
            treatmentPlan.Method = treatmentPlanUpdateDTO.Method;
            treatmentPlan.PatientDetailId = treatmentPlanUpdateDTO.PatientDetailId;
            treatmentPlan.StartDate = treatmentPlanUpdateDTO.StartDate.HasValue ? 
                new DateTime(treatmentPlanUpdateDTO.StartDate.Value.Year, 
                            treatmentPlanUpdateDTO.StartDate.Value.Month, 
                            treatmentPlanUpdateDTO.StartDate.Value.Day) : null;
            treatmentPlan.EndDate = treatmentPlanUpdateDTO.EndDate.HasValue ? 
                new DateTime(treatmentPlanUpdateDTO.EndDate.Value.Year, 
                            treatmentPlanUpdateDTO.EndDate.Value.Month, 
                            treatmentPlanUpdateDTO.EndDate.Value.Day) : null;
            treatmentPlan.Status = treatmentPlanUpdateDTO.Status;
            treatmentPlan.TreatmentDescription = treatmentPlanUpdateDTO.TreatmentDescription;

            _context.Entry(treatmentPlan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Update PatientDetail status if treatment plan status is "Completed"
                if (treatmentPlanUpdateDTO.Status == "Completed")
                {
                    patientDetail.TreatmentStatus = "Completed";
                    _context.Entry(patientDetail).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TreatmentPlanExists(id))
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
                return StatusCode(500, new { message = $"An error occurred while updating the treatment plan: {ex.Message}" });
            }
        }

        // DELETE: api/TreatmentPlan/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTreatmentPlan(string id)
        {
            var treatmentPlan = await _context.TreatmentPlans.FindAsync(id);
            if (treatmentPlan == null)
            {
                return NotFound($"Treatment plan with ID {id} not found");
            }

            // Check if there are related treatment processes
            var hasProcesses = await _context.TreatmentProcesses.AnyAsync(tp => tp.TreatmentPlanId == id);
            if (hasProcesses)
            {
                return BadRequest("Cannot delete treatment plan because it has related treatment processes");
            }

            _context.TreatmentPlans.Remove(treatmentPlan);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PATCH: api/TreatmentPlan/{id}/UpdateStatus
        [HttpPatch("{id}/UpdateStatus")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> UpdateTreatmentPlanStatus(string id, [FromBody] string status)
        {
            var treatmentPlan = await _context.TreatmentPlans.FindAsync(id);
            if (treatmentPlan == null)
            {
                return NotFound($"Treatment plan with ID {id} not found");
            }

            treatmentPlan.Status = status;
            _context.Entry(treatmentPlan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                // Update PatientDetail status if treatment plan status is "Completed"
                if (status == "Completed")
                {
                    var patientDetail = await _context.PatientDetails.FindAsync(treatmentPlan.PatientDetailId);
                    if (patientDetail != null)
                    {
                        patientDetail.TreatmentStatus = "Completed";
                        _context.Entry(patientDetail).State = EntityState.Modified;
                        await _context.SaveChangesAsync();
                    }
                }

                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TreatmentPlanExists(id))
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
                return StatusCode(500, new { message = $"An error occurred while updating the treatment plan status: {ex.Message}" });
            }
        }

        private async Task<bool> TreatmentPlanExists(string id)
        {
            return await _context.TreatmentPlans.AnyAsync(tp => tp.TreatmentPlanId == id);
        }
    }
}
