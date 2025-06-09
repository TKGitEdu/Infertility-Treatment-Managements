using Infertility_Treatment_Managements.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infertility_Treatment_Managements.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public RoleController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        /*
         GET api/Role: Lấy danh sách tất cả vai trò.
         GET api/Role/{id}: Lấy vai trò theo ID, trả 404 nếu không tìm thấy.
         POST api/Role: Tạo vai trò mới, trả 201 kèm dữ liệu.
         PUT api/Role/{id}: Cập nhật vai trò, trả 204 nếu thành công, 400 nếu ID không khớp, 404 nếu không tìm thấy.
         DELETE api/Role/{id}: Xóa vai trò, trả 204 nếu thành công, 404 nếu không tìm thấy.
         */


        // GET: api/Role
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetRole()
        {
            return await _context.Roles.ToListAsync();
        }

        // GET: api/Role/5

        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRole(int id)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == id);
            if (role == null) return NotFound();
            return role;
        }

        // POST: api/Role
        [HttpPost]
        public async Task<ActionResult<Role>> PostRole(RoleCreateDTO roleCreateDTO)
        {
            // Create a new Role entity from the DTO
            var role = new Role
            {
                RoleName = roleCreateDTO.RoleName
            };

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRole), new { id = role.RoleId }, role);
        }

        // PUT: api/Role/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRole(int id, RoleUpdateDTO roleUpdateDTO)
        {
            if (id != roleUpdateDTO.RoleId) return BadRequest();

            var role = await _context.Roles.FindAsync(id);
            if (role == null) return NotFound();

            // Update the role properties
            role.RoleName = roleUpdateDTO.RoleName;

            _context.Entry(role).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await RoleExistsAsync(id)) return NotFound();
                throw;
            }
            return NoContent();
        }

        // DELETE: api/Role/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                var role = await _context.Roles.FindAsync(id);
                if (role == null)
                {
                    return NotFound(new { message = $"Role with ID {id} not found." });
                }

                if (await _context.Users.AnyAsync(u => u.RoleId == id))
                {
                    return BadRequest(new { message = "Cannot delete role because it is referenced by existing users." });
                }

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(400, new { message = "Failed to delete role due to database constraints.", error = ex.Message });
            }
            catch (Exception ex)
            {
                // Xử lý các lỗi khác
                return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
            }
        }

        private async Task<bool> RoleExistsAsync(int id)
        {
            return await _context.Roles.AnyAsync(r => r.RoleId == id);
        }
    }
}