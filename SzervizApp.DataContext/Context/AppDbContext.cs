using Microsoft.EntityFrameworkCore;
using SzervizApp.DataContext.Entities;

namespace SzervizApp.DataContext.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Ugyfel> Ugyfelek { get; set; }
        public DbSet<Eszkoz> Eszkozok { get; set; }
        public DbSet<Javitas> Javitasok { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Eszkoz>()
                .HasOne(e => e.Tulajdonos)
                .WithMany()
                .HasForeignKey(e => e.TulajdonosId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Javitas>()
                .HasOne(j => j.Ugyfel)
                .WithMany()
                .HasForeignKey(j => j.UgyfelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Javitas>()
                .HasOne(j => j.Eszkoz)
                .WithMany()
                .HasForeignKey(j => j.EszkozId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
