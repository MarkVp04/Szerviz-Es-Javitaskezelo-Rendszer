using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
        public DbSet<Jogosultsag> Jogosultsagok { get; set; }
        public DbSet<Felhasznalo> Felhasznalok { get; set; }
        public DbSet<Bevizsgalas> Bevizsgalasok { get; set; }
        public DbSet<Munkanaplo> Munkanaplok { get; set; }
        public DbSet<Alkatresz> Alkatreszek { get; set; }
        public DbSet<AlkatreszFelhasznalas> AlkatreszFelhasznalasok { get; set; }
        public DbSet<JavitasKoltseg> JavitasKoltsegek { get; set; }
        public DbSet<Visszaadas> Visszaadasok { get; set; }
        public DbSet<Uzenet> Uzenetek { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Eszkoz>()
                .HasOne(e => e.Tulajdonos)
                .WithMany()
                .HasForeignKey(e => e.TulajdonosId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Jogosultsag>().HasIndex(j => j.Kod).IsUnique();
            modelBuilder.Entity<Jogosultsag>().HasData(
                new Jogosultsag { Id = 1, Kod = Szerepkorok.Recepcios, Nev = "Recepciós" },
                new Jogosultsag { Id = 2, Kod = Szerepkorok.Szerelo, Nev = "Szerelő" },
                new Jogosultsag { Id = 3, Kod = Szerepkorok.Manager, Nev = "Manager" },
                new Jogosultsag { Id = 4, Kod = Szerepkorok.Ugyfel, Nev = "Ügyfél" });

            modelBuilder.Entity<Felhasznalo>().HasIndex(f => f.NormalizaltFelhasznalonev).IsUnique();
            modelBuilder.Entity<Alkatresz>().HasIndex(a => a.Cikkszam).IsUnique();
            modelBuilder.Entity<Bevizsgalas>().HasIndex(b => b.JavitasId).IsUnique();
            modelBuilder.Entity<Visszaadas>().HasIndex(v => v.JavitasId).IsUnique();
            modelBuilder.Entity<Ugyfel>().Property(u => u.Aktiv).HasDefaultValue(true);
            modelBuilder.Entity<Javitas>().Property(j => j.Verzio).HasDefaultValue(1);

            // A jelszavak és a belépési verzió nincs a migrációkba égetve.
            // A régi pilot javításaihoz nem ismert a rögzítő, ezért ezek a kapcsolatok opcionálisak.
            modelBuilder.Entity<Javitas>().HasOne(j => j.FelvevoFelhasznalo)
                .WithMany().HasForeignKey(j => j.FelvevoFelhasznaloId);
            modelBuilder.Entity<Javitas>().HasOne(j => j.FelelosFelhasznalo)
                .WithMany().HasForeignKey(j => j.FelelosFelhasznaloId);

            // A javítási előzményeket nem töröljük egy törzsadat törlésével együtt.
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var foreignKey in entity.GetForeignKeys())
                    foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
                foreach (var property in entity.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                        property.SetValueConverter(new ValueConverter<DateTime, DateTime>(
                            value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc)));
                }
            }

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
