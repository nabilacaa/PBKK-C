using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace InventarisLab;

/// <summary>
/// Penghubung ke database SQLite (inventaris.db) yang dibuat otomatis
/// di folder yang sama dengan file aplikasi.
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Alat> DaftarAlat => Set<Alat>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string path = Path.Combine(System.AppContext.BaseDirectory, "inventaris.db");
        optionsBuilder.UseSqlite($"Data Source={path}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Kode alat wajib unik
        modelBuilder.Entity<Alat>().HasIndex(a => a.Kode).IsUnique();
    }

    /// <summary>Mengisi beberapa data contoh saat database masih kosong.</summary>
    public void IsiDataAwal()
    {
        if (DaftarAlat.Any()) return;

        DaftarAlat.AddRange(
            new Alat { Kode = "LAB-001", Nama = "PC Workstation", Kategori = "Komputer & Jaringan", Lokasi = "Lab Pemrograman", Jumlah = 30, Kondisi = "Baik", TanggalMasuk = new DateTime(2024, 8, 12) },
            new Alat { Kode = "LAB-002", Nama = "Router Cisco 2901", Kategori = "Komputer & Jaringan", Lokasi = "Lab Jaringan", Jumlah = 8, Kondisi = "Rusak Ringan", TanggalMasuk = new DateTime(2022, 2, 3) },
            new Alat { Kode = "LAB-003", Nama = "Arduino Uno R3", Kategori = "Mikrokontroler & IoT", Lokasi = "Lab Sistem Tertanam", Jumlah = 25, Kondisi = "Baik", TanggalMasuk = new DateTime(2025, 1, 20) },
            new Alat { Kode = "LAB-004", Nama = "Osiloskop Digital", Kategori = "Alat Ukur", Lokasi = "Lab Sistem Tertanam", Jumlah = 4, Kondisi = "Rusak Berat", TanggalMasuk = new DateTime(2019, 9, 9) }
        );
        SaveChanges();
    }
}
