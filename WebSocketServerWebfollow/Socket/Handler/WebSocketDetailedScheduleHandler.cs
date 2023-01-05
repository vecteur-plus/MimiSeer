using Newtonsoft.Json;
using Serilog;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using WebSocketServerWebfollow.Service;
using WebSocketServerWebfollow.SocketManager;
using WebSocketSupervisorCommunicationLibrary;
using WebSocketSupervisorCommunicationLibrary.SiteCollectInformation.Model;

namespace WebSocketServerWebfollow.Handler
{
    //websocket used to send or subcribe to detailed information
    public class WebSocketDetailedScheduleHandler : SocketHandler
    {
        private readonly ILogger _logger;
        private readonly ServiceFiltre _ServiceFiltre;

        public WebSocketDetailedScheduleHandler(ConnectionManager connections_, ServiceFiltre serviceFiltre_) : base(connections_)
        {
            _logger = Log.Logger.ForContext<WebSocketDetailedScheduleHandler>();
            _ServiceFiltre = serviceFiltre_;
        }

        public override async Task OnConnected(WebSocket socket)
        {
            _logger.Information("new user connected with ID : {id} on detail site", Connections.GetId(socket));
            await base.OnConnected(socket);
        }

        public override async Task Receive(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {           
            var socketId = Connections.GetId(socket);

            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var filtre = JsonConvert.DeserializeObject<BasicSiteCollectInformationInquiry>(message);

            _logger.Information("user with id : {id} ask detail information", Connections.GetId(socket));

            Connections.SetFiltreById(socketId, filtre);
           
            var resultFilter = _ServiceFiltre.FiltreById(Connections.GetUserById(socketId));

            var json = new DetailedSiteCollectInformationCallBack(resultFilter, EAction.ADD).GetJson();
            await SendMessage(socket, json);
        }

        public override async Task OnDisconnected(WebSocket socket)
        {
            _logger.Information("user with id : {id} disconnected to detail", Connections.GetId(socket));

            await base.OnDisconnected(socket);
        }
    }
}