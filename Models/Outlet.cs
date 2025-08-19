using System.Text.Json.Serialization;

namespace MemberCard.Models;

public class Outlet
{
    public string? Kode { get; set; }
    public string? Nama { get; set; }
    public string? Wilayah { get; set; }
    public string? Alamat { get; set; }
    public string? Kota { get; set; }
    public string? Propinsi { get; set; }
    public string? OpeningHour { get; set; }
    public string? Telepon { get; set; }
    public string? Email { get; set; }
    public string? Whatsapp { get; set; }
    public string? Operator { get; set; }
    public string? Waktu { get; set; }
    public string? GeoCode { get; set; }         // "lat,long"
    public string? ReverseGeoCode { get; set; }  // nama tempat / label
    public string? Outletcol { get; set; }
}
