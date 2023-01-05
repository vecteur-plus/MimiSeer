using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using SupervisorProcessing.Dao;
using SupervisorProcessing.DataContext.External;
using SupervisorProcessing.Repository;
using SupervisorProcessing.Service;
using System.Collections.Generic;

namespace SupervisorProcessingTest.Service
{
    public class ServiceFlagTests
    {
        private readonly ServiceFlag serviceFlag;
        private readonly ServiceFlag serviceFlagFake;

        private IDbContextFactory<DbContextSiteWeb> _DbContextFactory;

        private IDbContextFactory<DbContextSiteWeb> _DbContextFactoryFake;

        public ServiceFlagTests()
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

            serviceFlag = new ServiceFlag(new FlagRepository(_DbContextFactory));
            serviceFlagFake = new ServiceFlag(new FlagRepository(_DbContextFactoryFake));
        }

        [SetUp]
        public void Setup()
        {
        }

        // QA = 1 QD = 0 QM = 0
        [Test]
        public void TestDoublennageOnlyAdd()
        {
            var flags = new List<Flag>
            {
                new() { Id = 1, Name = "1", TypeModification = "ADD" }
            };

            flags = serviceFlag.Dedoublenage(flags);
            Assert.IsTrue(flags.Count == 1 && flags[0].TypeModification == "ADD");
        }

        // QA = 0 QD = 1 QM = 0
        [Test]
        public void TestDoublennageOnlyDelete()
        {
            var flags = new List<Flag>();
            flags.Add(new() { Id = 2, Name = "1", TypeModification = "DELETE" });

            flags = serviceFlag.Dedoublenage(flags);
            Assert.IsTrue(flags.Count == 1 && flags[0].TypeModification == "DELETE");
        }

        // QA = 0 QD = 0 QM = 1
        [Test]
        public void TestDoublennageOnlyModify()
        {
            var flags = new List<Flag>();
            flags.Add(new() { Id = 1, Name = "1", TypeModification = "MODIFY" });

            flags = serviceFlag.Dedoublenage(flags);
            Assert.IsTrue(flags.Count == 1 && flags[0].TypeModification == "MODIFY");
        }

        // QA > QD ; IA > ID ; QM = {N*}
        [Test]
        public void TestDoublennageMoreAddThanDeleteAndAddPlacedAfterDelete()
        {
            var flags = new List<Flag>();
            flags.Add(new() { Id = flags.Count + 1, Name = "1", TypeModification = "ADD" });
            flags.Add(new() { Id = flags.Count + 1, Name = "1", TypeModification = "MODIFY" });
            flags.Add(new() { Id = flags.Count + 1, Name = "1", TypeModification = "DELETE" });
            flags.Add(new() { Id = flags.Count + 1, Name = "1", TypeModification = "ADD" });
            flags.Add(new() { Id = flags.Count + 1, Name = "1", TypeModification = "MODIFY" });

            flags = serviceFlag.Dedoublenage(flags);
            Assert.IsTrue(flags.Count == 1 && flags[0].TypeModification == "ADD");
        }

        // QD > QA ; ID > IA ; QM = {N*}
        [Test]
        public void TestDoublennageMoreDeleteThanAddAndDeletePlacedAfterAdd()
        {
            var flags = new List<Flag>();
            flags.Add(new() { Id = flags.Count + 1, Name = "1", TypeModification = "DELETE" });
            flags.Add(new() { Id = flags.Count + 1, Name = "1", TypeModification = "ADD" });
            flags.Add(new() { Id = flags.Count + 1, Name = "1", TypeModification = "DELETE" });

            flags = serviceFlag.Dedoublenage(flags);
            Assert.IsTrue(flags.Count == 1 && flags[0].TypeModification == "DELETE");
        }

        // QD = QA ; IA > ID ; QM = {N*}
        [Test]
        public void TestDoublennageDeleteEqualAddAndAddPlacedAfterDelete()
        {
            var flags = new List<Flag>();
            flags.Add(new() { Id = 1, Name = "1", TypeModification = "DELETE" });
            flags.Add(new() { Id = 2, Name = "1", TypeModification = "ADD" });

            flags = serviceFlag.Dedoublenage(flags);
            Assert.IsTrue(flags.Count == 1 && flags[0].TypeModification == "MODIFY");
        }

        // QD = QA ; ID > IA ; QM = {N*}
        [Test]
        public void TestDoublennageDeleteEqualAddAndDeletePlacedAfterAdd()
        {
            var flags = new List<Flag>();
            flags.Add(new() { Id = 1, Name = "1", TypeModification = "ADD" });
            flags.Add(new() { Id = 2, Name = "1", TypeModification = "DELETE" });

            flags = serviceFlag.Dedoublenage(flags);
            Assert.IsTrue(flags.Count == 0);
        }

        // QA <= QM ; IA < IM ; QD = 0
        [Test]
        public void TestDoublennageMoreAddThanModifyAndModifyPlacedAfterAdd()
        {
            var flags = new List<Flag>();
            flags.Add(new() { Id = 1, Name = "1", TypeModification = "ADD" });
            flags.Add(new() { Id = flags.Count + 1, Name = "1", TypeModification = "MODIFY" });
            flags.Add(new() { Id = flags.Count + 1, Name = "1", TypeModification = "MODIFY" });

            flags = serviceFlag.Dedoublenage(flags);
            Assert.IsTrue(flags.Count == 1 && flags[0].TypeModification == "ADD");
        }

        // QD <= QM ; ID > IM ; QA = 0
        [Test]
        public void TestDoublennageMoreAddThanModifyAndAddPlacedAfterModify()
        {
            var flags = new List<Flag>();

            flags.Add(new() { Id = flags.Count + 1, Name = "1", TypeModification = "MODIFY" });
            flags.Add(new() { Id = flags.Count + 1, Name = "1", TypeModification = "MODIFY" });
            flags.Add(new() { Id = flags.Count + 1, Name = "1", TypeModification = "DELETE" });

            flags = serviceFlag.Dedoublenage(flags);
            Assert.IsTrue(flags.Count == 1 && flags[0].TypeModification == "DELETE");
        }
    }
}