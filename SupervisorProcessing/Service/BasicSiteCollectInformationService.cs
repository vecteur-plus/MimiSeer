using SupervisorProcessing.Model.Internal;
using System.Collections.Generic;
using System.Linq;
using WebSocketSupervisorCommunicationLibrary.SiteCollectInformation.Map;
using WebSocketSupervisorCommunicationLibrary.SiteCollectInformation.Model;

namespace SupervisorProcessing.Service
{
    public class BasicSiteCollectInformationService
    {
        
        private readonly DetailedSiteCollectInformationService _DetailedInformationService;

        public BasicSiteCollectInformationService(DetailedSiteCollectInformationService detailedInformationService_)
        {
            
            _DetailedInformationService = detailedInformationService_;  
        }

        public IEnumerable<BasicSiteCollectInformation> GetAllWithScheduleInError()
        {
           var result = Mapper.GetMapper()
                .Map<List<ExtendedDetailedSiteCollectInformation>, List<BasicSiteCollectInformation>>(_DetailedInformationService.GetAllWithScheduleInError().ToList());

            return result;
        }

        public IEnumerable<BasicSiteCollectInformation> GetAll()
        {
            var result = Mapper.GetMapper()
                .Map<List<ExtendedDetailedSiteCollectInformation>, List<BasicSiteCollectInformation>>(_DetailedInformationService.GetAll().ToList());

            return result;
        }
    }
}
