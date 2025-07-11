﻿using Infertility_Treatment_Managements.DTOs;
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
    public class ServiceController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public ServiceController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        // GET: api/Service
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DTOs.ServiceDTO>>> GetServices()
        {
            var services = await _context.Services
                .Include(s => s.BookingsFk)
                .ToListAsync();

            return Ok(services.Select(s => s.ToDTO()));
        }

        // Fix for the CS0029 error in the GetService method
        [HttpGet("{id}")]
        public async Task<ActionResult<DTOs.ServiceDTO>> GetService(string id)
        {
            var service = await _context.Services
                .Include(s => s.BookingsFk)
                .FirstOrDefaultAsync(s => s.ServiceId == id);

            if (service == null)
            {
                return NotFound();
            }

            return Ok(service.ToDTO()); // Wrap the result in Ok() to match the expected ActionResult type
        }

        // GET: api/Service/Status/Active
        [HttpGet("Status/{status}")]
        public async Task<ActionResult<IEnumerable<DTOs.ServiceDTO>>> GetServicesByStatus(string status)
        {
            var services = await _context.Services
                .Where(s => s.Status == status)
                .Include(s => s.BookingsFk)
                .ToListAsync();

            return Ok(services.Select(s => s.ToDTO()));
        }

        // POST: api/Service
        [HttpPost]
        public async Task<ActionResult<DTOs.ServiceDTO>> CreateService(ServiceCreateDTO serviceCreateDTO)
        {
            var service = serviceCreateDTO.ToEntity();
            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetService), new { id = service.ServiceId }, service.ToDTO());
        }

        // PUT: api/Service/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(string id, ServiceUpdateDTO serviceUpdateDTO)
        {
            if (id != serviceUpdateDTO.ServiceId)
            {
                return BadRequest("ID mismatch");
            }

            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            serviceUpdateDTO.UpdateEntity(service);
            _context.Entry(service).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ServiceExists(id))
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

        // PATCH: api/Service/5/UpdateStatus
        [HttpPatch("{id}/UpdateStatus")]
        public async Task<IActionResult> UpdateServiceStatus(string id, [FromBody] string status)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            service.Status = status;
            _context.Entry(service).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ServiceExists(id))
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

        // DELETE: api/Service/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteService(string id)
        {
            try
            {
                var service = await _context.Services.FindAsync(id);
                if (service == null)
                {
                    return NotFound(new { message = $"Service with ID {id} not found." });
                }

                if (await _context.Bookings.AnyAsync(b => b.ServiceId == id))
                {
                    return BadRequest(new { message = "Cannot delete service because it is referenced by existing bookings." });
                }

                _context.Services.Remove(service);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(400, new { message = "Failed to delete service due to database constraints.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
            }
        }

        private async Task<bool> ServiceExists(string id)
        {
            return await _context.Services.AnyAsync(s => s.ServiceId == id);
        }

        // Thêm vào ServiceController
        [HttpGet("Category/{category}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DTOs.ServiceDTO>>> GetServicesByCategory(string category)
        {
            var services = await _context.Services
                .Where(s => s.Category == category && s.Status == "Active")
                .Include(s => s.BookingsFk)
                .ToListAsync();

            return Ok(services.Select(s => s.ToDTO()));
        }

    }
}