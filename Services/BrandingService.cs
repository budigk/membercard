using System.Text.Json;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using MemberCard.Models;

namespace MemberCard.Services;

public sealed class BrandingService
{
    public BrandConfig Config { get; private set; } = new();
    private const string BrandFile = "brand.json"; // Resources/Raw/brand.json (MauiAsset)

    public async Task<BrandConfig> LoadAsync()
    {
        using var s = await FileSystem.OpenAppPackageFileAsync(BrandFile);
        using var r = new StreamReader(s);
        var json = await r.ReadToEndAsync();

        Config = JsonSerializer.Deserialize<BrandConfig>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new BrandConfig();

        ApplyToResources(Config);
        return Config;
    }

    public void ApplyToResources(BrandConfig b)
    {
        var res = Application.Current!.Resources;

        // Theme (DynamicResource)
        res["ColorPrimary"] = Color.FromArgb(b.Theme.Primary);
        res["ColorSecondary"] = Color.FromArgb(b.Theme.Secondary);
        res["ColorAccent"] = Color.FromArgb(b.Theme.Accent);
        res["ColorBackground"] = Color.FromArgb(b.Theme.Background);
        res["ColorText"] = Color.FromArgb(b.Theme.Text);
        res["ColorCardBackground"] = Color.FromArgb(b.Theme.CardBackground);

        // Assets (DynamicResource: string filename → FileImageSource)
        res["LogoImage"] = b.Assets.Logo;
        res["CardImage"] = b.Assets.Card;

        // App title untuk dipakai di header dalam app (OS label tetap via csproj)
        res["AppTitle"] = b.AppTitle;
    }
}
