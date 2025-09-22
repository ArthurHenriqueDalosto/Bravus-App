using BravusApp.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace BravusApp.Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Operator> Operators => Set<Operator>();
        public DbSet<Duty> Duties => Set<Duty>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Operator>()
                .HasIndex(o => o.Cpf)
                .IsUnique();


            modelBuilder.Entity<Duty>()
                .HasOne(d => d.Operator)
                .WithMany(o => o.Duties)
                .HasForeignKey(d => d.OperatorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
