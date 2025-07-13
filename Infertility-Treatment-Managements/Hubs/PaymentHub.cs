using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Infertility_Treatment_Managements.Hubs
{
    public class PaymentHub : Hub
    {
        public async Task JoinPaymentGroup(string appTransId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, appTransId);
        }

        public async Task LeavePaymentGroup(string appTransId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, appTransId);
        }
    }
}