using Microsoft.EntityFrameworkCore;
using SupervisorProcessing.Dao;
using SupervisorProcessing.DataContext;
using SupervisorProcessing.Model.Internal;
using SupervisorProcessing.Repository;
using SupervisorProcessing.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupervisorProcessing.Service
{
    public class ServiceSchedule
    {
        private readonly ScheduleRepository _repoContentGrabber;
        private readonly IDbContextFactory<DbContextIntern> _DbContextFactory;
       

        public ServiceSchedule(ScheduleRepository repoContentGrabber_, IDbContextFactory<DbContextIntern>  dbContextFactory_)
        {
          
            _repoContentGrabber = repoContentGrabber_;
            _DbContextFactory = dbContextFactory_;
          
            
        }

        //check ifdatabase is accessible
        public bool DatabaseIsAccessible()
        {
            return _repoContentGrabber.IsAccessible();
        }

        //find schedule not save in DbContextIntern
        public List<CModelSchedule> FindNewSchedules()
        {
           
            using var context = _DbContextFactory.CreateDbContext();

            ConcurrentBag<CModelSchedule> newSchedules = new();

            var guidSchedules = context.Schedules.Select(s => s.GuidSchedule).ToList();

            var result = _repoContentGrabber.FindOffList(guidSchedules).ToList();

            var agentNames = result.Select(r => r.agent_name_).ToList();

            var sites = context.Sites
                .Include(s => s.Agent)
                .Where(s => agentNames.Contains(s.Agent.AgentName))
                .ToList();

            Parallel.ForEach(sites, s =>
            {
                List<Schedule> values = new();

                List<Guid> Guids = s.Schedules.Select(s => s.GuidSchedule).ToList();

                //add value if site's agent is multisession and site's name equal to one of the schedule's sessionId
                //or if site's agent is not multisession and site's name equal to one of the schedule's agent name
                //and if schedule is not already insert 
                values.AddRange(result
                    .Where(r => ((s.Agent.IsMultiSession && r.session_id_ == s.Name) ||
                    (!s.Agent.IsMultiSession && r.agent_name_ == s.Agent.AgentName)) &&
                    !Guids.Contains(r.schedule_id_)));

                var schedules = Mapper.GetMapper().Map<List<CModelSchedule>>(values);
                schedules.ForEach(sc => sc.IdSite = s.Id);
                newSchedules.AddRange(schedules);
            });

            return newSchedules.ToList();
        }

        public List<CModelSchedule> FindSchedulesToCheck()
        {
            return Mapper.GetMapper().Map<List<CModelSchedule>>(_repoContentGrabber.FindAll());
        }
    }
}