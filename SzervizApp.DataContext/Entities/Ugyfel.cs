using System.ComponentModel.DataAnnotations;

namespace SzervizApp.DataContext.Entities
{
    public class Ugyfel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nev { get; set; } = null!;

        [Required]
        [MaxLength(30)]
        public string Telefon { get; set; } = null!;

        [EmailAddress]
        [MaxLength(254)]
        public string? Email { get; set; }
    }
}
