using Infertility_Treatment_Managements.DTOs;
using Infertility_Treatment_Managements.Helpers;
using Infertility_Treatment_Managements.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Infertility_Treatment_Managements.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorDashBoardController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public DoctorDashBoardController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        [HttpGet("mybookings")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetMyDoctorBookings([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("userId is required");
            }

            // Truy xuất DoctorId từ UserId
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null)
            {
                return NotFound("Doctor not found for the given userId");
            }

            var doctorId = doctor.DoctorId;

            var bookings = await _context.Bookings
                .Include(b => b.Service)
                .Include(b => b.Patient)
                .Include(b => b.Slot)
                .Where(b => b.DoctorId == doctorId)
                .OrderByDescending(b => b.DateBooking)
                .ToListAsync();

            return Ok(bookings.Select(b => b.ToDTO()));
        }

    }
}