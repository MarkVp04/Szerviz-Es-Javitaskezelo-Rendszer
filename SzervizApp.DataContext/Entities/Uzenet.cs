using System.ComponentModel.DataAnnotations;

namespace SzervizApp.DataContext.Entities
{
    public class Uzenet
    {
        public int Id { get; set; }
        public int UgyfelId { get; set; }
        public int? JavitasId { get; set; }
        public int FeladoFelhasznaloId { get; set; }
        [Required, MaxLength(4000)]
        public string Tartalom { get; set; } = null!;
        public DateTime KuldesDatuma { get; set; } = DateTime.UtcNow;
        public Ugyfel Ugyfel { get; set; } = null!;
        public Javitas? Javitas { get; set; }
        public Felhasznalo FeladoFelhasznalo { get; set; } = null!;
    }
}
