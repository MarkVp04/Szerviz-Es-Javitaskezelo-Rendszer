using System.Text;
using Microsoft.Extensions.Configuration;

namespace SzervizApp.Services;

public sealed class JwtBeallitasok
{
    public string Issuer { get; }
    public string Audience { get; }
    public byte[] SecretKey { get; }
    public double ExpiresInDays { get; }

    public JwtBeallitasok(IConfiguration configuration)
    {
        var section = configuration.GetSection("JwtSettings");
        Issuer = section["Issuer"] ?? "";
        Audience = section["Audience"] ?? "";
        SecretKey = Encoding.UTF8.GetBytes(section["SecretKey"] ?? "");
        ExpiresInDays = section.GetValue<double>("ExpiresInDays", 1);
        if (string.IsNullOrWhiteSpace(Issuer) || string.IsNullOrWhiteSpace(Audience) || SecretKey.Length < 32
            || !double.IsFinite(ExpiresInDays) || ExpiresInDays <= 0 || ExpiresInDays > 7)
            throw new InvalidOperationException("Hiányos JwtSettings: Issuer, Audience, legalább 32 bájtos SecretKey és 0–7 nap közötti pozitív ExpiresInDays kell. A fejlesztői beállítások a Development környezetben érhetők el.");
    }
}
