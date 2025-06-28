using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infertility_Treatment_Managements.Models;
using System.Threading.Tasks;
using System.Linq;

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

        // GET: api/DoctorDashBoard/profile?doctorId=DOC_1
        [HttpGet("profile")]
        public async Task<ActionResult<object>> GetDoctorProfile([FromQuery] string doctorId)
        {
            if (string.IsNullOrEmpty(doctorId))
            {
                return BadRequest("doctorId is required.");
            }

            var doctor = await _context.Doctors
                .Where(d => d.DoctorId == doctorId)
                .Select(d => new
                {
                    doctorId = d.DoctorId,
                    name = d.DoctorName,
                    email = d.Email,
                    specialty = d.Specialization
                })
                .FirstOrDefaultAsync();

            if (doctor == null)
            {
                return NotFound("Doctor not found.");
            }

            return Ok(doctor);
        }

        [HttpGet("/api/appointments")]
        public async Task<ActionResult<IEnumerable<object>>> GetAppointments([FromQuery] string type = "today")
        {
            var today = DateTime.Today;

            IQueryable<Booking> query = _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Service)
                .Include(b => b.Slot);

            if (type == "today")
            {
                query = query.Where(b =>
                    b.Slot != null &&
                    b.DateBooking.Date == today);
            }
            else if (type == "upcoming")
            {
                query = query.Where(b =>
                    b.Slot != null &&
                    b.DateBooking.Date > today);
            }

            var appointments = await query
                .OrderBy(b => b.DateBooking)
                .Select(b => new
                {
                    id = b.BookingId,
                    patientName = b.Patient != null ? b.Patient.Name : null,
                    patientId = b.PatientId,
                    service = b.Service != null ? b.Service.Name : null,
                    time = b.DateBooking != null && b.Slot != null
                        ? $"{b.DateBooking:yyyy-MM-dd} {b.Slot.StartTime}-{b.Slot.EndTime}"
                        : null,
                    status = b.Status
                })
                .ToListAsync();

            return Ok(appointments);
        }
        //lấy thông báo danh sách:
        //GET /api/notifications
        //[
        //  {
        //    "id": "number",
        //    "type": "appointment | test | treatment",
        //    "message": "string",
        //    "time": "string"
        //  }
        //]


    }
}