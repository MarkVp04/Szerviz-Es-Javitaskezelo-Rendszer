using System.ComponentModel.DataAnnotations;

namespace SzervizApp.DataContext.Entities
{
    public class Bevizsgalas
    {
        public int Id { get; set; }
        public int JavitasId { get; set; }
        [Required, MaxLength(4000)]
        public string Eredmeny { get; set; } = null!;
        public decimal? BecsultKoltseg { get; set; }
        public DateTime Datum { get; set; } = DateTime.UtcNow;
        public int VegezteFelhasznaloId { get; set; }
        public Javitas Javitas { get; set; } = null!;
        public Felhasznalo VegezteFelhasznalo { get; set; } = null!;
    }
}
