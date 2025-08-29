// Helpers/Ui.cs
namespace MemberCard.Helpers;
public static class Ui
{
    public static Task Alert(string title, string message, string ok = "OK")
    {
        var page = Application.Current?.MainPage;
        if (page != null) return page.DisplayAlert(title, message, ok);
        return Task.CompletedTask; // fallback diam jika belum ada MainPage
    }
}
