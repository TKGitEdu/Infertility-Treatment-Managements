namespace Infertility_Treatment_Managements.Services.ZaloPay
{
    public class ZaloPayOptions
    {
        public string AppId { get; set; }
        public string Key1 { get; set; }
        public string Key2 { get; set; }
        public string CreateOrderUrl { get; set; } = "https://sb-openapi.zalopay.vn/v2/create";
        public string QueryOrderUrl { get; set; } = "https://sb-openapi.zalopay.vn/v2/query";
        public string RefundUrl { get; set; } = "https://sb-openapi.zalopay.vn/v2/refund";
        public string QueryRefundUrl { get; set; } = "https://sb-openapi.zalopay.vn/v2/query_refund";
        public string CallbackUrl { get; set; } 
    }
}