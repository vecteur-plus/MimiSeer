using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SupervisorProcessing.Dao;

namespace SupervisorProcessing.DataContext.External
{
    public class DbContextContentGrabber : DbContext
    {
        public DbContextContentGrabber()
        {
        }

        [ActivatorUtilitiesConstructor]
        public DbContextContentGrabber(DbContextOptions<DbContextContentGrabber> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }

        public virtual DbSet<Schedule> Schedules { get; set; }
    }
}