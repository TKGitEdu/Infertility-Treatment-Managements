using Infertility_Treatment_Managements.DTOs;
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

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentCreateDTO paymentDto)
        {
            if (paymentDto == null || string.IsNullOrEmpty(paymentDto.BookingId))
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ hoặc thiếu BookingId" });
            }

            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.BookingId == paymentDto.BookingId);

            if (existingPayment != null)
            {
                return BadRequest(new { success = false, message = "Đã tồn tại hóa đơn cho lịch hẹn này" });
            }

            var payment = new Payment
            {
                PaymentId = Guid.NewGuid().ToString(),
                BookingId = paymentDto.BookingId,
                TotalAmount = paymentDto.TotalAmount,
                Status = paymentDto.Status,
                Method = paymentDto.Method,
                Confirmed = paymentDto.Confirmed
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Tạo payment thành công", payment });
        }

        //thêm hàm update trường status và confirmed của payment
        [HttpPut("{paymentId}")]
        public async Task<IActionResult> UpdatePayment(string paymentId, [FromBody] PaymentUpdateDTO paymentDto)
        {
            if (paymentDto == null || string.IsNullOrEmpty(paymentId))
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ hoặc thiếu PaymentId" });
            }
            var payment = await _context.Payments.FindAsync(paymentId);
            if (payment == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy hóa đơn" });
            }
            payment.BookingId = paymentDto.BookingId;
            payment.TotalAmount = paymentDto.TotalAmount;
            payment.Status = paymentDto.Status;
            payment.Method = paymentDto.Method;
            payment.Confirmed = paymentDto.Confirmed != null? paymentDto.Confirmed:false; // Chỉ cập nhật trường Confirmed nếu cần, mặc định là false
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Cập nhật payment thành công", payment });
        }

        // hàm lấy tất cả payment
        [HttpGet]
        public async Task<IActionResult> GetAllPayments()
        {
            var payments = await _context.Payments
                .Include(p => p.Booking) // Bao gồm thông tin Booking liên quan
                .ToListAsync();
            if (payments == null || payments.Count == 0)
            {
                return NotFound(new { success = false, message = "Không tìm thấy hóa đơn" });
            }
            return Ok(new { success = true, payments });
        }
    }
}