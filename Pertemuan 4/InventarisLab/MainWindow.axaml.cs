using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace InventarisLab;

public partial class MainWindow : Window
{
    private const string SemuaKategori = "Semua Kategori";

    private readonly AppDbContext _db = new();
    private Alat? _alatTerpilih;

    public MainWindow()
    {
        InitializeComponent();

        // Siapkan database dan data contoh
        _db.Database.EnsureCreated();
        _db.IsiDataAwal();

        // Isi pilihan ComboBox
        cmbKategori.ItemsSource = Alat.DaftarKategori;
        cmbLokasi.ItemsSource = Alat.DaftarLokasi;
        cmbFilter.ItemsSource = new[] { SemuaKategori }.Concat(Alat.DaftarKategori).ToArray();
        cmbFilter.SelectedIndex = 0;

        BersihkanForm();
        MuatData();
    }

    // ---------------------------------------------------------------
    // Menampilkan data, pencarian, filter, dan statistik
    // ---------------------------------------------------------------
    private void MuatData()
    {
        string kataKunci = txtCari?.Text?.Trim().ToLower() ?? string.Empty;
        string kategori = cmbFilter?.SelectedItem as string ?? SemuaKategori;

        var query = _db.DaftarAlat.AsQueryable();

        if (!string.IsNullOrEmpty(kataKunci))
        {
            query = query.Where(a => a.Kode.ToLower().Contains(kataKunci)
                                  || a.Nama.ToLower().Contains(kataKunci)
                                  || a.Lokasi.ToLower().Contains(kataKunci));
        }

        if (kategori != SemuaKategori)
        {
            query = query.Where(a => a.Kategori == kategori);
        }

        var hasil = query.OrderBy(a => a.Kode).ToList();
        dgAlat.ItemsSource = hasil;

        UpdateStatistik(hasil.Count);
    }

    private void UpdateStatistik(int jumlahTampil)
    {
        var semua = _db.DaftarAlat.ToList();

        txtJenis.Text = semua.Count.ToString();
        txtUnit.Text = semua.Sum(a => a.Jumlah).ToString("N0");

        int unitTotal = semua.Sum(a => a.Jumlah);
        int unitBaik = semua.Where(a => a.Kondisi == "Baik").Sum(a => a.Jumlah);
        txtBaik.Text = unitTotal == 0 ? "0%" : $"{Math.Round(100.0 * unitBaik / unitTotal)}%";

        txtRusak.Text = semua.Count(a => a.Kondisi != "Baik").ToString();

        txtJumlah.Text = $"Menampilkan {jumlahTampil} dari {semua.Count} alat";
    }

    private void TxtCari_TextChanged(object? sender, TextChangedEventArgs e) => MuatData();

    private void CmbFilter_SelectionChanged(object? sender, SelectionChangedEventArgs e) => MuatData();

    // ---------------------------------------------------------------
    // Validasi formulir. Mengembalikan pesan kesalahan, atau null jika valid.
    // ---------------------------------------------------------------
    private string? Validasi(out Alat data)
    {
        data = new Alat();

        string kode = txtKode.Text?.Trim().ToUpper() ?? string.Empty;
        string nama = txtNama.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(kode)) { txtKode.Focus(); return "Kode alat harus diisi."; }
        if (string.IsNullOrWhiteSpace(nama)) { txtNama.Focus(); return "Nama alat harus diisi."; }
        if (cmbKategori.SelectedItem is not string kategori) return "Silakan pilih kategori alat.";
        if (cmbLokasi.SelectedItem is not string lokasi) return "Silakan pilih lokasi penyimpanan.";
        if (numJumlah.Value is null || numJumlah.Value < 0) return "Jumlah unit harus berupa angka 0 atau lebih.";
        if (dpTanggal.SelectedDate is not DateTime tanggal) return "Silakan pilih tanggal masuk.";
        if (tanggal.Date > DateTime.Today) return "Tanggal masuk tidak boleh melebihi hari ini.";

        string? kondisi = rbBaik.IsChecked == true ? "Baik"
                        : rbRusakRingan.IsChecked == true ? "Rusak Ringan"
                        : rbRusakBerat.IsChecked == true ? "Rusak Berat"
                        : null;
        if (kondisi is null) return "Silakan pilih kondisi alat.";

        data = new Alat
        {
            Kode = kode,
            Nama = nama,
            Kategori = kategori,
            Lokasi = lokasi,
            Jumlah = (int)numJumlah.Value,
            TanggalMasuk = tanggal.Date,
            Kondisi = kondisi
        };
        return null;
    }

    // ---------------------------------------------------------------
    // Event tombol
    // ---------------------------------------------------------------
    private async void BtnTambah_Click(object? sender, RoutedEventArgs e)
    {
        string? galat = Validasi(out Alat baru);
        if (galat != null)
        {
            await Dialog.Pesan(this, "Data belum lengkap", galat, Dialog.Jenis.Peringatan);
            return;
        }

        if (_db.DaftarAlat.Any(a => a.Kode == baru.Kode))
        {
            await Dialog.Pesan(this, "Kode sudah terdaftar",
                $"Kode alat {baru.Kode} sudah dipakai. Gunakan kode lain.", Dialog.Jenis.Peringatan);
            txtKode.Focus();
            return;
        }

        _db.DaftarAlat.Add(baru);
        _db.SaveChanges();

        BersihkanForm();
        MuatData();
        txtStatus.Text = $"{baru.Nama} berhasil ditambahkan.";
    }

    private async void BtnSimpan_Click(object? sender, RoutedEventArgs e)
    {
        if (_alatTerpilih == null)
        {
            await Dialog.Pesan(this, "Belum ada pilihan", "Pilih alat pada tabel terlebih dahulu.");
            return;
        }

        string? galat = Validasi(out Alat ubah);
        if (galat != null)
        {
            await Dialog.Pesan(this, "Data belum lengkap", galat, Dialog.Jenis.Peringatan);
            return;
        }

        // Kode boleh sama hanya jika milik alat yang sedang diedit
        int idTerpilih = _alatTerpilih.Id;
        if (_db.DaftarAlat.Any(a => a.Kode == ubah.Kode && a.Id != idTerpilih))
        {
            await Dialog.Pesan(this, "Kode sudah terdaftar",
                $"Kode alat {ubah.Kode} sudah dipakai alat lain.", Dialog.Jenis.Peringatan);
            return;
        }

        _alatTerpilih.Kode = ubah.Kode;
        _alatTerpilih.Nama = ubah.Nama;
        _alatTerpilih.Kategori = ubah.Kategori;
        _alatTerpilih.Lokasi = ubah.Lokasi;
        _alatTerpilih.Jumlah = ubah.Jumlah;
        _alatTerpilih.TanggalMasuk = ubah.TanggalMasuk;
        _alatTerpilih.Kondisi = ubah.Kondisi;
        _db.SaveChanges();

        BersihkanForm();
        MuatData();
        txtStatus.Text = "Perubahan berhasil disimpan.";
    }

    private async void BtnHapus_Click(object? sender, RoutedEventArgs e)
    {
        if (_alatTerpilih == null)
        {
            await Dialog.Pesan(this, "Belum ada pilihan", "Pilih baris yang ingin dihapus.");
            return;
        }

        bool setuju = await Dialog.Konfirmasi(this, "Hapus alat?",
            $"Data {_alatTerpilih.Nama} ({_alatTerpilih.Kode}) akan dihapus permanen dari inventaris.");
        if (!setuju) return;

        string nama = _alatTerpilih.Nama;
        _db.DaftarAlat.Remove(_alatTerpilih);
        _db.SaveChanges();

        BersihkanForm();
        MuatData();
        txtStatus.Text = $"{nama} telah dihapus.";
    }

    private void BtnReset_Click(object? sender, RoutedEventArgs e)
    {
        BersihkanForm();
        txtStatus.Text = "Formulir dikosongkan.";
    }

    // ---------------------------------------------------------------
    // Memilih baris tabel -> isi formulir untuk diedit
    // ---------------------------------------------------------------
    private void DgAlat_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (dgAlat.SelectedItem is not Alat a) return;

        _alatTerpilih = a;
        txtKode.Text = a.Kode;
        txtNama.Text = a.Nama;
        cmbKategori.SelectedItem = a.Kategori;
        cmbLokasi.SelectedItem = a.Lokasi;
        numJumlah.Value = a.Jumlah;
        dpTanggal.SelectedDate = a.TanggalMasuk;
        rbBaik.IsChecked = a.Kondisi == "Baik";
        rbRusakRingan.IsChecked = a.Kondisi == "Rusak Ringan";
        rbRusakBerat.IsChecked = a.Kondisi == "Rusak Berat";

        btnTambah.IsEnabled = false;
        btnSimpan.IsEnabled = true;
        btnHapus.IsEnabled = true;
        txtStatus.Text = $"Mengedit: {a.Nama}";
    }

    private void BersihkanForm()
    {
        txtKode.Text = string.Empty;
        txtNama.Text = string.Empty;
        cmbKategori.SelectedIndex = -1;
        cmbLokasi.SelectedIndex = -1;
        numJumlah.Value = 1;
        dpTanggal.SelectedDate = DateTime.Today;
        rbBaik.IsChecked = false;
        rbRusakRingan.IsChecked = false;
        rbRusakBerat.IsChecked = false;

        _alatTerpilih = null;
        dgAlat.SelectedItem = null;
        btnTambah.IsEnabled = true;
        btnSimpan.IsEnabled = false;
        btnHapus.IsEnabled = false;

        txtKode.Focus();
    }
}
