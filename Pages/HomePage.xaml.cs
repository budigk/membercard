using System.Collections.ObjectModel;
using Microsoft.Maui.ApplicationModel; // Launcher
using Microsoft.Maui.Storage;          // Preferences

namespace MemberCard.Pages;

public partial class HomePage : ContentPage
{
    public class PromoSlide
    {
        public string Title { get; set; } = "";
        public string Image { get; set; } = "";
    }

    private readonly ObservableCollection<PromoSlide> _slides = new();

    public HomePage()
    {
        InitializeComponent();

        // Carousel
        carousel.ItemsSource = _slides;
        carousel.IndicatorView = indicator;

        // Data awal (placeholder; nanti ganti dari API)
        _slides.Clear();
        _slides.Add(new PromoSlide { Title = "Promo Spesial Minggu Ini", Image = "promo1.png" });
        _slides.Add(new PromoSlide { Title = "Diskon Fresh 30%", Image = "promo2.png" });
        _slides.Add(new PromoSlide { Title = "Voucher Spesial", Image = "promo3.png" });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        //Preferences.Set("MemberEmail", member.Email);
        //Preferences.Set("MemberKode", member.Kode ?? "");
        //Preferences.Set("MemberNama", member.Nama ?? "");
        //Preferences.Set("MemberNoKartu", member.NoKartu ?? "");
        // TODO: ganti ke data dari API
        lblMemberName.Text = Preferences.Get("MemberNama", ""); // "Agung Pratama";
        lblCardNo.Text = Preferences.Get("MemberNoKartu", "");
        lblValid.Text = "Valid thru";
        lblPoints.Text = "0";
    }

    private async void OnWhatsAppTapped(object sender, TappedEventArgs e)
    {
        // Ambil dari konfigurasi branding
        var waNumber = Preferences.Get("WhatsAppNumber", "62812XXXXXXX");
        var text = Uri.EscapeDataString("Halo Irian Supermarket, saya butuh bantuan iCard.");
        await Launcher.OpenAsync($"https://wa.me/{waNumber}?text={text}");
    }

    private async void OnShowBarcode(object sender, EventArgs e)
    {
        var cardNo = Preferences.Get("MemberNoKartu","") ;
        var name = Preferences.Get("MemberNama", "");

        await Shell.Current.GoToAsync($"{nameof(ShowBarcodePage)}?CardNo={cardNo}&MemberName={Uri.EscapeDataString(name)}");
    }

}
