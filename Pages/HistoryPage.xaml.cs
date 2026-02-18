using MemberCard.Models;
using MemberCard.Services;
using Microsoft.Maui.Storage;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MemberCard.Pages;

public partial class HistoryPage : ContentPage
{
    private readonly ApiServices _api;
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    public HistoryPage(ApiServices api)
    {
        InitializeComponent();
        _api = api;
        BindingContext = this;
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
        //try
        //{
        //    // kode & tanggal default di ApiServices (kode dari Preferences, tanggal = today-365)
        //    var items = await _api.GetHistoryAsync();
        //    List.ItemsSource = items;
        //}
        //catch (Exception ex)
        //{
        //    System.Diagnostics.Debug.WriteLine(ex);
        //    await DisplayAlert("Gagal memuat", "Tidak dapat memuat riwayat.", "OK");
        //    List.ItemsSource = Array.Empty<TransactionItem>();
        //}
        try
        {
            IsLoading = true;

            var items = await _api.GetHistoryAsync();
            List.ItemsSource = items ?? Array.Empty<TransactionItem>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            await DisplayAlert("Gagal memuat", "Tidak dapat memuat riwayat.", "OK");
            List.ItemsSource = Array.Empty<TransactionItem>();
        }
        finally
        {
            IsLoading = false;
            if (HistoryRefresh.IsRefreshing)
                HistoryRefresh.IsRefreshing = false;
        }
    }

    // INotifyPropertyChanged
    public new event PropertyChangedEventHandler? PropertyChanged;
    protected new void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}