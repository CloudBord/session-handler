using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Session.Socket.Controllers
{
    [ApiController]
    public class SessionController : ControllerBase
    {
        // Investigate other thread-safe implementations
        private static readonly Dictionary<string, List<WebSocket>> _sessions = [];

        public SessionController() { }

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
                    // TODO: Implement cache storage with Redis and pass docs here on initialization of socket connection
                }
            };
            var json = JsonSerializer.Serialize(initMsg);
            var bytes = Encoding.UTF8.GetBytes(json);
            await webSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }


        private async Task Send(WebSocket webSocket, string room)
        {
            try
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
            catch (Exception ex)
            {
                _sessions[room].Remove(webSocket);
            }
        }

        private void RemoveSocket(WebSocket webSocket, string  room)
        {
            //while (true)
            //{
            //    if(!_sessions.TryGetValue(room, out var list))
            //    {
            //        break;
            //    }

            //    List<WebSocket> newList;
            //    lock (list)
            //    {
            //        newList = list.Where(ws => ws != webSocket).ToList();
            //    }

            //    if(newList.Count > 0)
            //    {
            //        if(_sessions.TryUpdate(key: room, newValue: newList, comparisonValue: list))
            //        {
            //            return;
            //        }
            //    }
            //    else
            //    {
            //        if(_sessions.TryRemove(key: room, out _))
            //        {
            //            return;
            //        }
            //    }
            //}
        }
    }
}
