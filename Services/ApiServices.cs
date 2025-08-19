using MemberCard.Models;
using System.Net.Http.Json;
using System.Text.Json;

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


    //public string? BaseUrl
    //{
    //    get => Preferences.Get("APIServer", null);
    //    set => Preferences.Set("APIServer", value);
    //}

    private const string PATH_LOGIN = "/api/auth/login";
    private const string PATH_HISTORY = "/api/listhistoripoint";

    public ApiServices(BrandingService brand) => _brand = brand;
    private string BaseUrl => _brand.Config.ApiBaseUrl.TrimEnd('/');

    async Task<T> DelayReturn<T>(T value)
    {
        await Task.Delay(250);
        return value;
    }

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        var member = new Member
        {
            MemberId = "M0001",
            Name = string.IsNullOrWhiteSpace(email) ? "Demo User" : email.Split('@')[0],
            CardNumber = "1234 5678 9012",
            Points = 12345
        };
        return await DelayReturn(new LoginResponse { AccessToken = "mock-token", Member = member });
    }

    public Task<bool> RegisterAsync(RegisterRequest req)
        => DelayReturn(true);

    public Task<Member?> GetMemberAsync()
        => DelayReturn(new Member { MemberId = "M0001", Name = "Demo User", CardNumber = "1234 5678 9012", Points = 12345 });

    public async Task<LoginResponse?> VerifyMemberCardAsync(string scannedValue)
    {
        await Task.Delay(300); // simulasi network
                               // DEMO: nilai yang diawali "AFF-" dianggap valid
        if (!string.IsNullOrWhiteSpace(scannedValue) && scannedValue.StartsWith("888", StringComparison.OrdinalIgnoreCase))
        {
            return new LoginResponse
            {
                AccessToken = "mock-token",
                Member = new Member { MemberId = "M-" + scannedValue, Name = "Member Card", CardNumber = scannedValue, Points = 12345 }
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
            kode = Preferences.Get("KodeMember", string.Empty).Trim();

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

    public Task<List<Outlet>> GetOutletsAsync()
        => DelayReturn(new List<Outlet>
        {
            new() { Name = "Outlet City Garden", Address = "Jl. Example 1, Jakarta", PhotoUrl = "https://picsum.photos/seed/out1/160/160", Latitude = -6.2, Longitude = 106.8 },
            new() { Name = "Outlet Volendam", Address = "Jl. Example 2, Tangerang", PhotoUrl = "https://picsum.photos/seed/out2/160/160", Latitude = -6.24, Longitude = 106.62 },
            new() { Name = "Outlet Giethoorn", Address = "Jl. Example 3, Bekasi", PhotoUrl = "https://picsum.photos/seed/out3/160/160", Latitude = -6.3, Longitude = 106.9 }
        });

    public Task<bool> RedeemAsync(string voucherCode)
        => DelayReturn(!string.IsNullOrWhiteSpace(voucherCode));

    private sealed class HistoryWrapper
    {
        public List<TransactionItem>? data { get; set; }
        public int total { get; set; }
    }
}