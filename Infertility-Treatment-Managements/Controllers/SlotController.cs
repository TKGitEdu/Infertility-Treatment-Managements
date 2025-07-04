﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infertility_Treatment_Managements.Models;
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
            var slots = await _context.Slots
                .Include(s => s.Bookings)
                .ToListAsync();

            return slots.Select(s => s.ToDTO()).ToList();
        }

        // GET: api/Slot/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SlotDTO>> GetSlot(string id)
        {
            var slot = await _context.Slots
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.SlotId == id);

            if (slot == null) return NotFound();

            return slot.ToDTO();
        }
        // Thêm endpoint này vào SlotController.cs
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<SlotBasicDTO>>> GetAvailableSlots(
            [FromQuery] string date,
            [FromQuery] string doctorId)
        {
            if (string.IsNullOrEmpty(doctorId) || !DateTime.TryParse(date, out DateTime parsedDate))
            {
                return BadRequest("Invalid doctor ID or date format");
            }

            // Lấy tất cả slot
            var allSlots = await _context.Slots
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            // Lấy danh sách slot đã được đặt cho bác sĩ này vào ngày này
            var bookedSlots = await _context.Bookings
                .Where(b => b.DoctorId == doctorId &&
                       b.DateBooking.Date == parsedDate.Date)
                .Select(b => b.SlotId)
                .ToListAsync();

            // Lọc ra các slot còn trống
            var availableSlots = allSlots
                .Where(s => !bookedSlots.Contains(s.SlotId))
                .Select(s => new SlotBasicDTO
                {
                    SlotId = s.SlotId,
                    SlotName = s.SlotName,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime
                })
                .ToList();

            return Ok(availableSlots);
        }
        // POST: api/Slot
        [HttpPost]
        public async Task<ActionResult<SlotDTO>> PostSlot(SlotCreateDTO slotCreateDTO)
        {
            var slot = slotCreateDTO.ToEntity();
            _context.Slots.Add(slot);
            await _context.SaveChangesAsync();

            // Reload the slot with its relations for returning
            var createdSlot = await _context.Slots
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.SlotId == slot.SlotId);

            return CreatedAtAction(nameof(GetSlot), new { id = slot.SlotId }, createdSlot.ToDTO());
        }

        // PUT: api/Slot/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSlot(string id, SlotUpdateDTO slotUpdateDTO)
        {
            if (id != slotUpdateDTO.SlotId) return BadRequest();

            var slot = await _context.Slots.FindAsync(id);
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
        public async Task<IActionResult> DeleteSlot(string id)
        {
            try
            {
                var slot = await _context.Slots.FindAsync(id);
                if (slot == null)
                {
                    return NotFound(new { message = $"Slot with ID {id} not found." });
                }

                if (await _context.Bookings.AnyAsync(b => b.SlotId == id))
                {
                    return BadRequest(new { message = "Cannot delete slot because it is referenced by existing bookings." });
                }

                _context.Slots.Remove(slot);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(400, new { message = "Failed to delete slot due to database constraints.", error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", error = ex.Message });
            }
        }

        private async Task<bool> SlotExistsAsync(string id)
        {
            return await _context.Slots.AnyAsync(s => s.SlotId == id);
        }
    }
}