using Microsoft.EntityFrameworkCore;
using Serilog;
using SupervisorProcessing.Model.Exchange;
using SupervisorProcessing.Model.Internal;
using SupervisorProcessing.Model.Internal.Entry;
using SupervisorProcessing.Service.Collecte;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSocketServerWebfollow.Handler;
using WebSocketServerWebfollow.Model.Internal;
using WebSocketSupervisorCommunicationLibrary;
using WebSocketSupervisorCommunicationLibrary.FilterCriteria.Service;
using WebSocketSupervisorCommunicationLibrary.InformationRun.Model;
using WebSocketSupervisorCommunicationLibrary.SiteCollectInformation.Model;
using WebSocketSupervisorCommunicationLibrary.SiteCollectInformation.Service;

namespace WebSocketServerWebfollow.Service
{
    public class ProcessingRecepter
    {
        private readonly ILogger _Logger;
        private readonly ServiceFiltre _ServiceFiltre;
        private readonly WebSocketResumeHandler _WebSocketResumeHandler;
        private readonly WebSocketDetailedScheduleHandler _WebSocketDetailScheduleHandler;
        private readonly WebSocketInformationRunHandler _WebSocketInformationRunHandler;
        private readonly WebSocketFilterCriteriaHandler _WebSocketFilterCriteriaHandler;

        public ProcessingRecepter(ServiceFiltre serviceFiltre_, WebSocketResumeHandler webSocketResumeHandler_,
            WebSocketDetailedScheduleHandler webSocketDetailScheduleHandler_, WebSocketInformationRunHandler webSocketInformationRunHandler_,
            WebSocketFilterCriteriaHandler webSocketFilterCriteriaHandler_)
        {

            ServiceCollect.EndIterationProcessingHook += SupervisorRecepter;

            _Logger = Log.Logger.ForContext<ProcessingRecepter>();
            _ServiceFiltre = serviceFiltre_;
            _WebSocketResumeHandler = webSocketResumeHandler_;
            _WebSocketDetailScheduleHandler = webSocketDetailScheduleHandler_;
            _WebSocketInformationRunHandler = webSocketInformationRunHandler_;
            _WebSocketFilterCriteriaHandler = webSocketFilterCriteriaHandler_;
        }

        private void SupervisorRecepter(object sender_, EventArgs e_)
        {
            EnvoieActualisationAsync(sender_ as ResultIteration);
        }

        private void EnvoieActualisationAsync(ResultIteration result_)
        {
            _Logger.Information("{@result_}", result_);


            //Get user
            var usersDetail = _WebSocketDetailScheduleHandler.Connections.GetAllConnections().Values;
            var usersInformationRun = _WebSocketInformationRunHandler.Connections.GetAllConnections().Values;
            var usersBasic = _WebSocketResumeHandler.Connections.GetAllConnections().Values;
            var usersFilterCriteria = _WebSocketFilterCriteriaHandler.Connections.GetAllConnections().Values;

            var ConnectedUserCount = usersBasic.Count + usersDetail.Count + usersInformationRun.Count;
            _Logger.Information($"il y a {ConnectedUserCount} utilisateur connecté");


            SendBasicInformationToUsers(result_, usersBasic);
            SendDetailInformationToUsers(result_, usersDetail);
            SendRunInformationToUsers(result_, usersInformationRun);
            SendFilterCriteriaToUsers(result_, usersFilterCriteria);
        }

        private void SendFilterCriteriaToUsers(ResultIteration result_, ICollection<User> users_)
        {


            var TypeIndexationToAdd = result_.TypeIndexationsAdded.Select(t => t.TypeIndexation).ToList();
            var agentToAdd = result_.AgentsAdded.Select(a => a.AgentName).ToList();
            var AgentToDelete = result_.AgentDeleted.Select(a => a.AgentName).ToList();
            var MessageToAdd = result_.MessageAdded;
            var MessageToDelete = result_.MessageDeleted;


            if (!agentToAdd.Any() && !AgentToDelete.Any() && !MessageToAdd.Any() && !MessageToDelete.Any() && !TypeIndexationToAdd.Any())
            {
                return;
            }

            FilterCriteriaMessageCreator messageCreator = new FilterCriteriaMessageCreator();
            var message = messageCreator.CreateMessage(ETypeMessage.UPDATE);

            messageCreator.AddTypeIndexations(TypeIndexationToAdd, message);
            messageCreator.AddAgents(agentToAdd, message);
            messageCreator.DeleteAgents(AgentToDelete, message);
            messageCreator.AddMessagesSchedule(MessageToAdd, message);
            messageCreator.DeleteMessagesSchedule(MessageToDelete, message);

            foreach (var user in users_)
            {
                Task.Run(() => _WebSocketFilterCriteriaHandler.SendMessage(user.WebSocket, message.GetJson()).Wait());
            }

        }

        private void SendBasicInformationToUsers(ResultIteration result_, ICollection<User> users_)
        {

            var mapperSiteCollectInformation = WebSocketSupervisorCommunicationLibrary.SiteCollectInformation.Map.Mapper.GetMapper();

            BasicSiteCollectInformationMessageCreator summarizedMessageCreator = new();

            //Get filter information to modify


            foreach (var user in users_)
            {


                List<ExtendedDetailedSiteCollectEntry> resultEntries = new();

                //Get SiteCollect information in function filter criteria
                if (user.FilterCriteria != null)
                {
                    switch (user.FilterCriteria.TypeFilter)
                    {
                        case EBasicInquiryAction.WithFilter:
                            resultEntries = _ServiceFiltre.FiltreList(result_.DetailedSiteCollectInformationEntries, user);
                            break;

                        case EBasicInquiryAction.OnlyError:
                            resultEntries = _ServiceFiltre.FiltrerScheduleWithError(result_.DetailedSiteCollectInformationEntries, user);
                            break;

                        default:
                            break;
                    }
                }

                //check if something is to be modified 
                if (resultEntries.Any())
                {
                    //create message

                    BasicSiteCollectInformationCallback message = new BasicSiteCollectInformationMessageCreator().CreateMessage(ETypeMessage.UPDATE);


                    var ResumeToAdd = mapperSiteCollectInformation.Map<IEnumerable<ExtendedDetailedSiteCollectInformation>, List<BasicSiteCollectInformation>>(
                        resultEntries.Where(r => r.State == EntityState.Added).Select(r => r.Entity));
                    var ResumeToDelete = mapperSiteCollectInformation.Map<IEnumerable<ExtendedDetailedSiteCollectInformation>, List<BasicSiteCollectInformation>>(
                        resultEntries.Where(r => r.State == EntityState.Deleted).Select(r => r.Entity));
                    var ResumeToModify = mapperSiteCollectInformation.Map<IEnumerable<ExtendedDetailedSiteCollectInformation>, List<BasicSiteCollectInformation>>(
                        resultEntries.Where(r => r.State == EntityState.Modified).Select(r => r.Entity));
                    summarizedMessageCreator.AffecterResume(ResumeToAdd, message, EAction.ADD);
                    summarizedMessageCreator.AffecterResume(ResumeToDelete, message, EAction.REMOVE);
                    summarizedMessageCreator.AffecterResume(ResumeToModify, message, EAction.UPDATE);

                    Task.Run(() => _WebSocketResumeHandler.SendMessage(user.WebSocket, message.GetJson()).Wait());
                }
            }
        }

        private void SendDetailInformationToUsers(ResultIteration result_, ICollection<User> users_)
        {
            foreach (var user in users_.Where(u => u.FilterCriteria != null))
            {

                //get the siteCollect information with the id fill in user filter criteria if this one underwent a change of state
                var result = _ServiceFiltre.FiltreById(result_.DetailedSiteCollectInformationEntries, user);

                if (result != null)
                {

                    DetailedSiteCollectInformationCallBack message;
                    switch (result.State)
                    {
                        case EntityState.Deleted:
                            message = new(result.Entity, EAction.REMOVE);
                            break;

                        case EntityState.Modified:
                            message = new(result.Entity, EAction.UPDATE);
                            break;

                        case EntityState.Added:
                            message = new(result.Entity, EAction.ADD);
                            break;

                        default:
                            continue;

                    }
                    Task.Run(() => _WebSocketDetailScheduleHandler.SendMessage(user.WebSocket, message.GetJson()).Wait());
                }
            }

        }

        private void SendRunInformationToUsers(ResultIteration result_, ICollection<User> users_)
        {
            foreach (var user in users_)
            {
                //get siteCollect informations with schedule id list fill in user filter criteria if these underwent a change of state
                var result = _ServiceFiltre.FiltreEntriesByIdSchedules(result_.DetailedSiteCollectInformationEntries, user.IdSchedules);
                _Logger.Information("{cout} schedule trouvé : {@result_}", result.Count, result);

                if (result.Count > 0)
                {
                    var message = new InformationRunCallBack
                    {
                        InformationRunStates = result.Select(r => new InformationRunState()
                        {
                            IdSchedule = r.Entity.IdSchedule,
                            IsRunning = r.Entity.IsRunning,
                            LastRun = r.Entity.LastRun
                        }).ToList()
                    };
                    Task.Run(() => _WebSocketInformationRunHandler.SendMessage(user.WebSocket, message.GetJson()).Wait());
                }
            }
        }
    }

}