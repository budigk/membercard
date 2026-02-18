using Android.Media.TV;
using MemberCard.Models;
using MemberCard.Services;
using System.Text.RegularExpressions;

namespace MemberCard.Pages;

public partial class RegisterPage : ContentPage
{
    private readonly ApiServices _api;
    private bool _otpVerified;
    private string? _serverOtp;

    // Cooldown
    private CancellationTokenSource? _otpCooldownCts;
    private bool _cooldownActive;

    public RegisterPage(ApiServices api)
    {
        InitializeComponent();
        _api = api;
        UpdateUiState();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        CancelOtpCooldown();
    }

    // ================= UI state ================
    private void SetBusy(bool busy, string? info = null)
    {
        aiBusy.IsVisible = aiBusy.IsRunning = busy;
        lblInfo.Text = info ?? string.Empty;

        // Tombol OTP:
        // - kalau verified -> permanen disabled
        // - kalau cooldown -> disabled
        // - kalau busy -> disabled
        btnSendOtp.IsEnabled = !_otpVerified && !_cooldownActive && !busy;
        btnVerifyOtp.IsEnabled = !_otpVerified && !busy;
        btnDaftar.IsEnabled = _otpVerified && !busy;

        // Field lain dikunci sampai OTP valid
        bool en = _otpVerified && !busy;
        //txtPassword.IsEnabled = en;
        //txtConfirmPassword.IsEnabled = en;
        txtNoKtp.IsEnabled = en;
        txtNama.IsEnabled = en;
        txtNoPonsel.IsEnabled = en;
        txtAlamat.IsEnabled = en;
        txtKecamatan.IsEnabled = en;
        txtKota.IsEnabled = en;
        pkAgama.IsEnabled = en;
        txtTempatLahir.IsEnabled = en;
        dpTanggalLahir.IsEnabled = en;
        // txtNoKartu.IsEnabled      = en; // jika digunakan
    }

    private void UpdateUiState()
    {
        btnVerifyOtp.Text = _otpVerified ? "OTP Terverifikasi ✓" : "Verifikasi OTP";
        txtEmail.IsEnabled = !_otpVerified; // lock email setelah verified
        if (!_cooldownActive && !_otpVerified) btnSendOtp.Text = "Kirim Kode";
        SetBusy(false);
    }

    private static bool IsValidEmail(string email)
      => !string.IsNullOrWhiteSpace(email)
      && Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    // ============== Cooldown helper (60s) ==============
    private async Task StartOtpCooldownAsync(int seconds, CancellationToken token)
    {
        _cooldownActive = true;
        btnSendOtp.IsEnabled = false;

        try
        {
            for (int s = seconds; s >= 1; s--)
            {
                if (token.IsCancellationRequested || _otpVerified) break;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    btnSendOtp.Text = $"Kirim Ulang ({s}s)";
                });


                await Task.Delay(1000, token);
            }
        }
        catch (TaskCanceledException) { /* ignore */ }

        _cooldownActive = false;

        // Jika sudah verified, tetap terkunci; kalau belum, aktifkan kembali
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (!_otpVerified)
            {
                btnSendOtp.Text = "Kirim Kode";
                btnSendOtp.IsEnabled = true;
            }
            else
            {
                btnSendOtp.IsEnabled = false;
            }
        });
    }

    private void CancelOtpCooldown()
    {
        try { _otpCooldownCts?.Cancel(); } catch { }
        _cooldownActive = false;

        if (!_otpVerified)
        {
            btnSendOtp.Text = "Kirim Kode";
            btnSendOtp.IsEnabled = true;
        }
        else
        {
            btnSendOtp.IsEnabled = false;
        }
    }

    // ================== Events ==================
    private async void OnSendOtpClicked(object sender, EventArgs e)
    {
        var email = txtEmail.Text?.Trim() ?? "";
        if (!IsValidEmail(email))
        {
            await DisplayAlert("Error", "Format email tidak valid.", "OK");
            return;
        }

        try
        {
            //SetBusy(true, "Memeriksa email...");
            //if (await _api.CheckUserExistsAsync(email))
            //{
            //    await DisplayAlert("Info", "Email sudah terdaftar. Silakan login atau gunakan email lain.", "OK");
            //    return;
            //}

            SetBusy(true, "Mengirim OTP ke email...");
            var (ok, otp, serverMsg) = await _api.RequestEmailOtpAsync(email,  "REGISTRASI");
            if (!ok)
            {
                await DisplayAlert("Gagal", serverMsg ?? "Tidak dapat mengirim OTP.", "OK");
                return;
            }

            // simpan OTP utk pembanding verifikasi
            _serverOtp = otp;

            await DisplayAlert("Info", serverMsg ?? "OTP terkirim.", "OK");
            
            txtOtp.Focus();

            // mulai cooldown 60s
            CancelOtpCooldown();
            _otpCooldownCts = new CancellationTokenSource();
            _ = StartOtpCooldownAsync(60, _otpCooldownCts.Token);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void OnVerifyOtpClicked(object sender, EventArgs e)
    {
        var input = txtOtp.Text?.Trim();
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

        if (string.Equals(input, _serverOtp, StringComparison.Ordinal))
        {
            _otpVerified = true;

            // lock email & tombol OTP, hentikan cooldown
            CancelOtpCooldown();
            btnSendOtp.IsEnabled = false;
            txtEmail.IsEnabled = false;

            UpdateUiState();

            txtOtp.Text = string.Empty;
            lblInfo.Text = "Email terverifikasi ✓ Silakan lengkapi data pendaftaran.";

            await DisplayAlert("Sukses", "Email terverifikasi.", "OK");
        }
        else
        {
            await DisplayAlert("Gagal", "OTP tidak sesuai.", "OK");
        }
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        if (!_otpVerified)
        {
            await DisplayAlert("Error", "Verifikasi OTP terlebih dahulu.", "OK");
            return;
        }

        var email = txtEmail.Text?.Trim() ?? "";
        //var pass = txtPassword.Text ?? "";
        //var pass2 = txtConfirmPassword.Text ?? "";

        if (!IsValidEmail(email))
        {
            await DisplayAlert("Error", "Format email tidak valid.", "OK");
            return;
        }

        //if (pass.Length < 6)
        //{
        //    await DisplayAlert("Error", "Password minimal 6 karakter.", "OK");
        //    return;
        //}
        //if (!string.Equals(pass, pass2))
        //{
            //await DisplayAlert("Error", "Konfirmasi password tidak sama.", "OK");
            //return;
        //}

        if (string.IsNullOrWhiteSpace(txtNoKtp.Text)
         || string.IsNullOrWhiteSpace(txtNama.Text)
         || string.IsNullOrWhiteSpace(txtNoPonsel.Text))
        {
            await DisplayAlert("Error", "No KTP, Nama, dan Nomor Ponsel wajib diisi.", "OK");
            return;
        }

        DateTime? tgl = dpTanggalLahir.Date;

        if (tgl == null)
        {
            await DisplayAlert("Error", "Tanggal lahir wajib diisi.", "OK");
            return;
        }

        if (tgl > DateTime.Today)
        {
            await DisplayAlert("Error", "Tanggal lahir tidak boleh lebih dari hari ini.", "OK");
            return;
        }

        try
        {
            SetBusy(true, "Menyimpan data...");

            // 1) Tandai email confirmed
            //var ureq = new UserUpsertRequest { Email = email, EmailConfirmed = true };
            //if (!await _api.CreateOrUpdateUserAsync(ureq))
            //{
            //    await DisplayAlert("Gagal", "Gagal menyimpan data user.", "OK");
            //    return;
            //}

            // 2) Simpan member
            var m = new Member
            {
                Kode = "",
                NoKartu = "",
                Email = email,
                Password = "",
                IdPengenal = txtNoKtp.Text!.Trim(),
                Nama = txtNama.Text!.Trim(),
                Ponsel = txtNoPonsel.Text!.Trim(),
                Alamat = txtAlamat.Text?.Trim(),
                Wilayah = txtKecamatan.Text?.Trim(),
                Kota = txtKota.Text?.Trim(),
                Propinsi = "",
                KodePos = "",
                Negara = "Indonesia",
                Telepon = "",
                Fax = "",
                
                TipeId = "KTP",
                IsKirim = false,
                TglUpload = "",
                TglReady = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                Operator = "ONLINE",
                Agama = pkAgama.SelectedItem?.ToString(),
                TptLahir = txtTempatLahir.Text?.Trim(),
                TglLahir = tgl.Value.ToString("yyyy-MM-dd"),
                PointAkhir = 0,
                TglRegis = DateTime.Now.ToString("yyyy-MM-dd"),
                TglAktif = DateTime.Now.ToString("yyyy-MM-dd"),
                TglBerakhir = DateTime.Now.ToString("yyyy-MM-dd"),
                Status = "Aktif",
                TglStatus = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                //Waktu = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                TglKenal = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"),
                StatusDesc = "",
                PointAwal = 0,
                GolDar = "",
                NamaIbu = "",
                Pendapatan = 0,
                Horeka = "",
                NilaiMinus = 0,
                NilaiPlus = 0,
                Gender = "Pria",
                TelKantor = "",
                Ponsel2 = "",
                PinBB = "",
                Pekerjaan = "SWASTA",
                RangeDapat = "",
                SSosial = "Belum Menikah",
                Pasangan = "",
                Anak1 = "",
                Anak2 = "",
                Anak3 = "",
                LvHarga ="Regular",
                Outlet = "",
                TglCrossDate = "",
                JMember = "Regular",
                Kategori = "",
                Grup = "",
                Alias = "",
                Mobile = false,
                Kelompok = "REGULAR",
                FingerScan = ""
            };

            var (ok, msg) = await _api.CreateMemberAsync(m);

            if (!ok)
            {
                //await DisplayAlert("Gagal", msg ?? "Gagal menyimpan data member.", "OK");
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(msg);
                    if (doc.RootElement.TryGetProperty("Message", out var messageProp))
                    {
                        var pesan = messageProp.GetString();
                        await DisplayAlert("Gagal", pesan ?? "Terjadi kesalahan.", "OK");
                    }
                    else
                    {
                        // Kalau JSON tapi tidak ada field Message
                        await DisplayAlert("Gagal", msg ?? "Gagal menyimpan data member.", "OK");
                    }
                }
                catch
                {
                    // Kalau msg ternyata bukan JSON (plain text)
                    await DisplayAlert("Gagal", msg ?? "Gagal menyimpan data member.", "OK");
                }

                return;
            }

            await DisplayAlert("Sukses", "Pendaftaran berhasil. Silakan login.", "OK");

            // reset state page
            _otpVerified = false;
            _serverOtp = null;
            CancelOtpCooldown();
            UpdateUiState();

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            SetBusy(false);
        }
    }
}
