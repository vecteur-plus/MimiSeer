using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using SupervisorProcessing.Dao;
using SupervisorProcessing.DataContext.External;
using SupervisorProcessing.Repository;
using System.Collections.Generic;
using System.Linq;

namespace SupervisorProcessingTest
{
    [TestFixture]
    public class UTRepoSite
    {
        private IDbContextFactory<DbContextSiteWeb> _DbContextFactory;

        private IDbContextFactory<DbContextSiteWeb> _DbContextFactoryFake;

        public UTRepoSite()
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
        }

        private void ClearFakeDbContext()
        {
            _DbContextFactoryFake.CreateDbContext().RemoveRange(_DbContextFactoryFake.CreateDbContext().Sites);
            _DbContextFactoryFake.CreateDbContext().SaveChanges();
        }

        private IEnumerable<Site> populateDbSetSite(int quantity_)
        {
            List<Site> sites = new List<Site>();

            for (int i = 0; i < quantity_; i++)
            {
                sites.Add(new()
                {
                    Id = i,
                    AgentName = i.ToString(),
                    CleGeneral = i.ToString(),
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

        [SetUp]
        public void Init()
        {
            ClearFakeDbContext();
        }

        [Test]
        public void TestIsAccesibleDbContext()
        {
            var repo = new SiteRepository(_DbContextFactory);
            Assert.That(repo.IsAccessible(), Is.True);
        }

        [Test]
        public void TestFakeFindAll()
        {
            var sites = populateDbSetSite(10);
            var DbContextrepo = new SiteRepository(_DbContextFactoryFake);
            var response = DbContextrepo.FindAll();

            response.Should().HaveCount(10).And.BeEquivalentTo(sites);
        }

        [Test]
        public void TestFindAll()
        {
            var DbContextrepo = new SiteRepository(_DbContextFactory);

            Assert.DoesNotThrow(() => DbContextrepo.FindAll());
        }

        [Test]
        public void TestFakeFindByNames()
        {
            IEnumerable<Site> sites = populateDbSetSite(4);

            var siteSelected = sites.Take(2);

            var Repo = new SiteRepository(_DbContextFactoryFake);
            IEnumerable<Site> response = Repo.FindByNames(siteSelected.Select(s => s.Name));

            response.Should().HaveCount(2).And.BeEquivalentTo(siteSelected);
        }

        [Test]
        public void TestFindByNames()
        {
            var repo = new SiteRepository(_DbContextFactory);

            var siteSelected = repo.FindAll().Take(30);

            IEnumerable<Site> response = repo.FindByNames(siteSelected.Select(s => s.Name));

            response.Should().HaveCount(30).And.BeEquivalentTo(siteSelected);
        }

        [Test]
        public void TestFakeFindDistinctTypeIndexationOffList()
        {
            var sizeDataSet = 30;
            var sizeDataSelected = 3;

            IEnumerable<Site> sites = populateDbSetSite(sizeDataSet);

            var siteSelected = sites.Take(sizeDataSelected);

            var repo = new SiteRepository(_DbContextFactoryFake);

            IEnumerable<string> response = repo.FindDistinctTypeIndexationOffList(siteSelected.Select(s => s.TypeIndexation));

            response.Should().HaveCount(sizeDataSet - sizeDataSelected).And.NotContain(siteSelected.Select(s => s.TypeIndexation));
        }

        [Test]
        public void TestFindDistinctTypeIndexationOffList()
        {
            var sizeDataSelected = 3;

            var repo = new SiteRepository(_DbContextFactory);
            var sites = repo.FindAll();

            var sizeDataSet = sites.Select(s => s.TypeIndexation).Distinct().Count();

            var siteSelected = sites.Take(sizeDataSelected);

            IEnumerable<string> response = repo.FindDistinctTypeIndexationOffList(siteSelected.Select(s => s.TypeIndexation));

            response.Should().HaveCount(sizeDataSet - siteSelected.Distinct().Count()).And.NotContain(siteSelected.Select(s => s.TypeIndexation));
        }

        [Test]
        public void TestFakeFindDistinctAgentOffList()
        {
            var sizeDataSet = 30;
            var sizeDataSelected = 3;

            IEnumerable<Site> sites = populateDbSetSite(sizeDataSet);

            var siteSelected = sites.Take(sizeDataSelected);

            var repo = new SiteRepository(_DbContextFactoryFake);

            IEnumerable<string> response = repo.FindDistinctAgentOffList(siteSelected.Select(s => s.AgentName));

            response.Should().HaveCount(sizeDataSet - sizeDataSelected).And.NotContain(siteSelected.Select(s => s.AgentName));
        }

        [Test]
        public void TestFindDistinctAgentOffList()
        {
            var sizeDataSelected = 3;

            var repo = new SiteRepository(_DbContextFactory);
            var sites = repo.FindAll();

            var sizeDataSet = sites.Select(s => s.AgentName).Distinct().Count();

            var siteSelected = sites.Take(sizeDataSelected);

            IEnumerable<string> response = repo.FindDistinctAgentOffList(siteSelected.Select(s => s.AgentName));

            response.Should().HaveCount(sizeDataSet - siteSelected.Distinct().Count()).And.NotContain(siteSelected.Select(s => s.AgentName));
        }
    }
}