using System.Text.Json;

namespace MemberCard.Services;

public class BrandingService
{
    const string BrandFile = "brand.json"; // placed in Resources/Raw

    public async Task<BrandConfig> LoadAsync()
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync(BrandFile);
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        var brand = JsonSerializer.Deserialize<BrandConfig>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new BrandConfig();

        ApplyToResources(brand);
        Application.Current!.Resources["AppName"] = brand.AppName;
        return brand;
    }

    public void ApplyToResources(BrandConfig b)
    {
        var res = Application.Current!.Resources;
        res["PrimaryColor"] = Color.FromArgb(b.Primary);
        res["SecondaryColor"] = Color.FromArgb(b.Secondary);
        res["AccentColor"] = Color.FromArgb(b.Accent);
        res["BackgroundColor"] = Color.FromArgb(b.Background);
        res["TextColor"] = Color.FromArgb(b.Text);
        res["CardBackgroundColor"] = Color.FromArgb(b.CardBackground);
    }
}