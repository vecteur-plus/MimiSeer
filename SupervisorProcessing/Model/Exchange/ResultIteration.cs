using SupervisorProcessing.Model.Internal;
using SupervisorProcessing.Model.Internal.Entry;
using System.Collections.Generic;

namespace SupervisorProcessing.Model.Exchange
{
    public class ResultIteration
    {
        public List<CModelTypeIndexation> TypeIndexationsAdded { get; set; }
        public List<CModelAgent> AgentsAdded { get; set; }
        public List<CModelAgent> AgentDeleted { get; set; }
        public List<string> MessageAdded { get; set; }
        public List<string> MessageDeleted { get; set; }
        public List<ExtendedDetailedSiteCollectEntry> DetailedSiteCollectInformationEntries { get; set; }
    }
}