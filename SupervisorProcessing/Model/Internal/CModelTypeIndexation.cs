using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupervisorProcessing.Model.Internal
{
    public class CModelTypeIndexation
    {
        public CModelTypeIndexation()
        {
        }

        public CModelTypeIndexation(string typeIndexation_)
        {
            TypeIndexation = typeIndexation_;
        }

        public string TypeIndexation { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [JsonIgnore]
        public List<CModelSite> Sites { get; set; }

        public CModelTypeIndexation GetSimplified()
        {
            return new CModelTypeIndexation() { Id = this.Id, TypeIndexation = this.TypeIndexation };
        }
    }
}