namespace MemberCard.Models;

public class Member
{
    // ——— Wajib
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;

    public string NoKTP { get; set; } = default!;
    public string Nama { get; set; } = default!;
    public string NoPonsel { get; set; } = default!;
    public DateTime TanggalLahir { get; set; }

    // ——— Opsional
    public string? Alamat { get; set; }
    public string? Kecamatan { get; set; }
    public string? Kota { get; set; }
    public string? Agama { get; set; }
    public string? TempatLahir { get; set; }

    // ——— Tambahan baru
    public string? NoKartu { get; set; }    // kalau auto-generate di server, biarkan null
    public int PointAkhir { get; set; }     // default 0
}
