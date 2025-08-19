using MemberCard.Services;

namespace MemberCard.Pages;

public partial class HistoryPage : ContentPage
{
    readonly ApiServices _api;
    public HistoryPage(ApiServices api) { InitializeComponent(); _api = api; Loaded += HistoryPage_Loaded; }
    async void HistoryPage_Loaded(object? s, EventArgs e) { try { List.ItemsSource = await _api.GetHistoryAsync(); } catch { } }
}