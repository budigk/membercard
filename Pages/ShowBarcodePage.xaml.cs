using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using ZXing.Net.Maui; // untuk BarcodeFormat enum

namespace MemberCard.Pages;

[QueryProperty(nameof(CardNo), "CardNo")]
[QueryProperty(nameof(MemberName), "MemberName")]
public partial class ShowBarcodePage : ContentPage
{
    private string _cardNo = "000000";
    private string _memberName = "Member Name";
    private bool _isQr = false;
    private bool _animating = false;

    public string CardNo
    {
        get => _cardNo;
        set
        {
            _cardNo = string.IsNullOrWhiteSpace(value) ? "000000" : value;
            ApplyValue();
        }
    }

    public string MemberName
    {
        get => _memberName;
        set
        {
            _memberName = string.IsNullOrWhiteSpace(value) ? "Member Name" : value;
            ApplyValue();
        }
    }

    public ShowBarcodePage()
    {
        InitializeComponent();

        // Set posisi awal segThumb; update saat layout segmented berubah
        Loaded += (s, e) => { PositionThumb(animated: false).ConfigureAwait(false); UpdateSegmentTextColors(); };
        segmentedGrid.SizeChanged += (s, e) => { PositionThumb(animated: false).ConfigureAwait(false); };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApplyValue();
        UpdateSegmentTextColors();
    }

    private void ApplyValue()
    {
        if (codeView == null || lblCardNo == null || lblMemberName == null) return;

        codeView.Value = _cardNo;
        lblCardNo.Text = $"Card No: {_cardNo}";
        lblMemberName.Text = _memberName;
    }

    private async void OnSelectBarcode(object sender, EventArgs e)
    {
        if (_animating || !_isQr) return;
        _isQr = false;
        await SwitchModeAsync(toQr: false);
        await PositionThumb(animated: true);
        UpdateSegmentTextColors();
    }

    private async void OnSelectQr(object sender, EventArgs e)
    {
        if (_animating || _isQr) return;
        _isQr = true;
        await SwitchModeAsync(toQr: true);
        await PositionThumb(animated: true);
        UpdateSegmentTextColors();
    }

    // Animasi: fade-out + scale, ganti format & ukuran, fade-in
    private async Task SwitchModeAsync(bool toQr)
    {
        if (codeView == null) return;

        _animating = true;

        // Fade out sedikit
        await Task.WhenAll(
            codeView.FadeTo(0, 140, Easing.CubicOut),
            codeView.ScaleTo(0.96, 140, Easing.CubicOut)
        );

        // Ganti format & ukuran
        if (toQr)
        {
            codeView.Format = BarcodeFormat.QrCode;
            codeView.WidthRequest = 220;
            codeView.HeightRequest = 220;
        }
        else
        {
            codeView.Format = BarcodeFormat.Code128;
            codeView.WidthRequest = 350;
            codeView.HeightRequest = 130;
        }

        // Pastikan value tetap sama
        codeView.Value = _cardNo;

        // Fade in
        await Task.WhenAll(
            codeView.FadeTo(1, 200, Easing.CubicIn),
            codeView.ScaleTo(1.0, 200, Easing.CubicOut)
        );

        _animating = false;
    }

    private async Task PositionThumb(bool animated)
    {
        if (segThumb == null || segmentedGrid == null) return;

        var totalWidth = segmentedGrid.Width;
        if (totalWidth <= 0)
        {
            Grid.SetColumn(segThumb, 0);
            segThumb.TranslationX = 0;
            return;
        }

        int current = Grid.GetColumn(segThumb);
        int target = _isQr ? 1 : 0;
        if (current == target)
        {
            segThumb.TranslationX = 0;
            return;
        }

        double colWidth = totalWidth / 2.0;
        if (animated)
        {
            await segThumb.TranslateTo((target - current) * colWidth, 0, 180, Easing.CubicInOut);
        }

        Grid.SetColumn(segThumb, target);
        segThumb.TranslationX = 0;
    }

    private void UpdateSegmentTextColors()
    {
        if (btnSegBarcode == null || btnSegQr == null) return;

        // Teks segment terpilih -> putih; lainnya -> default
        var selectedColor = Colors.White;
        var unselectedLight = Color.FromArgb("#111827");
        var unselectedDark = Color.FromArgb("#FFFFFF");

        if (_isQr)
        {
            btnSegBarcode.TextColor = Application.Current!.RequestedTheme == AppTheme.Dark ? unselectedDark : unselectedLight;
            btnSegQr.TextColor = selectedColor;
        }
        else
        {
            btnSegBarcode.TextColor = selectedColor;
            btnSegQr.TextColor = Application.Current!.RequestedTheme == AppTheme.Dark ? unselectedDark : unselectedLight;
        }
    }
}
