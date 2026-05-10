using AttendanceSync.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace AttendanceSync.Infrastructure
{

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<IntegrationJobLog> IntegrationJobLogs => Set<IntegrationJobLog>();
        public DbSet<IntegrationErrorLog> IntegrationErrorLogs => Set<IntegrationErrorLog>();
        public DbSet<SyncedAttendanceLog> SyncedAttendanceLogs => Set<SyncedAttendanceLog>();
        public DbSet<HikvisionToken> HikvisionTokens => Set<HikvisionToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SyncedAttendanceLog>()
                .HasIndex(x => x.HikvisionRecordId)
                .IsUnique();

            modelBuilder.Entity<IntegrationJobLog>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            modelBuilder.Entity<IntegrationErrorLog>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            modelBuilder.Entity<SyncedAttendanceLog>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            modelBuilder.Entity<HikvisionToken>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");
        }
    }

}
