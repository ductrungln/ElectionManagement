using Microsoft.EntityFrameworkCore;
using ElectionManagement.Models;

namespace ElectionManagement.Data
{
    public class ElectionDbContext : DbContext
    {
        public ElectionDbContext(DbContextOptions<ElectionDbContext> options) : base(options)
        {
        }

        public DbSet<ElectionResult> ElectionResults { get; set; }
        public DbSet<ElectionProgress> ElectionProgresses { get; set; }
        public DbSet<ImportLog> ImportLogs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<BallotVerification> BallotVerifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ElectionProgress indexes
            modelBuilder.Entity<ElectionProgress>()
                .HasIndex(e => new { e.Stt, e.TenKhuVuc })
                .IsUnique();

            // User indexes
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}
