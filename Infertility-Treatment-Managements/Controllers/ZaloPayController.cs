using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using QRCoder;
using Infertility_Treatment_Managements.Services.ZaloPay;
using Infertility_Treatment_Managements.Models.ZaloPay;
using Infertility_Treatment_Managements.Hubs;

namespace Infertility_Treatment_Managements.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZaloPayController : ControllerBase
    {
        private readonly IZaloPayService _zaloPayService;
        private readonly IHubContext<PaymentHub> _hubContext;
        private readonly ILogger<ZaloPayController> _logger;

        public ZaloPayController(
            IZaloPayService zaloPayService,
            IHubContext<PaymentHub> hubContext,
            ILogger<ZaloPayController> logger)
        {
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
        public IActionResult HandleCallback([FromBody] ZaloPayCallback callback)
        {
            try
            {
                // Xác thực chữ ký callback
                if (!_zaloPayService.VerifyCallback(callback.Data, callback.Mac))
                {
                    _logger.LogWarning("Chữ ký MAC không hợp lệ");
                    return BadRequest(new { return_code = -1, return_message = "MAC không hợp lệ" });
                }

                // Phân tích dữ liệu callback
                var callbackData = JsonSerializer.Deserialize<CallbackData>(callback.Data);

                // Thông báo cho frontend qua SignalR (theo appTransId)
                if (callbackData != null)
                {
                    _ = _hubContext.Clients.Group(callbackData.AppTransId).SendAsync(
                        "PaymentCompleted",
                        new
                        {
                            appTransId = callbackData.AppTransId,
                            zpTransId = callbackData.ZpTransId,
                            amount = callbackData.Amount,
                            status = callbackData.Status // 1: thành công, 2: thất bại
                        });
                }

                // Trả về thành công cho ZaloPay
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