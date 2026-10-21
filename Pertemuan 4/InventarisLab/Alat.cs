using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarisLab;

/// <summary>
/// Model yang mewakili satu data alat laboratorium.
/// Properti bertanda [NotMapped] hanya untuk tampilan dan tidak disimpan ke database.
/// </summary>
public class Alat
{
    public int Id { get; set; }
    public string Kode { get; set; } = string.Empty;
    public string Nama { get; set; } = string.Empty;
    public string Kategori { get; set; } = string.Empty;
    public string Lokasi { get; set; } = string.Empty;
    public int Jumlah { get; set; }
    public string Kondisi { get; set; } = "Baik";
    public DateTime TanggalMasuk { get; set; } = DateTime.Today;

    public static readonly string[] DaftarKategori =
    {
        "Komputer & Jaringan",
        "Elektronika",
        "Mikrokontroler & IoT",
        "Alat Ukur",
        "Perangkat Pendukung"
    };

    public static readonly string[] DaftarLokasi =
    {
        "Lab Pemrograman",
        "Lab Jaringan",
        "Lab Sistem Tertanam",
        "Lab Rekayasa Perangkat Lunak",
        "Gudang"
    };

    [NotMapped]
    public string Inisial
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Nama)) return "?";
            var bagian = Nama.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (bagian.Length == 1) return bagian[0][..Math.Min(2, bagian[0].Length)].ToUpper();
            return $"{bagian[0][0]}{bagian[1][0]}".ToUpper();
        }
    }

    // Warna ikon mengikuti kategori agar mudah dikenali
    [NotMapped]
    public string IkonBg => Kategori switch
    {
        "Komputer & Jaringan" => "#0E7490",
        "Elektronika" => "#B45309",
        "Mikrokontroler & IoT" => "#047857",
        "Alat Ukur" => "#6D28D9",
        _ => "#475569"
    };

    [NotMapped]
    public string KondisiBg => Kondisi switch
    {
        "Baik" => "#DCFCE7",
        "Rusak Ringan" => "#FEF3C7",
        _ => "#FEE2E2"
    };

    [NotMapped]
    public string KondisiFg => Kondisi switch
    {
        "Baik" => "#166534",
        "Rusak Ringan" => "#92400E",
        _ => "#991B1B"
    };

    [NotMapped]
    public string KodeLokasi => $"{Kode} · {Lokasi}";

    [NotMapped]
    public string TanggalText => TanggalMasuk.ToString("dd MMM yyyy");
}
