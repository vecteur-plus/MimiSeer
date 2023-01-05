using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using SupervisorProcessing.Dao;
using SupervisorProcessing.DataContext.External;
using SupervisorProcessing.Model.Internal;
using SupervisorProcessing.Repository;
using SupervisorProcessing.Service;
using SupervisorProcessing.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SupervisorProcessingTest.Service
{
    public class ServiceScheduleTests
    {
        private readonly ServiceSchedule serviceSchedule;
        private readonly ServiceSchedule serviceScheduleFake;

        private IDbContextFactory<DbContextContentGrabber> _DbContextFactory;

        private IDbContextFactory<DbContextContentGrabber> _DbContextFactoryFake;

        private ServiceStockage _ServiceStockage = new();

        public ServiceScheduleTests()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(TestContext.CurrentContext.TestDirectory)
                .AddJsonFile($"appsettings.Test.json", optional: false, reloadOnChange: true).Build();

            var connectionString = configuration.GetConnectionString("ContentGrabberDatabase");
            var dbcontext = new DbContextContentGrabber(new DbContextOptionsBuilder<DbContextContentGrabber>().UseSqlite(connectionString).Options);
            var mockFactory = new Mock<IDbContextFactory<DbContextContentGrabber>>();
            mockFactory.Setup(f => f.CreateDbContext())
            .Returns(dbcontext);
            _DbContextFactory = mockFactory.Object;

            dbcontext = new DbContextContentGrabber(new DbContextOptionsBuilder<DbContextContentGrabber>()
                .UseInMemoryDatabase("InMemoryTest")
                .Options);

            mockFactory = new Mock<IDbContextFactory<DbContextContentGrabber>>();
            mockFactory.Setup(f => f.CreateDbContext())
            .Returns(dbcontext);
            _DbContextFactoryFake = mockFactory.Object;

           // serviceSchedule = new ServiceSchedule(new RepoSchedule(_DbContextFactory), _ServiceStockage);
          //  serviceScheduleFake = new ServiceSchedule(new RepoSchedule(_DbContextFactoryFake), _ServiceStockage);
        }

        /*  private IEnumerable<CModelSchedule> populateDbSetSchedule(int quantity_)
          {
              List<Schedule> schedules = new List<Schedule>();

              for (int i = 0; i < quantity_; i++)
              {
                  schedules.Add(new() { schedule_id_ = Guid.NewGuid() });
              }

              _DbContextFactoryFake.CreateDbContext().Schedules.AddRange(schedules);
              _DbContextFactoryFake.CreateDbContext().SaveChanges();

              return schedules;
          }
        */

        private IEnumerable<CModelAgent> populateStockageAgent(int quantity)
        {
            List<CModelAgent> agents = new();

            for (int i = 0; i < quantity; i++)
            {
                agents.Add(new CModelAgent()
                {
                    Id = i,
                    AgentName = i.ToString()
                });
            }

            _ServiceStockage.Agents.AddRange(agents);
            return agents;
        }

        private IEnumerable<CModelSite> populateStockageSite(int quantity_)
        {
            List<CModelSite> sites = new();

            Random random = new();

            for (int i = 0; i < quantity_; i++)
            {
                var agent = _ServiceStockage.Agents[random.Next(_ServiceStockage.Agents.Count)];

                var site = new CModelSite()
                {
                    Id = i,
                    Agent = agent,
                    CleGeneral = i.ToString(),
                    Commentaire = "",
                    Name = $"web.{i}.1",
                    Schedules = new()
                };

                agent.Sites.Add(site);
                sites.Add(site);
            }

            _ServiceStockage.Sites.AddRange(sites);
            return sites;
        }

        private IEnumerable<Schedule> populateSchedule(int quantity_)
        {
            List<Schedule> schedules = new();
            Random random = new();

            for (int i = 1; i < quantity_ + 1; i++)
            {
                var site = _ServiceStockage.Sites[random.Next(_ServiceStockage.Sites.Count)];

                schedules.Add(new Schedule()
                {
                    schedule_id_ = new Guid(),
                    agent_name_ = site.Agent.AgentName,
                    session_id_ = site.Name
                });
            }

            _DbContextFactoryFake.CreateDbContext().Schedules.AddRange(schedules);
            _DbContextFactoryFake.CreateDbContext().SaveChanges();

            return schedules;
        }

        [SetUp]
        public void Setup()
        {
            ClearFakeDbContext();
            ClearStockage();
        }

        private void ClearFakeDbContext()
        {
            _DbContextFactoryFake.CreateDbContext().RemoveRange(_DbContextFactoryFake.CreateDbContext().Schedules);
            _DbContextFactoryFake.CreateDbContext().SaveChanges();
        }

        private void ClearStockage()
        {
            _ServiceStockage.ClearAll();
        }

        [Test]
        public void TestDatabaseIsAccessible()
        {
            serviceSchedule.DatabaseIsAccessible().Should().BeTrue();
        }

        [Test]
        public void TestFakeFindNewSchedules()
        {
            var agents = populateStockageAgent(2);
            var sites = populateStockageSite(3);
            populateSchedule(2);
            _ServiceStockage.Schedules.AddRange(serviceScheduleFake.FindNewSchedules());

            var schedules = populateSchedule(3);

            var result = serviceScheduleFake.FindNewSchedules();

            result.Select(r => r.GuidSchedule).Should().BeEquivalentTo(schedules.Select(s => s.schedule_id_));
        }

        [Test]
        public void TestFakeFindSchedulesToCheck()
        {
            populateStockageAgent(1);
            populateStockageSite(1);
            var schedules = populateSchedule(3);

            var result = serviceScheduleFake.FindSchedulesToCheck();

            result.Select(r => r.GuidSchedule).Should().BeEquivalentTo(schedules.Select(s => s.schedule_id_));
        }
    }
}