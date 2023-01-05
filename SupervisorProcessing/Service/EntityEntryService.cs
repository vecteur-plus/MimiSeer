using Microsoft.EntityFrameworkCore;
using SupervisorProcessing.DataContext;
using SupervisorProcessing.Model.Internal;
using SupervisorProcessing.Model.Internal.Entry;
using System.Collections.Generic;
using System.Linq;

namespace SupervisorProcessing.Service
{
    public class EntityEntryService
    {

        public List<GenericEntry> Entries { get; set; } = new();

        public EntityEntryService()
        {
            
        }

        public IEnumerable<T> GetValue<T>(EntityState state_)
        {
            return Entries
                  .Where(e => e.Entity is T && e.State == state_)
                  .Select(e => e.Entity)
                  .Cast<T>();
        }

        public IEnumerable<T> GetValue<T>()
        {
            return Entries
                  .Where(e => e.Entity is T)
                  .Select(e => e.Entity)
                  .Cast<T>();
        }



        public IEnumerable<ExtendedDetailedSiteCollectEntry> GetExtendedDetailedSiteCollectInformation()
        {
            return Entries
                  .Where(e => e.Entity is ExtendedDetailedSiteCollectInformation)
                  .Select(e => new ExtendedDetailedSiteCollectEntry((ExtendedDetailedSiteCollectInformation)e.Entity, e.State));
                           
                 
        }

        //get number of object with type T and state equal entityState_
        public int Count<T>(EntityState entityState_)
        {
            return Entries.Count(e => e.State == entityState_ && e.Entity is T);
        }

        public int Count<T>()
        {
            return Entries.Count(e => e.Entity is T);
        }

        public int Count()
        {
            return Entries.Count();
        }

    }
}
