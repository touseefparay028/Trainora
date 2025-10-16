using LearningManagementSystem.Models.IdentityEntities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LearningManagementSystem.ChatHubs
{
    public class ChatHub : Hub
    {
        private readonly UserManager<ApplicationUser> userManager;

        public ChatHub(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }
        public async Task JoinBatch(string batchId)
        {
            if (string.IsNullOrWhiteSpace(batchId))
            {
               
                // Don't add to any group if no valid batch
                await Clients.Caller.SendAsync("ReceiveMessage", "System", "❌ Invalid batch. Cannot join chat.");
                return;
            }
          var userid = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await userManager.FindByIdAsync(userid);
            var name = user?.Name ?? user?.UserName ?? "Unknown User";

            await Groups.AddToGroupAsync(Context.ConnectionId, batchId);
            await Clients.Group(batchId).SendAsync("ReceiveMessage", "System", $"✅ {name} joined chat.");
        }

        public async Task SendMessage(string batchId, string user, string message)
        {
            if (string.IsNullOrWhiteSpace(batchId)) return;
            await Clients.Group(batchId).SendAsync("ReceiveMessage", user, message, DateTime.Now.ToString("hh:mm tt"));
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            // Optional cleanup if you want to remove the connection from all groups
            // (SignalR automatically handles it in most cases)
            await base.OnDisconnectedAsync(exception);
        }
    }
}
