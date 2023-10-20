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
using WebSocketSupervisorCommunicationLibrary.FilterCriteria.Model;
using WebSocketSupervisorCommunicationLibrary.FilterCriteria.Service;

namespace WebSocketServerWebfollow.Handler
{
    //websocket used to send or subcribe to basic information
    public class WebSocketFilterCriteriaHandler : SocketHandler
    {
        private ILogger _logger;
        private readonly DbContextIntern _DbContextIntern;

        public WebSocketFilterCriteriaHandler(ConnectionManager connections_,
            IDbContextFactory<DbContextIntern> dbContextFactory_) : base(connections_)
        {
            _logger = Log.Logger.ForContext<WebSocketFilterCriteriaHandler>();

            _DbContextIntern = dbContextFactory_.CreateDbContext();
        }

        //When user connect to websocket, it send filter information
        public override async Task OnConnected(WebSocket socket)
        {

            await base.OnConnected(socket);

            var socketId = Connections.GetId(socket);

            _logger.Information("new user connected with ID : {id} on filter criteria", Connections.GetId(socket));

            var serviceMessage = new FilterCriteriaMessageCreator();
            var message = serviceMessage.CreateMessage(ETypeMessage.NEW);

            //add filter information
            AddFilterToMessage(message);

            var json = message.GetJson();
            await SendMessage(socketId, json);
        }

        public override async Task Receive(WebSocket socket, WebSocketReceiveResult result, byte[] buffer)
        {
            //FilterCriteriaInquiry inquiry;

            var serviceMessage = new FilterCriteriaMessageCreator();
            var messageResponse = serviceMessage.CreateMessage(ETypeMessage.NEW);
            var socketId = Connections.GetId(socket);

            _logger.Information("user with id : {id} ask to get filter information", Connections.GetId(socket));

            


            //send information filter like agent name

            AddFilterToMessage(messageResponse);
            var json = messageResponse.GetJson();
            await SendMessage(socketId, json);



        }

        public override async Task OnDisconnected(WebSocket socket)
        {
            _logger.Information("user with id : {id} disconnected to resume", Connections.GetId(socket));

            await base.OnDisconnected(socket);
        }

       

        private void AddFilterToMessage(FilterCriteriaReponse message_)
        {
            var serviceMessage = new FilterCriteriaMessageCreator();

            serviceMessage.AddAgents(_DbContextIntern.Agents.Select(a => a.AgentName).ToList(), message_);
            serviceMessage.AddTypeIndexations(_DbContextIntern.TypeIndexations.Select(t => t.TypeIndexation).ToList(), message_);
            serviceMessage.AddMessagesSchedule(_DbContextIntern.ScheduleMessages.Select(m => m.Message).ToList(), message_);
        }

       /* private async void SendErrorMessage(WebSocket webSocket_, string message_)
        {
            BasicSiteCollectInformationCallback callback = new();
            callback.TypeMessage = ETypeMessage.ERROR;
            callback.Message = message_;

            _logger.Information("Error message sent");
            await SendMessage(webSocket_, callback.GetJson());
        }*/
    }
}