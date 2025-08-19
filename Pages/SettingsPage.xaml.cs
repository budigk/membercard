using MemberCard.Services;

namespace MemberCard.Pages;

public partial class SettingsPage : ContentPage
{
    readonly BrandingService _branding;
    readonly ApiServices _api;

    public SettingsPage(BrandingService b, ApiServices api)
    {
        InitializeComponent();          // WAJIB
        _branding = b;
        _api = api;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApiEntry.Text = _api.BaseUrl;    // <-- ini sekarang dikenal
    }

    async void OnSave(object sender, EventArgs e)
    {
        _api.BaseUrl = ApiEntry.Text?.Trim();
        if (App.Brand is not null) _branding.ApplyToResources(App.Brand);
        await DisplayAlert("Saved", "Settings updated", "OK");
    }
}
