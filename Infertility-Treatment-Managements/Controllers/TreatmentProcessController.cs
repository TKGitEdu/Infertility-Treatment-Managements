using Infertility_Treatment_Managements.DTOs;
using Infertility_Treatment_Managements.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infertility_Treatment_Managements.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TreatmentProcessController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _dbContext;

        public TreatmentProcessController(InfertilityTreatmentManagementContext context)
        {
            _dbContext = context;
        }

        // GET: api/TreatmentProcess
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TreatmentProcessDTO>>> GetTreatmentProcesses()
        {
            var treatmentProcesses = await _dbContext.TreatmentProcesses
                .Include(tp => tp.PatientDetail)
                .ThenInclude(pd => pd.Patient)
                .ToListAsync();

            return treatmentProcesses.Select(tp => tp.ToDTO()).ToList();
        }

        // GET: api/TreatmentProcess/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TreatmentProcessDTO>> GetTreatmentProcess(int id)
        {
            var treatmentProcess = await _dbContext.TreatmentProcesses
                .Include(tp => tp.PatientDetail)
                .ThenInclude(pd => pd.Patient)
                .FirstOrDefaultAsync(tp => tp.TreatmentProcessId == id);

            if (treatmentProcess == null)
            {
                return NotFound();
            }

            return treatmentProcess.ToDTO();
        }

        // POST: api/TreatmentProcess
        [HttpPost]
        public async Task<ActionResult<TreatmentProcessDTO>> CreateTreatmentProcess(TreatmentProcessCreateDTO createDTO)
        {
            // Validate if PatientDetail exists
            var patientDetailExists = await _dbContext.PatientDetails.AnyAsync(pd => pd.PatientDetailId == createDTO.PatientDetailId);
            if (!patientDetailExists)
            {
                return BadRequest("The specified PatientDetailId does not exist.");
            }

            var treatmentProcess = createDTO.ToEntity();
            _dbContext.TreatmentProcesses.Add(treatmentProcess);
            await _dbContext.SaveChangesAsync();

            // Reload with related data
            await _dbContext.Entry(treatmentProcess)
                .Reference(tp => tp.PatientDetail)
                .LoadAsync();

            if (treatmentProcess.PatientDetail != null)
            {
                await _dbContext.Entry(treatmentProcess.PatientDetail)
                    .Reference(pd => pd.Patient)
                    .LoadAsync();
            }

            return CreatedAtAction(
                nameof(GetTreatmentProcess),
                new { id = treatmentProcess.TreatmentProcessId },
                treatmentProcess.ToDTO()
            );
        }

        // PUT: api/TreatmentProcess/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTreatmentProcess(int id, TreatmentProcessUpdateDTO updateDTO)
        {
            if (id != updateDTO.TreatmentProcessId)
            {
                return BadRequest("ID mismatch");
            }

            // Validate if PatientDetail exists
            var patientDetailExists = await _dbContext.PatientDetails.AnyAsync(pd => pd.PatientDetailId == updateDTO.PatientDetailId);
            if (!patientDetailExists)
            {
                return BadRequest("The specified PatientDetailId does not exist.");
            }

            var treatmentProcess = await _dbContext.TreatmentProcesses.FindAsync(id);
            if (treatmentProcess == null)
            {
                return NotFound();
            }

            updateDTO.UpdateEntity(treatmentProcess);
            _dbContext.Entry(treatmentProcess).State = EntityState.Modified;

            try
            {
                await _dbContext.SaveChangesAsync();
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

            return NoContent();
        }

        // DELETE: api/TreatmentProcess/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTreatmentProcess(int id)
        {
            var treatmentProcess = await _dbContext.TreatmentProcesses.FindAsync(id);
            if (treatmentProcess == null)
            {
                return NotFound();
            }

            _dbContext.TreatmentProcesses.Remove(treatmentProcess);
            await _dbContext.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> TreatmentProcessExists(int id)
        {
            return await _dbContext.TreatmentProcesses.AnyAsync(tp => tp.TreatmentProcessId == id);
        }
    }
}