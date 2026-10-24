namespace SzervizApp.Services;

// Az API alakítja HTTP-válasszá; a szolgáltatások nem hoznak létre MVC-válaszobjektumokat.
public sealed class SzervizKivetel(int kod, string message) : Exception(message)
{
    public int Kod { get; } = kod;
}
