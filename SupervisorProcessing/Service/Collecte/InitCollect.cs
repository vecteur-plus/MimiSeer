using Microsoft.EntityFrameworkCore;
using Serilog;
using SupervisorProcessing.DataContext;
using SupervisorProcessing.Model.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SupervisorProcessing.Service.Collecte
{
    public partial class ServiceCollect
    {
    
        private readonly ServiceSite _serviceSite;
        private readonly ServiceFlag _serviceFlag;
        private readonly ServiceSchedule _serviceSchedule;
        private readonly ServiceTypeIndexation _ServiceTypeIndexation;
        private readonly ServiceAgent _ServiceAgent;
        private static ILogger _Logger;       
        public static EventHandler EndIterationProcessingHook;
        private readonly DbContextIntern _DbContextIntern;
        private readonly ScheduleMessageService _ScheduleMessageService;
        private readonly EntityEntryService _EntityEntryService;

        public ServiceCollect(ServiceSchedule serviceSchedule_,ServiceSite serviceSite_, ServiceFlag serviceFlag_,
            ServiceTypeIndexation serviceTypeIndexation_, ServiceAgent serviceAgent_,
            ScheduleMessageService scheduleMessageService_,EntityEntryService entityEntryService_,
           IDbContextFactory<DbContextIntern> dbContextFactory_)
        {      
         
            _serviceSite = serviceSite_;
            _serviceFlag = serviceFlag_;
            _Logger = Log.Logger.ForContext<ServiceCollect>();
            _serviceSchedule = serviceSchedule_;
            _ServiceTypeIndexation = serviceTypeIndexation_;
            _ServiceAgent = serviceAgent_;
            _DbContextIntern = dbContextFactory_.CreateDbContext();
            _ScheduleMessageService = scheduleMessageService_;
            _EntityEntryService = entityEntryService_;  
            
        }

        public void Initialisation()
        {
            _Logger.Information("Start initilization");

            InitialiseTypeIndexation();
            InitialisationAgent();
            InitialisationSites();
            InitialisationSchedules();        
            InitialisationDetailedInformation();
            InitialisationScheduleMessage();

           
            _EntityEntryService.Entries.Clear();

            _Logger.Information("Initilization finished");
        }

        private void InitialisationDetailedInformation()
        {
            //get detailedSiteCollectInformation
            var sites = _DbContextIntern.Sites.ToList();
            var schedules = _DbContextIntern.Schedules.ToList();
            var info = DetailedSiteCollectInformationService.TransformIntoDetail(sites, schedules);
    
            _DbContextIntern.DetailedSiteCollectInformations.AddRange(info);
            _DbContextIntern.SaveChanges();

            _Logger.Information("{count} new entries found", info.Count);

            _EntityEntryService.Entries.Clear();
            
        }

        private void InitialisationScheduleMessage()
        {
            var message = _ScheduleMessageService.FindDistinctScheduleOffList(_DbContextIntern.ScheduleMessages).ToList();
            _DbContextIntern.ScheduleMessages.AddRange(message);

            _DbContextIntern.SaveChanges();
            _Logger.Information("{count} new schedule messages found", message.Count);
        }

        private void InitialisationSchedules()
        {
            var Schedules = _serviceSchedule.FindNewSchedules();

            _Logger.Information("{count} schedules found", Schedules.Count);

            _DbContextIntern.Schedules.AddRange(Schedules);
            _DbContextIntern.SaveChanges();
        }

        private void InitialisationAgent()
        {
            List<CModelAgent> Agents = _ServiceAgent.FindDistinctAgentOffList(_DbContextIntern.Agents);

            _Logger.Information("{count} agents found", Agents.Count);

            _DbContextIntern.Agents.AddRange(Agents);
            _DbContextIntern.SaveChanges();
        }

        private void InitialisationSites()
        {
            _serviceFlag.UpdatedFlagToSeen();

            _Logger.Information("flag set at see");

            var Sites = _serviceSite.FindAllSites();

            _Logger.Information("{count} sites found", Sites.Count);
     
            _DbContextIntern.Sites.AddRange(Sites);

            _DbContextIntern.SaveChanges();

        }

        private void InitialiseTypeIndexation()
        {
            List<CModelTypeIndexation> TypeIndexations = _ServiceTypeIndexation
                .FindTypeIndexationOffList(_DbContextIntern.TypeIndexations);

            _Logger.Information("{count} type of indexation found", TypeIndexations.Count);

            _DbContextIntern.TypeIndexations.AddRange(TypeIndexations);

            _DbContextIntern.SaveChanges();
        }
    }
}