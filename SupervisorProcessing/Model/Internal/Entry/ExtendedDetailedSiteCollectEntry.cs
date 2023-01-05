using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupervisorProcessing.Model.Internal.Entry
{
    public class ExtendedDetailedSiteCollectEntry : AEntry<ExtendedDetailedSiteCollectInformation>
        
    {
        public ExtendedDetailedSiteCollectEntry()
        {
        }

        public ExtendedDetailedSiteCollectEntry(EntityEntry entityEntry_) : base(entityEntry_)
        {
        }

        public ExtendedDetailedSiteCollectEntry(ExtendedDetailedSiteCollectInformation entity_, EntityState state_) : base(entity_, state_)
        {
        }
    }
}
