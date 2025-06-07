using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Infertility_Treatment_Managements.DTOs;
using Infertility_Treatment_Managements.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infertility_Treatment_Managements.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SlotController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public SlotController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        // GET: api/Slot
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SlotDTO>>> GetSlot()
        {
            var slots = await _context.Slot
                .Include(s => s.Bookings)
                .ToListAsync();

            return slots.Select(s => s.ToDTO()).ToList();
        }

        // GET: api/Slot/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SlotDTO>> GetSlot(int id)
        {
            var slot = await _context.Slot
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.SlotId == id);

            if (slot == null) return NotFound();

            return slot.ToDTO();
        }

        // POST: api/Slot
        [HttpPost]
        public async Task<ActionResult<SlotDTO>> PostSlot(SlotCreateDTO slotCreateDTO)
        {
            var slot = slotCreateDTO.ToEntity();
            _context.Slot.Add(slot);
            await _context.SaveChangesAsync();

            // Reload the slot with its relations for returning
            var createdSlot = await _context.Slot
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.SlotId == slot.SlotId);

            return CreatedAtAction(nameof(GetSlot), new { id = slot.SlotId }, createdSlot.ToDTO());
        }

        // PUT: api/Slot/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSlot(int id, SlotUpdateDTO slotUpdateDTO)
        {
            if (id != slotUpdateDTO.SlotId) return BadRequest();

            var slot = await _context.Slot.FindAsync(id);
            if (slot == null) return NotFound();

            slotUpdateDTO.UpdateEntity(slot);
            _context.Entry(slot).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await SlotExistsAsync(id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Slot/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSlot(int id)
        {
            var slot = await _context.Slot.FindAsync(id);
            if (slot == null) return NotFound();

            _context.Slot.Remove(slot);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> SlotExistsAsync(int id)
        {
            return await _context.Slot.AnyAsync(s => s.SlotId == id);
        }
    }
}