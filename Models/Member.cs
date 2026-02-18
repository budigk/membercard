namespace MemberCard.Models;

public class Member
{
    // ——— Wajib
    public string Kode { get; set; } = "";
    public string NoKartu{ get; set; } = "";
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string TipeId { get; set; } = "KTP";
    public string IdPengenal { get; set; } = "";
    public string Nama { get; set; } = default!;
    public string Ponsel { get; set; } = default!;
    public string Telepon { get; set; } = default!;
    public string Fax { get; set; } = default!;
    public string TglLahir { get; set; }

    // ——— Opsional
    public string? Alamat { get; set; }
    public string? Wilayah { get; set; }
    public string? Kota { get; set; }
    public string? Propinsi { get; set; }
    public string? KodePos { get; set; }
    public string? Negara { get; set; }
    public string Agama { get; set; } = "";
    public string? TptLahir { get; set; }

    // ——— Tambahan baru
    //public string? NoKartu { get; set; }    // kalau auto-generate di server, biarkan null
    public float PointAkhir { get; set; } = 0;  // default 0
    public string TglRegis { get; set; }
    public string TglAktif { get; set; }
    public string TglBerakhir { get; set; }
    public string Status { get; set; } = "Aktif"; // default Aktif
    public string TglStatus { get; set; }
    public string StatusDesc { get; set; } = "";
    public float PointAwal { get; set; } = 0;   // default 0
    public string GolDar { get; set; } = "";        // default kosong
    public string NamaIbu { get; set; } = "";   // default kosong
    public double Pendapatan { get; set; } = 0;

    public string Operator { get; set; } = "ONLINE";
    //public string Waktu { get; set; }
    public string TglKenal { get; set; }
    public bool IsKirim { get; set; } = false;
    public string TglUpload { get; set; } = "";
    public string TglReady { get; set; } = "";

    public string Horeka { get; set; } = ""; // default kosong
    public double NilaiPlus { get; set; } = 0;
    public double NilaiMinus { get; set; } = 0;
    public string Gender { get; set; } = ""; // default kosong
    public string TelKantor { get; set; } = ""; // default kosong
    public string Ponsel2 { get; set; } = ""; // default kosong
    public string PinBB { get; set; } = ""; // default kosong
    public string Pekerjaan { get; set; } = "";
    public string RangeDapat { get; set; } = "";

    public string SSosial { get; set; } = ""; // default kosong
    public string Pasangan { get; set; } = ""; // default kosong
    public string Anak1 { get; set; } = "";
    public string Anak2 { get; set; } = ""; 
    public string Anak3 { get; set; } = "";
    public string LvHarga { get; set; } = "Regular";
    public string Outlet { get; set; } = "";
    public string TglCrossDate { get; set; } = "";
    public string JMember { get; set; } = "Regular";
    public string Kategori { get; set; } = "";
    public string Grup { get; set; } = "";
    public string Alias { get; set; } = "";
    public bool Mobile { get; set; } = false;
    public string Kelompok { get; set; } = "REGULAR";
    public string FingerScan { get; set; } = "";

}
