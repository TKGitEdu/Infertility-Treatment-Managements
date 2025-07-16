using System.Text.Json.Serialization;

namespace Infertility_Treatment_Managements.Models.ZaloPay
{
    public class CreateOrderRequest
    {
        public long Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string AppUser { get; set; } = "user1";
        public string? EmbedData { get; set; } = null; // Nếu muốn truyền redirect_url
        public string? CallbackUrl { get; set; } = null; // Nếu muốn truyền callback_url
    }

    public class CreateOrderResponse
    {
        [JsonPropertyName("return_code")]
        public int ReturnCode { get; set; }

        [JsonPropertyName("return_message")]
        public string ReturnMessage { get; set; }

        [JsonPropertyName("sub_return_code")]
        public int SubReturnCode { get; set; }

        [JsonPropertyName("sub_return_message")]
        public string SubReturnMessage { get; set; }

        [JsonPropertyName("order_url")]
        public string OrderUrl { get; set; }

        [JsonPropertyName("zp_trans_token")]
        public string ZpTransToken { get; set; }

        [JsonPropertyName("order_token")]
        public string OrderToken { get; set; }

        [JsonPropertyName("qr_code")]
        public string QrCode { get; set; }
    }

    public class QueryOrderResponse
    {
        [JsonPropertyName("return_code")]
        public int ReturnCode { get; set; }

        [JsonPropertyName("return_message")]
        public string ReturnMessage { get; set; }

        [JsonPropertyName("sub_return_code")]
        public int SubReturnCode { get; set; }

        [JsonPropertyName("sub_return_message")]
        public string SubReturnMessage { get; set; }

        [JsonPropertyName("is_processing")]
        public bool IsProcessing { get; set; }

        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        [JsonPropertyName("zp_trans_id")]
        public string ZpTransId { get; set; }

        [JsonPropertyName("discount_amount")]
        public long DiscountAmount { get; set; }
    }

    public class ZaloPayCallback
    {
        [JsonPropertyName("data")]
        public string Data { get; set; }

        [JsonPropertyName("mac")]
        public string Mac { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }
    }

    public class CallbackData
    {
        [JsonPropertyName("app_id")]
        public long AppId { get; set; }

        [JsonPropertyName("app_trans_id")]
        public string AppTransId { get; set; }

        [JsonPropertyName("app_time")]
        public long AppTime { get; set; }

        [JsonPropertyName("app_user")]
        public string AppUser { get; set; }

        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        [JsonPropertyName("embed_data")]
        public string EmbedData { get; set; }

        [JsonPropertyName("item")]
        public string Item { get; set; }

        [JsonPropertyName("zp_trans_id")]
        public string ZpTransId { get; set; }

        [JsonPropertyName("server_time")]
        public long ServerTime { get; set; }

        [JsonPropertyName("channel")]
        public int Channel { get; set; }

        [JsonPropertyName("merchant_user_id")]
        public string MerchantUserId { get; set; }

        [JsonPropertyName("user_fee_amount")]
        public long UserFeeAmount { get; set; }

        [JsonPropertyName("discount_amount")]
        public long DiscountAmount { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }
    }

    public class RefundRequest
    {
        public string ZpTransId { get; set; }
        public long Amount { get; set; }
        public string Description { get; set; }
        public long? RefundFeeAmount { get; set; }
    }

    public class RefundResponse
    {
        [JsonPropertyName("return_code")]
        public int ReturnCode { get; set; }

        [JsonPropertyName("return_message")]
        public string ReturnMessage { get; set; }

        [JsonPropertyName("sub_return_code")]
        public int SubReturnCode { get; set; }

        [JsonPropertyName("sub_return_message")]
        public string SubReturnMessage { get; set; }

        [JsonPropertyName("refund_id")]
        public long RefundId { get; set; }
    }

    public class QueryRefundResponse
    {
        [JsonPropertyName("return_code")]
        public int ReturnCode { get; set; }

        [JsonPropertyName("return_message")]
        public string ReturnMessage { get; set; }

        [JsonPropertyName("sub_return_code")]
        public int SubReturnCode { get; set; }

        [JsonPropertyName("sub_return_message")]
        public string SubReturnMessage { get; set; }
    }
}