using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupervisorProcessing.Model.Internal
{
    public class ScheduleMessage
    {
        [Key]
        public string Message { get; set; }

        public ScheduleMessage()
        {

        }
    }
}
