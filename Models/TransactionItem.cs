using System.Globalization;
using System.Text.Json.Serialization;

namespace MemberCard.Models;

public class TransactionItem
{
    public string Kode { get; set; } = "";
    public string Tanggal { get; set; } = "";
    public string NoTrans { get; set; } = "";
    public string KodeHadiah{ get; set; } = "";
    public string Keterangan { get; set; } = "";
    public double Total { get; set; } = 0;
    public double Point { get; set; } = 0;
    // --- Properti bantu untuk UI ---

    // Jika KodeHadiah == "" → mask NoTrans (4 awal + 9 akhir), else tampil full
    [JsonIgnore]
    public string NoTransMasked
    {
        get
        {
            if (!string.IsNullOrEmpty(KodeHadiah)) return NoTrans; // hadiah → full
            if (string.IsNullOrEmpty(NoTrans)) return "";
            if (NoTrans.Length <= 13) return NoTrans; // terlalu pendek buat di-mask
            return NoTrans.Substring(0, 4) + NoTrans.Substring(NoTrans.Length - 9);
        }
    }

    // Format tanggal pendek untuk tampilan (dd MMM yyyy). Jika gagal parse, pakai string asli.
    [JsonIgnore]
    public string TanggalShort
    {
        get
        {
            if (DateTime.TryParseExact(
                    Tanggal,
                    "yyyy-MM-dd HH:mm:ss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal,
                    out var dt))
            {
                return dt.ToString("dd MMM yyyy", new CultureInfo("id-ID"));
            }
            return Tanggal;
        }
    }

    // Hanya tampilkan Total kalau KodeHadiah == ""
    [JsonIgnore]
    public bool ShowTotal => string.IsNullOrEmpty(KodeHadiah);
}