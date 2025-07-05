using Infertility_Treatment_Managements.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infertility_Treatment_Managements.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infertility_Treatment_Managements.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TreatmentPlanController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public TreatmentPlanController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }


        [HttpGet("{treatmentPlanId}/medications")]
        public async Task<ActionResult<IEnumerable<TreatmentMedication>>> GetMedicationsByTreatmentPlanId(string treatmentPlanId)
        {
            var medications = await _context.TreatmentMedications
                .Where(m => m.TreatmentPlanId == treatmentPlanId)
                .ToListAsync();

            if (medications == null || medications.Count == 0)
                return NotFound("Không tìm thấy thuốc cho kế hoạch điều trị này.");

            return Ok(medications);
        }
        //Dưới đây là hàm API lấy danh sách booking theo treatmentPlanId.Hàm này sẽ:
        //1.	Tìm TreatmentPlan theo treatmentPlanId.
        //2.	Lấy DoctorId, ServiceId, và PatientDetailId từ TreatmentPlan.
        //3.	Dùng PatientDetailId để lấy PatientId.
        //4.	Truy vấn bảng Booking theo DoctorId, ServiceId, PatientId.
        [HttpGet("{treatmentPlanId}/bookings")]
        public async Task<ActionResult> GetBookingsByTreatmentPlanId(string treatmentPlanId)
        {
            var plan = await _context.TreatmentPlans
                .FirstOrDefaultAsync(tp => tp.TreatmentPlanId == treatmentPlanId);

            if (plan == null)
                return NotFound("Không tìm thấy kế hoạch điều trị.");

            var patientDetail = await _context.PatientDetails
                .FirstOrDefaultAsync(pd => pd.PatientDetailId == plan.PatientDetailId);

            if (patientDetail == null || string.IsNullOrEmpty(patientDetail.PatientId))
                return NotFound("Không tìm thấy thông tin bệnh nhân cho kế hoạch điều trị này.");

            var bookings = await _context.Bookings
                .Where(b =>
                    b.DoctorId == plan.DoctorId &&
                    b.ServiceId == plan.ServiceId &&
                    b.PatientId == patientDetail.PatientId)
                .ToListAsync();

            if (bookings == null || bookings.Count == 0)
                return NotFound("Không tìm thấy booking tương ứng.");

            var slotIds = bookings.Select(b => b.SlotId).Distinct().ToList();
            var slots = await _context.Slots
                .Where(s => slotIds.Contains(s.SlotId))
                .ToDictionaryAsync(s => s.SlotId, s => s);

            var bookingdto = bookings.Select(b => new
            {
                b.BookingId,
                b.PatientId,
                b.ServiceId,
                b.PaymentId,
                b.DoctorId,
                DateBooking = b.DateBooking, // Sửa lại đúng tên trường
                b.SlotId,
                b.Description,
                b.Status,
                b.Note,
                SlotName = b.SlotId != null && slots.ContainsKey(b.SlotId) ? slots[b.SlotId].SlotName : null
            });

            return Ok(bookingdto);
        }
        //Dưới đây là hàm API lấy danh sách Examination theo 
        //bookingId, patientId, doctorId được truyền vào.Hàm 
        //này sẽ kiểm tra các điều kiện và trả về danh sách 
        //các examination phù hợp.
        [HttpGet("examinations")]
        public async Task<ActionResult> GetExaminationsByBookingPatientDoctor(
    [FromQuery] string bookingId,
    [FromQuery] string patientId,
    [FromQuery] string doctorId)
        {
            // Kiểm tra đầu vào
            if (string.IsNullOrEmpty(bookingId) || string.IsNullOrEmpty(patientId) || string.IsNullOrEmpty(doctorId))
                return BadRequest("Thiếu thông tin đầu vào.");

            // Lấy các examination thỏa mãn bookingId, patientId, doctorId
            var examinations = await _context.Examinations
                .Where(e =>
                    e.BookingId == bookingId &&
                    e.PatientId == patientId &&
                    e.DoctorId == doctorId)
                .ToListAsync();

            if (examinations == null || examinations.Count == 0)
                return NotFound("Không tìm thấy examination phù hợp.");

            return Ok(examinations);
        }

        // Dưới đây là hàm API lấy danh sách TreatmentStep theo treatmentPlanId được truyền vào.Hàm này sẽ:
        [HttpGet("{treatmentPlanId}/steps")]
        public async Task<ActionResult<IEnumerable<TreatmentStep>>> GetTreatmentStepsByTreatmentPlanId(string treatmentPlanId)
        {
            var steps = await _context.TreatmentSteps
                .Where(ts => ts.TreatmentPlanId == treatmentPlanId)
                .ToListAsync();

            if (steps == null || steps.Count == 0)
                return NotFound("Không tìm thấy bước điều trị cho kế hoạch này.");

            return Ok(steps);
        }

    }
}