using Microsoft.EntityFrameworkCore;
using SupervisorProcessing.DataContext;
using SupervisorProcessing.Model.Exchange;
using SupervisorProcessing.Model.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SupervisorProcessing.Service.Collecte
{
    public partial class ServiceCollect
    {
        public void Actualisation()
        {
            _Logger.Information("=================================================================================");
            _Logger.Information("Début boucle");

            _EntityEntryService.Entries.Clear();
            _DbContextIntern.ChangeTracker.Clear();
            //_DbContextIntern.EntityEntries.RemoveRange(_DbContextIntern.EntityEntries.ToList());
           

            ActualiseTypeIndexation();

            AddNewAgent();

            ActualiseSites();

            DeleteRemovedAgent();

            ActualiseSchedules();

            ActualiseDetailedInformation();

            ActualiseScheduleMessage();         

            var nbEntries = _EntityEntryService.Count();

            _Logger.Information("{count} entries found", nbEntries);

            if (nbEntries == 0)
            {
                return;
            }


            var result = new ResultIteration
            {
                //Recupere le type et l'etat de l'entrée en base
                TypeIndexationsAdded = _EntityEntryService.GetValue<CModelTypeIndexation>(EntityState.Added)
                .Select(type => type.GetSimplified())
                .ToList(),

                AgentsAdded = _EntityEntryService.GetValue<CModelAgent>(EntityState.Added)
                .Select(agent => agent.GetSimplified())
                .ToList(),

                AgentDeleted = _EntityEntryService.GetValue<CModelAgent>(EntityState.Deleted)
                .Select(agent => agent.GetSimplified())
                .ToList(),

                MessageAdded = _EntityEntryService.GetValue<ScheduleMessage>(EntityState.Added)
                    .Select(m => m.Message)
                    .Distinct()
                    .ToList(),

                MessageDeleted = _EntityEntryService.GetValue<ScheduleMessage>(EntityState.Deleted)
                    .Select(m => m.Message)
                    .Distinct()
                    .ToList(),

                DetailedSiteCollectInformationEntries = _EntityEntryService.GetExtendedDetailedSiteCollectInformation().ToList(),
            };

            EndIterationProcessingHook?.Invoke(result, null);

        }

        private void ActualiseScheduleMessage()
        {
                    
            var messageToAdd = _ScheduleMessageService.FindDistinctScheduleOffList(_DbContextIntern.ScheduleMessages.AsNoTracking().ToList()).ToList();
            var messageToRemove = _ScheduleMessageService.FindScheduleMessageToDelete().ToList();
           
            _Logger.Information("{count} messages deleted", messageToAdd.Count);
            _Logger.Information("{count} messages added", messageToRemove.Count);

            
            _DbContextIntern.ScheduleMessages.AddRange(messageToAdd);
            _DbContextIntern.ScheduleMessages.RemoveRange(messageToRemove);

            _DbContextIntern.SaveChanges();
        }

        private void ActualiseDetailedInformation()
        {
            int nbAdded = AddNewDetailedInformations();
            int nbModified = ModifyDetailedInformations();
            int nbDeleted = DeleteDetailedInformations();

            _Logger.Information("{count} detailed informations added", nbAdded);
            _Logger.Information("{count} detailed informations modified", nbModified);
            _Logger.Information("{count} detailed informations deleted", nbDeleted);

        }

        private void ActualiseTypeIndexation()
        {
            List<CModelTypeIndexation> TypeIndexations = _ServiceTypeIndexation
                .FindTypeIndexationOffList(_DbContextIntern.TypeIndexations);

            _Logger.Information("{count} new types of indexation found", TypeIndexations.Count);

            _DbContextIntern.TypeIndexations.AddRange(TypeIndexations);

            _DbContextIntern.SaveChanges();

        }

        private void ActualiseSites()
        {
            //get flag
            var flags = _serviceFlag.FindFlag().ToList();

            _Logger.Information("{count} new information found", flags.Count);

            var SitesToAdd = _serviceSite.FindSites(flags.Where(f => f.TypeModification == "ADD").Select(f => f.Name).ToList());
            var SitesToModify = _serviceSite.FindSites(flags.Where(f => f.TypeModification == "MODIFY").Select(f => f.Name).ToList());
            List<CModelSite> SitesToDelete = new();

            flags.Where(f => f.TypeModification == "DELETE").ToList().ForEach(flag =>
            {
                if (_DbContextIntern.Sites.ToDictionary(s => s.Name).TryGetValue(flag.Name, out CModelSite site))
                {
                    SitesToDelete.Add(site);
                }
            });

            _Logger.Information("{count} sites to delete found", SitesToDelete.Count);
            _Logger.Information("{count} sites to add found", SitesToAdd.Count);
            _Logger.Information("{count} sites to modify found", SitesToModify.Count);

            List<string> names = new();
            names.AddRange(SitesToAdd.Select(s => s.Name));
            names.AddRange(SitesToModify.Select(s => s.Name));
            names.AddRange(flags.Where(f => f.TypeModification == "DELETE").Select(f => f.Name));

            if (_serviceFlag.UpdatedListflagToSeen(flags.Where(f => names.Contains(f.Name)).ToList()))
            {
                _DbContextIntern.Sites.AddRange(SitesToAdd);
                _DbContextIntern.Sites.RemoveRange(SitesToDelete);

                var siteDic = _DbContextIntern.Sites.ToDictionary(s => s.Name);

                SitesToModify.ForEach(site =>
                {
                    if (siteDic.TryGetValue(site.Name, out CModelSite temp))
                    {
                        temp.Update(site);
                    }
                });
            }

            var a = _DbContextIntern.ChangeTracker.Entries().Where(e => e.State != EntityState.Unchanged);
            _DbContextIntern.SaveChanges();
        }

        private void DeleteRemovedAgent()
        {
            var AgentToDelete = _ServiceAgent.FindAgentToDelete(_DbContextIntern.Agents.ToList());

            _Logger.Information("{count} agents to delete found", AgentToDelete.Count);

            //delete agent
            _DbContextIntern.Agents.RemoveRange(AgentToDelete);
            _DbContextIntern.SaveChanges();
        }

        private void AddNewAgent()
        {
            var AgentToAdd = _ServiceAgent.FindDistinctAgentOffList(_DbContextIntern.Agents.ToList());
            _Logger.Information("{count} agents to add found", AgentToAdd.Count);

            _DbContextIntern.Agents.AddRange(AgentToAdd);
            _DbContextIntern.SaveChanges();

        }

        private void ActualiseSchedules()
        {
            var ExistingSchedules = _DbContextIntern.Schedules.Include(s => s.Site).ToList();

            _Logger.Information("{count} schedules existing", ExistingSchedules.Count);

            var SchedulesToAdd = _serviceSchedule.FindNewSchedules();

            _Logger.Information("{count} schedules to add", SchedulesToAdd.Count);

            var SchedulesToCheck = _serviceSchedule.FindSchedulesToCheck();

            _Logger.Information("{count} schedules to check", SchedulesToCheck.Count);

            var DicSchedulesToCheck = SchedulesToCheck.ToDictionary(s => s.IdSchedule);

            ConcurrentBag<CModelSchedule> SchedulesToDelete = new();
            ConcurrentBag<CModelSchedule> SchedulesToUpdate = new();

            ExistingSchedules.ForEach(schedule =>
            {
                if (DicSchedulesToCheck.TryGetValue(schedule.IdSchedule, out var value))
                {
                    if (!schedule.Equals(value))
                    {
                        SchedulesToUpdate.Add(value);
                    }
                }
                else
                {
                    SchedulesToDelete.Add(schedule);
                   
                }
            });

            _Logger.Information("{count} schedules to delete", SchedulesToDelete.Count);
            _Logger.Information("{count} schedules to update", SchedulesToUpdate.Count);

            _DbContextIntern.Schedules.AddRange(SchedulesToAdd.ToList());


            _DbContextIntern.Schedules.RemoveRange(SchedulesToDelete.ToList());


            SchedulesToUpdate.ToList().ForEach(s =>
            {
                _DbContextIntern.Schedules.ToDictionary(s => s.IdSchedule).TryGetValue(s.IdSchedule, out CModelSchedule TempSchedule);
                TempSchedule.Update(s);
            });

          

            _DbContextIntern.SaveChanges();

           
        }

        public int AddNewDetailedInformations()
        {
            var sites = _EntityEntryService.GetValue<CModelSite>(EntityState.Added).ToList();

            var schedules = _EntityEntryService.GetValue<CModelSchedule>(EntityState.Added).ToList();

            var siteInformations = DetailedSiteCollectInformationService.TransformIntoDetail(sites);
            var scheduleInformations = DetailedSiteCollectInformationService.TransformIntoDetail(schedules);

            _DbContextIntern.DetailedSiteCollectInformations
                .RemoveRange(_DbContextIntern
                    .DetailedSiteCollectInformations
                    .Where(r => scheduleInformations
                        .Select(s => s.IdSite)
                        .Contains(r.IdSite) && r.IdSchedule == ""));

            _DbContextIntern.DetailedSiteCollectInformations.AddRange(siteInformations.Concat(scheduleInformations));

            _DbContextIntern.SaveChanges();

            return siteInformations.Count() + scheduleInformations.Count();
        }

        public int ModifyDetailedInformations()
        {
            var sites = _EntityEntryService.GetValue<CModelSite>(EntityState.Modified).ToList();

            var schedules = _EntityEntryService.GetValue<CModelSchedule>(EntityState.Modified).ToList();


            foreach (var site in sites)
            {
                var values = _DbContextIntern.DetailedSiteCollectInformations
                    .Where(r => r.IdSite == site.Id.ToString()).ToList();

                values.ForEach(v => v.Update(site));
            }


            foreach (var schedule in schedules)
            {
                var values = _DbContextIntern.DetailedSiteCollectInformations
                    .Where(r => r.IdSchedule == schedule.IdSchedule).ToList();

                values.ForEach(v => v.Update(schedule));
            }

            _DbContextIntern.SaveChanges();


            var count = _EntityEntryService.Count<ExtendedDetailedSiteCollectInformation>(EntityState.Modified);

            return count;

        }

        public int DeleteDetailedInformations()
        {
            var sites = _EntityEntryService.GetValue<CModelSite>(EntityState.Deleted).ToList();
            var schedules = _EntityEntryService.GetValue<CModelSchedule>(EntityState.Deleted).ToList();

            _DbContextIntern.DetailedSiteCollectInformations
                .RemoveRange(_DbContextIntern.DetailedSiteCollectInformations
                .Where(r => sites.Select(s => s.Id.ToString()).Contains(r.IdSite)));


            foreach (var schedule in schedules)
            {

                _DbContextIntern.DetailedSiteCollectInformations
                    .RemoveRange(_DbContextIntern.DetailedSiteCollectInformations
                    .Where(r => r.IdSchedule == schedule.IdSchedule));

                if (schedule.Site != null
                    && _DbContextIntern.DetailedSiteCollectInformations
                    .Where(r => r.IdSite == schedule.Site.Id.ToString()).Count() == 1)
                {
                    _DbContextIntern.DetailedSiteCollectInformations
                        .Add(DetailedSiteCollectInformationService.TransformIntoDetailWithoutSchedule(schedule));
                }

            }
            _DbContextIntern.SaveChanges();

            var count = _EntityEntryService.Count<ExtendedDetailedSiteCollectInformation>(EntityState.Deleted);

            return count;


        }
    }
}