using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Infertility_Treatment_Managements.Models.ZaloPay;

namespace Infertility_Treatment_Managements.Services.ZaloPay
{
    public class ZaloPayService : IZaloPayService
    {
        private readonly HttpClient _httpClient;
        private readonly ZaloPayOptions _options;

        public ZaloPayService(IOptions<ZaloPayOptions> options)
        {
            _options = options.Value;
            _httpClient = new HttpClient();
        }

        public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request)
        {
            var appTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var appTransId = $"{DateTime.Now:yyMMdd}_{Guid.NewGuid().ToString("N").Substring(0, 10)}";
            var appId = _options.AppId;

            // Nếu frontend không truyền embed_data, backend sẽ tự sinh
            var embedData = !string.IsNullOrWhiteSpace(request.EmbedData)
                ? request.EmbedData
                : "{}";

            var param = new Dictionary<string, string>
            {
                ["app_id"] = appId,
                ["app_user"] = request.AppUser,
                ["app_time"] = appTime.ToString(),
                ["amount"] = request.Amount.ToString(),
                ["app_trans_id"] = appTransId,
                ["bank_code"] = "zalopayapp",
                ["embed_data"] = embedData,
                ["item"] = "[]",
                ["callback_url"] = _options.CallbackUrl,
                ["description"] = request.Description
            };

            // Tạo MAC
            var hmacInput = $"{appId}|{appTransId}|{request.AppUser}|{request.Amount}|{appTime}|{embedData}|[]";
            var mac = CreateHmacSha256(hmacInput, _options.Key1);
            param.Add("mac", mac);

            // Gọi API ZaloPay
            try
            {
                var content = new FormUrlEncodedContent(param);
                var response = await _httpClient.PostAsync(_options.CreateOrderUrl, content);
                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadFromJsonAsync<CreateOrderResponse>();
                return responseData;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi gọi API tạo đơn hàng ZaloPay: {ex.Message}", ex);
            }
        }

        public async Task<QueryOrderResponse> QueryOrderAsync(string appTransId)
        {
            var appId = int.TryParse(_options.AppId, out var id) ? id : 0;
            var param = new Dictionary<string, string>
            {
                ["app_id"] = appId.ToString(),
                ["app_trans_id"] = appTransId
            };

            var hmacInput = $"{appId}|{appTransId}|{_options.Key1}";
            var mac = CreateHmacSha256(hmacInput, _options.Key1);
            param.Add("mac", mac);

            try
            {
                var content = new FormUrlEncodedContent(param);
                var response = await _httpClient.PostAsync(_options.QueryOrderUrl, content);
                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadFromJsonAsync<QueryOrderResponse>();
                return responseData;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi truy vấn trạng thái đơn hàng ZaloPay: {ex.Message}", ex);
            }
        }

        public bool VerifyCallback(string data, string mac)
        {
            var computedMac = CreateHmacSha256(data, _options.Key2);
            return string.Equals(computedMac, mac, StringComparison.OrdinalIgnoreCase);
        }

        public async Task<RefundResponse> RefundAsync(RefundRequest request)
        {
            var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            var mRefundId = $"{DateTime.Now:yyMMdd}_{_options.AppId}_{Guid.NewGuid().ToString("N").Substring(0, 10)}";
            var appId = int.TryParse(_options.AppId, out var id) ? id : 0;

            var param = new Dictionary<string, string>
            {
                ["app_id"] = appId.ToString(),
                ["m_refund_id"] = mRefundId,
                ["zp_trans_id"] = request.ZpTransId,
                ["amount"] = request.Amount.ToString(),
                ["timestamp"] = timestamp,
                ["description"] = request.Description
            };

            string hmacInput;
            if (request.RefundFeeAmount.HasValue)
            {
                param.Add("refund_fee_amount", request.RefundFeeAmount.Value.ToString());
                hmacInput = $"{appId}|{request.ZpTransId}|{request.Amount}|{request.RefundFeeAmount.Value}|{request.Description}|{timestamp}";
            }
            else
            {
                hmacInput = $"{appId}|{request.ZpTransId}|{request.Amount}|{request.Description}|{timestamp}";
            }

            var mac = CreateHmacSha256(hmacInput, _options.Key1);
            param.Add("mac", mac);

            try
            {
                var content = new FormUrlEncodedContent(param);
                var response = await _httpClient.PostAsync(_options.RefundUrl, content);
                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadFromJsonAsync<RefundResponse>();
                return responseData;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi hoàn tiền giao dịch ZaloPay: {ex.Message}", ex);
            }
        }

        public async Task<QueryRefundResponse> QueryRefundAsync(string mRefundId)
        {
            var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            var appId = int.TryParse(_options.AppId, out var id) ? id : 0;

            var param = new Dictionary<string, string>
            {
                ["app_id"] = appId.ToString(),
                ["m_refund_id"] = mRefundId,
                ["timestamp"] = timestamp
            };

            var hmacInput = $"{appId}|{mRefundId}|{timestamp}";
            var mac = CreateHmacSha256(hmacInput, _options.Key1);
            param.Add("mac", mac);

            try
            {
                var content = new FormUrlEncodedContent(param);
                var response = await _httpClient.PostAsync(_options.QueryRefundUrl, content);
                response.EnsureSuccessStatusCode();

                var responseData = await response.Content.ReadFromJsonAsync<QueryRefundResponse>();
                return responseData;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi truy vấn trạng thái hoàn tiền ZaloPay: {ex.Message}", ex);
            }
        }

        private string CreateHmacSha256(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}