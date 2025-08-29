using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using MemberCard.Services;

namespace MemberCard.Services;

public class OtpLoginViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

    string _phone = "";
    public string Phone { get => _phone; set { _phone = value; OnPropertyChanged(); } }

    string _otp = "";
    public string Otp { get => _otp; set { _otp = value; OnPropertyChanged(); } }

    bool _isOtpSent;
    public bool IsOtpSent { get => _isOtpSent; set { _isOtpSent = value; OnPropertyChanged(); } }

    bool _isBusy;
    public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

    readonly WaOtpService _wa;
    readonly ApiServices _api;
    //readonly MemberService _member = new();

    static string GenerateCode(int digits = 6)
    {
        var bytes = RandomNumberGenerator.GetBytes(digits);
        var sb = new StringBuilder(digits);
        foreach (var b in bytes) sb.Append((b % 10).ToString());
        return sb.ToString(0, digits);
    }

    public Command SendOtpCommand => new(async () =>
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            if (string.IsNullOrWhiteSpace(Phone))
            {
                await Shell.Current.DisplayAlert("Info", "Nomor ponsel wajib diisi.", "OK");
                return;
            }

            var code = GenerateCode(6);

            // simpan sementara agar survive re-render
            Preferences.Set("PendingOtpPhone", Phone);
            Preferences.Set("PendingOtpCode", code);
            Preferences.Set("PendingOtpExpires", DateTime.UtcNow.AddMinutes(5).ToString("o"));

            var ok = await _wa.SendOtpAsync(Phone, code);
            if (!ok)
            {
                await Shell.Current.DisplayAlert("Gagal", "Kirim OTP gagal. Coba lagi.", "OK");
                return;
            }

            IsOtpSent = true;
            await Shell.Current.DisplayAlert("Berhasil", "OTP dikirim ke WhatsApp Anda.", "OK");
        }
        finally { IsBusy = false; }
    });

    public Command VerifyOtpCommand => new(async () =>
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;

            var pendingPhone = Preferences.Get("PendingOtpPhone", Phone);
            var pendingCode = Preferences.Get("PendingOtpCode", "");
            var expStr = Preferences.Get("PendingOtpExpires", "");

            if (string.IsNullOrWhiteSpace(Otp) || string.IsNullOrWhiteSpace(pendingCode))
            {
                await Shell.Current.DisplayAlert("Info", "OTP belum dikirim atau tidak valid.", "OK");
                return;
            }
            if (DateTime.TryParse(expStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var exp) && DateTime.UtcNow > exp)
            {
                await Shell.Current.DisplayAlert("Kadaluarsa", "OTP sudah kadaluarsa. Kirim ulang.", "OK");
                return;
            }
            if (!string.Equals(Otp.Trim(), pendingCode, StringComparison.Ordinal))
            {
                await Shell.Current.DisplayAlert("Salah", "OTP tidak cocok.", "OK");
                return;
            }

            // Ambil data member berdasarkan ponsel, lalu simpan ke Preferences
            var member = await _api.GetMemberAsync(pendingPhone);
            //if (member)
            //{
            //    await Shell.Current.DisplayAlert("Gagal", "Gagal mengambil data member.", "OK");
            //    return;
            //}

            // bersihkan pending OTP
            Preferences.Remove("PendingOtpPhone");
            Preferences.Remove("PendingOtpCode");
            Preferences.Remove("PendingOtpExpires");

            // Masuk ke HomePage (pastikan route //HomePage ada di AppShell)
            await Shell.Current.GoToAsync("//HomePage");
        }
        finally { IsBusy = false; }
    });
}
