using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SupervisorProcessing.Model.Internal;
using SupervisorProcessing.Model.Internal.Entry;
using SupervisorProcessing.Service;
using System.Collections.Generic;
using System.Linq;

namespace SupervisorProcessing.DataContext
{
    public class DbContextIntern : DbContext
    {

        private readonly EntityEntryService _EntityEntryService;

        public DbContextIntern(DbContextOptions<DbContextIntern> options,EntityEntryService entityEntryService_) : base(options)
        {
            _EntityEntryService = entityEntryService_;
            SavingChanges += _dbContext_SavingChanges;
        }

        private void _dbContext_SavingChanges(object sender, SavingChangesEventArgs e)
        {
            _EntityEntryService.Entries.AddRange(ChangeTracker
                .Entries()
                .Where(e => e.State != EntityState.Unchanged)
                .Select(e => new GenericEntry(e)));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .EnableSensitiveDataLogging();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CModelSite>(
                s =>
                {
                    s.Property(s => s.Id)
                    .ValueGeneratedOnAdd();

                    s.HasKey(s => s.Id);

                    s.HasOne(s => s.TypeIndexation)
                  .WithMany(type => type.Sites)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasForeignKey(s => s.IdTypeIndexation)
                  .HasPrincipalKey(type => type.Id);

                    s.HasOne(s => s.Agent)
                 .WithMany(agent => agent.Sites)
                 .OnDelete(DeleteBehavior.Cascade)
                 .HasForeignKey(s => s.IdAgent)
                 .HasPrincipalKey(agent => agent.Id);
                }
                );

            modelBuilder.Entity<CModelSchedule>(
                s =>
                {
                    s.Property(s => s.Id)
                   .ValueGeneratedOnAdd();

                    s.Property(schedule => schedule.IdSchedule)
                    .IsRequired();

                    s.HasKey(schedule => schedule.IdSchedule);

                    s.HasIndex(schedule => schedule.IdSchedule);

                    s.HasOne(site => site.Site)
                    .WithMany(schedule => schedule.Schedules)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasForeignKey(schedule => schedule.IdSite)
                    .HasPrincipalKey(site => site.Id);
                }
                );

            modelBuilder.Entity<CModelTypeIndexation>(
                type =>
                {
                    type.Property(type => type.TypeIndexation)
                    .IsRequired();

                    type.Property(type => type.Id)
                    .ValueGeneratedOnAdd()
                    .IsRequired();
                }
                );

            modelBuilder.Entity<CModelAgent>(
               agent =>
               {
                   agent.Property(agent => agent.AgentName)
                   .IsRequired();

                   agent.Property(agent => agent.Id)
                   .ValueGeneratedOnAdd()
                   .IsRequired();
               }
               );
            modelBuilder.Entity<ExtendedDetailedSiteCollectInformation>(
                detailed =>
                {
                    detailed.HasKey(d => new { d.IdSchedule, d.IdSite });
                }
                );

         
        }

        public DbSet<CModelSite> Sites { get; set; }

        public DbSet<CModelSchedule> Schedules { get; set; }

        public DbSet<CModelTypeIndexation> TypeIndexations { get; set; }

        public DbSet<CModelAgent> Agents { get; set; }

        public DbSet<ExtendedDetailedSiteCollectInformation> DetailedSiteCollectInformations { get; set; }
      
        public DbSet<ScheduleMessage> ScheduleMessages { get; set; }




    }
}