using Microsoft.EntityFrameworkCore;
using Serilog;
using SupervisorProcessing.Dao;
using SupervisorProcessing.DataContext.External;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

//using Microsoft.EntityFrameworkCore.Sqlite;

namespace SupervisorProcessing.Repository
{
    public class ScheduleRepository
    {
        private readonly DbContextContentGrabber _DbContextFactory;
        private ILogger _Logger;

        public ScheduleRepository(IDbContextFactory<DbContextContentGrabber> dbContextFactory_)
        {
            _DbContextFactory = dbContextFactory_.CreateDbContext();
            _Logger = Log.Logger.ForContext<ScheduleRepository>();
        }

        public bool IsAccessible()
        {
            return _DbContextFactory.Database
                .CanConnect();
        }

        //Get all schedule not deleted
        public IEnumerable<Schedule> FindAll()
        {
            try
            {
                return _DbContextFactory.Schedules
                    .Where(s => s.is_deleted_ == false)
                    .AsNoTracking()
                    .ToList();
            }
            catch (Exception e)
            {
                _Logger.Error(e, "this is an exception");
                return Enumerable.Empty<Schedule>();
            }
        }

        //Get all schedule not deleted and whose id not contains in scheduleIds_
        public IEnumerable<Schedule> FindOffList(IEnumerable<Guid> scheduleIds_)
        {
            try
            {
                return _DbContextFactory.Schedules
                    .Where(s => s.is_deleted_ == false)
                    .AsNoTracking()
                    .ToList()
                    .Where(s => !scheduleIds_.Contains(s.schedule_id_));
            }
            catch (Exception e)
            {
                _Logger.Error(e, "this is an exception");
                return Enumerable.Empty<Schedule>();
            }
        }
    }
}