using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupervisorProcessing.Model.Internal
{
    public class CModelAgent
    {
        public CModelAgent()
        {
            Sites = new();
            Sites.ListChanged += new ListChangedEventHandler(Sites_ListChanged);
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string AgentName { get; set; }

        [JsonIgnore]
        public string AgentNameSqlFormat { get => "\"" + AgentName + "\""; }

        [JsonIgnore]
        public bool IsMultiSession { get; private set; }

        [JsonIgnore]
        public BindingList<CModelSite> Sites { get; set; }

        private void Sites_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (Sites.Count > 1)
            {
                IsMultiSession = true;
            }
            else
            {
                IsMultiSession = false;
            }
        }

        public CModelAgent GetSimplified()
        {
            return new CModelAgent() { AgentName = this.AgentName, Id = this.Id };
        }
    }
}