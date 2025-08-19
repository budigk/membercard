using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using AndroidX.Core.View;          // untuk WindowInsetsControllerCompat
using AColor = Android.Graphics.Color;  // << alias biar tidak bentrok

namespace MemberCard
{
    [Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
                           ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density,
    ScreenOrientation = ScreenOrientation.Portrait  // <— kunci portrait
)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Samakan warna status bar dengan background (putih)
            Window?.SetStatusBarColor(AColor.Rgb(255, 255, 255));

            // Icon status bar gelap (supaya kontras di background putih)
            if (Window != null)
            {
                var controller = new WindowInsetsControllerCompat(Window, Window.DecorView);
                controller.AppearanceLightStatusBars = true;
            }
        }
    }
}
