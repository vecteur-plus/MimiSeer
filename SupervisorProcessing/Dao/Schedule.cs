using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupervisorProcessing.Dao
{
    [TableAttribute("SWS Schedules")]
    public class Schedule
    {
        public Schedule()
        {
        }

#pragma warning disable IDE1006 // Styles d'affectation de noms

        [Key]
        public Guid schedule_id_ { get; set; }

        public string agent_name_ { get; set; }
        public string session_id_ { get; set; }
        public bool is_paused_ { get; set; }
        public DateTime? next_run_time_ { get; set; }
        public DateTime? last_run_time_ { get; set; }
        public DateTime? start_time_ { get; set; }
        public string last_exit_message_ { get; set; }
        public bool is_deleted_ { get; set; }
        public bool is_running_ { get; set; }
        public string input_parameters_ { get; set; }
        public string cron_ { get; set; }

#pragma warning restore IDE1006 // Styles d'affectation de noms

        /* public override bool Equals(object obj)
         {
             //Check for null and compare run-time types.
             if ((obj == null) || !this.GetType().Equals(obj.GetType()))
             {
                 return false;
             }
             else
             {
                 CModelSchedule p = (CModelSchedule)obj;
                 return (IsPaused == p.IsPaused) && (LastExistMessage == p.LastExistMessage) && (NextRun == p.NextRun) &&
                     (SessionId == p.SessionId) && (LastRun == p.LastRun) && (IdSchedule == p.IdSchedule);
             }
         }*/
    }
}