using System.Net.WebSockets;

namespace Session.Socket.Services
{
    public interface ISessionHandler
    {
        Task HandleWebsSocketConnection(WebSocket socket, string room);
    }
}
