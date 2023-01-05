using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using SupervisorProcessing.Dao;
using SupervisorProcessing.DataContext.External;
using SupervisorProcessing.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SupervisorProcessingTest
{
    [TestFixture]
    public class UTRepoContentGrabber
    {
        private IDbContextFactory<DbContextContentGrabber> _DbContextFactoryOnline;
        private IDbContextFactory<DbContextContentGrabber> _DbContextFactoryFake;

        public UTRepoContentGrabber()
        {
            var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(TestContext.CurrentContext.TestDirectory)
            .AddJsonFile($"appsettings.Test.json", optional: false, reloadOnChange: true);

            var dbcontext = new DbContextContentGrabber(new DbContextOptionsBuilder<DbContextContentGrabber>().UseSqlite(configurationBuilder.Build().GetConnectionString("ContentGrabberDatabase")).Options);
            var mockFactory = new Mock<IDbContextFactory<DbContextContentGrabber>>();
            mockFactory.Setup(f => f.CreateDbContext())
            .Returns(dbcontext);
            _DbContextFactoryOnline = mockFactory.Object;

            dbcontext = new DbContextContentGrabber(new DbContextOptionsBuilder<DbContextContentGrabber>()
           .UseInMemoryDatabase("InMemoryTest")
           .Options);
            mockFactory = new Mock<IDbContextFactory<DbContextContentGrabber>>();
            mockFactory.Setup(f => f.CreateDbContext())
            .Returns(dbcontext);
            _DbContextFactoryFake = mockFactory.Object;
        }

        private void ClearFakeDbContext()
        {
            _DbContextFactoryFake.CreateDbContext().RemoveRange(_DbContextFactoryFake.CreateDbContext().Schedules);
            _DbContextFactoryFake.CreateDbContext().SaveChanges();
        }

        private IEnumerable<Schedule> populateDbSetSchedule(int quantity_)
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

        [SetUp]
        public void Init()
        {
            ClearFakeDbContext();
        }

        [Test]
        public void TestIsAccesibleDbContext()
        {
            var repo = new ScheduleRepository(_DbContextFactoryOnline);
            Assert.IsTrue(repo.IsAccessible());
        }

        [Test]
        public void TestFindAllWithFakeDbContext()
        {
            IEnumerable<Schedule> schedules = populateDbSetSchedule(2000);
            var Repo = new ScheduleRepository(_DbContextFactoryFake);
            IEnumerable<Schedule> response = Repo.FindAll();

            Assert.True(Enumerable.SequenceEqual(response.Select(s => s.schedule_id_), schedules.Select(s => s.schedule_id_)));
        }

        [Test]
        public void TestFindAllWithDbContext()
        {
            var Repo = new ScheduleRepository(_DbContextFactoryOnline);

            Assert.DoesNotThrow(() => Repo.FindAll());
        }

        [Test]
        public void TestFindOffListWithFakeDbContext()
        {
            IEnumerable<Schedule> schedules = populateDbSetSchedule(100);
            var Repo = new ScheduleRepository(_DbContextFactoryFake);
            var selectedGuid = schedules.Select(s => s.schedule_id_).Take(schedules.Count() / 2);

            var value = Repo.FindOffList(selectedGuid);

            var listScheduleId = schedules.Select(s => s.schedule_id_);
            var listGuidSelected = listScheduleId.Except(selectedGuid);

            var listAreEqual = Enumerable.SequenceEqual(value.Select(s => s.schedule_id_), listGuidSelected);

            Assert.True(listAreEqual);
        }

        [Test]
        public void TestFindOffListWithDbContext()
        {
            var repo = new ScheduleRepository(_DbContextFactoryOnline);

            var schedules = repo.FindAll().ToList();
            var selectedGuid = schedules.Select(s => s.schedule_id_).Take(schedules.Count() - 5);

            var value = repo.FindOffList(selectedGuid).Select(s => s.schedule_id_).ToList();

            var listScheduleId = schedules.Select(s => s.schedule_id_);
            var listGuidSelected = listScheduleId.Except(selectedGuid);

            var listAreEqual = Enumerable.SequenceEqual(value, listGuidSelected);

            Assert.True(listAreEqual);
        }

        [Test]
        public void TestFindOffListWithFakeDbContextWithEmptyList()
        {
            IEnumerable<Schedule> schedules = populateDbSetSchedule(100);
            var Repo = new ScheduleRepository(_DbContextFactoryFake);
            var selectedGuid = Enumerable.Empty<Guid>();

            var value = Repo.FindOffList(selectedGuid);

            var listScheduleId = schedules.Select(s => s.schedule_id_);
            var listGuidSelected = listScheduleId.Except(selectedGuid);

            var listAreEqual = Enumerable.SequenceEqual(value.Select(s => s.schedule_id_), listGuidSelected);

            Assert.True(listAreEqual);
        }

        [Test]
        public void TestFindOffListWithDbContextWithEmptyList()
        {
            var repo = new ScheduleRepository(_DbContextFactoryOnline);

            var schedules = repo.FindAll().ToList();
            var selectedGuid = Enumerable.Empty<Guid>();

            var value = repo.FindOffList(selectedGuid).Select(s => s.schedule_id_).ToList();

            var listScheduleId = schedules.Select(s => s.schedule_id_);
            var listGuidSelected = listScheduleId.Except(selectedGuid);

            var listAreEqual = Enumerable.SequenceEqual(value, listGuidSelected);

            Assert.True(listAreEqual);
        }
    }
}