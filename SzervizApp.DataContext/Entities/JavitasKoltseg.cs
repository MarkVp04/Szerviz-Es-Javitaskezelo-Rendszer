using System.ComponentModel.DataAnnotations;

namespace SzervizApp.DataContext.Entities
{
    public enum KoltsegTipus
    {
        Egyeb = 1,
        Szallitas = 2,
        Bevizsgalas = 3
    }

    public class JavitasKoltseg
    {
        public int Id { get; set; }
        public int JavitasId { get; set; }
        public int RogzitoFelhasznaloId { get; set; }
        [Required, MaxLength(200)]
        public string Megnevezes { get; set; } = null!;
        public KoltsegTipus Tipus { get; set; }
        public decimal Osszeg { get; set; }
        public DateTime Datum { get; set; } = DateTime.UtcNow;
        public Javitas Javitas { get; set; } = null!;
        public Felhasznalo RogzitoFelhasznalo { get; set; } = null!;
    }
}
