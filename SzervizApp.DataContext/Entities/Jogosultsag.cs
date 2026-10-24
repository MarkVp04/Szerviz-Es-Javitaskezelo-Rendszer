using System.ComponentModel.DataAnnotations;

namespace SzervizApp.DataContext.Entities
{
    public static class Szerepkorok
    {
        public const string Recepcios = "Recepcios";
        public const string Szerelo = "Szerelo";
        public const string Manager = "Manager";
        public const string Ugyfel = "Ugyfel";
        public const string Munkatarsak = Recepcios + "," + Szerelo + "," + Manager;
        public const string Ugyfelszolgalat = Recepcios + "," + Manager;
        public const string Muszaki = Szerelo + "," + Manager;
    }

    public class Jogosultsag
    {
        public int Id { get; set; }
        [Required, MaxLength(30)]
        public string Kod { get; set; } = null!;
        [Required, MaxLength(100)]
        public string Nev { get; set; } = null!;
        [MaxLength(500)]
        public string? Leiras { get; set; }
    }
}
