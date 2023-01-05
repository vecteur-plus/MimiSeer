using Microsoft.Extensions.Options;
using SupervisorProcessing.Service.Collecte;
using SupervisorProcessing.Utils;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using WebSocketServerWebfollow.Model;
using WebSocketServerWebfollow.Model.Internal;
using WebSocketSupervisorCommunicationLibrary.SiteCollectInformation.Model;

namespace WebSocketServerWebfollow.SocketManager
{
    public class ConnectionManager
    {
        private ConcurrentDictionary<string, User> _connections = new();
        private readonly IOptions<ConfigTimeCollectLoop> _Config;

        public ConnectionManager(IOptions<ConfigTimeCollectLoop> config_)
        {
            _Config = config_;
        }

        public WebSocket GetSocketById(string id)
        {
            return _connections.FirstOrDefault(x => x.Key == id).Value.WebSocket;
        }

        public User GetUserById(string id)
        {
            return _connections.FirstOrDefault(x => x.Key == id).Value;
        }

        public ConcurrentDictionary<string, User> GetAllConnections()
        {
            return _connections;
        }

        public string GetId(WebSocket socket)
        {
            return _connections.FirstOrDefault(x => x.Value.WebSocket == socket).Key;
        }

        public async Task RemoveSocketAsync(string id)
        {
            if (_connections.TryRemove(id, out var user))
            {
                await user.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "socket connection closed", CancellationToken.None);
               
                if (_connections.Count == 0)
                {
                    CollectManager.ModifyIntervalLoop(_Config.Value.TimeCollectLoopWithoutUser);
                }
            }
        }

        public void AddSocket(WebSocket socket)
        {
            User user = new(socket);
            if (_connections.TryAdd(GetConnectionId(), user))
            {
                //modify speed
                if (_connections.Count == 1)
                {
                    CollectManager.ModifyIntervalLoop(_Config.Value.TimeCollectLoopWithUser);
                }
            }
        }

        private string GetConnectionId()
        {
            return Guid.NewGuid().ToString("N");
        }

        public FilterCriteriaSiteCollectInformation GetFiltreById(string Id)
        {
            return _connections.FirstOrDefault(x => x.Key == Id).Value.FilterCriteria;
        }

        public void SetFiltreById(string Id, BasicSiteCollectInformationInquiry Filtre_)
        {
            var user = GetUserById(Id);
            user.FilterCriteria = new(Filtre_);
        }

        public void SetFiltreById(string Id, DetailedSiteCollectInformationInquiry Filtre_)
        {
            var user = GetUserById(Id);
            user.FilterCriteria = new(Filtre_);
        }

        public int GetNumberConnection()
        {
            return _connections.Count;
        }
    }
}