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
        private readonly IPharmacyService _pharmacyService;

        public AdminHub(IPharmacyService pharmacyService)
        {
            _pharmacyService = pharmacyService;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;

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

                Console.WriteLine($"{userRole} joined PharmaLinkAdmin group | ConnID: {Context.ConnectionId}");
                await Groups.AddToGroupAsync(Context.ConnectionId, "PharmaLinkAdmin");
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

        // Fixed: Convert string to int and proper async handling
        public async Task CheckUserExists(string pharmacyId)
        {
            Console.WriteLine($"=== Searching for Pharmacy ID: {pharmacyId} ===");
            bool found = false;

            // Convert string to int for the service call
            if (pharmacyId != null)
            {
                string? acc_Id = await _pharmacyService.GetAccountIdByPharmacyIdAsync(pharmacyId);
                
                if (!string.IsNullOrEmpty(acc_Id))
                {
                    Console.WriteLine($"Converted Pharmacy ID {pharmacyId} to Account ID: {acc_Id}");
                    
                    foreach (var role in _connectionsList.Keys)
                    {
                        var userConnections = _connectionsList[role].Where(c => c.UserId == acc_Id).ToList();
                        if (userConnections.Any())
                        {
                            Console.WriteLine($"Found user {pharmacyId} (Account ID: {acc_Id}) in role {role}:");
                            foreach (var conn in userConnections)
                            {
                                Console.WriteLine($"  - ConnectionId: {conn.ConnectionId}");
                            }
                            found = true;
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Could not find Account ID for Pharmacy ID: {pharmacyId}");
                }
            }
            else
            {
                Console.WriteLine($"Invalid Pharmacy ID format: {pharmacyId}");
            }

            if (!found)
            {
                Console.WriteLine($"Pharmacy {pharmacyId} not found in any role");
            }
            Console.WriteLine("================================");
        }

        // Fixed: Convert string to int and proper async handling
        public async Task<List<string>> GetConnectionId(string Role, string? pharmacyId = null)
        {
            if (_connectionsList.TryGetValue(Role, out List<ConnectionInfo> conInfo))
            {
                if (pharmacyId == null)
                {
                    Console.WriteLine($"Getting all connections for role {Role}: {string.Join(", ", conInfo.Select(c => c.ConnectionId))}");
                    return conInfo.Select(c => c.ConnectionId).ToList();
                }
                else
                {
                    // Convert string to int for the service call
                    if (pharmacyId!=null)
                    {
                        string? acc_Id = await _pharmacyService.GetAccountIdByPharmacyIdAsync(pharmacyId);
                        
                        if (!string.IsNullOrEmpty(acc_Id))
                        {
                            var matchingConnections = conInfo.Where(c => c.UserId == acc_Id).Select(c => c.ConnectionId).ToList();
                            Console.WriteLine($"Getting connections for role {Role}, Pharmacy ID {pharmacyId} (Account ID: {acc_Id}): {string.Join(", ", matchingConnections)}");
                            return matchingConnections;
                        }
                        else
                        {
                            Console.WriteLine($"Could not find Account ID for Pharmacy ID: {pharmacyId}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Invalid Pharmacy ID format: {pharmacyId}");
                    }
                }
            }
            
            Console.WriteLine($"Role {Role} not found in connections list");
            return new List<string>();
        }

        public async Task SendRegistrationRequest(string message)
        {
            var AdminsConnectionID = await GetConnectionId("Admin");
            await Clients.Clients(AdminsConnectionID).SendAsync("NewUserRegistration",message);
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

            Console.WriteLine("=== Sending acceptance to all Pharmacies ===");
            if (!PhramaciesConnectionID.Any())
            {
                Console.WriteLine("No active Pharmacy connections found!");
            }
            else
            {
                Console.WriteLine($"Connections found: {string.Join(", ", PhramaciesConnectionID)}");
                await Clients.Clients(PhramaciesConnectionID).SendAsync("DrugRequestAccepted", message);
                Console.WriteLine($"Message sent to all clients: {message}");
            }

        }

        public async Task SendRejectionToPharmacy(string pharmacyId, string message)
        {
            Console.WriteLine($"Attempting to send rejection to Pharmacy ID: {pharmacyId}");

            // Debug: Check if user exists in any role (now properly awaited)
            await CheckUserExists(pharmacyId);

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
                if (pharmacyId != null)
                {
                    string? accountId = await _pharmacyService.GetAccountIdByPharmacyIdAsync(pharmacyId);
                    if (!string.IsNullOrEmpty(accountId))
                    {
                        foreach (var role in _connectionsList.Keys)
                        {
                            var userInRole = _connectionsList[role].Any(c => c.UserId == accountId);
                            if (userInRole)
                            {
                                Console.WriteLine($"WARNING: Pharmacy {pharmacyId} (Account ID: {accountId}) found under role '{role}' instead of 'Pharmacy'");
                            }
                        }
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
