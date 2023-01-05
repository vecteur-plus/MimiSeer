using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SupervisorProcessing.DataContext;
using SupervisorProcessing.Model.Internal;
using System.Collections.Generic;
using System.Linq;
using WebSocketServerWebfollow.Model;
using WebSocketServerWebfollow.Service;
using WebSocketSupervisorCommunicationLibrary;
using WebSocketSupervisorCommunicationLibrary.InformationRun.Model;
using WebSocketSupervisorCommunicationLibrary.SiteCollectInformation.Map;
using WebSocketSupervisorCommunicationLibrary.SiteCollectInformation.Model;
using WebSocketSupervisorCommunicationLibrary.SiteCollectInformation.Service;

namespace WebSocketServerWebfollow.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class SupervisorProcessingController : Controller
    {
        private readonly ServiceFiltre _ServiceFiltre;
        private readonly ILogger Logger;
        private readonly IDbContextFactory<DbContextIntern> _DbContextFactory;


        public SupervisorProcessingController(ServiceFiltre serviceFiltre_, IDbContextFactory<DbContextIntern> dbContextFactory_)
        {
            _ServiceFiltre = serviceFiltre_;
            _DbContextFactory = dbContextFactory_;
            Logger = Log.Logger.ForContext<SupervisorProcessingController>();
        }

        /// <summary>
        /// return schedule id of all schedule in error
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/SupervisorProcessing/GetAllSchedulesInError")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<List<string>> GetAllSchedulesInError()
        {
            return CreatedAtAction(nameof(GetAllSchedulesInError), _ServiceFiltre.FiltreIdSchedulesWithError());
        }

        /// <summary>
        /// get id schedules list in function site name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/SupervisorProcessing/GetIdScheduleByName/{name}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<string> GetIdScheduleByName(string name)
        {
            var value = _DbContextFactory.CreateDbContext().DetailedSiteCollectInformations.Where(d => d.SiteName == name).Select(d => d.IdSchedule).ToList();

            return CreatedAtAction(nameof(GetIdScheduleByName), value);
        }

        /// <summary>
        /// get information run of list of id schedule
        /// </summary>
        /// <param name="informationRunInquiry_"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/SupervisorProcessing/GetInfomationRunByIdsSchedule")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<InformationRunResponse> GetInfomationRunByIdsSchedule([FromBody] InformationRunInquiry informationRunInquiry_)
        {
            var value = new InformationRunResponse() { InformationRuns = _ServiceFiltre.FiltreByIdSchedules(informationRunInquiry_.IdSchedules) };

            return CreatedAtAction(nameof(GetInfomationRunByIdsSchedule), value);
        }

        /// <summary>
        /// get id schedule list of an agent
        /// </summary>
        /// <param name="AgentName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("~/api/SupervisorProcessing/GetScheduleIdByAgentName/{AgentName}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<string> GetScheduleIdByAgentName(string AgentName)
        {
            return CreatedAtAction(nameof(GetScheduleIdByAgentName), _DbContextFactory.CreateDbContext().DetailedSiteCollectInformations.Where(x => x.AgentName == AgentName && x.ScheduleExist == true).Select(x => x.IdSchedule));
        }

        /// <summary>
        /// get basic Site Collect information list in function filter criteria sent
        /// </summary>
        /// <param name="summarizedInformationInquiry_"></param>
        /// <returns></returns>      
        [HttpPost]
        [Route("~/api/SupervisorProcessing/GetFilterSchedule")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<string> GetFilterSchedule([FromBody] string summarizedInformationInquiry_)
        {
            var basicSiteCollectInformationInquiry = BasicSiteCollectInformationInquiry.DeserializeFromJson(summarizedInformationInquiry_);

            List<ExtendedDetailedSiteCollectInformation> result;


            var filter = new FilterCriteriaSiteCollectInformation(basicSiteCollectInformationInquiry);
            switch (filter.TypeFilter)
            {
                case EBasicInquiryAction.WithFilter:

                    result = _ServiceFiltre.FilterAllSites(filter);
                    break;

                case EBasicInquiryAction.OnlyError:
                    result = _ServiceFiltre.FiltrerScheduleWithError(filter);
                    break;

                default:
                    result = new List<ExtendedDetailedSiteCollectInformation>();
                    break;
            }
            var serviceMessage = new BasicSiteCollectInformationMessageCreator();
            var messageResponse = serviceMessage.CreateMessage(ETypeMessage.NEW);
            serviceMessage.AffecterResume(Mapper.GetMapper().Map<List<ExtendedDetailedSiteCollectInformation>, List<BasicSiteCollectInformation>>(result)
              , messageResponse, EAction.ADD);

            return CreatedAtAction(nameof(GetFilterSchedule), messageResponse.GetJson());
        }
    }
}