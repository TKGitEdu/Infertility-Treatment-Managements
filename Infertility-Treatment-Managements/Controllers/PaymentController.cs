using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories.Models;
using Infertility_Treatment_Management.DTOs;
using Infertility_Treatment_Management.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infertility_Treatment_Management.Controllers
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

        // GET: api/Payment
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentDTO>>> GetPayments()
        {
            var payments = await _context.Payment
                .Include(p => p.Booking)
                .ToListAsync();

            return payments.Select(p => p.ToDTO()).ToList();
        }

        // GET: api/Payment/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDTO>> GetPayment(int id)
        {
            var payment = await _context.Payment
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null)
            {
                return NotFound();
            }

            return payment.ToDTO();
        }

        // GET: api/Payment/Booking/5
        [HttpGet("Booking/{bookingId}")]
        public async Task<ActionResult<PaymentDTO>> GetPaymentByBooking(int bookingId)
        {
            var bookingExists = await _context.Booking.AnyAsync(b => b.BookingId == bookingId);
            if (!bookingExists)
            {
                return NotFound("Booking not found");
            }

            var payment = await _context.Payment
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.BookingId == bookingId);

            if (payment == null)
            {
                return NotFound("Payment not found for this booking");
            }

            return payment.ToDTO();
        }

        // GET: api/Payment/Status/Completed
        [HttpGet("Status/{status}")]
        public async Task<ActionResult<IEnumerable<PaymentDTO>>> GetPaymentsByStatus(string status)
        {
            var payments = await _context.Payment
                .Where(p => p.Status == status)
                .Include(p => p.Booking)
                .ToListAsync();

            return payments.Select(p => p.ToDTO()).ToList();
        }

        // POST: api/Payment
        [HttpPost]
        public async Task<ActionResult<PaymentDTO>> CreatePayment(PaymentCreateDTO paymentCreateDTO)
        {
            // Validate booking exists if provided
            if (paymentCreateDTO.BookingId.HasValue)
            {
                var bookingExists = await _context.Booking.AnyAsync(b => b.BookingId == paymentCreateDTO.BookingId);
                if (!bookingExists)
                {
                    return BadRequest("Invalid BookingId: Booking does not exist");
                }

                // Check if payment already exists for this booking
                var paymentExists = await _context.Payment.AnyAsync(p => p.BookingId == paymentCreateDTO.BookingId);
                if (paymentExists)
                {
                    return BadRequest("A payment already exists for this booking");
                }
            }

            var payment = paymentCreateDTO.ToEntity();
            _context.Payment.Add(payment);
            await _context.SaveChangesAsync();

            // Reload with related data for return
            var createdPayment = await _context.Payment
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.PaymentId == payment.PaymentId);

            return CreatedAtAction(nameof(GetPayment), new { id = createdPayment.PaymentId }, createdPayment.ToDTO());
        }

        // PUT: api/Payment/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(int id, PaymentUpdateDTO paymentUpdateDTO)
        {
            if (id != paymentUpdateDTO.PaymentId)
            {
                return BadRequest("ID mismatch");
            }

            var payment = await _context.Payment.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            // Validate booking exists if provided
            if (paymentUpdateDTO.BookingId.HasValue)
            {
                var bookingExists = await _context.Booking.AnyAsync(b => b.BookingId == paymentUpdateDTO.BookingId);
                if (!bookingExists)
                {
                    return BadRequest("Invalid BookingId: Booking does not exist");
                }

                // Check if another payment exists for this booking (excluding this one)
                var duplicatePayment = await _context.Payment
                    .AnyAsync(p => p.PaymentId != id && p.BookingId == paymentUpdateDTO.BookingId);
                if (duplicatePayment)
                {
                    return BadRequest("Another payment already exists for this booking");
                }
            }

            paymentUpdateDTO.UpdateEntity(payment);
            _context.Entry(payment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PaymentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PATCH: api/Payment/5/UpdateStatus
        [HttpPatch("{id}/UpdateStatus")]
        public async Task<IActionResult> UpdatePaymentStatus(int id, [FromBody] string status)
        {
            var payment = await _context.Payment.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            payment.Status = status;
            _context.Entry(payment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PaymentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Payment/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var payment = await _context.Payment.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            // Check if this payment is associated with a booking
            var hasBooking = await _context.Booking.AnyAsync(b => b.PaymentId == id);
            if (hasBooking)
            {
                return BadRequest("Cannot delete payment that is associated with a booking");
            }

            _context.Payment.Remove(payment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> PaymentExists(int id)
        {
            return await _context.Payment.AnyAsync(p => p.PaymentId == id);
        }
    }
}