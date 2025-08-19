using MemberCard.Models;

namespace MemberCard.Pages;

public partial class OutletDetailPage : ContentPage, IQueryAttributable
{
    public Outlet? Outlet { get; set; }

    public OutletDetailPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("Outlet", out var obj) && obj is Outlet item)
        {
            Outlet = item;
            OnPropertyChanged(nameof(Outlet));
        }
    }

    private void OnCallClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Outlet?.Telepon)) return;
        try { PhoneDialer.Open(Outlet.Telepon); }
        catch { /* abaikan error dialer */ }
    }

    private async void OnWhatsAppClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Outlet?.Whatsapp)) return;
        var url = BuildWhatsAppUrl(Outlet.Whatsapp);
        if (url != null) await Launcher.OpenAsync(url);
    }

    private async void OnMapClicked(object sender, EventArgs e)
    {
        var url = BuildMapsUrl(Outlet?.GeoCode, Outlet?.ReverseGeoCode ?? Outlet?.Nama);
        if (url != null) await Launcher.OpenAsync(url);
    }

    // Helpers (sama seperti di list)
    private static Uri? BuildWhatsAppUrl(string? number)
    {
        if (string.IsNullOrWhiteSpace(number)) return null;
        var n = number.Trim();
        if (n.StartsWith("+")) n = n[1..];
        if (n.StartsWith("0")) n = "62" + n[1..];
        return new Uri($"https://wa.me/{n}");
    }
    private static Uri? BuildMapsUrl(string? geoCode, string? label)
    {
        if (!string.IsNullOrWhiteSpace(geoCode))
            return new Uri($"https://www.google.com/maps?q={Uri.EscapeDataString(geoCode)}");
        if (!string.IsNullOrWhiteSpace(label))
            return new Uri($"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(label)}");
        return null;
    }
}
