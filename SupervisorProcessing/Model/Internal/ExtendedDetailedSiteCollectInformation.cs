using Newtonsoft.Json;
using WebSocketSupervisorCommunicationLibrary.SiteCollectInformation.Interface;
using WebSocketSupervisorCommunicationLibrary.SiteCollectInformation.Model;

namespace SupervisorProcessing.Model.Internal
{
    public class ExtendedDetailedSiteCollectInformation : DetailedSiteCollectInformation,IExtendedSiteCollectInformation
    {
        [JsonIgnore]
        public string IdSite { get; set; }

        [JsonIgnore]
        public string IdSchedule { get; set; }

        [JsonIgnore]
        public override string CleGeneral { get; set; }
        
        public void Update(CModelSchedule schedule_)
        {
            this.LastRun = schedule_.LastRun;
            this.NextRun = schedule_.NextRun;
            this.Message = schedule_.LastExistMessage;
            this.IsPaused = schedule_.IsPaused;
            this.IsRunning = schedule_.IsRunning;
            this.InputParameters = schedule_.InputParameters;
            this.Cron = schedule_.Cron;
            this.StartTime = schedule_.StartTime;
            this.IsMultiSession = schedule_.Site.Agent.IsMultiSession;
            this.Commentaire = schedule_.Site.Commentaire;
        }

        public void Update(CModelSite site_)
        {
            this.TypeIndexation = site_.TypeIndexation.TypeIndexation;
            this.AgentName = site_.Agent.AgentName;
            this.IsMultiSession = site_.Agent.IsMultiSession;
            this.Commentaire = site_.Commentaire;
        }
    }
}