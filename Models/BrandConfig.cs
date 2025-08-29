namespace MemberCard.Models;

public sealed class BrandConfig
{
    public string AppTitle { get; set; } = "Affari Member";
    public string ApiBaseUrl { get; set; } = "https://api.affariretail.id/irian"; // tanpa trailing slash
    public string apiWAUrl { get; set; } = "";
    public string waClientId { get; set; } = "";
    public ThemeConfig Theme { get; set; } = new();
    public AssetConfig Assets { get; set; } = new();
}

public sealed class ThemeConfig
{
    public string Primary { get; set; } = "#0B1220";
    public string Secondary { get; set; } = "#111827";
    public string Accent { get; set; } = "#2563EB";
    public string Background { get; set; } = "#FFFFFF";
    public string Text { get; set; } = "#111827";
    public string CardBackground { get; set; } = "#EEF2FF";
}

public sealed class AssetConfig
{
    // nama file yang dibundel di Resources/Images
    public string Logo { get; set; } = "logo_default.png";
    public string Card { get; set; } = "card_default.png";
}
