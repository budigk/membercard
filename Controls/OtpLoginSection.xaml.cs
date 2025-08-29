using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using MemberCard.Services;

namespace MemberCard.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OtpLoginSection : ContentView
    {
        /// <summary>
        /// Akses cepat ke ViewModel yang dibinding.
        /// </summary>
        public OtpLoginViewModel? VM => BindingContext as OtpLoginViewModel;

        /// <summary>
        /// Ctor default dipakai saat dibuat dari XAML.
        /// </summary>
        public OtpLoginSection()
        {
            InitializeComponent();

            // Jika BindingContext tidak di-set dari XAML (mis. Build Action XAML salah),
            // fallback ke instance baru supaya Command/Binding tidak null.
            BindingContext ??= new OtpLoginViewModel();
        }

        /// <summary>
        /// Ctor opsional kalau ingin inject ViewModel via kode.
        /// </summary>
        public OtpLoginSection(OtpLoginViewModel vm)
        {
            InitializeComponent();
            BindingContext = vm ?? new OtpLoginViewModel();
        }

        /// <summary>
        /// Opsional: ketika view ter-attach, pulihkan state OTP pending dari Preferences (jika ada).
        /// Tidak wajib, tapi berguna kalau view re-render.
        /// </summary>
        protected override void OnParentSet()
        {
            base.OnParentSet();

            if (VM is null) return;

            var pendingPhone = Preferences.Get("PendingOtpPhone", string.Empty);
            var pendingCode = Preferences.Get("PendingOtpCode", string.Empty);
            var pendingExpires = Preferences.Get("PendingOtpExpires", string.Empty);

            if (!string.IsNullOrWhiteSpace(pendingPhone))
            {
                VM.Phone = pendingPhone;
            }

            // Jika sebelumnya sudah kirim OTP (tersimpan di Preferences), tampilkan panel verifikasi.
            if (!string.IsNullOrWhiteSpace(pendingCode))
            {
                VM.IsOtpSent = true;
            }
        }
    }
}
