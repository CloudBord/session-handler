using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Session.Socket.Services;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

namespace Session.Socket.Controllers
{
    [ApiController]
    public class SessionController : ControllerBase
    {
        private static readonly Dictionary<string, List<WebSocket>> _sessions = [];
        private readonly Dictionary<string, string> _state = [];

        [Route("/{room}")]
        public async Task Get([FromRoute] string room)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                if (!_sessions.ContainsKey(room))
                {
                    _sessions[room] = [];
                }
                _sessions[room].Add(webSocket);

                await Echo(webSocket, room);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async Task Echo(WebSocket webSocket, string room)
        {
            var buffer = new byte[1024 * 8];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
    
            while (!result.CloseStatus.HasValue)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var serverMsg = Encoding.UTF8.GetBytes(message);

                var tasks = _sessions[room].Where(ws => ws != webSocket)
                                           .Select(ws => ws.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None))
                                           .ToArray();
                await Task.WhenAll(tasks);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            // Remove client from room when the connection is closed
            if (!string.IsNullOrEmpty(room) && _sessions.ContainsKey(room))
            {
                _sessions[room].Remove(webSocket);
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
