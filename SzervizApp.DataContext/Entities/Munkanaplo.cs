using System.ComponentModel.DataAnnotations;

namespace SzervizApp.DataContext.Entities
{
    public class Munkanaplo
    {
        public int Id { get; set; }
        public int JavitasId { get; set; }
        public int FelhasznaloId { get; set; }
        [Required, MaxLength(4000)]
        public string Leiras { get; set; } = null!;
        public DateTime Kezdes { get; set; }
        public DateTime? Befejezes { get; set; }
        public decimal LedolgozottOra { get; set; }
        public decimal Oradij { get; set; }
        public Javitas Javitas { get; set; } = null!;
        public Felhasznalo Felhasznalo { get; set; } = null!;
    }
}
