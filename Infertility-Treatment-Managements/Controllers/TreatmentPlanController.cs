using Infertility_Treatment_Managements.DTOs;
using Infertility_Treatment_Managements.Models;
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
    public class TreatmentPlanController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public TreatmentPlanController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        // GET: api/TreatmentPlan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TreatmentPlanBasicDTO>>> GetTreatmentPlans()
        {
            var treatmentPlans = await _context.TreatmentPlans
                .Select(tp => new TreatmentPlanBasicDTO
                {
                    TreatmentPlanId = tp.TreatmentPlanId,
                    Method = tp.Method,
                    StartDate = tp.StartDate.HasValue ? DateOnly.FromDateTime(tp.StartDate.Value) : null,
                    EndDate = tp.EndDate.HasValue ? DateOnly.FromDateTime(tp.EndDate.Value) : null,
                    Status = tp.Status,
                    TreatmentDescription = tp.TreatmentDescription
                })
                .ToListAsync();

            return Ok(treatmentPlans);
        }

        // GET: api/TreatmentPlan/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TreatmentPlanBasicDTO>> GetTreatmentPlan(int id)
        {
            var treatmentPlan = await _context.TreatmentPlans
                .Select(tp => new TreatmentPlanBasicDTO
                {
                    TreatmentPlanId = tp.TreatmentPlanId,
                    Method = tp.Method,
                    StartDate = tp.StartDate.HasValue ? DateOnly.FromDateTime(tp.StartDate.Value) : null,
                    EndDate = tp.EndDate.HasValue ? DateOnly.FromDateTime(tp.EndDate.Value) : null,
                    Status = tp.Status,
                    TreatmentDescription = tp.TreatmentDescription
                })
                .FirstOrDefaultAsync(tp => tp.TreatmentPlanId == id);

            if (treatmentPlan == null)
            {
                return NotFound();
            }

            return Ok(treatmentPlan);
        }

        // POST: api/TreatmentPlan
        [HttpPost]
        public async Task<ActionResult<TreatmentPlanDTO>> CreateTreatmentPlan(TreatmentPlanCreateDTO createDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate DoctorId and PatientDetailId
            if (!await _context.Doctors.AnyAsync(d => d.DoctorId == createDTO.DoctorId))
            {
                return BadRequest("Invalid DoctorId");
            }

            if (!await _context.PatientDetails.AnyAsync(pd => pd.PatientDetailId == createDTO.PatientDetailId))
            {
                return BadRequest("Invalid PatientDetailId");
            }

            var treatmentPlan = new TreatmentPlan
            {
                DoctorId = createDTO.DoctorId,
                Method = createDTO.Method,
                PatientDetailId = createDTO.PatientDetailId,
                StartDate = createDTO.StartDate.HasValue ? createDTO.StartDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                EndDate = createDTO.EndDate.HasValue ? createDTO.EndDate.Value.ToDateTime(TimeOnly.MinValue) : null,
                Status = createDTO.Status,
                TreatmentDescription = createDTO.TreatmentDescription
            };

            _context.TreatmentPlans.Add(treatmentPlan);
            await _context.SaveChangesAsync();

            var treatmentPlanDTO = new TreatmentPlanDTO
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

            return CreatedAtAction(nameof(GetTreatmentPlan), new { id = treatmentPlan.TreatmentPlanId }, treatmentPlanDTO);
        }

        // PUT: api/TreatmentPlan/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTreatmentPlan(int id, TreatmentPlanUpdateDTO updateDTO)
        {
            if (id != updateDTO.TreatmentPlanId)
            {
                return BadRequest("TreatmentPlanId mismatch");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var treatmentPlan = await _context.TreatmentPlans.FindAsync(id);
            if (treatmentPlan == null)
            {
                return NotFound();
            }

            // Validate DoctorId and PatientDetailId
            if (!await _context.Doctors.AnyAsync(d => d.DoctorId == updateDTO.DoctorId))
            {
                return BadRequest("Invalid DoctorId");
            }

            if (!await _context.PatientDetails.AnyAsync(pd => pd.PatientDetailId == updateDTO.PatientDetailId))
            {
                return BadRequest("Invalid PatientDetailId");
            }

            // Update properties
            treatmentPlan.DoctorId = updateDTO.DoctorId;
            treatmentPlan.Method = updateDTO.Method;
            treatmentPlan.PatientDetailId = updateDTO.PatientDetailId;
            treatmentPlan.StartDate = updateDTO.StartDate.HasValue ? updateDTO.StartDate.Value.ToDateTime(TimeOnly.MinValue) : null;
            treatmentPlan.EndDate = updateDTO.EndDate.HasValue ? updateDTO.EndDate.Value.ToDateTime(TimeOnly.MinValue) : null;
            treatmentPlan.Status = updateDTO.Status;
            treatmentPlan.TreatmentDescription = updateDTO.TreatmentDescription;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.TreatmentPlans.AnyAsync(tp => tp.TreatmentPlanId == id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/TreatmentPlan/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTreatmentPlan(int id)
        {
            var treatmentPlan = await _context.TreatmentPlans.FindAsync(id);
            if (treatmentPlan == null)
            {
                return NotFound();
            }

            _context.TreatmentPlans.Remove(treatmentPlan);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}