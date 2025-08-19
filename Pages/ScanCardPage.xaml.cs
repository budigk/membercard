using MemberCard.Services;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Extensions.DependencyInjection;

namespace MemberCard.Pages;

public partial class ScanCardPage : ContentPage
{
    private ApiServices? _api;
    private CameraBarcodeReaderView _cameraView = null!;
    private bool _isHandling;

    // UI helpers
    private Grid _root = null!;
    private ActivityIndicator _spinner = null!;
    private Label _toast = null!;

    public ScanCardPage()
    {
        InitializeComponent();
        BuildUi();
    }

    public ScanCardPage(ApiServices api) : this()
    {
        _api = api;
    }

    private void BuildUi()
    {
        _cameraView = new CameraBarcodeReaderView
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
            IsDetecting = false, // aktifkan nanti di OnAppearing
            Options = new BarcodeReaderOptions
            {
                AutoRotate = true,
                TryHarder = true,
                Multiple = false
            },
            CameraLocation = CameraLocation.Rear
        };
        _cameraView.BarcodesDetected += OnBarcodesDetected;

        // Reticle (kotak)
        var reticle = new Border
        {
            Stroke = Colors.White,
            StrokeThickness = 3,
            Background = Colors.Transparent,
            WidthRequest = 260,
            HeightRequest = 260,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Shadow = new Shadow { Brush = Brush.Black, Opacity = 0.3f, Offset = new Point(0, 2), Radius = 8 }
        };
        var tap = new TapGestureRecognizer();
        tap.Tapped += (s, e) => { try { _cameraView.AutoFocus(); } catch { } };
        reticle.GestureRecognizers.Add(tap);

        // Icon senter di bawah kotak
        var torchBtn = new ImageButton
        {
            Source = "flash.png", // Resources/Images/flash.png (MauiImage)
            WidthRequest = 40,
            HeightRequest = 40,
            BackgroundColor = Colors.Transparent,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
        torchBtn.Clicked += (s, e) => { try { _cameraView.IsTorchOn = !_cameraView.IsTorchOn; } catch { } };

        var centerStack = new VerticalStackLayout
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Spacing = 10,
            Children = { reticle, torchBtn }
        };

        // Spinner tengah
        _spinner = new ActivityIndicator
        {
            IsVisible = false,
            IsRunning = false,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        // Toast bawah (non-blocking)
        _toast = new Label
        {
            TextColor = Colors.White,
            BackgroundColor = Color.FromArgb("#B3000000"),
            Padding = new Thickness(12, 8),
            Opacity = 0,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.End,
            Margin = new Thickness(0, 0, 0, 40)
        };

        _root = new Grid();
        _root.Children.Add(_cameraView);    // layer 0
        _root.Children.Add(centerStack);    // layer 1
        _root.Children.Add(_spinner);       // layer 2
        _root.Children.Add(_toast);         // layer 3
        Content = _root;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _api ??= ResolveApi();

        var status = await Permissions.RequestAsync<Permissions.Camera>();
        _isHandling = false;

        if (status == PermissionStatus.Granted)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                _cameraView.IsDetecting = true;
            });
            try { _cameraView.AutoFocus(); } catch { }
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _cameraView.IsDetecting = false;
    }

    async void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        var value = e.Results?.FirstOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(value) || _isHandling) return;
        await HandleScannedAsync(value);
    }

    private async Task HandleScannedAsync(string value)
    {
        _isHandling = true;

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            _cameraView.IsDetecting = false;
            SetBusy(true);
        });

        try
        {
            try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(50)); } catch { }

            // panggil API di thread pool; jangan blok UI
            var api = _api ?? ResolveApi();
            var res = await api.VerifyMemberCardAsync(value).ConfigureAwait(false);

            if (res is null)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await ShowToast("Kartu tidak terdaftar / tidak valid");
                    SetBusy(false);
                    _cameraView.IsDetecting = true;
                    _isHandling = false;
                });
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                Preferences.Set("AccessToken", res.AccessToken);
                SetBusy(false);
                await Shell.Current.GoToAsync("//home");
            });
        }
        catch (Exception ex)
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await ShowToast($"Error: {ex.Message}");
                SetBusy(false);
                _cameraView.IsDetecting = true;
                _isHandling = false;
            });
        }
    }

    private void SetBusy(bool busy)
    {
        _spinner.IsVisible = busy;
        _spinner.IsRunning = busy;
        _root.InputTransparent = busy; // cegah tap saat proses
    }

    private async Task ShowToast(string message, int ms = 1500)
    {
        _toast.Text = message;
        _toast.Opacity = 0;
        await _toast.FadeTo(1, 150, Easing.CubicOut);
        await Task.Delay(ms);
        await _toast.FadeTo(0, 200, Easing.CubicIn);
    }

    private static ApiServices ResolveApi()
    {
        return App.Services.GetRequiredService<ApiServices>();
    }
}
