namespace SzervizApp.DataContext.Entities
{
    public class AlkatreszFelhasznalas
    {
        public int Id { get; set; }
        public int JavitasId { get; set; }
        public int AlkatreszId { get; set; }
        public int RogzitoFelhasznaloId { get; set; }
        public decimal Mennyiseg { get; set; }
        public decimal Egysegar { get; set; }
        public DateTime Datum { get; set; } = DateTime.UtcNow;
        public Javitas Javitas { get; set; } = null!;
        public Alkatresz Alkatresz { get; set; } = null!;
        public Felhasznalo RogzitoFelhasznalo { get; set; } = null!;
    }
}
