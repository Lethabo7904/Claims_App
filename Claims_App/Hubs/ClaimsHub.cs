using Microsoft.AspNetCore.SignalR;
using ClaimsApp.Hubs;

namespace ClaimsApp.Hubs
{
    public class ClaimsHub : Hub
    {
        // Server can call Clients.All.SendAsync("ClaimStatusChanged", claimId, newStatus);
    }
}

