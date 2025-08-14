 using Microsoft.AspNetCore.SignalR;
using PharmaLink_API.Models;
using PharmaLink_API.Repository.Interfaces;
using System.Security.Claims;

namespace PharmaLink_API.Hubs
{
    public class OrderHub : Hub
    {
        private readonly IPharmacyRepository _pharmacyRepository;

        public OrderHub(IPharmacyRepository pharmacyRepository)
        {
            _pharmacyRepository = pharmacyRepository;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public async Task JoinGroup(string pharmacyId)
        {
            if (!string.IsNullOrEmpty(pharmacyId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, pharmacyId);
                Console.WriteLine($"Pharmacy {pharmacyId} connected to group.");
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
