using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Session.Handler.Models;

namespace Session.Handler
{
    public class SignalRHandler
    {
        private const string HubName = "cloudbord";

        private readonly ILogger _logger;
        private Dictionary<string, string> _groups;
        private readonly string _groupPrefix = "room-";

        public SignalRHandler(ILogger<SignalRHandler> logger)
        {
            _logger = logger;
            _groups = new Dictionary<string, string>();
        }

        //// Generate connection token for client and send it back
        //[Function("Negotiate")]
        //public async Task<SignalRConnectionInfo> Negotiate(
        //    [HttpTrigger(AuthorizationLevel.Anonymous, "post")] 
        //            HttpRequestData req,
        //    [SignalRConnectionInfoInput(HubName = "CloudBord", UserId = "{query.userId}")] 
        //            SignalRConnectionInfo signalRConnectionInfo
        //)
        //{
        //    _logger.LogInformation("Executing negotiation.");
        //    return signalRConnectionInfo;
        //}

        // Generate connection token for client and send it back
        [Function("Negotiate")]
        public async Task<HttpResponseData> Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
                    HttpRequestData req,
            [SignalRConnectionInfoInput(HubName = HubName, UserId = "{query.userId}")]
                    SignalRConnectionInfo signalRConnectionInfo
        )
        {
            _logger.LogInformation("Executing negotiation.");
            var response = req.CreateResponse();
            await response.WriteAsJsonAsync(signalRConnectionInfo);
            return response;
        }

        //[Function("OnConnected")]
        //[SignalROutput(HubName = "CloudBord")]
        //public SignalRMessageAction OnConnected(
        //    [SignalRTrigger("CloudBord" , "connections", "connected")] 
        //            SignalRInvocationContext invocationContext
        //)
        //{
        //    invocationContext.Headers.TryGetValue("Authorization", out var auth);
        //    _logger.LogInformation($"{invocationContext.ConnectionId} has connected");
        //    return new SignalRMessageAction("newConnection")
        //    {
        //        Arguments = [new NewConnection(invocationContext.ConnectionId, auth)],
        //    };
        //}
        
        [Function("JoinGroup")]
        [SignalROutput(HubName = HubName)]
        public SignalRGroupAction JoinGroup([SignalRTrigger(HubName, "messages", "JoinGroup", "connectionId", "groupName")] SignalRInvocationContext invocationContext, string connectionId, string groupName)
        {
            return new SignalRGroupAction(SignalRGroupActionType.Add)
            {
                GroupName = groupName,
                ConnectionId = connectionId
            };
        }

        [Function("SendToGroup")]
        [SignalROutput(HubName = HubName)]
        public SignalRMessageAction SendToGroup([SignalRTrigger(HubName, "messages", "SendToGroup", "groupName", "message")] SignalRInvocationContext invocationContext, string groupName, string message)
        {
            string newMessage = JsonSerializer.Serialize(new NewMessage(invocationContext, message));

            return new SignalRMessageAction("newMessage")
            {
                GroupName = groupName,
                Arguments = [new NewMessage(invocationContext, newMessage)]
            };
        }

        [Function("OnDisconnected")]
        [SignalROutput(HubName = HubName)]
        public void OnDisconnected([SignalRTrigger(HubName, "connections", "disconnected")] SignalRInvocationContext invocationContext)
        {
            _logger.LogInformation($"{invocationContext.ConnectionId} has disconnected");
        }
    }
}
