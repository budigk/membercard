using MemberCard.Models;
using MemberCard.Services;

namespace MemberCard.Pages;

public partial class RegisterPage : ContentPage
{
    readonly ApiServices _api;
    public RegisterPage(ApiServices api)
    {
        InitializeComponent();
        _api = api;
    }

    async void OnCreate(object sender, EventArgs e)
    {
        var ok = await _api.RegisterAsync(new RegisterRequest
        {
            Name = NameEntry.Text?.Trim() ?? string.Empty,
            Email = EmailEntry.Text?.Trim() ?? string.Empty,
            Password = PasswordEntry.Text ?? string.Empty
        });
        if (ok) { await DisplayAlert("Success", "Account created", "OK"); await Shell.Current.GoToAsync("..\\"); }
        else { await DisplayAlert("Error", "Failed creating account", "OK"); }
    }
}