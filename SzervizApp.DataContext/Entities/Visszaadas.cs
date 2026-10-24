using System.ComponentModel.DataAnnotations;

namespace SzervizApp.DataContext.Entities
{
    public class Visszaadas
    {
        public int Id { get; set; }
        public int JavitasId { get; set; }
        public int RogzitoFelhasznaloId { get; set; }
        public DateTime VisszaadasDatuma { get; set; } = DateTime.UtcNow;
        [Required, MaxLength(150)]
        public string AtvevoNev { get; set; } = null!;
        [MaxLength(2000)]
        public string? Megjegyzes { get; set; }
        public Javitas Javitas { get; set; } = null!;
        public Felhasznalo RogzitoFelhasznalo { get; set; } = null!;
    }
}
