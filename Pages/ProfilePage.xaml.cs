using MemberCard.Services;

namespace MemberCard.Pages;

public partial class ProfilePage : ContentPage
{
    readonly ApiServices _api;
    public ProfilePage(ApiServices api) { InitializeComponent(); _api = api; Loaded += ProfilePage_Loaded; }

    async void ProfilePage_Loaded(object? s, EventArgs e)
    {
        //try
        //{
        //    //var m = await _api.GetMemberAsync();
        //    if (m != null) { NameEntry.Text = m.Name; CardEntry.Text = m.CardNumber; }
        //}
        //catch { }
    }

    async void OnSave(object sender, EventArgs e) => await DisplayAlert("Saved", "Profile saved (mock)", "OK");

    async void OnLogout(object sender, EventArgs e)
    {
        Preferences.Remove("AccessToken");
        await Shell.Current.GoToAsync("/login");
    }
}