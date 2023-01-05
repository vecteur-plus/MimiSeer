using Microsoft.EntityFrameworkCore;
using SupervisorProcessing.Dao;
using SupervisorProcessing.DataContext;
using SupervisorProcessing.Model.Internal;
using SupervisorProcessing.Repository;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupervisorProcessing.Service
{
    public class ServiceSite
    {
        
        private readonly SiteRepository _RepoRepliTbSites;
        private readonly DbContextIntern _DbContextIntern;

        public ServiceSite(SiteRepository repoRepliTbSites_
            , IDbContextFactory<DbContextIntern> dbContextFactory_)
        {
             _RepoRepliTbSites = repoRepliTbSites_;
            _DbContextIntern = dbContextFactory_.CreateDbContext();
        }

        public bool DatabaseIsAccessible()
        {
            return _RepoRepliTbSites.IsAccessible();
        }

        
        public List<CModelSite> FindAllSites()
        {
            var DbSites = _RepoRepliTbSites.FindAll().ToList();

            return Map(DbSites);
        }

        public List<CModelSite> FindSites(IEnumerable<string> names_)
        {
            var DbSites = _RepoRepliTbSites.FindByNames(names_).ToList();

            return Map(DbSites);
        }

        //convert site get by repo to cModelSite used in other service
        private List<CModelSite> Map(List<Site> DbSites)
        {
            

            var DicTypeIndexations = _DbContextIntern.TypeIndexations.ToDictionary(t => t.TypeIndexation);
            var DicAgents = _DbContextIntern.Agents.ToDictionary(a => a.AgentName);

            ConcurrentBag<CModelSite> Sites = new();

            //excute foreach in multi threading
            Parallel.ForEach(DbSites, DbSite =>
            {
                CModelTypeIndexation TypeIndexation = new();
                CModelAgent Agent = new();

                //try to get type indexation model which matches with type indexation get in dbsite
                if (!DicTypeIndexations.TryGetValue(DbSite.TypeIndexation, out TypeIndexation))
                {
                    return;
                }

                //try to get agent model which matches with agent get in dbsite
                if (!DicAgents.TryGetValue(DbSite.AgentNameClean, out Agent))
                {
                    return;
                }
                // create model
                Sites.Add(new CModelSite(DbSite.Name, DbSite.CleGeneral, Agent, TypeIndexation, DbSite.Commentaire ?? ""));
            });

            return Sites.ToList();
        }
    }
}