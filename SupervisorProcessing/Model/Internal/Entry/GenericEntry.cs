using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupervisorProcessing.Model.Internal.Entry
{
    public class GenericEntry : AEntry<object>
    {

        public GenericEntry(): base()
        {

        }

        public GenericEntry(EntityEntry entityEntry_) : base(entityEntry_)
        {
        }

        public GenericEntry(object entity_, EntityState state_) : base(entity_, state_)
        {
        }
    }
}
