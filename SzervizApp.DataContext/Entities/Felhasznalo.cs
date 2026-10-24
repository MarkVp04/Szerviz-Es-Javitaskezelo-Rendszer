using System.ComponentModel.DataAnnotations;

namespace SzervizApp.DataContext.Entities
{
    public class Felhasznalo
    {
        public int Id { get; set; }
        [Required, MaxLength(80)]
        public string Felhasznalonev { get; set; } = null!;
        [Required, MaxLength(80)]
        public string NormalizaltFelhasznalonev { get; set; } = null!;
        [Required, MaxLength(500)]
        public string JelszoHash { get; set; } = null!;
        [Required, MaxLength(150)]
        public string Nev { get; set; } = null!;
        [EmailAddress, MaxLength(254)]
        public string? Email { get; set; }
        [MaxLength(30)]
        public string? Telefon { get; set; }
        public bool Aktiv { get; set; } = true;
        public int JogosultsagId { get; set; }
        public int? UgyfelId { get; set; }
        public Guid BelepesVerzio { get; set; } = Guid.NewGuid();
        public Jogosultsag Jogosultsag { get; set; } = null!;
        public Ugyfel? Ugyfel { get; set; }
    }
}
