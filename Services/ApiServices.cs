using MemberCard.Models;

namespace MemberCard.Services;

// MOCK implementation — no internet/backend required
public class ApiServices
{
    public string? BaseUrl
    {
        get => Preferences.Get("APIServer", null);
        set => Preferences.Set("APIServer", value);
    }

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

    public Task<List<TransactionItem>> GetHistoryAsync()
        => DelayReturn(new List<TransactionItem>
        {
            new() { Date = DateTime.Today.AddDays(-1), Description = "Belanja Toko Central", PointsDelta = +120 },
            new() { Date = DateTime.Today.AddDays(-3), Description = "Redeem Voucher #VX001", PointsDelta = -500 },
            new() { Date = DateTime.Today.AddDays(-7), Description = "Topup Promo", PointsDelta = +1000 }
        });

    public Task<List<Outlet>> GetOutletsAsync()
        => DelayReturn(new List<Outlet>
        {
            new() { Name = "Outlet City Garden", Address = "Jl. Example 1, Jakarta", PhotoUrl = "https://picsum.photos/seed/out1/160/160", Latitude = -6.2, Longitude = 106.8 },
            new() { Name = "Outlet Volendam", Address = "Jl. Example 2, Tangerang", PhotoUrl = "https://picsum.photos/seed/out2/160/160", Latitude = -6.24, Longitude = 106.62 },
            new() { Name = "Outlet Giethoorn", Address = "Jl. Example 3, Bekasi", PhotoUrl = "https://picsum.photos/seed/out3/160/160", Latitude = -6.3, Longitude = 106.9 }
        });

    public Task<bool> RedeemAsync(string voucherCode)
        => DelayReturn(!string.IsNullOrWhiteSpace(voucherCode));
}