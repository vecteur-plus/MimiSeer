using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using SupervisorProcessing.DataContext;
using SupervisorProcessing.Model.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using WebSocketServerWebfollow.Service;
using WebSocketServerWebfollow.SocketManager;
using WebSocketSupervisorCommunicationLibrary;
using WebSocketSupervisorCommunicationLibrary.SiteCollectInformation.Map;
using WebSocketSupervisorCommunicationLibrary.SiteCollectInformation.Model;
using WebSocketSupervisorCommunicationLibrary.SiteCollectInformation.Service;

namespace WebSocketServerWebfollow.Handler
{
    //websocket used to send or subcribe to basic information
    public class WebSocketResumeHandler : SocketHandler
    {
        private ILogger _logger;
        private readonly ServiceFiltre _ServiceFiltre;
        private readonly DbContextIntern _DbContextIntern;

        public WebSocketResumeHandler(ConnectionManager connections_, ServiceFiltre serviceFiltre_,
            IDbContextFactory<DbContextIntern> dbContextFactory_) : base(connections_)
        {
            _logger = Log.Logger.ForContext<WebSocketResumeHandler>();
            _ServiceFiltre = serviceFiltre_;
            _DbContextIntern = dbContextFactory_.CreateDbContext();
        }

        //When user connect to websocket, it send filter information
        public override async Task OnConnected(WebSocket socket)
        {

            await base.OnConnected(socket);

            var socketId = Connections.GetId(socket);

            _logger.Information("new user connected with ID : {id} on resume", socketId);

        }

        public override async Task Receive(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            BasicSiteCollectInformationInquiry filter;

            var serviceMessage = new BasicSiteCollectInformationMessageCreator();
            var messageResponse = serviceMessage.CreateMessage(ETypeMessage.NEW);
            var socketId = Connections.GetId(socket);

            _logger.Information("user with id : {id} ask to filter information", Connections.GetId(socket));

            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            try
            {
                filter = JsonConvert.DeserializeObject<BasicSiteCollectInformationInquiry>(message);
            }
            catch (Exception e_)
            {
                _logger.Error("An error occured during message deserilization : {message}, Error : {error}", message, e_.Message);
                SendErrorMessage(socket, "An error occured during message deserilization");
                return;
            }

            if(filter == null)
            {
                _logger.Error("An error occured filter is null, user id :{id}",socketId);
                SendErrorMessage(socket, "An error occured filter is null");
                return;
            } 


            List<ExtendedDetailedSiteCollectInformation> resultFilter;


            switch (filter.TypeFilter)
            {
                case EBasicInquiryAction.WithFilter:

                    SetFilter(socketId, filter);

                    _logger.Information("user with id : {id}, get only schedules which match to filter criteria ", Connections.GetId(socket));
                    resultFilter = _ServiceFiltre.FilterAllSites(Connections.GetUserById(socketId));
                    break;

                case EBasicInquiryAction.OnlyError:
                 
                    SetFilter(socketId, filter);
                    _logger.Information("user with id : {id}, get only schedules in error which match to filter criteria ", Connections.GetId(socket));
                    resultFilter = _ServiceFiltre.FiltrerScheduleWithError(Connections.GetUserById(socketId));
                    break;              
                default:

                    _logger.Error(" An error with the filter type of user with id : {id} , json : {json}", Connections.GetId(socket), message);
                    SendErrorMessage(socket, "An error with the filter type of user");
                    return;
            }

            //transform detailed information into basic information
            var basicInformations = Mapper.GetMapper().Map<List<ExtendedDetailedSiteCollectInformation>,
                List<BasicSiteCollectInformation>>(resultFilter);

            serviceMessage.AffecterResume(basicInformations, messageResponse, EAction.ADD);

            await SendMessage(socket, messageResponse.GetJson());
        }

        public override async Task OnDisconnected(WebSocket socket)
        {
            _logger.Information("user with id : {id} disconnected to resume", Connections.GetId(socket));

            await base.OnDisconnected(socket);
        }

        private void SetFilter(string id_, BasicSiteCollectInformationInquiry inquiry_)
        {
            _logger.Information("user with id : {id} set new filter criteria", id_);
            Connections.SetFiltreById(id_, inquiry_);
        }

      
        private async void SendErrorMessage(WebSocket webSocket_, string message_)
        {
            BasicSiteCollectInformationCallback callback = new();
            callback.TypeMessage = ETypeMessage.ERROR;
            callback.Message = message_;

            _logger.Information("Error message sent");
            await SendMessage(webSocket_, callback.GetJson());
        }
    }
}