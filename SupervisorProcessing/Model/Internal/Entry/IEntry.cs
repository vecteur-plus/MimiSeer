using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupervisorProcessing.Model.Internal.Entry
{
    public interface IEntry<T> where T : class
    {
        public T Entity { get; set; }
        public EntityState State { get; set; }
   
    }
}
