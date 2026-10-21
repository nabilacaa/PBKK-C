# Sistem Absensi Mahasiswa (WPF + MVVM + MySQL)

Aplikasi desktop C# WPF dengan pola MVVM dan database MySQL untuk mengelola data mahasiswa dan mencatat kehadiran per mata kuliah.

## Fitur

**Tab Data Mahasiswa**
- Tambah, ubah, hapus, dan reset data mahasiswa (NRP, nama, jurusan, email).
- Validasi: NRP wajib 10 digit angka dan unik, nama minimal 3 huruf, jurusan wajib dipilih, format email valid.
- TextBox NRP menolak input non-angka sejak diketik.
- Pencarian langsung berdasarkan NRP, nama, atau jurusan.
- Konfirmasi sebelum menghapus (absensi mahasiswa tersebut ikut terhapus).

**Tab Absensi**
- Catat absensi: mahasiswa, mata kuliah, tanggal, status (Hadir/Izin/Sakit/Alpha), keterangan.
- Validasi: semua field wajib, tanggal tidak boleh melebihi hari ini, keterangan wajib untuk Izin/Sakit.
- Pencegahan absensi ganda (mahasiswa + mata kuliah + tanggal sama), dicek di aplikasi dan dijaga constraint `UNIQUE` di database.
- Ubah dan hapus absensi dengan dialog konfirmasi.
- Filter berdasarkan mata kuliah dan tanggal.
- Rekap otomatis Total, Hadir, Izin, Sakit, Alpha, dan persentase kehadiran sesuai filter.

## Kebutuhan

- Windows 10/11
- .NET SDK 8 atau lebih baru (Visual Studio 2022 / VS Code + C# Dev Kit)
- MySQL atau MariaDB (XAMPP / Laragon / MySQL Server)

## Cara Menjalankan

1. Jalankan MySQL, lalu eksekusi `database.sql` (via phpMyAdmin, MySQL Workbench, atau `mysql -u root < database.sql`).
2. Sesuaikan connection string di `Data/Database.cs` bila user/password MySQL berbeda.
3. Di folder proyek, jalankan:
   ```
   dotnet restore
   dotnet run
   ```
   atau buka `AbsensiMahasiswa.csproj` di Visual Studio lalu tekan F5.

## Struktur Proyek

```
AbsensiMahasiswa/
├── App.xaml / App.xaml.cs            konfigurasi aplikasi + style global
├── Models/
│   ├── Student.cs                    model mahasiswa
│   └── Attendance.cs                 model absensi
├── Data/
│   ├── Database.cs                   connection string + pembuka koneksi
│   ├── StudentRepository.cs          CRUD tabel students
│   └── AttendanceRepository.cs       CRUD tabel attendances + cek data ganda
├── ViewModels/
│   ├── BaseViewModel.cs              INotifyPropertyChanged + SetField
│   ├── RelayCommand.cs               implementasi ICommand
│   ├── StudentViewModel.cs           logika tab Data Mahasiswa
│   ├── AttendanceViewModel.cs        logika tab Absensi + filter + rekap
│   └── MainViewModel.cs              induk yang menyatukan kedua tab
├── Services/DialogService.cs         pembungkus MessageBox
├── Converters/StatusToBoolConverter.cs  binding RadioButton ke property Status
├── Views/MainWindow.xaml(.cs)        tampilan utama
└── database.sql                      skema + data contoh
```

## Pembagian MVVM

- **Model**: `Student`, `Attendance`.
- **View**: `MainWindow.xaml`, hanya berisi layout dan binding. Code-behind hanya menangani urusan tampilan (event `Loaded`, `Closing`, dan `PreviewTextInput` NRP).
- **ViewModel**: `StudentViewModel`, `AttendanceViewModel`, `MainViewModel` mengelola state, validasi, dan command.
- **Data access**: repository menangani query MySQL dengan parameter (`@nrp`, `@sid`, dll.) untuk mencegah SQL injection.

## Control yang Digunakan

TextBlock, TextBox, ComboBox, DatePicker, RadioButton, Button, DataGrid, TabControl, Border, WrapPanel, ScrollViewer.
