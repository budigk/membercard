using MemberCard.Pages;

namespace MemberCard;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("login", typeof(LoginPage));
        Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        Routing.RegisterRoute("scan", typeof(ScanCardPage));
        Routing.RegisterRoute(nameof(ShowBarcodePage), typeof(ShowBarcodePage));
        Routing.RegisterRoute(nameof(OutletDetailPage), typeof(MemberCard.Pages.OutletDetailPage));
    }
}
