using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SzervizApp.DataContext.Entities
{
    public enum JavitasAllapot
    {
        Atvett = 1,
        Lezart = 2,
        BevizsgalasAlatt = 3,
        JavitasAlatt = 4,
        AlkatreszreVar = 5,
        Visszaadva = 6
    }

    public enum JavitasEredmeny
    {
        Javitva = 1,
        NemJavithato = 2,
        JavitasNelkulLezarva = 3
    }

    public class Javitas
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Ugyfel))]
        public int UgyfelId { get; set; }

        [ForeignKey(nameof(Eszkoz))]
        public int EszkozId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string BejelentettHiba { get; set; } = null!;

        public DateTime AtvetelDatuma { get; set; } = DateTime.UtcNow;

        public JavitasAllapot Allapot { get; set; } = JavitasAllapot.Atvett;

        public DateTime? LezarasDatuma { get; set; }

        public JavitasEredmeny? Eredmeny { get; set; }

        [MaxLength(2000)]
        public string? LezarasIndoklasa { get; set; }

        public int? FelvevoFelhasznaloId { get; set; }
        public int? FelelosFelhasznaloId { get; set; }
        public DateTime? TervezettBefejezes { get; set; }
        public bool Garancialis { get; set; }

        [MaxLength(2000)]
        public string? Megjegyzes { get; set; }

        public bool KoltsegekVeglegesitve { get; set; }

        [ConcurrencyCheck]
        public int Verzio { get; set; } = 1;

        public Felhasznalo? FelvevoFelhasznalo { get; set; }
        public Felhasznalo? FelelosFelhasznalo { get; set; }

        public Ugyfel Ugyfel { get; set; } = null!;

        public Eszkoz Eszkoz { get; set; } = null!;
    }
}
