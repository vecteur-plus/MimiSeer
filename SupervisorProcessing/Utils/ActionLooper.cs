using System;
using System.Threading.Tasks;

namespace SupervisorProcessing.Utils
{
    /// <summary>
    /// loop an action every x millisecond or wait the end before restart
    /// </summary>
    public class ActionLooper
    {
        private enum EStateLooper
        { Started, Stopped };

        private Action _Action;

        private EStateLooper _State;
        private System.Timers.Timer _Timer;
        private bool _Running;
        private bool _TimerFinished;
        private bool _ActiveGarbageCollector;

        public ActionLooper(Action action, double interval, bool activeGarbageCollector)
        {
            //action which run in loop
            _Action = action;

            _State = EStateLooper.Stopped;
           //setup timer 
            _Timer = new(interval);
            _Timer.AutoReset = false;
            _Timer.Elapsed += _Timer_Elapsed;

            _Running = false;
            _TimerFinished = true;
            _ActiveGarbageCollector = activeGarbageCollector;
        }

        private void _Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _TimerFinished = true;
            //_Timer.Close();
            Task.Run(() => Loop());
        }

        private void StartTimer()
        {
            _TimerFinished = false;
            _Timer.Start();
        }

        public void Start()
        {
            if (_State != EStateLooper.Started)
            {
                _State = EStateLooper.Started;
                Task.Run(() => Loop());
            }
        }

        public void Stop()
        {
            if (_State != EStateLooper.Stopped)
            {
                _State = EStateLooper.Stopped;
            }
        }

        private void Loop()
        {
            //check if action finished and actionlooper started and the time since starting >= interval chose
            if (_TimerFinished && !_Running && _State == EStateLooper.Started)
            {
                StartTimer();
                _Running = true;
                             
                Task _Task = new(_Action);                             
                _Task.Start();
                _Task.Wait();
                
                //destroy task
                _Task.Dispose();
                _Task = null;

                if (_ActiveGarbageCollector)
                {
                    GC.Collect();
                }
                _Running = false;

                Task.Run(() => Loop());
            }
        }

        public void SetInterval(double interval_, bool ForceRestart = false)
        {
            _Timer.Interval = interval_;

            //force the loop to start without waiting the end of timer
            if (ForceRestart)
            {
                _Timer.Stop();
                _Timer_Elapsed(null, null);
            }
        }
    }
}