using Microsoft.Extensions.DependencyInjection;
using SupervisorProcessing.Repository;
using SupervisorProcessing.Service.Collecte;
using SupervisorProcessing.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SupervisorProcessing.Utils
{
    public static class SupervisorProcessingStartup
    {
        public static void AddSingleton(IServiceCollection services_)
        {
            //list of signletion to add For proper functioning of DI
            services_.AddSingleton<ServiceCollect>();
            services_.AddSingleton<CollectManager>();
            services_.AddSingleton<ScheduleRepository>();
            services_.AddSingleton<SiteRepository>();
            services_.AddSingleton<ServiceSchedule>();
            services_.AddSingleton<ServiceSite>();
            services_.AddSingleton<FlagRepository>();
            services_.AddSingleton<ServiceFlag>();
            services_.AddSingleton<ServiceTypeIndexation>();
            services_.AddSingleton<ServiceAgent>();
            services_.AddSingleton<ScheduleMessageService>();
            services_.AddSingleton<EntityEntryService>();
        }
    }
}
