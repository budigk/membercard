
using Microsoft.Maui.Controls;
using MemberCard.Services;

namespace MemberCard;

public partial class App : Application
{
    private readonly BrandingService _branding;
    public static IServiceProvider Services => Current!.Handler!.MauiContext!.Services!;

    //public static BrandConfig? Brand { get; private set; }

    public App(BrandingService branding)
    {
        InitializeComponent();

        _branding = branding;
        // Tampilkan Shell dulu
        MainPage = new AppShell();

        // Lanjutkan async di UI thread setelah Shell siap
        Dispatcher.Dispatch(async () =>
        {
            // (opsional) load branding tanpa blocking UI
            //Brand = await branding.LoadAsync();

            var hasToken = !string.IsNullOrEmpty(Preferences.Get("AccessToken", null));

            if (!hasToken)
            {
                // FIX: global route => pakai relative (tanpa "//")
                await Shell.Current.GoToAsync("login");
            }
            else
            {
                // Kalau mau pindah ke tab "Home" secara eksplisit,
                // pastikan AppShell punya Route="home" (lihat langkah #2).
                await Shell.Current.GoToAsync("//home");  // aman jika Route ada
                // Atau: biarkan kosong — default-nya sudah di tab pertama (Home)
            }
        });
    }

    protected override async void OnStart()
    {
        base.OnStart();
        await _branding.LoadAsync();   // baca brand.json → set warna, logo, kartu, base URL

        Preferences.Set("KodeMember", "M/AKS200200108");
    }
}
