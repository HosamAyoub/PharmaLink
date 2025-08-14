using Microsoft.AspNetCore.SignalR;

namespace PharmaLink_API.Hubs
{
    public class StatusChangeHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            Console.WriteLine($"Connected UserIdentifier: {Context.UserIdentifier}");
            await base.OnConnectedAsync();
        }
    }
}
