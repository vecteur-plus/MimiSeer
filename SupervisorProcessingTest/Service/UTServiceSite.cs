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
    public class ServiceSiteTests
    {
        private readonly ServiceSite serviceSiteFake;
        private readonly ServiceSite serviceSite;

        private IDbContextFactory<DbContextSiteWeb> _DbContextFactory;

        private IDbContextFactory<DbContextSiteWeb> _DbContextFactoryFake;

        private ServiceStockage _ServiceStockage = new();

        public ServiceSiteTests()
        {
            var configuration = new ConfigurationBuilder()
         .SetBasePath(TestContext.CurrentContext.TestDirectory)
         .AddJsonFile($"appsettings.Test.json", optional: false, reloadOnChange: true).Build();

            var connectionString = configuration.GetConnectionString("SiteWebDatabase");
            var dbcontext = new DbContextSiteWeb(new DbContextOptionsBuilder<DbContextSiteWeb>().UseMySQL(connectionString).Options);
            var mockFactory = new Mock<IDbContextFactory<DbContextSiteWeb>>();
            mockFactory.Setup(f => f.CreateDbContext())
            .Returns(dbcontext);
            _DbContextFactory = mockFactory.Object;

            dbcontext = new DbContextSiteWeb(new DbContextOptionsBuilder<DbContextSiteWeb>()
                .UseInMemoryDatabase("InMemoryTest")
                .Options);

            mockFactory = new Mock<IDbContextFactory<DbContextSiteWeb>>();
            mockFactory.Setup(f => f.CreateDbContext())
            .Returns(dbcontext);
            _DbContextFactoryFake = mockFactory.Object;

           // serviceSiteFake = new ServiceSite(_ServiceStockage, new RepoSite(_DbContextFactoryFake));
           // serviceSite = new ServiceSite(_ServiceStockage, new RepoSite(_DbContextFactory));
        }

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

        private IEnumerable<CModelTypeIndexation> populateStockageTypeIndexation(int quantity)
        {
            List<CModelTypeIndexation> typeIndexations = new();

            for (int i = 0; i < quantity; i++)
            {
                typeIndexations.Add(new CModelTypeIndexation()
                {
                    Id = i,
                    TypeIndexation = i.ToString()
                });
            }

            _ServiceStockage.TypeIndexations.AddRange(typeIndexations);
            return typeIndexations;
        }

        private IEnumerable<Site> populateDbSite(int quantity_)
        {
            List<Site> sites = new();

            Random random = new Random();

            var count = _DbContextFactoryFake.CreateDbContext().Sites.Count();

            for (int i = count; i < quantity_ + count; i++)
            {
                var agent = _ServiceStockage.Agents[random.Next(_ServiceStockage.Agents.Count)];
                var typeIndexation = _ServiceStockage.TypeIndexations[random.Next(_ServiceStockage.TypeIndexations.Count)];

                var site = new Site()
                {
                    Id = i,
                    CleGeneral = i.ToString(),
                    Commentaire = "",
                    Name = $"web.{i}.1",
                    AgentName = agent.AgentNameSqlFormat,
                    TypeIndexation = typeIndexation.TypeIndexation,
                    StatutProduction = true
                };

                sites.Add(site);
            }

            _DbContextFactoryFake.CreateDbContext().Sites.AddRange(sites);
            _DbContextFactoryFake.CreateDbContext().SaveChanges();
            return sites;
        }

        [SetUp]
        public void Setup()
        {
            ClearFakeDbContext();
            ClearStockage();
        }

        private void ClearFakeDbContext()
        {
            _DbContextFactoryFake.CreateDbContext().RemoveRange(_DbContextFactoryFake.CreateDbContext().Sites);
            _DbContextFactoryFake.CreateDbContext().SaveChanges();
        }

        private void ClearStockage()
        {
            _ServiceStockage.ClearAll();
        }

        [Test]
        public void TestDatabaseIsAccessible()
        {
            serviceSite.DatabaseIsAccessible().Should().BeTrue();
        }

        [Test]
        public void TestFakeFindAllSites()
        {
            var agents = populateStockageAgent(2);
            var typeIndexations = populateStockageTypeIndexation(2);

            var sites = populateDbSite(5);

            var result = serviceSiteFake.FindAllSites();

            result.Select(r => r.Name).Should().BeEquivalentTo(sites.Select(s => s.Name));
        }

        [Test]
        public void TestFakeFindSites()
        {
            var agents = populateStockageAgent(2);
            var typeIndexations = populateStockageTypeIndexation(2);

            var sites = populateDbSite(5);

            var selectedSites = sites.Take(3).Select(s => s.Name);

            var result = serviceSiteFake.FindSites(selectedSites);

            result.Select(r => r.Name).Should().BeEquivalentTo(selectedSites);
        }
    }
}