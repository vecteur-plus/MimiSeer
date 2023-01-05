using Microsoft.EntityFrameworkCore;
using Serilog;
using SupervisorProcessing.Dao;
using SupervisorProcessing.DataContext.External;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SupervisorProcessing.Repository
{
    public class FlagRepository
    {
        private readonly DbContextSiteWeb _DbContextSiteWeb;
        private static ILogger _Logger;

        public FlagRepository(IDbContextFactory<DbContextSiteWeb> dbContextFactory_)
        {
            _Logger = Log.Logger.ForContext<FlagRepository>();
            _DbContextSiteWeb = dbContextFactory_.CreateDbContext();
        }

       
        public void UpdateAllFlagToSeen()
        {
            try
            {
                 
                _DbContextSiteWeb.Flags
                     .Where(f => f.IsSeen == false)
                     .ToList()
                     .ForEach(f => f.IsSeen = true);

                _DbContextSiteWeb.SaveChanges();
            }
            catch (Exception e)
            {
                _Logger.Error(e, "this is an exception");
            }
        }

        //for all flag with column IsSeen at false and id contains in ids_, set this collumn has true
        public bool UpdateListFlagToSeen(IEnumerable<int> ids_)
        {
            if (ids_ == null)
            {
                return true;
            }

            try
            {
                
                    _DbContextSiteWeb.Flags
                         .Where(f => f.IsSeen == false && ids_.Contains(f.Id))
                         .ToList()
                         .ForEach(f => f.IsSeen = true);

                    _DbContextSiteWeb.SaveChanges();
               

                return true;
            }
            catch (Exception e)
            {
                _Logger.Error(e, "this is an exception");
                return false;
            }
        }

        //Get all flag with collumn IsSeen at true
        public IList<Flag> FindFlags()
        {
            try
            {
                return _DbContextSiteWeb.Flags
                    .Where(f => f.IsSeen == false)
                    .AsNoTracking()
                    .ToList();
            }
            catch (Exception e)
            {
                _Logger.Error(e, "this is an exception");
                return new List<Flag>();
            }
        }
      
    }
}