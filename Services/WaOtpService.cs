using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace MemberCard.Services;

public class WaOtpService
{
    readonly HttpClient _http = new();
    private readonly BrandingService _brand;
    //public WaOtpService(BrandingService brand) => _brand = brand;
    //private string BaseWaUrl => _brand.Config.apiWAUrl.TrimEnd('/');
    //private string BaseWaUrl = "";

    public WaOtpService(BrandingService brand)
    {
        _brand = brand;
        _http.BaseAddress = new Uri(_brand.Config.apiWAUrl);
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _http.DefaultRequestHeaders.Add("x-api-key", "affariretail");
        _http.DefaultRequestHeaders.Add("X-Client-ID", _brand.Config.waClientId);
    }

    static string Normalize62(string phoneRaw)
    {
        var digits = new string((phoneRaw ?? "").Where(char.IsDigit).ToArray());
        if (string.IsNullOrEmpty(digits)) return digits;
        if (digits.StartsWith("0")) digits = "62" + digits[1..];
        return digits;
    }

    public async Task<bool> SendOtpAsync(string phoneRaw, string code, string? brandOverride = null, CancellationToken ct = default)
    {
        var to = Normalize62(phoneRaw);
        //var brand = brandOverride;
        //if (string.IsNullOrWhiteSpace(brand))
        //{
        //    var info = await BrandService.GetCurrentAsync();
        //    brand = info.WaClientId; // ← header X-Client-ID
        //}

        using var req = new HttpRequestMessage(HttpMethod.Post, "/api/otp/send");
        //req.Headers.Remove("X-Client-ID");
        //req.Headers.Add("X-Client-ID", brand!);

        // body WA API
        var body = new { to, text = $"Kode OTP kamu {code}" };
        req.Content = JsonContent.Create(body);

        var res = await _http.SendAsync(req, ct);
        return res.IsSuccessStatusCode;
    }
}