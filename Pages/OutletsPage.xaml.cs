using MemberCard.Models;
using MemberCard.Services;

namespace MemberCard.Pages;

public partial class OutletsPage : ContentPage
{
    readonly ApiServices _api;
    public OutletsPage(ApiServices api) { InitializeComponent(); _api = api; Loaded += OutletsPage_Loaded; }

    async void OutletsPage_Loaded(object? s, EventArgs e) { try { List.ItemsSource = await _api.GetOutletsAsync(); } catch { } }

    async void OnMap(object sender, EventArgs e)
    {
        if ((sender as Button)?.BindingContext is Outlet o)
        {
            var url = $"https://www.google.com/maps/search/?api=1&query={o.Latitude},{o.Longitude}";
            await Launcher.OpenAsync(url);
        }
    }
}