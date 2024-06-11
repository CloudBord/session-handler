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
        private readonly Dictionary<string, List<WebSocket>> _sessions = [];

        private const string tlboard_ = "tlboard_";

        [HttpGet("/open/{id}")]
        public IActionResult Open([FromRoute] uint id)
        {
            string board = tlboard_ + id.ToString();
            if (!_sessions.ContainsKey(board)) {
                _sessions.Add(board, []);
            }
            return Ok(board);
        }

        [Route("/ws/{board}")]
        public async Task Get([FromRoute] string board)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                if (!_sessions.ContainsKey(board))
                {
                    _sessions[board] = [];
                }
                _sessions[board].Add(webSocket);

                await Echo(webSocket, board);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async Task Echo(WebSocket webSocket, string board)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            Console.WriteLine(result);

            while (!result.CloseStatus.HasValue)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var serverMsg = Encoding.UTF8.GetBytes(message);

                var tasks = _sessions[board].Where(ws => ws != webSocket)
                                           .Select(ws => ws.SendAsync(new ArraySegment<byte>(serverMsg, 0, serverMsg.Length), result.MessageType, result.EndOfMessage, CancellationToken.None))
                                           .ToArray();
                await Task.WhenAll(tasks);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            // Remove client from room when the connection is closed
            if (!string.IsNullOrEmpty(board) && _sessions.ContainsKey(board))
            {
                _sessions[board].Remove(webSocket);
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
