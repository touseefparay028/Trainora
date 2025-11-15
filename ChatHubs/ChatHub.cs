using LearningManagementSystem.Models.IdentityEntities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LearningManagementSystem.ChatHubs
{
    [Microsoft.AspNetCore.Authorization.Authorize(AuthenticationSchemes = "TeacherAuth,StudentAuth", Roles = "Teacher,Student")]
    public class ChatHub : Hub
    {
       

        private readonly UserManager<ApplicationUser> _userManager;

        public ChatHub(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task JoinBatch(string batchId)
        {
            if (string.IsNullOrWhiteSpace(batchId))
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "System", "❌ Invalid batch. Cannot join chat.");
                return;
            }
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            string userName = "Unknown User";

            // ✅ Fetch from AspNetUsers (ApplicationUser table)
            if (!string.IsNullOrWhiteSpace(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    // Fetch your actual column (example: Name property in ApplicationUser)
                    userName = user.Name ?? user.UserName ?? "Unknown User";
                }
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, batchId);

            await Clients.Group(batchId)
                .SendAsync("ReceiveMessage", "System", $"✅ {userName} joined chat.", DateTime.Now.ToString("hh:mm tt"));
        }


        public async Task SendMessage(string batchId, string user, string message)
        {
            if (string.IsNullOrWhiteSpace(batchId)) return;

            await Clients.Group(batchId).SendAsync("ReceiveMessage", user, message, DateTime.Now.ToString("hh:mm tt"));
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}
