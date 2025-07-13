using System.Threading.Tasks;
using Infertility_Treatment_Managements.Models.ZaloPay;

namespace Infertility_Treatment_Managements.Services.ZaloPay
{
    public interface IZaloPayService
    {
        Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request);
        Task<QueryOrderResponse> QueryOrderAsync(string appTransId);
        bool VerifyCallback(string data, string mac);
        Task<RefundResponse> RefundAsync(RefundRequest request);
        Task<QueryRefundResponse> QueryRefundAsync(string mRefundId);
    }
}