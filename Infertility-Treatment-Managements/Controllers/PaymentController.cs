using Infertility_Treatment_Managements.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Infertility_Treatment_Managements.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public PaymentController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tạo mới một payment dựa trên BookingId, không cho phép tạo nếu đã có payment cho booking này
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] Payment payment)
        {
            if (payment == null || string.IsNullOrEmpty(payment.BookingId))
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ hoặc thiếu BookingId" });
            }

            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.BookingId == payment.BookingId);

            if (existingPayment != null)
            {
                return BadRequest(new { success = false, message = "Đã tồn tại hóa đơn cho lịch hẹn này" });
            }

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Tạo payment thành công", payment });
        }
    }
}