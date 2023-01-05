using System.Collections.Generic;
using System.Net.WebSockets;
using WebSocketSupervisorCommunicationLibrary.SiteCollectInformation.Model;

namespace WebSocketServerWebfollow.Model.Internal
{
    public class User
    {
        public User(WebSocket webSocket_)
        {
            WebSocket = webSocket_;
        }

        public WebSocket WebSocket { get; set; }
        public FilterCriteriaSiteCollectInformation FilterCriteria { get; set; }
        public List<string> IdSiteCollectInformationTracked  { get; set; }
        public List<string> IdSchedules { get; set; } = new();
    }
}