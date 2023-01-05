using Microsoft.EntityFrameworkCore;
using SupervisorProcessing.DataContext;
using SupervisorProcessing.Model.Internal;
using System.Collections.Generic;
using System.Linq;

namespace SupervisorProcessing.Service
{
    public class ScheduleMessageService
    {
        private readonly DbContextIntern _DBContextIntern;

        public ScheduleMessageService(IDbContextFactory<DbContextIntern> dbContextFactory_)
        {
            _DBContextIntern = dbContextFactory_.CreateDbContext();
        }

        //get message not find in scheduleMessages_
        public IEnumerable<ScheduleMessage> FindDistinctScheduleOffList(IEnumerable<ScheduleMessage> scheduleMessages_)
        {

            var distinctMessage = GetDistinctMessage();

           return distinctMessage.Except(scheduleMessages_.Select(s => s.Message))
                .Select(s => new ScheduleMessage() { Message = s});
        }

        //get message existing in _DBContextIntern.ScheduleMessages but not find in _DBContextIntern.DetailedSiteCollectInformations
        public IEnumerable<ScheduleMessage> FindScheduleMessageToDelete()
        {
            return _DBContextIntern.ScheduleMessages
                .AsNoTracking()
                .Where(m => !GetDistinctMessage().Contains(m.Message))
                .Select(s => new ScheduleMessage() { Message = s.Message });
             
        }

        //Get distinct message
        private List<string> GetDistinctMessage()
        {
            return _DBContextIntern.DetailedSiteCollectInformations.Select(d => d.Message)
                .Distinct().ToList();
        }


    }
}
