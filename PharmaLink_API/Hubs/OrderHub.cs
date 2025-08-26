 using Microsoft.AspNetCore.SignalR;
using PharmaLink_API.Models;
using PharmaLink_API.Repository.Interfaces;
using System.Security.Claims;

namespace PharmaLink_API.Hubs
{
    public class OrderHub : Hub
    {
        public async Task JoinGroup(string pharmacyId)
        {
            if (!string.IsNullOrEmpty(pharmacyId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, pharmacyId);
            }
        }
    }
}
