using Infertility_Treatment_Managements.DTOs;
using Infertility_Treatment_Managements.Helpers;
using Infertility_Treatment_Managements.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServiceDTO = Infertility_Treatment_Managements.DTOs.ServiceDTO;

namespace Infertility_Treatment_Managements.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminServiceController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public AdminServiceController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        // GET: api/AdminService
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceDTO>>> GetAllServices()
        {
            var services = await _context.Services
                .Include(s => s.BookingsFk)
                .ToListAsync();

            return Ok(services.Select(s => s.ToDTO()));
        }

        // GET: api/AdminService/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceDTO>> GetServiceById(string id)
        {
            var service = await _context.Services
                .Include(s => s.BookingsFk)
                .FirstOrDefaultAsync(s => s.ServiceId == id);

            if (service == null)
            {
                return NotFound($"Không tìm thấy dịch vụ với ID {id}");
            }

            return Ok(service.ToDTO());
        }

        // POST: api/AdminService
        [HttpPost]
        public async Task<ActionResult<ServiceDTO>> CreateService(ServiceCreateDTO serviceCreateDTO)
        {
            try
            {
                var service = serviceCreateDTO.ToEntity();
                service.ServiceId = "SRV_" + Guid.NewGuid().ToString().Substring(0, 8);

                _context.Services.Add(service);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetServiceById), new { id = service.ServiceId }, service.ToDTO());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi tạo dịch vụ: {ex.Message}");
            }
        }

        // PUT: api/AdminService/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(string id, ServiceUpdateDTO serviceUpdateDTO)
        {
            if (id != serviceUpdateDTO.ServiceId)
            {
                return BadRequest("ID không khớp");
            }

            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound($"Không tìm thấy dịch vụ với ID {id}");
            }

            // Cập nhật thông tin
            service.Name = serviceUpdateDTO.Name;
            service.Description = serviceUpdateDTO.Description;
            service.Price = serviceUpdateDTO.Price;
            service.Status = serviceUpdateDTO.Status;
            service.Category = serviceUpdateDTO.Category;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
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
        }

        // PATCH: api/AdminService/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateServiceStatus(string id, [FromBody] string status)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound($"Không tìm thấy dịch vụ với ID {id}");
            }

            service.Status = status;
            _context.Entry(service).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
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
        }

        // DELETE: api/AdminService/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteService(string id)
        {
            try
            {
                var service = await _context.Services.FindAsync(id);
                if (service == null)
                {
                    return NotFound($"Không tìm thấy dịch vụ với ID {id}");
                }

                // Kiểm tra xem dịch vụ có đang được sử dụng không
                if (await _context.Bookings.AnyAsync(b => b.ServiceId == id))
                {
                    // Thay vì xóa, cập nhật trạng thái thành Inactive
                    service.Status = "Inactive";
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Dịch vụ đã được đánh dấu là không hoạt động vì đang có lịch hẹn sử dụng dịch vụ này." });
                }

                _context.Services.Remove(service);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đã xóa dịch vụ thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi xóa dịch vụ: {ex.Message}");
            }
        }

        // GET: api/AdminService/category/{category}
        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<ServiceDTO>>> GetServicesByCategory(string category)
        {
            var services = await _context.Services
                .Where(s => s.Category == category)
                .Include(s => s.BookingsFk)
                .ToListAsync();

            return Ok(services.Select(s => s.ToDTO()));
        }

        // GET: api/AdminService/status/{status}
        [HttpGet("status/{status}")]
        public async Task<ActionResult<IEnumerable<ServiceDTO>>> GetServicesByStatus(string status)
        {
            var services = await _context.Services
                .Where(s => s.Status == status)
                .Include(s => s.BookingsFk)
                .ToListAsync();

            return Ok(services.Select(s => s.ToDTO()));
        }

        private async Task<bool> ServiceExists(string id)
        {
            return await _context.Services.AnyAsync(s => s.ServiceId == id);
        }
    }
}