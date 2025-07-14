using Infertility_Treatment_Managements.Hubs;
using Infertility_Treatment_Managements.Models;
using Infertility_Treatment_Managements.Models.ZaloPay;
using Infertility_Treatment_Managements.Services.ZaloPay;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QRCoder;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infertility_Treatment_Managements.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZaloPayController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;
        private readonly IZaloPayService _zaloPayService;
        private readonly IHubContext<PaymentHub> _hubContext;
        private readonly ILogger<ZaloPayController> _logger;

        public ZaloPayController(
        InfertilityTreatmentManagementContext context,
        IZaloPayService zaloPayService,
        IHubContext<PaymentHub> hubContext,
        ILogger<ZaloPayController> logger)
        {
            _context = context;
            _zaloPayService = zaloPayService;
            _hubContext = hubContext;
            _logger = logger;
        }

        /// <summary>
        /// Tạo đơn hàng ZaloPay, trả về order_url và mã QR cho frontend
        /// </summary>
        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var response = await _zaloPayService.CreateOrderAsync(request);

                if (response.ReturnCode == 1)
                {
                    // Tạo mã QR từ order_url
                    var qrCodeImage = GenerateQrCode(response.OrderUrl);

                    return Ok(new
                    {
                        success = true,
                        orderUrl = response.OrderUrl,
                        qrCodeBase64 = qrCodeImage,
                        zpTransToken = response.ZpTransToken,
                        message = "Đã tạo đơn hàng thành công"
                    });
                }

                return BadRequest(new
                {
                    success = false,
                    errorCode = response.ReturnCode,
                    subErrorCode = response.SubReturnCode,
                    message = response.ReturnMessage,
                    subMessage = response.SubReturnMessage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo đơn hàng ZaloPay");
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi khi xử lý yêu cầu của bạn" });
            }
        }

        /// <summary>
        /// Nhận callback từ ZaloPay khi thanh toán thành công/thất bại
        /// </summary>
        [HttpPost("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleCallback([FromBody] ZaloPayCallback callback)
        {
            try
            {
                if (!_zaloPayService.VerifyCallback(callback.Data, callback.Mac))
                {
                    _logger.LogWarning("Chữ ký MAC không hợp lệ");
                    return BadRequest(new { return_code = -1, return_message = "MAC không hợp lệ" });
                }

                var callbackData = JsonSerializer.Deserialize<CallbackData>(callback.Data) ?? new CallbackData();

                // Tìm Payment theo PaymentId (nếu AppTransId chính là PaymentId)
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(p => p.PaymentId == callbackData.AppTransId);

                if (payment == null)
                {
                    _logger.LogWarning($"Không tìm thấy giao dịch với PaymentId: {callbackData.AppTransId}");
                    return BadRequest(new { return_code = -1, return_message = "Không tìm thấy giao dịch" });
                }

                // Gọi ZaloPay để xác thực trạng thái đơn hàng
                var queryResponse = await _zaloPayService.QueryOrderAsync(callbackData.AppTransId);
                if (queryResponse.ReturnCode == 1 && !queryResponse.IsProcessing && queryResponse.Amount > 0)
                {
                    payment.Status = "chưa thanh toán";//"Đã thanh toán"
                    await _context.SaveChangesAsync();

                    await _hubContext.Clients.Group(callbackData.AppTransId).SendAsync(
                        "PaymentCompleted",
                        new
                        {
                            paymentId = payment.PaymentId,
                            zpTransId = callbackData.ZpTransId,
                            amount = callbackData.Amount,
                            status = payment.Status
                        });
                }
                else if (queryResponse.ReturnCode == 1 && queryResponse.IsProcessing)
                {
                    payment.Status = "Đang xử lý";
                    await _context.SaveChangesAsync();
                }
                else
                {
                    payment.Status = "Thất bại";
                    await _context.SaveChangesAsync();
                    _logger.LogWarning($"Truy vấn trạng thái thất bại: {queryResponse.ReturnMessage}");
                }

                return Ok(new { return_code = 1, return_message = "Thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý callback từ ZaloPay");
                return StatusCode(500, new { return_code = 99, return_message = "Lỗi hệ thống" });
            }
        }

        private string GenerateQrCode(string text)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            var pngBytes = new PngByteQRCode(qrCodeData).GetGraphic(20);
            return Convert.ToBase64String(pngBytes);
        }
    }
}