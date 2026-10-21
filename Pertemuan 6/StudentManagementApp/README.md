# Student Management — Avalonia UI + SQL Server

Aplikasi desktop pengelolaan data mahasiswa yang terhubung ke SQL Server melalui ADO.NET (`Microsoft.Data.SqlClient`). Antarmuka dibuat dengan Avalonia UI 12 memakai pola MVVM (CommunityToolkit.Mvvm).

**Fitur**

- Tab **Mahasiswa**: tambah, tampil, cari (NRP/nama/email), update, dan hapus data mahasiswa, dengan validasi form dan dialog konfirmasi hapus.
- Tab **KRS**: mengambil mata kuliah untuk mahasiswa (tabel penghubung Many-to-Many), mengubah nilai, menghapus KRS, filter per mahasiswa, serta ringkasan jumlah MK, total SKS, dan IPK.
- Tab **Rekap Prodi**: hasil query `LEFT JOIN + GROUP BY + COUNT` jumlah mahasiswa per program studi.
- Pelanggaran constraint (UNIQUE 2627/2601, FOREIGN KEY/CHECK 547) ditangkap dan diterjemahkan menjadi pesan yang mudah dibaca.

## 1. Kebutuhan

- Windows 10/11 (juga berjalan di macOS/Linux karena Avalonia lintas platform)
- .NET SDK 8 atau lebih baru — cek dengan `dotnet --list-sdks`
- SQL Server Express/Developer dan SQL Server Management Studio (SSMS), atau VS Code dengan ekstensi **SQL Server (mssql)**

## 2. Menyiapkan database

1. Buka SSMS, sambungkan ke server `localhost\SQLEXPRESS` dengan **Windows Authentication**.
2. Buka `Database/db_student_management.sql`, lalu jalankan seluruh script (F5).
3. Script membuat database `StudentManagementDB` berisi tabel `Programs`, `Students`, `Courses`, `Enrollments`, view `vw_EnrollmentDetail`, data contoh, uji constraint, dan query JOIN/GROUP BY. Script aman dijalankan berulang kali.

## 3. Mengatur koneksi

Connection string dibaca dari `appsettings.json`:

```json
"Default": "Server=localhost\\SQLEXPRESS;Database=StudentManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

Sesuaikan bagian `Server` dengan nama instance di komputermu (lihat di jendela Connect SSMS). Contoh lain:

- SQL Server Developer/default instance: `Server=localhost;...`
- LocalDB: `Server=(localdb)\\MSSQLLocalDB;...`
- SQL Server di Docker (login `sa`): `Server=localhost,1433;Database=StudentManagementDB;User Id=sa;Password=<password>;TrustServerCertificate=True;`

Agar password tidak ikut ter-commit, connection string juga bisa diisi lewat environment variable `STUDENTDB_CONNECTION`, yang diprioritaskan di atas `appsettings.json`.

## 4. Menjalankan

```bash
cd StudentManagementApp
dotnet run
```

Status bar di bawah menampilkan server yang dipakai dan hasil setiap operasi (titik hijau = berhasil, merah = gagal).

## 5. Struktur project

```
StudentManagementApp/
├── Database/db_student_management.sql   DDL, data contoh, uji constraint, query
├── Data/
│   ├── AppConfig.cs                     membaca connection string
│   ├── StudentRepository.cs             SQL untuk Programs, Students, Courses
│   ├── EnrollmentRepository.cs          SQL untuk Enrollments dan view
│   └── SqlErrorTranslator.cs            nomor error SQL Server → pesan
├── Models/Models.cs                     StudyProgram, Student, Course, EnrollmentDetail, ProgramSummary
├── Services/IDialogService.cs           abstraksi dialog konfirmasi untuk ViewModel
├── ViewModels/
│   ├── MainViewModel.cs                 status bar dan koordinasi antar-tab
│   ├── StudentsViewModel.cs             CRUD mahasiswa
│   ├── EnrollmentsViewModel.cs          KRS dan ringkasan SKS/IPK
│   └── SummaryViewModel.cs              rekap per prodi
├── Views/
│   ├── MainWindow.axaml(.cs)            tampilan tiga tab
│   └── ConfirmDialog.axaml(.cs)         dialog Ya/Batal
├── App.axaml(.cs), Program.cs           tema dan titik masuk
└── appsettings.json
```

## 6. Skenario pengujian

1. Klik **Tambah Mahasiswa** dengan form kosong → pesan validasi NRP.
2. Tambah mahasiswa lengkap → data muncul di tabel dan di dropdown tab KRS.
3. Tambah mahasiswa dengan NRP yang sudah ada → pesan "NRP tersebut sudah terdaftar" (UNIQUE).
4. Pilih baris, ubah nama, klik **Update** → data berubah; cek `SELECT * FROM Students` di SSMS.
5. Hapus mahasiswa yang masih punya KRS → pesan relasi (FOREIGN KEY 547), data tidak terhapus.
6. Di tab KRS, ambil mata kuliah yang sama dua kali pada periode yang sama → pesan duplikat (UNIQUE komposit).
7. Pilih baris KRS, ubah nilai, klik **Update Nilai** → IPK pada ringkasan ikut berubah.
8. Hapus KRS → dialog konfirmasi muncul; pilih **Batal** lalu ulangi dengan **Ya, hapus**.
9. Tutup lalu buka lagi aplikasi → data tetap ada karena tersimpan di SQL Server.
