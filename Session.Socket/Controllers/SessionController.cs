using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Session.DataAccess.Repositories;
using Session.Socket.Services;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Session.Socket.Controllers
{
    [ApiController]
    public class SessionController : ControllerBase
    {
        private static readonly Dictionary<string, List<WebSocket>> _sessions = [];
        private readonly ISnapshotRepository _snapshotRepository;

        public SessionController(ISnapshotRepository snapshotRepository)
        {
            _snapshotRepository = snapshotRepository;
        }

        [Route("/{room}")]
        public async Task Initialize([FromRoute] string room, [FromQuery] uint boardId)
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = 400;
                return;
            }

            WebSocket? webSocket = null;
            try
            {
                webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                if (!_sessions.ContainsKey(room))
                {
                    _sessions[room] = [];
                }
                _sessions[room].Add(webSocket);

                await Task.WhenAll(Connect(webSocket, boardId), Send(webSocket, room));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if(webSocket != null)
                {
                    // Remove client from room when the connection is closed
                    if (!string.IsNullOrEmpty(room) && _sessions.ContainsKey(room))
                    {
                        _sessions[room].Remove(webSocket);
                    }
                    webSocket.Dispose();
                }
            }
        }

        private async Task Connect(WebSocket webSocket, uint boardId)
        {

            var initMsg = new
            {
                type = "init",
                data = new
                {

                }
            };
            var json = JsonSerializer.Serialize(initMsg);
            var bytes = Encoding.UTF8.GetBytes(json);
            await webSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }


        private async Task Send(WebSocket webSocket, string room)
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

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
