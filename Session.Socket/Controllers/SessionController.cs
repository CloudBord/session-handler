using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;

namespace Session.Socket.Controllers
{
    [ApiController]
    [Route("api/session")]
    public class SessionController : ControllerBase
    {
        private Dictionary<string, List<WebSocket>> _sessions = new Dictionary<string, List<WebSocket>>();

        [HttpGet("/open/{id}")]
        public async Task<IActionResult> Open([FromRoute] uint id)
        {
            string room = "tlboard_" + id;
            if (!_sessions.ContainsKey(room)) {
                _sessions.Add(room, new List<WebSocket>());
            }
            return new OkObjectResult(room);
        }

        [HttpGet("/ws/room-1")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                if (!_sessions.ContainsKey("room-1"))
                {
                    _sessions["room-1"] = new List<WebSocket>();
                }
                _sessions["room-1"].Add(webSocket);

                await Echo(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async Task Echo(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            Console.WriteLine(result);

            while (!result.CloseStatus.HasValue)
            {



                //var receivedText = Encoding.UTF8.GetString(buffer, 0, result.Count);
                //var responseText = $"Echo: {receivedText}";
                //var responseBuffer = Encoding.UTF8.GetBytes(responseText);

                //await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), result.MessageType, result.EndOfMessage, CancellationToken.None);

                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var serverMsg = Encoding.UTF8.GetBytes(message);

                var tasks = _sessions["room-1"].Where(ws => ws != webSocket)
                                           .Select(ws => ws.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None))
                                           .ToArray();
                await Task.WhenAll(tasks);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            // Remove client from room when the connection is closed
            if (!string.IsNullOrEmpty("room-1") && _sessions.ContainsKey("room-1"))
            {
                _sessions["room-1"].Remove(webSocket);
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
