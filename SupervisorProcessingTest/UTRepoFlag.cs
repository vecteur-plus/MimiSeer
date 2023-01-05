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
    public class UTRepoFlag
    {
        private IDbContextFactory<DbContextSiteWeb> _DbContextFactory;

        private IDbContextFactory<DbContextSiteWeb> _DbContextFactoryFake;

        public UTRepoFlag()
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(TestContext.CurrentContext.TestDirectory)
            .AddJsonFile($"appsettings.Test.json", optional: false, reloadOnChange: true)
            .Build();

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
            _DbContextFactoryFake.CreateDbContext().RemoveRange(_DbContextFactoryFake.CreateDbContext().Flags);
            _DbContextFactoryFake.CreateDbContext().SaveChanges();
        }

        private IEnumerable<Flag> populateDbSetSite(int quantity_)
        {
            List<Flag> flags = new List<Flag>();

            for (int i = 1; i < quantity_ + 1; i++)
            {
                flags.Add(new()
                {
                    Id = i,
                    IsSeen = false,
                });
            }

            _DbContextFactoryFake.CreateDbContext().Flags.AddRange(flags);
            _DbContextFactoryFake.CreateDbContext().SaveChanges();

            return flags;
        }

        [SetUp]
        public void Init()
        {
            ClearFakeDbContext();
        }

        [Test]
        public void TestFakeUpdateAllFlagToSeen()
        {
            var flags = populateDbSetSite(4);

            var repo = new FlagRepository(_DbContextFactoryFake);

            repo.UpdateAllFlagToSeen();

            _DbContextFactoryFake.CreateDbContext().Flags.Select(f => f.IsSeen).Should().OnlyContain(f => f == true);
        }

        [Test]
        public void TestUpdateAllFlagToSeen()
        {
            var repo = new FlagRepository(_DbContextFactory);

            repo.UpdateAllFlagToSeen();

            _DbContextFactoryFake.CreateDbContext().Flags.Where(f => f.IsSeen == false).ToList().Should().BeEmpty();
        }

        [Test]
        public void TestFakeUpdateListFlagToSeen()
        {
            var flags = populateDbSetSite(4);

            var flagSelected = flags.Select(f => f.Id).Take(2);

            var repo = new FlagRepository(_DbContextFactoryFake);

            repo.UpdateListFlagToSeen(flagSelected).Should().BeTrue();
        }

        [Test]
        public void TestUpdateListFlagToSeen()
        {
            var repo = new FlagRepository(_DbContextFactory);

            repo.SetFlagAtUnSeen(4);

            var flags = repo.FindFlags().ToList();

            var flagSelected = flags.Select(f => f.Id).Take(2);

            repo.UpdateListFlagToSeen(flagSelected);

            flags = repo.FindFlags().ToList();

            flags.ToList().Select(f => f.Id).Should().NotContain(flagSelected);
        }

        [Test]
        public void TestFakeFindFlags()
        {
            var flags = populateDbSetSite(4);

            var repo = new FlagRepository(_DbContextFactoryFake);

            repo.FindFlags().Should().BeEquivalentTo(flags);
        }

        [Test]
        public void TestFindFlags()
        {
            var repo = new FlagRepository(_DbContextFactory);

            Assert.DoesNotThrow(() => repo.FindFlags());
        }
    }
}