using Microsoft.AspNetCore.SignalR;

namespace EvanLindseyApi.Hubs
{
    public class MessageHub : Hub
    {
        public void Send(string message)
        {
            Clients.All.SendAsync("broadcastMessage", message);
        }
    }
}
