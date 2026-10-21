# Inventaris Alat Laboratorium (Avalonia + SQLite)

Aplikasi desktop untuk mencatat inventaris alat laboratorium. Tampilan dirancang dengan XAML (AXAML) dan logika program ditulis dalam C#. Avalonia dipakai agar aplikasi berjalan di Windows, macOS, maupun Linux; data disimpan permanen di SQLite melalui Entity Framework Core.

## Fitur

- Menambah, menampilkan, mengubah, dan menghapus data alat (CRUD).
- Validasi isian dan pengecekan kode alat ganda.
- Pencarian berdasarkan kode, nama alat, atau lokasi, ditambah filter kategori.
- Kartu statistik: jumlah jenis alat, total unit, persentase unit berkondisi baik, dan jumlah alat yang perlu perbaikan.
- Badge warna untuk kondisi alat (Baik, Rusak Ringan, Rusak Berat).
- Dialog pesan dan konfirmasi hapus buatan sendiri (Avalonia tidak punya MessageBox seperti WPF).
- Data contoh otomatis terisi saat database pertama kali dibuat.

Setiap alat memiliki atribut Kode Alat (unik, misalnya `LAB-001`), Nama Alat, Kategori, Lokasi Penyimpanan, Jumlah Unit, Tanggal Masuk, dan Kondisi.

## Menjalankan

Prasyarat: .NET SDK 10.

```bash
cd InventarisLab
dotnet run
```

Membuat file `.exe` mandiri untuk Windows:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Hasilnya ada di `bin/Release/net10.0/win-x64/publish/`. Database `inventaris.db` dibuat otomatis di folder yang sama dengan aplikasi.

## Struktur Berkas

| Berkas | Kegunaan |
|---|---|
| `InventarisLab.csproj` | Konfigurasi project: jenis aplikasi, target .NET, paket Avalonia dan EF Core SQLite. |
| `Program.cs` | Titik masuk aplikasi. |
| `App.axaml`, `App.axaml.cs` | Tema aplikasi (Fluent, Light) dan penentu jendela pertama (`MainWindow`). |
| `Alat.cs` | Kelas model satu data alat, termasuk properti tampilan (inisial, warna ikon, warna badge). |
| `AppDbContext.cs` | Koneksi ke SQLite, indeks unik pada kode, dan data contoh awal. |
| `Dialog.cs` | Dialog pesan dan konfirmasi pengganti MessageBox. |
| `MainWindow.axaml` | Desain antarmuka: formulir di kiri; statistik, pencarian, dan tabel di kanan. |
| `MainWindow.axaml.cs` | Logika program: event, validasi, CRUD, pencarian, filter, statistik. |

## Kontrol dan Event

| Kontrol | Nama | Fungsi |
|---|---|---|
| TextBox | `txtKode`, `txtNama`, `txtCari` | Input data alat dan kata kunci pencarian. |
| ComboBox | `cmbKategori`, `cmbLokasi`, `cmbFilter` | Pilihan kategori, lokasi, dan filter kategori tabel. |
| NumericUpDown | `numJumlah` | Jumlah unit alat. |
| CalendarDatePicker | `dpTanggal` | Tanggal masuk (bawaan: hari ini). |
| RadioButton | `rbBaik`, `rbRusakRingan`, `rbRusakBerat` | Kondisi alat. |
| Button | `btnTambah`, `btnSimpan`, `btnHapus`, `btnReset` | Tambah, simpan perubahan, hapus, reset. |
| DataGrid | `dgAlat` | Tabel data alat. |
| TextBlock | `txtJenis`, `txtUnit`, `txtBaik`, `txtRusak`, `txtJumlah`, `txtStatus` | Statistik, penghitung baris, dan pesan status. |

| Event / Method | Fungsi |
|---|---|
| `BtnTambah_Click` | Validasi isian, cek kode ganda, lalu menyimpan alat baru. |
| `BtnSimpan_Click` | Menyimpan perubahan pada alat yang dipilih. |
| `BtnHapus_Click` | Menghapus alat terpilih setelah konfirmasi. |
| `BtnReset_Click` | Mengosongkan formulir dan fokus kembali ke kolom kode. |
| `DgAlat_SelectionChanged` | Mengisi formulir dari baris yang dipilih dan masuk mode edit. |
| `TxtCari_TextChanged`, `CmbFilter_SelectionChanged` | Menyaring tabel saat kata kunci atau kategori berubah. |
| `MuatData`, `UpdateStatistik` | Memperbarui isi tabel, kartu statistik, dan teks jumlah. |

## Skenario Pengujian

1. Semua isian kosong lalu klik **Tambah Alat**: muncul dialog bahwa kode alat harus diisi.
2. Nama kosong: muncul dialog bahwa nama alat harus diisi.
3. Kategori belum dipilih: muncul dialog untuk memilih kategori.
4. Lokasi belum dipilih: muncul dialog untuk memilih lokasi.
5. Tanggal melebihi hari ini: muncul dialog bahwa tanggal tidak valid.
6. Kondisi belum dipilih: muncul dialog untuk memilih kondisi.
7. Semua data benar: data masuk ke tabel, formulir dikosongkan, statistik bertambah.
8. Kode sudah ada (misalnya `LAB-001`): muncul dialog kode sudah terdaftar, data tidak ditambahkan.
9. Klik baris tabel: formulir terisi, tombol Simpan dan Hapus aktif, tombol Tambah nonaktif.
10. Ubah jumlah atau kondisi lalu klik **Simpan Perubahan**: tabel dan statistik ikut berubah.
11. Klik **Hapus** lalu pilih **Ya, Hapus**: data terhapus dan jumlah jenis alat berkurang.
12. Ketik `sistem` di kolom cari: hanya alat di Lab Sistem Tertanam yang tampil, dan penghitung menjadi "Menampilkan 2 dari 4 alat".
13. Pilih kategori **Alat Ukur** pada filter: hanya alat berkategori Alat Ukur yang tampil.
14. Tutup lalu buka kembali aplikasi: data tetap ada karena tersimpan di SQLite.
