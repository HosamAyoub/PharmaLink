using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Client;
using PharmaLink_API.Repository.Interfaces;
using PharmaLink_API.Services.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PharmaLink_API.Hubs
{
    public class ConnectionInfo
    {
        public string UserId { get; set; }
        public string ConnectionId { get; set; }
    }

    public class AdminHub : Hub
    {
        private static readonly Dictionary<string, List<ConnectionInfo>> _connectionsList = new Dictionary<string, List<ConnectionInfo>>();
        private readonly IPharmacyService _pharmacyService ;

        public AdminHub( IPharmacyService pharmacyService)
        {
            _pharmacyService = pharmacyService;
        }

        public override async Task OnConnectedAsync()
        {
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            var userId = Context.UserIdentifier; // بيطلع من الـ Claims

            if (!string.IsNullOrEmpty(userRole) && !string.IsNullOrEmpty(userId))
            {
                if (!_connectionsList.ContainsKey(userRole))
                    _connectionsList[userRole] = new List<ConnectionInfo>();

                var existingConnections = _connectionsList[userRole]
                    .Where(c => c.UserId == userId)
                    .ToList();
                foreach (var oldConn in existingConnections)
                {
                    _connectionsList[userRole].Remove(oldConn);
                    Console.WriteLine($"Removed old connection for {userRole} {userId} | Old ConnID: {oldConn.ConnectionId}");
                }

                _connectionsList[userRole].Add(new ConnectionInfo
                {
                    UserId = userId,
                    ConnectionId = Context.ConnectionId
                });

                await Groups.AddToGroupAsync(Context.ConnectionId, "PharmaLinkAdmin");
                Console.WriteLine($"{userRole} joined PharmaLinkAdmin group | ConnID: {Context.ConnectionId}");
            }

            await base.OnConnectedAsync();
        }


        /// <summary>
        /// Debug method to print all current connections
        /// </summary>
        public void PrintAllConnections()
        {
            Console.WriteLine("=== Current Connections ===");
            foreach (var role in _connectionsList.Keys)
            {
                Console.WriteLine($"Role: {role}");
                foreach (var connection in _connectionsList[role])
                {
                    Console.WriteLine($"  - UserId: {connection.UserId}, ConnectionId: {connection.ConnectionId}");
                }
            }
            Console.WriteLine("========================");
        }

        // Change the method signature of CheckUserExists to async Task
        public async Task CheckUserExists(string userId)
        {
            Console.WriteLine($"=== Searching for User ID: {userId} ===");
            bool found = false;
            string acc_Id = await _pharmacyService.GetAccountIdByPharmacyIdAsync(userId);

            foreach (var role in _connectionsList.Keys)
            {
                var userConnections = _connectionsList[role].Where(c => c.UserId == acc_Id).ToList();
                if (userConnections.Any())
                {
                    Console.WriteLine($"Found user {userId} in role {role}:");
                    foreach (var conn in userConnections)
                    {
                        Console.WriteLine($"  - ConnectionId: {conn.ConnectionId}");
                    }
                    found = true;
                }
            }

            if (!found)
            {
                Console.WriteLine($"User {userId} not found in any role");
            }
            Console.WriteLine("================================");
        }

        public async Task<List<string>> GetConnectionId(string Role, string? userId = null)
        {

            if (_connectionsList.TryGetValue(Role, out List<ConnectionInfo> conInfo))
            {
                if (userId == null)
                {
                    Console.WriteLine($"Getting all connections for role {Role}: {string.Join(", ", conInfo.Select(c => c.ConnectionId))}");
                    return conInfo.Select(c => c.ConnectionId).ToList();
                }
                else
                {
                    string acc_Id = await _pharmacyService.GetAccountIdByPharmacyIdAsync(userId);
                    var matchingConnections = conInfo.Where(c => c.UserId == acc_Id).Select(c => c.ConnectionId).ToList();
                    Console.WriteLine($"Getting connections for role {Role}, userId {userId}: {string.Join(", ", matchingConnections)}");
                    return matchingConnections;
                }
            }
            
            Console.WriteLine($"Role {Role} not found in connections list");
            return new List<string>();
        }

        public async Task SendRegistrationRequest()
        {
            var AdminsConnectionID = await GetConnectionId("Admin");
            await Clients.Clients(AdminsConnectionID).SendAsync("NewUserRegistration");
            Console.WriteLine($"Registration request sent for Admins");
        }

        public async Task SendRequestToAdmin(string message)
        {
            var AdminsConnectionID = await GetConnectionId("Admin");
            await Clients.Clients(AdminsConnectionID).SendAsync("NewDrugRequest", message);
            Console.WriteLine($"Message sent to PharmaLinkAdmin : {message}");
        }

        public async Task SendAcceptanceToAll(string message)
        {
            var PhramaciesConnectionID = await GetConnectionId("Pharmacy");
            await Clients.Clients(PhramaciesConnectionID).SendAsync("DrugRequestAccepted", message);
            Console.WriteLine($"Message sent to all clients: {message}");
        }

        public async Task SendRejectionToPharmacy(string pharmacyId, string message)
        {
            Console.WriteLine($"Attempting to send rejection to Pharmacy ID: {pharmacyId}");

            // Debug: Check if user exists in any role
            CheckUserExists(pharmacyId);

            // Debug: Print all current connections
            PrintAllConnections();

            var pharmacyConnectionIds = await GetConnectionId("Pharmacy", pharmacyId);

            Console.WriteLine($"Found {pharmacyConnectionIds.Count} connection(s) for Pharmacy {pharmacyId}");

            if (pharmacyConnectionIds.Any())
            {
                await Clients.Clients(pharmacyConnectionIds).SendAsync("DrugRequestRejected", message);
                Console.WriteLine($"Message sent to Pharmacy {pharmacyId}: {message}");
            }
            else
            {
                Console.WriteLine($"No active connections found for Pharmacy {pharmacyId}");

                // Additional debugging: try to find the user in other roles
                Console.WriteLine("Checking if pharmacy exists under different roles...");
                foreach (var role in _connectionsList.Keys)
                {
                    var userInRole = _connectionsList[role].Any(c => c.UserId == pharmacyId);
                    if (userInRole)
                    {
                        Console.WriteLine($"WARNING: User {pharmacyId} found under role '{role}' instead of 'Pharmacy'");
                    }
                }
            }
        }


        

        public override async Task OnDisconnectedAsync(Exception? exception)
        {

            await base.OnDisconnectedAsync(exception);
        }
    }
}
