using System.ComponentModel.DataAnnotations;

namespace SzervizApp.DataContext.Entities
{
    public class Alkatresz
    {
        public int Id { get; set; }
        [Required, MaxLength(80)]
        public string Cikkszam { get; set; } = null!;
        [Required, MaxLength(150)]
        public string Nev { get; set; } = null!;
        [MaxLength(100)]
        public string? Gyarto { get; set; }
        [Required, MaxLength(20)]
        public string Egyseg { get; set; } = "db";
        public decimal AktualisEgysegar { get; set; }
        public bool Aktiv { get; set; } = true;
    }
}
