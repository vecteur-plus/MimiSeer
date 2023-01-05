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
    public class ServiceTypeIndexationTests
    {
        private readonly ServiceTypeIndexation serviceTypeIndexation;
        private readonly ServiceTypeIndexation serviceTypeIndexationFake;

        private IDbContextFactory<DbContextSiteWeb> _DbContextFactory;

        private IDbContextFactory<DbContextSiteWeb> _DbContextFactoryFake;

        

        public ServiceTypeIndexationTests()
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

            serviceTypeIndexation = new ServiceTypeIndexation(new SiteRepository(_DbContextFactory));
            serviceTypeIndexationFake = new ServiceTypeIndexation(new SiteRepository(_DbContextFactoryFake));
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

        private IEnumerable<Site> populateDbSetSite(int quantity_)
        {
            var count = _DbContextFactoryFake.CreateDbContext().Sites.Count();

            List<Site> sites = new List<Site>();

            for (int i = count; i < count + quantity_; i++)
            {
                sites.Add(new()
                {
                    Id = i,
                    AgentName = $"\"{i}\"",
                    CleGeneral = $"{i}",
                    Commentaire = "",
                    GrabbingSettings = "",
                    Name = "web." + i.ToString() + ".1",
                    StatutProduction = true,
                    TypeIndexation = i.ToString()
                });
            }

            _DbContextFactoryFake.CreateDbContext().Sites.AddRange(sites);
            _DbContextFactoryFake.CreateDbContext().SaveChanges();

            return sites;
        }

        private IEnumerable<Site> DeleteDbSetSite(int quantity_)
        {
            List<Site> sites = new List<Site>();

            Random random = new Random();
            for (int i = 0; i < quantity_; i++)
            {
                var count = _DbContextFactoryFake.CreateDbContext().Sites.Count();
                if (count == 0)
                {
                    break;
                }

                var site = _DbContextFactoryFake.CreateDbContext().Sites.ToList()[random.Next(count)];
                sites.Add(site);
                _DbContextFactoryFake.CreateDbContext().Sites.Remove(site);
            }

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
        public void TestFakeFindTypeIndexationOffListWithEmptyList()
        {
            var sites = populateDbSetSite(3);

            var result = serviceTypeIndexationFake.FindTypeIndexationOffList(new List<CModelTypeIndexation>());

            result.Select(r => r.TypeIndexation).Should().BeEquivalentTo(sites.Select(s => s.TypeIndexation));
        }

        [Test]
        public void TestFakeFindTypeIndexationOffList()
        {
            populateDbSetSite(3);

            var typeIndexation = serviceTypeIndexationFake.FindTypeIndexationOffList(new List<CModelTypeIndexation>());

            var sites = populateDbSetSite(3);

            var result = serviceTypeIndexationFake.FindTypeIndexationOffList(typeIndexation);

            result.Select(r => r.TypeIndexation).Should().BeEquivalentTo(sites.Select(s => s.TypeIndexation));
        }
    }
}