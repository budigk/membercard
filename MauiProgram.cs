using MemberCard.Pages;
using MemberCard.Services;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace MemberCard;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>()
            .UseBarcodeReader()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
        
        builder.Services.AddSingleton<BrandingService>();
        builder.Services.AddSingleton<ApiServices>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<HistoryPage>();
        builder.Services.AddTransient<OutletsPage>();
        builder.Services.AddTransient<ProfilePage>();

        return builder.Build();
    }
}