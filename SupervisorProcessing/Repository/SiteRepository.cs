using Microsoft.EntityFrameworkCore;
using Serilog;
using SupervisorProcessing.Dao;
using SupervisorProcessing.DataContext.External;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SupervisorProcessing.Repository
{
    public class SiteRepository
    {
        private readonly DbContextSiteWeb _DbContextFactory;
        private ILogger _logger;

        public SiteRepository(IDbContextFactory<DbContextSiteWeb> dbContextFactory_)
        {
            _DbContextFactory = dbContextFactory_.CreateDbContext();
            _logger = Log.Logger.ForContext<SiteRepository>();
        }

        public bool IsAccessible()
        {
            return _DbContextFactory.Database
                .CanConnect();
        }

        //Get all sites in production and running on content grabber
        public IEnumerable<Site> FindAll()
        {
            try
            {
                return _DbContextFactory.Sites
                     .AsNoTracking()
                     .Where(s => s.StatutProduction == true && s.AgentName != null)
                     .AsEnumerable()
                     .GroupBy(s => s.Name)
                     .Select(s => s.First());
            }
            catch (Exception e)
            {
                _logger.Error(e, "this is an exception");
                return Enumerable.Empty<Site>();
            }
        }

        //Get all sites in production and running on content grabber and with name content in names_
        public IEnumerable<Site> FindByNames(IEnumerable<string> names_)
        {
            try
            {
                return _DbContextFactory.Sites
                    .AsNoTracking()
                    .Where(s => s.StatutProduction == true && s.AgentName != null && names_.Contains(s.Name))
                    .AsEnumerable()
                    .GroupBy(s => s.Name)
                    .Select(s => s.First());
            }
            catch (Exception e)
            {
                _logger.Error(e, "this is an exception");
                return Enumerable.Empty<Site>();
            }
        }

        //Get type indexation of sites in production and running on content grabber not contains in TypeIndexations_
        public IEnumerable<string> FindDistinctTypeIndexationOffList(IEnumerable<string> TypeIndexations_)
        {
            if (TypeIndexations_ == null)
            {
                _logger.Error("List of indexation's type is null");
                return Enumerable.Empty<string>();
            }

            try
            {
                return _DbContextFactory.Sites
                    .Where(s => s.StatutProduction == true && s.AgentName != null && !TypeIndexations_.Contains(s.TypeIndexation))
                    .Select(s => s.TypeIndexation)
                    .Distinct();
            }
            catch (Exception e)
            {
                _logger.Error(e, "this is an exception");
                return Enumerable.Empty<string>();
            }
        }

        //Get agent name of sites in production and running on content grabber not contains in AgentNames_
        public IEnumerable<string> FindDistinctAgentOffList(IEnumerable<string> AgentNames_)
        {
            if (AgentNames_ == null)
            {
                _logger.Error("List of agent is null");
                return Enumerable.Empty<string>();
            }

            try
            {
                return _DbContextFactory.Sites
                    .Where(s => s.StatutProduction == true && s.AgentName != null && !AgentNames_.Contains(s.AgentName))
                    .Select(s => s.AgentName)
                    .Distinct();
            }
            catch (Exception e)
            {
                _logger.Error(e, "this is an exception");
                return Enumerable.Empty<string>();
            }
        }
    }
}