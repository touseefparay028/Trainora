namespace LearningManagementSystem.ChatHubs
{
    using Microsoft.AspNetCore.SignalR;

    public class ChatHub : Hub
    {
        public async Task JoinBatch(string batchId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, batchId);
        }

        public async Task SendMessage(string batchId, string user, string message)
        {
            await Clients.Group(batchId).SendAsync("ReceiveMessage", user, message, DateTime.Now);
        }
    }

}
