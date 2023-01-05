using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupervisorProcessing.Model.Internal
{
    public class CModelSchedule
    {
        public CModelSchedule()
        {
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string IdSchedule { get; set; }
        public byte[] IdscheduleByte { get; set; }
        public string SessionId { get; set; }
        public string LastExistMessage { get; set; }
        public bool IsPaused { get; set; }
        public string LastRun { get; set; }
        public string NextRun { get; set; }
        public bool IsRunning { get; set; }
        public string InputParameters { get; set; }
        public string Cron { get; set; }
        public string StartTime { get; set; }
        public Guid GuidSchedule { get => Guid.Parse(IdSchedule); }

        [JsonIgnore]
        public CModelSite Site { get; set; }

        public int IdSite { get; set; }

        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                CModelSchedule p = (CModelSchedule)obj;
                return (IsPaused == p.IsPaused) && (IsRunning == p.IsRunning) && (LastExistMessage == p.LastExistMessage) && (NextRun == p.NextRun) &&
                    (SessionId == p.SessionId) && (LastRun == p.LastRun) && (IdSchedule == p.IdSchedule) && (InputParameters == p.InputParameters);
            }
        }

        public void Update(CModelSchedule value_)
        {
            this.LastExistMessage = value_.LastExistMessage;
            this.SessionId = value_.SessionId;
            this.LastRun = value_.LastRun;
            this.NextRun = value_.NextRun;
            this.IsPaused = value_.IsPaused;
            this.IsRunning = value_.IsRunning;
            this.InputParameters = value_.InputParameters;
            this.Cron = value_.Cron;
            this.StartTime = value_.StartTime;
        }
    }
}