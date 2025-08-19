using MemberCard.Services;
using MemberCard.Models;
using Microsoft.Maui.Storage;

namespace MemberCard.Pages;

public partial class HistoryPage : ContentPage
{
    private readonly ApiServices _api;

    public HistoryPage(ApiServices api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async void OnRefresh(object? sender, EventArgs e)
    {
        try { await LoadAsync(); }
        finally { HistoryRefresh.IsRefreshing = false; }
    }

    private async Task LoadAsync()
    {
        try
        {
            // kode & tanggal default di ApiServices (kode dari Preferences, tanggal = today-365)
            var items = await _api.GetHistoryAsync();
            List.ItemsSource = items;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            await DisplayAlert("Gagal memuat", "Tidak dapat memuat riwayat.", "OK");
            List.ItemsSource = Array.Empty<TransactionItem>();
        }
    }
}