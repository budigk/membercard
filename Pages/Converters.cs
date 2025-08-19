using Microsoft.Maui.Controls;
using System.Globalization;

namespace MemberCard.Pages;

/// <summary>
/// Convert string tanggal "yyyy-MM-dd HH:mm:ss" -> "dd MMM yyyy, HH:mm" (id-ID)
/// </summary>
public sealed class TanggalPrettyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var s = value as string;
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        if (DateTime.TryParseExact(s, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture,
                                   DateTimeStyles.AssumeLocal, out var dt))
        {
            return dt.ToString("dd MMM yyyy, HH:mm", new CultureInfo("id-ID"));
        }
        return s; // fallback
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

/// <summary>
/// True kalau value numerik < 0 — untuk pewarnaan merah pada Poin.
/// </summary>
public sealed class IsNegativeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try
        {
            if (value is int i) return i < 0;
            if (value is long l) return l < 0;
            if (value is double d) return d < 0;
            if (value is float f) return f < 0;
            if (value is string s && double.TryParse(s, out var x)) return x < 0;
        }
        catch { }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}