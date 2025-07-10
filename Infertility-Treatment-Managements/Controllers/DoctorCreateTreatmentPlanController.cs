using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infertility_Treatment_Managements.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Infertility_Treatment_Managements.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorCreateTreatmentPlanController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public DoctorCreateTreatmentPlanController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy thông tin serviceId và patientDetailId dựa trên examinationId
        /// </summary>
        /// <param name="examinationId">ID của buổi khám</param>
        /// <returns>ServiceId và PatientDetailId tương ứng</returns>
        [HttpGet("examination-info/{examinationId}")]
        [Authorize(Roles = "Doctor")]
        public async Task<ActionResult<object>> GetServiceAndPatientDetailByExaminationId(string examinationId)
        {
            if (string.IsNullOrEmpty(examinationId))
            {
                return BadRequest("ExaminationId is required");
            }

            // Tìm thông tin examination
            var examination = await _context.Examinations
                .Include(e => e.Booking)
                .FirstOrDefaultAsync(e => e.ExaminationId == examinationId);

            if (examination == null)
            {
                return NotFound($"Examination with ID {examinationId} not found");
            }

            // Lấy serviceId từ booking
            string bookingId = examination.BookingId;
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null)
            {
                return NotFound($"Related booking not found for examination {examinationId}");
            }

            string serviceId = booking.ServiceId;
            string patientId = examination.PatientId;

            // Tìm hoặc tạo patientDetail
            var patientDetail = await _context.PatientDetails
                .FirstOrDefaultAsync(pd => pd.PatientId == patientId);

            if (patientDetail == null)
            {
                // Kiểm tra patient tồn tại
                var patientExists = await _context.Patients.AnyAsync(p => p.PatientId == patientId);
                if (!patientExists)
                {
                    return NotFound($"Patient with ID {patientId} not found");
                }

                // Tạo mới PatientDetail
                patientDetail = new PatientDetail
                {
                    PatientDetailId = "PATD_" + System.Guid.NewGuid().ToString().Substring(0, 8),
                    PatientId = patientId,
                    TreatmentStatus = "Mới"
                };

                _context.PatientDetails.Add(patientDetail);
                await _context.SaveChangesAsync();
            }

            // Trả về kết quả
            return Ok(new
            {
                ExaminationId = examinationId,
                ServiceId = serviceId,
                PatientId = patientId,
                PatientDetailId = patientDetail.PatientDetailId
            });
        }
    }


}
