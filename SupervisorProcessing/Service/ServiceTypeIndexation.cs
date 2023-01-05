using SupervisorProcessing.Model.Internal;
using SupervisorProcessing.Repository;
using System.Collections.Generic;
using System.Linq;

namespace SupervisorProcessing.Service
{
    public class ServiceTypeIndexation
    {
        public readonly SiteRepository _RepoSite;

        public ServiceTypeIndexation(SiteRepository repoSite_)
        {
            _RepoSite = repoSite_;
        }


        //Get list of distinct type indexation which not contain in list 
        public List<CModelTypeIndexation> FindTypeIndexationOffList(IEnumerable<CModelTypeIndexation> typeIndexations_)
        {
            var TypeIndexations = _RepoSite.FindDistinctTypeIndexationOffList(typeIndexations_.Select(t => t.TypeIndexation)).ToList();

            return TypeIndexations.Select(t => new CModelTypeIndexation(t)).ToList();
        }
    }
}