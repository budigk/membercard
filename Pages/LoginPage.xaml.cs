using MemberCard.Services;
using Microsoft.Maui.ApplicationModel; // AppInfo
using System.Text.RegularExpressions;
using System.Threading;

namespace MemberCard.Pages;

public partial class LoginPage : ContentPage
{
    readonly ApiServices _api;

    private string? _serverOtp;
    private bool _otpVerified;

    // Cooldown
    private CancellationTokenSource? _otpCooldownCts;
    private bool _cooldownActive;

    public LoginPage(ApiServices api)
    {
        InitializeComponent();
        _api = api;

        // initial state
        VerifyButton.IsVisible = false;
        OtpHintBox.IsVisible = false;
    }

    async void OnLoginWithCard(object sender, EventArgs e)
    {
        var sp = Application.Current?.Handler?.MauiContext?.Services;
        var scanPage = (ScanCardPage?)(sp?.GetService(typeof(ScanCardPage))) ?? new ScanCardPage();
        await Shell.Current.Navigation.PushAsync(scanPage);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        CancelOtpCooldown();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Set label versi dinamis dari metadata aplikasi
        if (VersionText != null)
            VersionText.Text = $"Versi {AppInfo.Current.VersionString}";
    }

    // ================= Helpers =================
    private static bool IsValidEmail(string email)
        => !string.IsNullOrWhiteSpace(email)
        && Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    private void ResetOtpState()
    {
        _otpVerified = false;
        _serverOtp = null;

        OtpEntry.Text = string.Empty;

        VerifyButton.IsVisible = false;
        OtpHintBox.IsVisible = false;

        CancelOtpCooldown();
    }

    private void SetOtpInfo(string text)
    {
        OtpHintBox.IsVisible = true;
        OtpHintLabel.Text = text;
    }

    // ============== Cooldown (60s) ==============
    private async Task StartOtpCooldownAsync(int seconds, CancellationToken token)
    {
        _cooldownActive = true;
        SendOtpButton.IsEnabled = false;

        try
        {
            for (int s = seconds; s >= 1; s--)
            {
                if (token.IsCancellationRequested || _otpVerified) break;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SendOtpButton.Text = $"Kirim Ulang ({s}s)";
                });

                await Task.Delay(1000, token);
            }
        }
        catch (TaskCanceledException) { /* ignore */ }

        _cooldownActive = false;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!_otpVerified)
            {
                SendOtpButton.Text = "Kirim Kode";
                SendOtpButton.IsEnabled = true;
            }
            else
            {
                SendOtpButton.IsEnabled = false;
            }
        });
    }

    private void CancelOtpCooldown()
    {
        try { _otpCooldownCts?.Cancel(); } catch { }
        _cooldownActive = false;

        if (!_otpVerified)
        {
            SendOtpButton.Text = "Kirim Kode";
            SendOtpButton.IsEnabled = true;
        }
        else
        {
            SendOtpButton.IsEnabled = false;
        }
    }

    // ================= Events =================
    void OnEmailChanged(object sender, TextChangedEventArgs e)
    {
        // kalau email diganti, reset OTP
        ResetOtpState();
    }

    private async void OnSendOtpClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim() ?? "";
        if (!IsValidEmail(email))
        {
            await DisplayAlert("Error", "Format email tidak valid.", "OK");
            return;
        }

        try
        {
            // disable tombol kirim sementara (biar nggak spam klik)
            SendOtpButton.IsEnabled = false;

            // Panggil API OTP (sama seperti RegisterPage)
            // Return: (ok, otp, serverMsg)
            var (ok, otp, serverMsg) = await _api.RequestEmailOtpAsync(email, "LOGIN");

            if (!ok)
            {
                await DisplayAlert("Gagal", serverMsg ?? "Tidak dapat mengirim OTP.", "OK");
                SendOtpButton.IsEnabled = true;
                return;
            }

            _serverOtp = otp;
            _otpVerified = false;

            // tampilkan hint + munculkan tombol verifikasi
            SetOtpInfo(serverMsg ?? "OTP terkirim. Silakan cek email Anda.");
            VerifyButton.IsVisible = true;

            // fokus input otp
            OtpEntry.Focus();

            // mulai cooldown 60 detik
            CancelOtpCooldown();
            _otpCooldownCts = new CancellationTokenSource();
            _ = StartOtpCooldownAsync(60, _otpCooldownCts.Token);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
            SendOtpButton.IsEnabled = true;
        }
    }

    private async void OnVerifyClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim() ?? "";
        var input = OtpEntry.Text?.Trim();

        if (!IsValidEmail(email))
        {
            await DisplayAlert("Error", "Format email tidak valid.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            await DisplayAlert("Error", "Masukkan kode OTP.", "OK");
            return;
        }

        if (string.IsNullOrEmpty(_serverOtp))
        {
            await DisplayAlert("Error", "OTP belum dikirim. Klik Kirim Kode dulu.", "OK");
            return;
        }

        // verifikasi lokal
        if (!string.Equals(input, _serverOtp, StringComparison.Ordinal))
        {
            await DisplayAlert("Gagal", "OTP tidak sesuai.", "OK");
            return;
        }

        try
        {
            _otpVerified = true;
            CancelOtpCooldown();

            // Lock email & tombol kirim OTP
            EmailEntry.IsEnabled = false;
            SendOtpButton.IsEnabled = false;

            SetOtpInfo("OTP terverifikasi ✓ Memuat data member...");

            // ======================
            // 1) GET MEMBER by email
            // ======================
            // ⚠️ Sesuaikan method ini sesuai ApiServices kamu.
            // Contoh yang ideal: return Member? (atau null kalau tidak ada)
            var member = await _api.GetMemberAsync("email", email);

            if (member == null)
            {
                // OTP valid tapi email tidak terdaftar sebagai member
                await DisplayAlert("Gagal", "Email belum terdaftar sebagai member. Silakan daftar dulu.", "OK");

                // reset biar user bisa daftar / coba lagi
                EmailEntry.IsEnabled = true;
                ResetOtpState();
                return;
            }

            // ======================
            // 2) Simpan session / profile (contoh)
            // ======================
            // Sesuaikan field Member model kamu
            Preferences.Set("MemberEmail", member.Email);
            Preferences.Set("MemberKode", member.Kode ?? "");
            Preferences.Set("MemberNama", member.Nama ?? "");
            Preferences.Set("MemberNoKartu", member.NoKartu ?? "");
            //Preferences.Set("KodeMember", "M/AKS200200108");
            // ======================
            // 3) Masuk ke Home
            // ======================
            await Shell.Current.GoToAsync("//home");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
            EmailEntry.IsEnabled = true;
            ResetOtpState();
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RegisterPage));
    }

    async void OnSkip(object sender, EventArgs e) => await Shell.Current.GoToAsync("//home");

    async void OnDesignerTapped(object sender, EventArgs e)
    {
        try { await Launcher.OpenAsync("http://affariretail.com"); }
        catch { await DisplayAlert("Info", "Tidak dapat membuka tautan.", "OK"); }
    }
}