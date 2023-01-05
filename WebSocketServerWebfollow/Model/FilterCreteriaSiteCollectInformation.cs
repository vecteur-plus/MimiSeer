using System.Collections.Generic;
using WebSocketSupervisorCommunicationLibrary;
using WebSocketSupervisorCommunicationLibrary.SiteCollectInformation.Model;

namespace WebSocketServerWebfollow.Model
{
    public class FilterCriteriaSiteCollectInformation
    {

        public FilterCriteriaSiteCollectInformation()
        {

        }
        public FilterCriteriaSiteCollectInformation(DetailedSiteCollectInformationInquiry detailedSiteCollectInformationInquiry_)
        {
            IdSiteCollectInformation = detailedSiteCollectInformationInquiry_.IdSiteCollectInformation;
        }

        public FilterCriteriaSiteCollectInformation(BasicSiteCollectInformationInquiry basicSiteCollectInformationInquiry_)
        {
            TypeFilter = basicSiteCollectInformationInquiry_.TypeFilter;
            TypeIndexations = basicSiteCollectInformationInquiry_.TypeIndexations;
            AgentNames = basicSiteCollectInformationInquiry_.AgentNames;
            SiteNames = basicSiteCollectInformationInquiry_.SiteNames;
            MessageSchedules = basicSiteCollectInformationInquiry_.MessageSchedules;
            IdSiteCollectInformation = basicSiteCollectInformationInquiry_.IdSiteCollectInformation;
            IdSite = basicSiteCollectInformationInquiry_.IdSite;
            IsPaused = basicSiteCollectInformationInquiry_.IsPaused;
            IsRunning = basicSiteCollectInformationInquiry_.IsRunning;  
            IsContains = basicSiteCollectInformationInquiry_.IsContains;
        }

        public EBasicInquiryAction TypeFilter { get; set; }
        public List<string> TypeIndexations { get; set; }
        public List<string> AgentNames { get; set; }
        public List<string> SiteNames { get; set; }
        public List<string> MessageSchedules { get; set; }       
        public string IdSiteCollectInformation { get; set; }
        public string IdSite { get; set; }
        public int IsPaused { get; set; }
        public int IsRunning { get; set; }
        public bool IsContains { get; set; }
    }
}
