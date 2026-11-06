using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SzervizApp.Client.Models;

namespace SzervizApp.Client.Api;

public sealed class ApiClient : IDisposable
{
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(20) };
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();
    private string? _token;

    public string BaseAddress => _http.BaseAddress?.ToString() ?? "";

    public void SetBaseAddress(string address)
    {
        if (!Uri.TryCreate(address.Trim().TrimEnd('/') + "/", UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new ArgumentException("Adj meg érvényes http vagy https API-címet.");

        _http.BaseAddress = uri;
    }

    public async Task<LoginResponseDto> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var response = await SendAsync<LoginResponseDto>(HttpMethod.Post, "api/auth/bejelentkezes",
            new LoginRequest(username, password), includeToken: false, ct);
        _token = response.Token;
        return response;
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        try { await SendNoContentAsync(HttpMethod.Post, "api/auth/kijelentkezes", null, ct); }
        finally { _token = null; }
    }

    public Task<List<JavitasDto>> GetJavitasokAsync(CancellationToken ct = default)
        => SendAsync<List<JavitasDto>>(HttpMethod.Get, "api/javitasok", null, true, ct);

    public Task<List<FeladatDto>> GetFeladatokAsync(bool sajat = false, CancellationToken ct = default)
        => SendAsync<List<FeladatDto>>(HttpMethod.Get, $"api/javitasok/feladatok?sajat={sajat.ToString().ToLowerInvariant()}", null, true, ct);

    public Task<JavitasDto> GetJavitasAsync(int id, CancellationToken ct = default)
        => SendAsync<JavitasDto>(HttpMethod.Get, $"api/javitasok/{id}", null, true, ct);

    public Task<JavitasDto> CreateJavitasAsync(UjJavitasRequest request, CancellationToken ct = default)
        => SendAsync<JavitasDto>(HttpMethod.Post, "api/javitasok", request, true, ct);

    public Task<List<UgyfelDto>> GetUgyfelekAsync(string? kereses = null, CancellationToken ct = default)
    {
        var query = string.IsNullOrWhiteSpace(kereses) ? "" : $"?kereses={Uri.EscapeDataString(kereses)}";
        return SendAsync<List<UgyfelDto>>(HttpMethod.Get, $"api/ugyfelek{query}", null, true, ct);
    }

    public Task<UgyfelDto> CreateUgyfelAsync(UgyfelRequest request, CancellationToken ct = default)
        => SendAsync<UgyfelDto>(HttpMethod.Post, "api/ugyfelek", request, true, ct);

    public Task<List<EszkozListaDto>> GetEszkozokAsync(string? kereses = null, CancellationToken ct = default)
    {
        var query = string.IsNullOrWhiteSpace(kereses) ? "" : $"?kereses={Uri.EscapeDataString(kereses)}";
        return SendAsync<List<EszkozListaDto>>(HttpMethod.Get, $"api/eszkozok{query}", null, true, ct);
    }

    public Task<EszkozListaDto> CreateEszkozAsync(EszkozRequest request, CancellationToken ct = default)
        => SendAsync<EszkozListaDto>(HttpMethod.Post, "api/eszkozok", request, true, ct);

    public Task<BevizsgalasDto> GetBevizsgalasAsync(int javitasId, CancellationToken ct = default)
        => SendAsync<BevizsgalasDto>(HttpMethod.Get, $"api/javitasok/{javitasId}/bevizsgalas", null, true, ct);

    public Task<BevizsgalasDto> SaveBevizsgalasAsync(int javitasId, BevizsgalasRequest request, bool letezik, CancellationToken ct = default)
        => SendAsync<BevizsgalasDto>(letezik ? HttpMethod.Put : HttpMethod.Post,
            $"api/javitasok/{javitasId}/bevizsgalas", request, true, ct);

    public Task<List<MunkanaploDto>> GetMunkanaplokAsync(int javitasId, CancellationToken ct = default)
        => SendAsync<List<MunkanaploDto>>(HttpMethod.Get, $"api/javitasok/{javitasId}/munkanaplo", null, true, ct);

    public Task<MunkanaploDto> CreateMunkanaploAsync(int javitasId, MunkanaploRequest request, CancellationToken ct = default)
        => SendAsync<MunkanaploDto>(HttpMethod.Post, $"api/javitasok/{javitasId}/munkanaplo", request, true, ct);

    public Task<KoltsegOsszesitesDto> GetKoltsegOsszesitesAsync(int javitasId, CancellationToken ct = default)
        => SendAsync<KoltsegOsszesitesDto>(HttpMethod.Get, $"api/javitasok/{javitasId}/koltsegek", null, true, ct);

    public Task<KoltsegDto> CreateKoltsegAsync(int javitasId, KoltsegRequest request, CancellationToken ct = default)
        => SendAsync<KoltsegDto>(HttpMethod.Post, $"api/javitasok/{javitasId}/koltsegek", request, true, ct);

    public Task<KoltsegOsszesitesDto> VeglegesitKoltsegeketAsync(int javitasId, CancellationToken ct = default)
        => SendAsync<KoltsegOsszesitesDto>(HttpMethod.Put, $"api/javitasok/{javitasId}/koltsegek/veglegesites", null, true, ct);

    public Task<List<AlkatreszDto>> GetAlkatreszekAsync(CancellationToken ct = default)
        => SendAsync<List<AlkatreszDto>>(HttpMethod.Get, "api/alkatreszek?aktiv=true", null, true, ct);

    public Task<List<AlkatreszFelhasznalasDto>> GetFelhasznaltAlkatreszekAsync(int javitasId, CancellationToken ct = default)
        => SendAsync<List<AlkatreszFelhasznalasDto>>(HttpMethod.Get, $"api/javitasok/{javitasId}/alkatreszek", null, true, ct);

    public Task<AlkatreszFelhasznalasDto> CreateAlkatreszFelhasznalasAsync(int javitasId, AlkatreszFelhasznalasRequest request, CancellationToken ct = default)
        => SendAsync<AlkatreszFelhasznalasDto>(HttpMethod.Post, $"api/javitasok/{javitasId}/alkatreszek", request, true, ct);

    public Task<List<FelhasznaloDto>> GetFelhasznalokAsync(CancellationToken ct = default)
        => SendAsync<List<FelhasznaloDto>>(HttpMethod.Get, "api/felhasznalok", null, true, ct);

    public Task<FelhasznaloDto> CreateFelhasznaloAsync(UjFelhasznaloRequest request, CancellationToken ct = default)
        => SendAsync<FelhasznaloDto>(HttpMethod.Post, "api/felhasznalok", request, true, ct);

    public Task<JavitasDto> SetFelelosAsync(int javitasId, FelelosRequest request, CancellationToken ct = default)
        => SendAsync<JavitasDto>(HttpMethod.Put, $"api/javitasok/{javitasId}/felelos", request, true, ct);

    public Task<JavitasDto> VallalJavitastAsync(int javitasId, CancellationToken ct = default)
        => SendAsync<JavitasDto>(HttpMethod.Post, $"api/javitasok/{javitasId}/vallalas", null, true, ct);

    public Task<JavitasDto> SetAllapotAsync(int javitasId, AllapotRequest request, CancellationToken ct = default)
        => SendAsync<JavitasDto>(HttpMethod.Put, $"api/javitasok/{javitasId}/allapot", request, true, ct);

    public Task<JavitasDto> LezarJavitastAsync(int javitasId, JavitasLezarasRequest request, CancellationToken ct = default)
        => SendAsync<JavitasDto>(HttpMethod.Put, $"api/javitasok/{javitasId}/lezaras", request, true, ct);

    private async Task<T> SendAsync<T>(HttpMethod method, string path, object? payload, bool includeToken, CancellationToken ct)
    {
        using var request = CreateRequest(method, path, payload, includeToken);
        using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        var json = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
            throw CreateApiException(response.StatusCode, json);

        if (string.IsNullOrWhiteSpace(json))
            throw new ApiException("A szerver üres választ küldött.", response.StatusCode);

        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions)
                ?? throw new ApiException("A szerver válasza nem értelmezhető.", response.StatusCode);
        }
        catch (JsonException ex)
        {
            throw new ApiException("A szerver válasza nem a várt formátumú.", response.StatusCode, ex);
        }
    }

    private async Task SendNoContentAsync(HttpMethod method, string path, object? payload, CancellationToken ct)
    {
        using var request = CreateRequest(method, path, payload, true);
        using var response = await _http.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NoContent)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw CreateApiException(response.StatusCode, body);
        }
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, object? payload, bool includeToken)
    {
        if (_http.BaseAddress is null)
            throw new InvalidOperationException("Nincs beállítva az API címe.");

        var request = new HttpRequestMessage(method, path);
        if (includeToken && !string.IsNullOrWhiteSpace(_token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        if (payload is not null)
        {
            var content = JsonSerializer.Serialize(payload, JsonOptions);
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");
        }
        return request;
    }

    private static ApiException CreateApiException(HttpStatusCode statusCode, string json)
    {
        string message = statusCode switch
        {
            HttpStatusCode.Unauthorized => "A felhasználónév vagy a jelszó hibás, vagy a munkamenet lejárt.",
            HttpStatusCode.Forbidden => "Ehhez a művelethez nincs megfelelő jogosultságod.",
            HttpStatusCode.NotFound => "A kért adat nem található.",
            HttpStatusCode.Conflict => "Az adatok időközben megváltoztak vagy ütköznek.",
            _ => $"Szerverhiba történt ({(int)statusCode})."
        };

        try
        {
            var detail = JsonSerializer.Deserialize<ApiErrorResponse>(json, JsonOptions);
            if (!string.IsNullOrWhiteSpace(detail?.Message)) message = detail.Message;
            else if (!string.IsNullOrWhiteSpace(detail?.Title)) message = detail.Title;
        }
        catch (JsonException) { }

        return new ApiException(message, statusCode);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    public void Dispose() => _http.Dispose();
}

public sealed class ApiException(string message, HttpStatusCode statusCode, Exception? inner = null)
    : Exception(message, inner)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
