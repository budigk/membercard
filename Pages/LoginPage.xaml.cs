using MemberCard.Services;
using Microsoft.Maui.ApplicationModel; // untuk AppInfo

namespace MemberCard.Pages;

public partial class LoginPage : ContentPage
{
    readonly ApiServices _api;
    public LoginPage(ApiServices api)
    {
        InitializeComponent();
        _api = api;
    }

    async void OnLoginWithCard(object sender, EventArgs e)
    {
        var sp = Application.Current?.Handler?.MauiContext?.Services;
        var scanPage = (ScanCardPage?)(sp?.GetService(typeof(ScanCardPage))) ?? new ScanCardPage();
        await Shell.Current.Navigation.PushAsync(scanPage);
    }


    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Set label versi dinamis dari metadata aplikasi
        if (VersionText != null)
            VersionText.Text = $"Versi {AppInfo.Current.VersionString}";
    }

    async void OnRegister(object sender, EventArgs e) => await Shell.Current.GoToAsync("register");

    async void OnSkip(object sender, EventArgs e) => await Shell.Current.GoToAsync("//home");

    async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            var email = EmailEntry.Text?.Trim();
            var pass = PasswordEntry.Text;
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(pass))
            {
                await DisplayAlert("Error", "Email & password required", "OK");
                return;
            }
            var res = await _api.LoginAsync(email, pass);
            if (res is null)
            {
                await DisplayAlert("Login failed", "Invalid credentials", "OK");
                return;
            }
            Preferences.Set("AccessToken", res.AccessToken);
            await Shell.Current.GoToAsync("//home");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    async void OnDesignerTapped(object sender, EventArgs e)
    {
        try { await Launcher.OpenAsync("http://affariretail.com"); }
        catch { await DisplayAlert("Info", "Tidak dapat membuka tautan.", "OK"); }
    }
}