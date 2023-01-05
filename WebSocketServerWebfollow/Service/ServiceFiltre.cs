using Microsoft.EntityFrameworkCore;
using SupervisorProcessing.DataContext;
using SupervisorProcessing.Model.Internal;
using SupervisorProcessing.Model.Internal.Entry;
using System.Collections.Generic;
using System.Linq;
using WebSocketServerWebfollow.Model;
using WebSocketServerWebfollow.Model.Internal;
using WebSocketSupervisorCommunicationLibrary.InformationRun.Model;


namespace WebSocketServerWebfollow.Service
{
    public class ServiceFiltre
    {
        private readonly IDbContextFactory<DbContextIntern> _DbContextFactory;

        public ServiceFiltre(IDbContextFactory<DbContextIntern> dbContextFactory_)
        {
            _DbContextFactory = dbContextFactory_;
        }

        public List<ExtendedDetailedSiteCollectInformation> FilterAllSites(User user_)
        {
            var data = FilterAllSites(user_.FilterCriteria);
            user_.IdSiteCollectInformationTracked = data.Select(d => d.Id).ToList();
            return data;
        }

        public List<ExtendedDetailedSiteCollectInformation> FilterAllSites(FilterCriteriaSiteCollectInformation filterCriteriaSiteCollectInformation)
        {
            var filtre = filterCriteriaSiteCollectInformation;

            var data = _DbContextFactory.CreateDbContext().DetailedSiteCollectInformations.ToList();


            data = FilterByIsPaused(data, filtre.IsPaused);
            data = FilterByIsRunning(data,filtre.IsRunning);
       
            if (filtre.MessageSchedules != null && filtre.MessageSchedules.Count > 0)
            {
                data = data.Where(d => filtre.MessageSchedules.Any(m => d.Message.Contains(m))).ToList();
            }

            if (filtre.AgentNames != null && filtre.AgentNames.Count > 0)
            {
                data = data.Where(d => filtre.AgentNames.Any(a => d.AgentName.Contains(a))).ToList();
            }

            if (filtre.TypeIndexations != null && filtre.TypeIndexations.Count > 0)
            {
                data = data.Where(d => filtre.TypeIndexations.Any(t => d.TypeIndexation.Contains(t))).ToList();
            }

            if (filtre.IsContains == true && filtre.SiteNames != null && filtre.SiteNames.Count > 0)
            {
                data = data.Where(d => filtre.SiteNames.Any(n => d.SiteName.Contains(n))).ToList();
            }

            if (filtre.IsContains == false && filtre.SiteNames != null && filtre.SiteNames.Count > 0)
            {
                data = data.Where(d => filtre.SiteNames.Any(n => d.SiteName == n)).ToList();
            }

            if (!string.IsNullOrEmpty(filtre.IdSiteCollectInformation))
            {
                data = data.Where(d => d.Id == filtre.IdSiteCollectInformation).ToList();
            }

            if (!string.IsNullOrEmpty(filtre.IdSite))
            {
                data = data.Where(d => d.CleGeneral == filtre.IdSite).ToList();
            }
            return data;
        }

        public ExtendedDetailedSiteCollectInformation FiltreById(User user_)
        {
            return _DbContextFactory.CreateDbContext().DetailedSiteCollectInformations.Where(r => r.Id == user_.FilterCriteria.IdSiteCollectInformation).FirstOrDefault();
        }

        public ExtendedDetailedSiteCollectEntry FiltreById(List<ExtendedDetailedSiteCollectEntry> resumeEntries_, User user_)
        {
            return resumeEntries_.Where(r => r.Entity.Id == user_.FilterCriteria.IdSiteCollectInformation).FirstOrDefault();
        }

        public List<ExtendedDetailedSiteCollectEntry> FiltreList(List<ExtendedDetailedSiteCollectEntry> resumeEntries_, User user_)
        {
            List<ExtendedDetailedSiteCollectEntry> result = new();
            var Filtre = user_.FilterCriteria;

            var ResumeEntryFilteredAD = resumeEntries_.Where(r => r.State == EntityState.Deleted || r.State == EntityState.Added).ToList();

            var ResumeEntryFilteredM = resumeEntries_.Where(r => r.State == EntityState.Modified).ToList();

            List<ExtendedDetailedSiteCollectEntry> ResumeEnregistered = ResumeEntryFilteredM.Where(r => user_.IdSiteCollectInformationTracked.Contains(r.Entity.Id)).ToList();
            var ResumeNotEnregistered = ResumeEntryFilteredM.Where(r => !user_.IdSiteCollectInformationTracked.Contains(r.Entity.Id)).ToList();

            ResumeEntryFilteredAD.AddRange(ResumeNotEnregistered
                .Select(r => new ExtendedDetailedSiteCollectEntry(r.Entity, EntityState.Added)).ToList());

            List<ExtendedDetailedSiteCollectEntry> ResumeEnregisteredToModify = ResumeEnregistered;
            List<ExtendedDetailedSiteCollectEntry> ResumeEnregisteredToDelete = ResumeEnregistered;

            List<ExtendedDetailedSiteCollectEntry> temp = new();

            switch (Filtre.IsPaused)
            {
                case 2:

                    ResumeEnregisteredToModify = ResumeEnregisteredToModify.Where(e => e.Entity.IsPaused == true).ToList();
                    temp.AddRange(ResumeEnregisteredToDelete.Where(e => e.Entity.IsPaused == false).ToList());
                    ResumeEntryFilteredAD = ResumeEntryFilteredAD.Where(e => e.Entity.IsPaused == true).ToList();
                    break;

                case 3:
                    ResumeEnregisteredToModify = ResumeEnregisteredToModify.Where(e => e.Entity.IsPaused == false).ToList();
                    temp.AddRange(ResumeEnregisteredToDelete.Where(e => e.Entity.IsPaused == true).ToList());
                    ResumeEntryFilteredAD = ResumeEntryFilteredAD.Where(e => e.Entity.IsPaused == false).ToList();
                    break;

                default:
                    break;
            }

            switch (Filtre.IsRunning)
            {
                case 2:
                    ResumeEnregisteredToModify = ResumeEnregisteredToModify.Where(e => e.Entity.IsRunning == true).ToList();
                    temp.AddRange(ResumeEnregisteredToDelete.Where(e => e.Entity.IsRunning == false).ToList());
                    ResumeEntryFilteredAD = ResumeEntryFilteredAD.Where(e => e.Entity.IsRunning == true).ToList();
                    break;

                case 3:
                    ResumeEnregisteredToModify = ResumeEnregisteredToModify.Where(e => e.Entity.IsRunning == false).ToList();
                    temp.AddRange(ResumeEnregisteredToDelete.Where(e => e.Entity.IsRunning == true).ToList());
                    ResumeEntryFilteredAD = ResumeEntryFilteredAD.Where(e => e.Entity.IsRunning == false).ToList();
                    break;

                default:
                    break;
            }

            if (Filtre.MessageSchedules != null && Filtre.MessageSchedules.Count > 0)
            {
                ResumeEnregisteredToModify = ResumeEnregisteredToModify.Where(e => Filtre.MessageSchedules.Any(m => e.Entity.Message.Contains(m))).ToList();
                temp.AddRange(ResumeEnregisteredToDelete.Where(e => !Filtre.MessageSchedules.Any(m => e.Entity.Message.Contains(m))).ToList());
                ResumeEntryFilteredAD = ResumeEntryFilteredAD.Where(e => Filtre.MessageSchedules.Any(m => e.Entity.Message.Contains(m))).ToList();
            }

            if (Filtre.AgentNames != null && Filtre.AgentNames.Count > 0)
            {
                ResumeEnregisteredToModify = ResumeEnregisteredToModify.Where(e => Filtre.AgentNames.Any(a => e.Entity.AgentName.Contains(a))).ToList();
                temp.AddRange(ResumeEnregisteredToDelete.Where(e => !Filtre.AgentNames.Any(a => e.Entity.AgentName.Contains(a))).ToList());
                ResumeEntryFilteredAD = ResumeEntryFilteredAD.Where(e => Filtre.AgentNames.Any(a => e.Entity.AgentName.Contains(a))).ToList();
            }

            if (Filtre.TypeIndexations != null && Filtre.TypeIndexations.Count > 0)
            {
                ResumeEnregisteredToModify = ResumeEnregisteredToModify.Where(e => Filtre.TypeIndexations.Any(t => e.Entity.TypeIndexation.Contains(t))).ToList();
                temp.AddRange(ResumeEnregisteredToDelete.Where(e => !Filtre.TypeIndexations.Any(t => e.Entity.TypeIndexation.Contains(t))).ToList());
                ResumeEntryFilteredAD = ResumeEntryFilteredAD.Where(e => Filtre.TypeIndexations.Any(t => e.Entity.TypeIndexation.Contains(t))).ToList();
            }

            if (Filtre.SiteNames != null && Filtre.SiteNames.Count > 0)
            {
                ResumeEnregisteredToModify = ResumeEnregisteredToModify.Where(e => Filtre.SiteNames.Any(n => e.Entity.SiteName.Contains(n))).ToList();
                temp.AddRange(ResumeEnregisteredToDelete.Where(e => !Filtre.SiteNames.Any(n => e.Entity.SiteName.Contains(n))).ToList());
                ResumeEntryFilteredAD = ResumeEntryFilteredAD.Where(e => Filtre.SiteNames.Any(n => e.Entity.SiteName.Contains(n))).ToList();
            }

            if (Filtre.IdSite != null && Filtre.IdSite != "")
            {
                ResumeEnregisteredToModify = ResumeEnregisteredToModify.Where(e => Filtre.IdSite == e.Entity.IdSite).ToList();
                temp.AddRange(ResumeEnregisteredToDelete.Where(e => Filtre.IdSite == e.Entity.IdSite).ToList());
                ResumeEntryFilteredAD = ResumeEntryFilteredAD.Where(e => Filtre.IdSite == e.Entity.IdSite).ToList();
            }

            ResumeEnregisteredToDelete =  temp.Distinct().Select(t => new ExtendedDetailedSiteCollectEntry(t.Entity,EntityState.Deleted)).ToList();


            result.AddRange(ResumeEnregisteredToModify);
            result.AddRange(ResumeEntryFilteredAD);
            result.AddRange(ResumeEnregisteredToDelete);

            result.ForEach(r =>
            {
                if (r.State == EntityState.Added)
                {
                    user_.IdSiteCollectInformationTracked.Add(r.Entity.Id);
                }
                else if (r.State == EntityState.Deleted)
                {
                    user_.IdSiteCollectInformationTracked.Remove(r.Entity.Id);
                }
            });

            return result;
        }

        public List<InformationRunData> FiltreByIdSchedules(List<string> IdSchedules)
        {
            var data = _DbContextFactory.CreateDbContext().DetailedSiteCollectInformations.ToList();

            return data.Where(r => IdSchedules.Contains(r.IdSchedule))
             .Select(s => new InformationRunData
             {
                 TypeIndexation = s.TypeIndexation,
                 AgentName = s.AgentName,
                 IdSchedule = s.IdSchedule,
                 Name = s.SiteName
             }).ToList();
        }

        public List<ExtendedDetailedSiteCollectEntry> FiltreEntriesByIdSchedules(List<ExtendedDetailedSiteCollectEntry> DetailedEntries_, List<string> IdSchedules)
        {
            return DetailedEntries_.Where(d => IdSchedules.Contains(d.Entity.IdSchedule)).ToList();
        }

        public List<string> FiltreIdSchedulesWithError()
        {
            return _DbContextFactory.CreateDbContext()
                .DetailedSiteCollectInformations
                .Where(s => s.IsPaused == false && s.Message != "Success" && s.Message != "" && s.Message != null)
               .Select(s => s.IdSchedule).ToList();
        }

        public List<ExtendedDetailedSiteCollectInformation> FiltrerScheduleWithError(User user_)
        {
            var data = FiltrerScheduleWithError(user_.FilterCriteria);
            user_.IdSiteCollectInformationTracked = data.Select(d => d.Id).ToList();
            return data;
        }

        public List<ExtendedDetailedSiteCollectInformation> FiltrerScheduleWithError(FilterCriteriaSiteCollectInformation filterCriteriaSiteCollectInformation_)
        {
            var filtre = filterCriteriaSiteCollectInformation_;

            var data = _DbContextFactory.CreateDbContext().DetailedSiteCollectInformations.ToList();

            data = FilterByIsPaused(data, filtre.IsPaused);
            data = FilterByIsRunning(data, filtre.IsRunning);

            data = data.Where(d => d.ScheduleExist == true).ToList();

            data = data.Where(d => d.Message != "Success").ToList();
            return data;
        }

        public List<ExtendedDetailedSiteCollectEntry> FiltrerScheduleWithError(List<ExtendedDetailedSiteCollectEntry> DetailedSiteCollectInformationEntries_,
            User user_)
        {
            List<ExtendedDetailedSiteCollectEntry> result = new();

            var Filtre = user_.FilterCriteria;

            var ResumeEntryFilteredAD = DetailedSiteCollectInformationEntries_.Where(r => r.State == EntityState.Deleted || r.State == EntityState.Added).ToList();
            var ResumeEntryFilteredM = DetailedSiteCollectInformationEntries_.Where(r => r.State == EntityState.Modified).ToList();

            var ResumeEnregistered = ResumeEntryFilteredM.Where(r => user_.IdSiteCollectInformationTracked.Contains(r.Entity.Id)).ToList();
            var ResumeNotEnregistered = ResumeEntryFilteredM.Where(r => !user_.IdSiteCollectInformationTracked.Contains(r.Entity.Id)).ToList();         

            ResumeEntryFilteredAD.AddRange(ResumeNotEnregistered
                .Select(r => new ExtendedDetailedSiteCollectEntry(r.Entity, EntityState.Added)).ToList());


            List<ExtendedDetailedSiteCollectEntry> ResumeEnregisteredToModify = ResumeEnregistered;
            List<ExtendedDetailedSiteCollectEntry> ResumeEnregisteredToDelete = ResumeEnregistered;
            List<ExtendedDetailedSiteCollectEntry> temp = new();

            switch (Filtre.IsPaused)
            {
                case 2:

                    ResumeEnregisteredToModify = ResumeEnregisteredToModify.Where(e => e.Entity.IsPaused == true).ToList();
                    temp.AddRange(ResumeEnregisteredToDelete.Where(e => e.Entity.IsPaused == false).ToList());
                    ResumeEntryFilteredAD = ResumeEntryFilteredAD.Where(e => e.Entity.IsPaused == true).ToList();
                    break;

                case 3:
                    ResumeEnregisteredToModify = ResumeEnregisteredToModify.Where(e => e.Entity.IsPaused == false).ToList();
                    temp.AddRange(ResumeEnregisteredToDelete.Where(e => e.Entity.IsPaused == true).ToList());
                    ResumeEntryFilteredAD = ResumeEntryFilteredAD.Where(e => e.Entity.IsPaused == false).ToList();
                    break;

                default:
                    break;
            }

            switch (Filtre.IsRunning)
            {
                case 2:
                    ResumeEnregisteredToModify = ResumeEnregisteredToModify.Where(e => e.Entity.IsRunning == true).ToList();
                    temp.AddRange(ResumeEnregisteredToDelete.Where(e => e.Entity.IsRunning == false).ToList());
                    ResumeEntryFilteredAD = ResumeEntryFilteredAD.Where(e => e.Entity.IsRunning == true).ToList();
                    break;

                case 3:
                    ResumeEnregisteredToModify = ResumeEnregisteredToModify.Where(e => e.Entity.IsRunning == false).ToList();
                    temp.AddRange(ResumeEnregisteredToDelete.Where(e => e.Entity.IsRunning == true).ToList());
                    ResumeEntryFilteredAD = ResumeEntryFilteredAD.Where(e => e.Entity.IsRunning == false).ToList();
                    break;

                default:
                    break;
            }

            ResumeEnregisteredToModify = ResumeEnregisteredToModify.Where(e => e.Entity.ScheduleExist == true).ToList();
            temp.AddRange(ResumeEnregisteredToDelete.Where(e => e.Entity.ScheduleExist == false).ToList());
            ResumeEntryFilteredAD = ResumeEntryFilteredAD.Where(e => e.Entity.ScheduleExist == true).ToList();

            ResumeEnregisteredToModify = ResumeEnregisteredToModify.Where(e => e.Entity.Message != "Success").ToList();
            temp.AddRange(ResumeEnregisteredToDelete.Where(e => e.Entity.Message == "Success").ToList());
            ResumeEntryFilteredAD = ResumeEntryFilteredAD.Where(e => e.Entity.Message != "Success").ToList();

            ResumeEnregisteredToDelete = temp.Distinct().Select(t => new ExtendedDetailedSiteCollectEntry(t.Entity, EntityState.Deleted)).ToList();

            result.AddRange(ResumeEnregisteredToModify);
            result.AddRange(ResumeEntryFilteredAD);
            result.AddRange(ResumeEnregisteredToDelete);

            result.ForEach(r =>
            {
                if (r.State == EntityState.Added)
                {
                    user_.IdSiteCollectInformationTracked.Add(r.Entity.Id);
                }
                else if (r.State == EntityState.Deleted)
                {
                    user_.IdSiteCollectInformationTracked.Remove(r.Entity.Id);
                }
            });

            return result;
        }
    
        private List<ExtendedDetailedSiteCollectInformation> FilterByIsPaused(List<ExtendedDetailedSiteCollectInformation> data_
            ,int filtervalue_)
        {
            switch (filtervalue_)
            {
                case 2:
                    return data_.Where(d => d.IsPaused == true).ToList();                 

                case 3:
                    return data_.Where(d => d.IsPaused == false).ToList();                   

                default:
                    return data_;
            }
        }

        private List<ExtendedDetailedSiteCollectInformation> FilterByIsRunning(List<ExtendedDetailedSiteCollectInformation> data_
          , int filtervalue_)
        {
            switch (filtervalue_)
            {
                case 2:
                    return data_.Where(d => d.IsRunning == true).ToList();

                case 3:
                    return data_.Where(d => d.IsRunning == false).ToList();

                default:
                    return data_;
            }
        }
    }
}