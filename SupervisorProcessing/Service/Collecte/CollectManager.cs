using Microsoft.Extensions.Options;
using Serilog;
using SupervisorProcessing.Utils;
using System;
using System.Threading.Tasks;

namespace SupervisorProcessing.Service.Collecte
{
    public class CollectManager
    {
        private static ILogger _Logger = Log.Logger.ForContext<CollectManager>();
        private static ActionLooper _ActionLooper;
        private ServiceCollect _ServiceCollect;

        public CollectManager(ServiceCollect serviceCollect_, IOptions<ConfigTimeCollectLoop> configTimeCollectLoop_)
        {
            _ServiceCollect = serviceCollect_;
            _ActionLooper = new ActionLooper(_ServiceCollect.Actualisation, configTimeCollectLoop_.Value.TimeCollectLoopWithoutUser, false);
        }

        public void StartCollect(double interval_)
        {
            ModifyIntervalLoop(interval_);
            StartCollect();
        }

        public void StartCollect()
        {
            _Logger.Information("Collect started");
            Task.Run(new Action(_ServiceCollect.Initialisation))
                .ContinueWith(delegate { _ActionLooper.Start(); });
        }

        public void StopCollect()
        {
            _Logger.Information("Collect stopped");
            _ActionLooper.Stop();
        }

        public static void ModifyIntervalLoop(double interval)
        {
            _Logger.Information($"Collect intrerval set at {interval} millisecond");
            _ActionLooper?.SetInterval(interval, true);
        }
    }
}