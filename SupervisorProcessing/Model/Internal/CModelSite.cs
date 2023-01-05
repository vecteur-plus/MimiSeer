using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupervisorProcessing.Model.Internal
{
    public class CModelSite
    {
        public CModelSite()
        {
        }

        public CModelSite(string Name_, string CleGeneral_, CModelAgent Agent_, CModelTypeIndexation TypeIndexation_, string commentaire_)
        {
            Name = Name_;
            CleGeneral = CleGeneral_;
            IdTypeIndexation = TypeIndexation_.Id;
            IdAgent = Agent_.Id;
            Commentaire = commentaire_;
            
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Name { get; set; }
        public string CleGeneral { get; set; }
        public string Commentaire { get; set; }

        [JsonIgnore]
        public virtual List<CModelSchedule> Schedules { get; set; } = new();

        [JsonIgnore]
        public CModelTypeIndexation TypeIndexation { get; set; }

        [JsonIgnore]
        public CModelAgent Agent { get; set; }

        public int IdAgent { get; set; }

        public int IdTypeIndexation { get; set; }

        public bool Equals(string Name_, string TypeIndexation_, string AgentName_)
        {
            //Check for null and compare run-time types.
            if (Name_ == null || TypeIndexation_ == null || AgentName_ == null)
            {
                return false;
            }
            else
            {
                return (Name == Name_ && TypeIndexation.TypeIndexation == TypeIndexation_ && Agent.AgentName == AgentName_);
            }
        }

        public void Update(CModelSite site_)
        {
            this.IdTypeIndexation = site_.IdTypeIndexation;
            this.IdAgent = site_.IdAgent;
            this.Commentaire = site_.Commentaire;
        }
    }
}