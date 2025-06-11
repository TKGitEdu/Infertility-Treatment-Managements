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

        // GET: api/PatientDetail
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PatientDetailDTO>>> GetPatientDetails()
        {
            var patientDetails = await _context.PatientDetails
                .Include(pd => pd.Patient)
                .Include(pd => pd.TreatmentProcessesFk)
                .ToListAsync();

            return patientDetails.Select(pd => pd.ToDTO()).ToList();
        }

        // GET: api/PatientDetail/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PatientDetailDTO>> GetPatientDetail(int id)
        {
            var patientDetail = await _context.PatientDetails
                .Include(pd => pd.Patient)
                .Include(pd => pd.TreatmentProcessesFk)
                .FirstOrDefaultAsync(pd => pd.PatientDetailId == id);

            if (patientDetail == null)
            {
                return NotFound();
            }

            return patientDetail.ToDTO();
        }

        // GET: api/PatientDetail/Patient/5
        [HttpGet("Patient/{patientId}")]
        public async Task<ActionResult<IEnumerable<PatientDetailDTO>>> GetPatientDetailsByPatient(int patientId)
        {
            var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == patientId);
            if (!patientExists)
            {
                return NotFound("Patient not found");
            }

            var patientDetails = await _context.PatientDetails
                .Where(pd => pd.PatientId == patientId)
                .Include(pd => pd.Patient)
                .Include(pd => pd.TreatmentProcessesFk)
                .ToListAsync();

            return patientDetails.Select(pd => pd.ToDTO()).ToList();
        }

        // GET: api/PatientDetail/Status/Active
        [HttpGet("Status/{status}")]
        public async Task<ActionResult<IEnumerable<PatientDetailDTO>>> GetPatientDetailsByStatus(string status)
        {
            var patientDetails = await _context.PatientDetails
                .Where(pd => pd.TreatmentStatus == status)
                .Include(pd => pd.Patient)
                .Include(pd => pd.TreatmentProcessesFk)
                .ToListAsync();

            return patientDetails.Select(pd => pd.ToDTO()).ToList();
        }

        // POST: api/PatientDetail
        [HttpPost]
        public async Task<ActionResult<PatientDetailDTO>> CreatePatientDetail(PatientDetailCreateDTO patientDetailCreateDTO)
        {
            // Validate patient exists
            var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == patientDetailCreateDTO.PatientId);
            if (!patientExists)
            {
                return BadRequest("Invalid PatientId: Patient does not exist");
            }

            var patientDetail = patientDetailCreateDTO.ToEntity();
            _context.PatientDetails.Add(patientDetail);
            await _context.SaveChangesAsync();

            // Reload with related data for return
            var createdPatientDetail = await _context.PatientDetails
                .Include(pd => pd.Patient)
                .FirstOrDefaultAsync(pd => pd.PatientDetailId == patientDetail.PatientDetailId);

            return CreatedAtAction(nameof(GetPatientDetail), new { id = createdPatientDetail.PatientDetailId }, createdPatientDetail.ToDTO());
        }

        // PUT: api/PatientDetail/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatientDetail(int id, PatientDetailUpdateDTO patientDetailUpdateDTO)
        {
            if (id != patientDetailUpdateDTO.PatientDetailId)
            {
                return BadRequest("ID mismatch");
            }

            var patientDetail = await _context.PatientDetails.FindAsync(id);
            if (patientDetail == null)
            {
                return NotFound();
            }

            // Validate patient exists
            var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == patientDetailUpdateDTO.PatientId);
            if (!patientExists)
            {
                return BadRequest("Invalid PatientId: Patient does not exist");
            }

            patientDetailUpdateDTO.UpdateEntity(patientDetail);
            _context.Entry(patientDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PatientDetailExists(id))
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

        // PATCH: api/PatientDetail/5/UpdateStatus
        [HttpPatch("{id}/UpdateStatus")]
        public async Task<IActionResult> UpdatePatientDetailStatus(int id, [FromBody] string treatmentStatus)
        {
            var patientDetail = await _context.PatientDetails.FindAsync(id);
            if (patientDetail == null)
            {
                return NotFound();
            }

            patientDetail.TreatmentStatus = treatmentStatus;
            _context.Entry(patientDetail).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PatientDetailExists(id))
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

        // DELETE: api/PatientDetail/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatientDetail(int id)
        {
            try
            {
                var patientDetail = await _context.PatientDetails.FindAsync(id);
                if (patientDetail == null)
                {
                    return NotFound(new { message = $"PatientDetail with ID {id} not found." });
                }

                if (await _context.TreatmentProcesses.AnyAsync(tp => tp.PatientDetailId == id))
                {
                    return BadRequest(new { message = "Cannot delete patient detail because it is referenced by existing treatment processes." });
                }

                if (await _context.TreatmentPlans.AnyAsync(tp => tp.PatientDetailId == id))
                {
                    return BadRequest(new { message = "Cannot delete patient detail because it is referenced by existing treatment plans." });
                }

                _context.PatientDetails.Remove(patientDetail);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(400, new { message = "Failed to delete patient detail due to database constraints.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
            }
        }

        private async Task<bool> PatientDetailExists(int id)
        {
            return await _context.PatientDetails.AnyAsync(pd => pd.PatientDetailId == id);
        }
    }
}