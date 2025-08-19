using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MemberCard.Models;
// SESUAIKAN namespace ApiServices kamu:
using MemberCard.Services; // <-- ganti jika berbeda

namespace MemberCard.Pages;

public partial class OutletsPage : ContentPage, INotifyPropertyChanged
{
    private bool _isLoading;
    private bool _showEmptyMessage;
    private bool _hasLoadedOnce;
    readonly ApiServices _api;

    public ObservableCollection<Outlet> Items { get; } = new();

    public bool IsLoading
    {
        get => _isLoading;
        set { if (_isLoading != value) { _isLoading = value; OnPropertyChanged(); } }
    }
    public bool ShowEmptyMessage
    {
        get => _showEmptyMessage;
        set { if (_showEmptyMessage != value) { _showEmptyMessage = value; OnPropertyChanged(); } }
    }

    public OutletsPage(ApiServices api)
    {
        InitializeComponent();
        BindingContext = this;

        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_hasLoadedOnce) return;  // first time only
        _hasLoadedOnce = true;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        ShowEmptyMessage = false;
        try
        {
            // >>> PAKAI APISERVICES, BUKAN HARDCODE URL
            var list = await _api.GetOutletsAsync();

            Items.Clear();
            foreach (var it in list) Items.Add(it);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsLoading = false;
            ShowEmptyMessage = Items.Count == 0;
            OutletRefresh.IsRefreshing = false;
        }
    }

    private async void OnRefresh(object sender, EventArgs e)
    {
        await LoadAsync();
    }

    private async void OnWhatsAppClicked(object sender, EventArgs e)
    {
        if (sender is not Element el || el.BindingContext is not Outlet it) return;
        var url = BuildWhatsAppUrl(it.Whatsapp);
        if (url != null) await Launcher.OpenAsync(url);
    }

    private async void OnMapClicked(object sender, EventArgs e)
    {
        if (sender is not Element el || el.BindingContext is not Outlet it) return;
        var url = BuildMapsUrl(it.GeoCode, it.ReverseGeoCode ?? it.Nama);
        if (url != null) await Launcher.OpenAsync(url);
    }

    private async void OnItemTapped(object? sender, TappedEventArgs e)
    {
        if (sender is Element el && el.BindingContext is Outlet it)
        {
            await Shell.Current.GoToAsync(nameof(OutletDetailPage), new Dictionary<string, object>
            {
                ["Outlet"] = it
            });
        }
    }

    // Helpers (tetap sama)
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

    // INotifyPropertyChanged
    public new event PropertyChangedEventHandler? PropertyChanged;
    protected new void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
