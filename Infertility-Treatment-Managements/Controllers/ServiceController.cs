using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Infertility_Treatment_Management.DTOs;
using Infertility_Treatment_Management.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infertility_Treatment_Management.Controllers
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
        public async Task<ActionResult<IEnumerable<ServiceDTO>>> GetServices()
        {
            var services = await _context.Service
                .Include(s => s.Booking)
                .ToListAsync();

            return services.Select(s => s.ToDTO()).ToList();
        }

        // GET: api/Service/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceDTO>> GetService(int id)
        {
            var service = await _context.Service
                .Include(s => s.Booking)
                .FirstOrDefaultAsync(s => s.ServiceId == id);

            if (service == null)
            {
                return NotFound();
            }

            return service.ToDTO();
        }

        // GET: api/Service/Status/Active
        [HttpGet("Status/{status}")]
        public async Task<ActionResult<IEnumerable<ServiceDTO>>> GetServicesByStatus(string status)
        {
            var services = await _context.Service
                .Where(s => s.Status == status)
                .Include(s => s.Booking)
                .ToListAsync();

            return services.Select(s => s.ToDTO()).ToList();
        }

        // POST: api/Service
        [HttpPost]
        public async Task<ActionResult<ServiceDTO>> CreateService(ServiceCreateDTO serviceCreateDTO)
        {
            var service = serviceCreateDTO.ToEntity();
            _context.Service.Add(service);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetService), new { id = service.ServiceId }, service.ToDTO());
        }

        // PUT: api/Service/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(int id, ServiceUpdateDTO serviceUpdateDTO)
        {
            if (id != serviceUpdateDTO.ServiceId)
            {
                return BadRequest("ID mismatch");
            }

            var service = await _context.Service.FindAsync(id);
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
        public async Task<IActionResult> UpdateServiceStatus(int id, [FromBody] string status)
        {
            var service = await _context.Service.FindAsync(id);
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
        public async Task<IActionResult> DeleteService(int id)
        {
            var service = await _context.Service.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }

            // Check if this service is associated with any bookings
            var hasBooking = await _context.Booking.AnyAsync(b => b.ServiceId == id);
            if (hasBooking)
            {
                return BadRequest("Cannot delete service that is associated with bookings");
            }

            _context.Service.Remove(service);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> ServiceExists(int id)
        {
            return await _context.Service.AnyAsync(s => s.ServiceId == id);
        }
    }
}