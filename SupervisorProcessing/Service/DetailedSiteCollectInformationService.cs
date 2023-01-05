using Microsoft.EntityFrameworkCore;
using SupervisorProcessing.DataContext;
using SupervisorProcessing.Model.Internal;
using System.Collections.Generic;
using System.Linq;

namespace SupervisorProcessing.Service
{
    public class DetailedSiteCollectInformationService
    {
        private readonly DbContextIntern _DbContextIntern;

        public DetailedSiteCollectInformationService(IDbContextFactory<DbContextIntern> dbContextFactory_)
        {
            _DbContextIntern = dbContextFactory_.CreateDbContext();
        } 
        

        public IEnumerable<ExtendedDetailedSiteCollectInformation> GetAll()
        {
            return _DbContextIntern.DetailedSiteCollectInformations;
        }


        public IEnumerable<ExtendedDetailedSiteCollectInformation> GetAllWithScheduleInError()
        {
            return _DbContextIntern.DetailedSiteCollectInformations
                .Where(d => d.Message != "Sucess");
        }

        public static IEnumerable<ExtendedDetailedSiteCollectInformation> TransformIntoDetail(IEnumerable<CModelSchedule> schedules_)
        {
            foreach (var schedule in schedules_)
            {
                yield return TransformIntoDetail(schedule);
            }
        }

        public static IEnumerable<ExtendedDetailedSiteCollectInformation> TransformIntoDetail(IEnumerable<CModelSite> sites_)
        {
            foreach (var site in sites_)
            {
                yield return TransformIntoDetail(site);
            }
        }

        public static List<ExtendedDetailedSiteCollectInformation> TransformIntoDetail(IEnumerable<CModelSite> site_, IEnumerable<CModelSchedule> schedules_)
        {
            return (from s in site_
                    join sc in schedules_ on s.Id equals sc.IdSite into gj
                    from x in gj.DefaultIfEmpty()
                    select new ExtendedDetailedSiteCollectInformation()
                    {
                        IdSite = s.Id.ToString(),
                        IdSchedule = x == null ? "" : x.IdSchedule,
                        CleGeneral = s.CleGeneral,
                        Id = s.Id.ToString() + "&" + (x == null ? "" : x.IdSchedule),
                        ScheduleExist = x != null,
                        SiteName = s.Name,
                        AgentName = s.Agent.AgentName,
                        TypeIndexation = s.TypeIndexation.TypeIndexation,
                        Message = x == null ? "Pas de schedule" : x.LastExistMessage,
                        LastRun = x == null ? "" : x.LastRun,
                        NextRun = x == null ? "" : x.NextRun,
                        IsPaused = x != null && x.IsPaused,
                        IsRunning = x != null && x.IsRunning,
                        InputParameters = x == null ? "" : x.InputParameters,
                        Cron = x == null ? "" : x.Cron,
                        StartTime = x == null ? "" : x.StartTime,
                        IsMultiSession = s.Agent.IsMultiSession,
                        Commentaire = s.Commentaire
                    }).ToList();
        }

        public static ExtendedDetailedSiteCollectInformation TransformIntoDetail(CModelSite site_)
        {
            return new ExtendedDetailedSiteCollectInformation()
            {
                IdSite = site_.Id.ToString(),
                IdSchedule = "",
                CleGeneral = site_.CleGeneral,
                Id = site_.Id.ToString() + "&",
                ScheduleExist = false,
                SiteName = site_.Name,
                AgentName = site_.Agent.AgentName,
                TypeIndexation = site_.TypeIndexation.TypeIndexation,
                Message = "Pas de schedule",
                LastRun = "",
                NextRun = "",
                IsPaused = false,
                IsRunning = false,
                InputParameters = "",
                Cron = "",
                StartTime = "",
                IsMultiSession = site_.Agent.IsMultiSession,
                Commentaire = site_.Commentaire
            };
        }

        public static ExtendedDetailedSiteCollectInformation TransformIntoDetailWithoutSchedule(CModelSchedule schedule_)
        {
            return new ExtendedDetailedSiteCollectInformation()
            {
                IdSite = schedule_.Site.Id.ToString(),
                IdSchedule = "",
                CleGeneral = schedule_.Site.CleGeneral,
                Id = schedule_.Site.Id.ToString() + "&",
                ScheduleExist = false,
                SiteName = schedule_.Site.Name,
                AgentName = schedule_.Site.Agent.AgentName,
                TypeIndexation = schedule_.Site.TypeIndexation.TypeIndexation,
                Message = "Pas de schedule",
                LastRun = "",
                NextRun = "",
                IsPaused = false,
                IsRunning = false,
                InputParameters = "",
                Cron = "",
                StartTime = "",
                IsMultiSession = schedule_.Site.Agent.IsMultiSession,
                Commentaire = schedule_.Site.Commentaire
            };
        }

        public static ExtendedDetailedSiteCollectInformation TransformIntoDetail(CModelSchedule schedule_)
        {
            return new ExtendedDetailedSiteCollectInformation()
            {
                IdSite = schedule_.Site.Id.ToString(),
                IdSchedule = schedule_.IdSchedule,
                CleGeneral = schedule_.Site.CleGeneral,
                Id = schedule_.Site.Id.ToString() + "&" + schedule_.IdSchedule,
                ScheduleExist = true,
                SiteName = schedule_.Site.Name,
                AgentName = schedule_.Site.Agent.AgentName,
                TypeIndexation = schedule_.Site.TypeIndexation.TypeIndexation,
                Message = schedule_.LastExistMessage,
                LastRun = schedule_.LastRun,
                NextRun = schedule_.NextRun,
                IsPaused = schedule_.IsPaused,
                IsRunning = schedule_.IsRunning,
                InputParameters = schedule_.InputParameters,
                Cron = schedule_.InputParameters,
                StartTime = schedule_.StartTime,
                IsMultiSession = schedule_.Site.Agent.IsMultiSession,
                Commentaire = schedule_.Site.Commentaire
            };
        }
    }
}