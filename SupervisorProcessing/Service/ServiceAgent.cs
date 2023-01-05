using SupervisorProcessing.Model.Internal;
using SupervisorProcessing.Repository;
using System.Collections.Generic;
using System.Linq;

namespace SupervisorProcessing.Service
{
    public class ServiceAgent
    {
        private readonly SiteRepository _RepoRepliTbSites;

        public ServiceAgent(SiteRepository repoRepliTbSites_)
        {
            _RepoRepliTbSites = repoRepliTbSites_;
        }

        public bool DatabaseIsAccessible()
        {
            return _RepoRepliTbSites.IsAccessible();
        }

        //Get agent existing in database but not in agents_
        public List<CModelAgent> FindDistinctAgentOffList(IEnumerable<CModelAgent> agents_)
        {
            var AgentNames = _RepoRepliTbSites.FindDistinctAgentOffList(agents_.Select(a => a.AgentNameSqlFormat).ToList()).ToList();

            return AgentNames.Select(a => new CModelAgent() { AgentName = a.Replace("\"", "") }).ToList();
        }

        ////Get agent existing in agents_ but not find in database
        public IList<CModelAgent> FindAgentToDelete(IEnumerable<CModelAgent> agents_)
        {
            var agentsNotFind = FindAgentsNotExisting(agents_.Select(a => a.AgentNameSqlFormat));

            return agents_.Where(a => agentsNotFind.Contains(a.AgentNameSqlFormat)).ToList();
        }

        //Get agent's name existing in agents_ but not find in database
        private IList<string> FindAgentsNotExisting(IEnumerable<string> AgentNames_)
        {
            return AgentNames_.Except(_RepoRepliTbSites.FindDistinctAgentOffList(Enumerable.Empty<string>())).ToList();
        }
    }
}