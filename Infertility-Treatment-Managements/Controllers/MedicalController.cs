using Infertility_Treatment_Managements.DTOs;
using Infertility_Treatment_Managements.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infertility_Treatment_Managements.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public MedicalController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        // GET: api/Medical/treatment-medications
        [HttpGet("treatment-medications")]
        public async Task<ActionResult<IEnumerable<TreatmentMedication>>> GetAll()
        {
            return Ok(await _context.TreatmentMedications.ToListAsync());
        }

        // GET: api/Medical/treatment-medications/{id}
        [HttpGet("treatment-medications/{id}")]
        public async Task<ActionResult<TreatmentMedication>> GetById(string id)
        {
            var medication = await _context.TreatmentMedications.FindAsync(id);
            if (medication == null)
                return NotFound();
            return Ok(medication);
        }

        // POST: api/Medical/treatment-medications
        [HttpPost("treatment-medications")]
        public async Task<ActionResult<TreatmentMedication>> Create([FromBody] MedicalDTO medicalDTO)
        {
            if (medicalDTO == null)
                return BadRequest("Dữ liệu không hợp lệ.");

            if (
                string.IsNullOrWhiteSpace(medicalDTO.DrugType) ||
                string.IsNullOrWhiteSpace(medicalDTO.DrugName))
            {
                return BadRequest("Thiếu thông tin DrugType hoặc DrugName bắt buộc.");
            }

            if (string.IsNullOrWhiteSpace(medicalDTO.MedicationId))
                medicalDTO.MedicationId = "MED_" + Guid.NewGuid().ToString("N").Substring(0, 8);

            if (medicalDTO.Description != null && medicalDTO.Description.Length > 500)
                return BadRequest("Mô tả không được vượt quá 500 ký tự.");
            TreatmentMedication treatmentedication = new TreatmentMedication
            {
                MedicationId = medicalDTO.MedicationId,
                TreatmentPlanId = medicalDTO.TreatmentPlanId == "" ? null : medicalDTO.TreatmentPlanId,
                DrugType = medicalDTO.DrugType,
                DrugName = medicalDTO.DrugName,
                Description = medicalDTO.Description
            };
            _context.TreatmentMedications.Add(treatmentedication);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = medicalDTO.MedicationId }, medicalDTO);
        }

        // PUT: api/Medical/treatment-medications/{id}
        [HttpPut("treatment-medications/{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] MedicalDTO medicalDTO)
        {
            if (medicalDTO == null || id != medicalDTO.MedicationId)
                return BadRequest("Dữ liệu không hợp lệ.");

            if (string.IsNullOrWhiteSpace(medicalDTO.TreatmentPlanId) ||
                string.IsNullOrWhiteSpace(medicalDTO.DrugType) ||
                string.IsNullOrWhiteSpace(medicalDTO.DrugName))
            {
                return BadRequest("Thiếu thông tin bắt buộc.");
            }

            if (medicalDTO.Description != null && medicalDTO.Description.Length > 500)
                return BadRequest("Mô tả không được vượt quá 500 ký tự.");

            var existing = await _context.TreatmentMedications.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.TreatmentPlanId = medicalDTO.TreatmentPlanId;
            existing.DrugType = medicalDTO.DrugType;
            existing.DrugName = medicalDTO.DrugName;
            existing.Description = medicalDTO.Description;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Medical/treatment-medications/{id}
        [HttpDelete("treatment-medications/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var medication = await _context.TreatmentMedications.FindAsync(id);
            if (medication == null)
                return NotFound();

            _context.TreatmentMedications.Remove(medication);
            await _context.SaveChangesAsync();
            return Ok("xóa rồi");
        }
    }
}
