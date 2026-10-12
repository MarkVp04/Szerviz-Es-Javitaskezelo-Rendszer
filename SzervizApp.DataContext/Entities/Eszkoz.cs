using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SzervizApp.DataContext.Entities
{
    public class Eszkoz
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Tulajdonos))]
        public int TulajdonosId { get; set; }

        [Required]
        [MaxLength(100)]
        public string EszkozTipus { get; set; } = null!;

        [MaxLength(100)]
        public string? Gyarto { get; set; }

        [MaxLength(100)]
        public string? Sorozatszam { get; set; }

        public Ugyfel Tulajdonos { get; set; } = null!;
    }
}
