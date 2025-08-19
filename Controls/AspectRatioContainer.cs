namespace MemberCard.Controls;

public class AspectRatioContainer : ContentView
{
    // Height = Width * Ratio
    public static readonly BindableProperty RatioProperty =
        BindableProperty.Create(nameof(Ratio), typeof(double), typeof(AspectRatioContainer), 0.628125);

    // Batasi lebar maksimum di layar lebar (untuk efek letterbox kiri/kanan)
    public static readonly BindableProperty MaxWidthProperty =
        BindableProperty.Create(nameof(MaxWidth), typeof(double), typeof(AspectRatioContainer), double.PositiveInfinity);

    public double Ratio
    {
        get => (double)GetValue(RatioProperty);
        set => SetValue(RatioProperty, value);
    }

    public double MaxWidth
    {
        get => (double)GetValue(MaxWidthProperty);
        set => SetValue(MaxWidthProperty, value);
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        // Limitasi lebar jika lebih besar dari MaxWidth
        if (MaxWidth > 0 && double.IsFinite(MaxWidth) && width > MaxWidth)
        {
            width = MaxWidth;
            WidthRequest = MaxWidth;
            HorizontalOptions = LayoutOptions.Center;
        }

        if (width > 0 && Ratio > 0)
            HeightRequest = width * Ratio;
    }
}
