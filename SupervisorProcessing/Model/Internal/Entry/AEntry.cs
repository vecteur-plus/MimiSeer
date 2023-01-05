using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupervisorProcessing.Model.Internal.Entry
{
    public abstract class AEntry<T>: IEntry<T> where T : class
    {
        public T Entity { get; set; }
        public EntityState State { get; set; }

        public AEntry()
        {

        }

        public AEntry(T entity_, EntityState state_)
        {
            Entity = entity_;
            State = state_;
        }

        public AEntry(EntityEntry entityEntry_)
        {
            Entity = (T)entityEntry_.Entity;
            State = entityEntry_.State;
        }
    }
}
