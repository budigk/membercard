using MemberCard.Models;
using Microsoft.Maui.ApplicationModel.Communication;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MemberCard.Services;

// MOCK implementation — no internet/backend required
public class ApiServices
{
    private readonly HttpClient _http = new();
    private readonly BrandingService _brand;
    
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private sealed class OtpSendEnvelope
    {
        public bool IsSuccess { get; set; }
        public OtpSendMessage? Message { get; set; }
        public string? StatusCode { get; set; }
    }
    private sealed class OtpSendMessage
    {
        public string? message { get; set; }
        public string? OTP { get; set; }
    }

    //public string? BaseUrl
    //{
    //    get => Preferences.Get("APIServer", null);
    //    set => Preferences.Set("APIServer", value);
    //}

    private const string PATH_LOGIN = "/api/auth/login";
    private const string PATH_HISTORY = "/api/listhistoripoint";
    private const string PATH_OUTLET = "/api/outlet";
    public const string PATH_USER_QUERY = "/api/users";            // GET ?email=...
    public const string PATH_USER_UPSERT = "/api/users";            // POST { email, emailConfirmed }
    public const string PATH_MEMBER = "/api/member";           // POST Member
    public const string PATH_OTP_EMAIL_SEND = "/api/otp/email/send"; // <- fixed

    public ApiServices(BrandingService brand) => _brand = brand;
    private string BaseUrl => _brand.Config.ApiBaseUrl.TrimEnd('/');

    async Task<T> DelayReturn<T>(T value)
    {
        await Task.Delay(250);
        return value;
    }

    static string Normalize62(string phoneRaw)
    {
        var digits = new string((phoneRaw ?? "").Where(char.IsDigit).ToArray());
        if (string.IsNullOrEmpty(digits)) return digits;
        if (digits.StartsWith("0")) digits = "62" + digits[1..];
        return digits;
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        var member = new Member
        {
            //MemberId = "M0001",
            //Name = string.IsNullOrWhiteSpace(email) ? "Demo User" : email.Split('@')[0],
            //CardNumber = "1234 5678 9012",
            //Points = 12345
        };
        return await DelayReturn(new LoginResponse { AccessToken = "mock-token", Member = member });
    }

    public Task<bool> RegisterAsync(RegisterRequest req)
        => DelayReturn(true);

    public async Task<LoginResponse?> VerifyMemberCardAsync(string scannedValue)
    {
        await Task.Delay(300); // simulasi network
                               // DEMO: nilai yang diawali "AFF-" dianggap valid
        if (!string.IsNullOrWhiteSpace(scannedValue) && scannedValue.StartsWith("888", StringComparison.OrdinalIgnoreCase))
        {
            return new LoginResponse
            {
                AccessToken = "mock-token",
                //Member = new Member { MemberId = "M-" + scannedValue, Name = "Member Card", CardNumber = scannedValue, Points = 12345 }
            };
        }
        return null;
    }

    public Task<List<SlidePhoto>> GetSlidesAsync()
        => DelayReturn(new List<SlidePhoto>
        {
            new() { ImageUrl = "https://picsum.photos/seed/slide1/800/400", Caption = "Promo Sepeda Listrik 10%" },
            new() { ImageUrl = "https://picsum.photos/seed/slide2/800/400", Caption = "Voucher Free Service" },
            new() { ImageUrl = "https://picsum.photos/seed/slide3/800/400", Caption = "Membership Double Points" }
        });

    public async Task<IReadOnlyList<TransactionItem>> GetHistoryAsync(string kode = "", string tanggal = "" , string? endpointOverride = null)
    {
        if (string.IsNullOrWhiteSpace(kode))
            kode = Preferences.Get("MemberKode", string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(kode))
        {
            // kalau belum ada kode member tersimpan, kembalikan list kosong
            return Array.Empty<TransactionItem>();
        }

        // 2) Tanggal default = hari ini mundur 365 hari (format yyyy-MM-dd)
        if (string.IsNullOrWhiteSpace(tanggal))
        {
            var start = DateTime.Now.Date.AddDays(-180);   // Asia/Jakarta sesuai system clock
            tanggal = start.ToString("yyyy-MM-dd");
        }

        var path = string.IsNullOrWhiteSpace(endpointOverride) ? PATH_HISTORY : endpointOverride;
        var url = $"{BaseUrl}{path}?kode={kode}&tanggal={tanggal}";
        System.Diagnostics.Debug.WriteLine($"[GET] {url}");

        using var resp = await _http.GetAsync(url);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync();

        try
        {
            var wrap = JsonSerializer.Deserialize<HistoryWrapper>(json, JsonOpts);
            if (wrap?.data != null) return wrap.data;
        }
        catch { /* fallback ke array murni */ }

        try
        {
            return JsonSerializer.Deserialize<List<TransactionItem>>(json, JsonOpts)
                    ?? new List<TransactionItem>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ParseError] {ex.Message}\n{json}");
            throw; // biar ketahuan kalau format benar2 tak cocok
        }
    }

    public async Task<List<Outlet>> GetOutletsAsync(CancellationToken ct = default)
    {
        // gabung BaseUrl + PATH_OUTLET (BaseUrl sudah TrimEnd('/'))
        var url = $"{BaseUrl}{PATH_OUTLET}";
        System.Diagnostics.Debug.WriteLine($"[GET] {url}");

        using var resp = await _http.GetAsync(url, ct);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync(ct);

        // Pakai helper generic: coba wrapper { data: [...] } → fallback array murni
        return DeserializeMaybeWrapped<List<Outlet>>(json) ?? new List<Outlet>();
    }

    public Task<bool> RedeemAsync(string voucherCode)
        => DelayReturn(!string.IsNullOrWhiteSpace(voucherCode));

    private sealed class HistoryWrapper
    {
        public List<TransactionItem>? data { get; set; }
        public int total { get; set; }
    }

    public static T? DeserializeMaybeWrapped<T>(string json) where T : class
    {
        try
        {
            var w = JsonSerializer.Deserialize<ApiWrapper<T>>(json, JsonOpts);
            if (w?.data is not null) return w.data;
        }
        catch { /* ignore */ }

        return JsonSerializer.Deserialize<T>(json, JsonOpts);
    }

    public async Task<bool> CheckUserExistsAsync(string email, CancellationToken ct = default)
    {
        var url = $"{BaseUrl}/api/member/?field=email&value={Uri.EscapeDataString(email)}";
        using var res = await _http.GetAsync(url, ct).ConfigureAwait(false);
        if (!res.IsSuccessStatusCode) return false;
        
        var json = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(json)) return false;

        try
        {
            var arr = JsonSerializer.Deserialize<object[]>(json, JsonOpts);
            if (arr is { Length: > 0 }) return true;
        }
        catch { /* ignore */ }

        return false;
    }

    // Kirim OTP: return (ok, otp, serverMessage)
    public async Task<(bool ok, string? otp, string? serverMsg)> RequestEmailOtpAsync(string email, string tipe, CancellationToken ct = default)
    {
        var url = $"{BaseUrl}{PATH_OTP_EMAIL_SEND}?email={Uri.EscapeDataString(email)}&tipe={Uri.EscapeDataString(tipe)}";
        using var res = await _http.PostAsync(url, content: null, ct).ConfigureAwait(false);

        // Sukses berdasarkan HTTP 200
        if (!res.IsSuccessStatusCode)
        {
            var err = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            err = Regex.Match(err ?? "", "\"Message\"\\s*:\\s*\"([^\"]+)\"").Groups[1].Value ?? err;
            return (false, null, string.IsNullOrWhiteSpace(err) ? $"HTTP {(int)res.StatusCode}" : err);
        }

        // Parse body
        var json = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        try
        {
            var env = JsonSerializer.Deserialize<OtpSendEnvelope>(json, JsonOpts);
            if (env?.IsSuccess == true && env.Message?.OTP is { Length: > 0 })
            {
                return (true, env.Message.OTP, env.Message.message ?? "OTP terkirim");
            }
            return (false, null, env?.Message?.message ?? "Gagal mengirim OTP");
        }
        catch
        {
            // fallback: kalau tiba-tiba bukan shape di atas
            return (true, null, "OTP terkirim");
        }
    }

    public async Task<bool> CreateOrUpdateUserAsync(UserUpsertRequest model, CancellationToken ct = default)
    {
        var url = $"{BaseUrl}{PATH_USER_UPSERT}";
        using var res = await _http.PostAsJsonAsync(url, model, ct).ConfigureAwait(false);
        return res.IsSuccessStatusCode;
    }

    public async Task<(bool ok, string? message)> CreateMemberAsync(Member model, CancellationToken ct = default)
    {
        var url = $"{BaseUrl.TrimEnd('/')}{PATH_MEMBER}";
        try
        {
            using var res = await _http.PostAsJsonAsync(url, model, ct).ConfigureAwait(false);

            if (res.IsSuccessStatusCode)
                return (true, null);

            // coba ambil error message dari body (kalau ada)
            var body = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            var msg = string.IsNullOrWhiteSpace(body)
                ? $"HTTP {(int)res.StatusCode} {res.ReasonPhrase}"
                : body;

            return (false, msg);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return (false, "Request dibatalkan.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }


    public async Task<Member> GetMemberAsync(string sField, string sValue, CancellationToken ct = default)
    {
        // gabung BaseUrl + PATH_OUTLET (BaseUrl sudah TrimEnd('/'))
        var url = $"{BaseUrl}{PATH_MEMBER}?field={sField}&value={sValue}";
        System.Diagnostics.Debug.WriteLine($"[GET] {url}");

        using var resp = await _http.GetAsync(url, ct);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync(ct);

        // Pakai helper generic: coba wrapper { data: [...] } → fallback array murni
        return DeserializeMaybeWrapped<Member>(json) ?? new Member();
        //return DeserializeMaybeWrapped<List<Member>>(json) ?? new List<Member>();


        //var url = $"{BaseUrl}{PATH_MEMBER}?ponsel={sField}&value={sValue}";
        //using var res = await _http.GetAsync(url, ct);
        //res.EnsureSuccessStatusCode();
        //var json = await res.Content.ReadAsStringAsync(ct);

        //try
        //{
        //    using var doc = JsonDocument.Parse(json);
        //    var root = doc.RootElement;

        //    string? Pick(params string[] keys)
        //    {
        //        foreach (var k in keys)
        //        {
        //            if (root.TryGetProperty(k, out var v) && v.ValueKind != JsonValueKind.Null)
        //                return v.ToString();
        //            foreach (var p2 in root.EnumerateObject())
        //                if (string.Equals(p2.Name, k, StringComparison.OrdinalIgnoreCase))
        //                    return p2.Value.ToString();
        //        }
        //        return null;
        //    }

        //    var kode = Pick("KodeMember", "kode", "Kode");
        //    var nama = Pick("NamaMember", "nama", "Nama");
        //    var valid = Pick("ValidThru", "TglBerakhir", "valid_thru");
        //    var point = Pick("PointAkhir", "pointAkhir", "Point");

        //    if (!string.IsNullOrEmpty(kode)) Preferences.Set("KodeMember", kode);
        //    if (!string.IsNullOrEmpty(nama)) Preferences.Set("NamaMember", nama);
        //    if (!string.IsNullOrEmpty(valid)) Preferences.Set("ValidThru", valid);
        //    if (!string.IsNullOrEmpty(point)) Preferences.Set("PointAkhir", point);

        //    return DeserializeMaybeWrapped<Member>(json) ?? new Member();
        //}
        //catch { return DeserializeMaybeWrapped<Member>(json) ?? new Member(); }
    }
}