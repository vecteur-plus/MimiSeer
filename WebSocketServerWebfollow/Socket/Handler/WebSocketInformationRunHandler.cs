using Serilog;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using WebSocketServerWebfollow.SocketManager;
using WebSocketSupervisorCommunicationLibrary.InformationRun.Model;

namespace WebSocketServerWebfollow.Handler
{
    //websocket used to subscribe or unsubscribe to receive SiteCollectInformation state like Isrunning,...
    public class WebSocketInformationRunHandler : SocketHandler
    {
        private readonly ILogger _logger;

        public WebSocketInformationRunHandler(ConnectionManager connections_) : base(connections_)
        {
            _logger = Log.Logger.ForContext<WebSocketInformationRunHandler>();
        }

        public override async Task OnConnected(WebSocket socket)
        {
            await base.OnConnected(socket);

            Log.Logger.Information("new user connected with ID : {id} on InformationRun", Connections.GetId(socket));
        }

        public override async Task Receive(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            await Task.Run(() =>
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var inquiry = InformationRunSubscriptionInquiry.DeserializeFromJson(message);

                _logger.Information("user with ID : {id} ask information", Connections.GetId(socket));
                var user = Connections.GetUserById(Connections.GetId(socket));

                //subscribe
                user.IdSchedules.AddRange(inquiry.IdSchedulesToAdd);

                //unsubscribe
                inquiry.IdSchedulesToDelete.ForEach(i => user.IdSchedules.Remove(i));
            });
        }

        public override async Task OnDisconnected(WebSocket socket)
        {
            _logger.Information("user with id : {id} disconnected to information run", Connections.GetId(socket));

            await base.OnDisconnected(socket);
        }

        public override async Task SendMessage(WebSocket socket, string message)
        {
            _logger.Information($"{message} send via WebSocketInformationRun");
            await base.SendMessage(socket, message);
        }
    }
}