using Microsoft.EntityFrameworkCore;
using SupervisorProcessing.Dao;

namespace SupervisorProcessing.DataContext.External
{
    public class DbContextSiteWeb : DbContext
    {
        public DbContextSiteWeb(DbContextOptions<DbContextSiteWeb> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Site>()
                .HasKey(s => new { s.Id, s.TypeIndexation });
        }

        public virtual DbSet<Site> Sites { get; set; }
        public virtual DbSet<Flag> Flags { get; set; }
    }
}