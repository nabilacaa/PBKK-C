using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using AbsensiMahasiswa.Data;
using AbsensiMahasiswa.Models;
using AbsensiMahasiswa.Services;
using MySqlConnector;

namespace AbsensiMahasiswa.ViewModels
{
    /// <summary>Logika tab "Absensi": catat, ubah, hapus, filter, dan rekap kehadiran.</summary>
    public class AttendanceViewModel : BaseViewModel
    {
        private const string SemuaMatkul = "Semua Mata Kuliah";
        private readonly AttendanceRepository _repo = new();

        /// <summary>Daftar mahasiswa dipakai bersama dengan tab Data Mahasiswa.</summary>
        public ObservableCollection<Student> Students { get; }

        public ObservableCollection<Attendance> Records { get; } = new();
        public ICollectionView RecordsView { get; }

        public List<string> MataKuliahList { get; } = new()
        {
            "Pemrograman Berbasis Kerangka Kerja",
            "Basis Data",
            "Matematika Diskrit",
            "Jaringan Komputer",
            "Kecerdasan Buatan",
            "Robotika"
        };

        public List<string> FilterMatkulList { get; }

        // ---------- Input form ----------
        private Student? _selectedStudentInput;
        public Student? SelectedStudentInput { get => _selectedStudentInput; set => SetField(ref _selectedStudentInput, value); }

        private string? _mataKuliahInput;
        public string? MataKuliahInput { get => _mataKuliahInput; set => SetField(ref _mataKuliahInput, value); }

        private DateTime? _tanggalInput = DateTime.Today;
        public DateTime? TanggalInput { get => _tanggalInput; set => SetField(ref _tanggalInput, value); }

        private string _statusInput = string.Empty;
        public string StatusInput
        {
            get => _statusInput;
            set
            {
                if (SetField(ref _statusInput, value))
                    OnPropertyChanged(nameof(KeteranganHint));
            }
        }

        private string _keteranganInput = string.Empty;
        public string KeteranganInput { get => _keteranganInput; set => SetField(ref _keteranganInput, value); }

        public string KeteranganHint => StatusInput is "Izin" or "Sakit"
            ? "Keterangan (wajib untuk Izin/Sakit):"
            : "Keterangan (opsional):";

        // ---------- Filter ----------
        private string _filterMatkul = SemuaMatkul;
        public string FilterMatkul
        {
            get => _filterMatkul;
            set { if (SetField(ref _filterMatkul, value)) RefreshView(); }
        }

        private DateTime? _filterTanggal;
        public DateTime? FilterTanggal
        {
            get => _filterTanggal;
            set { if (SetField(ref _filterTanggal, value)) RefreshView(); }
        }

        // ---------- Rekap ----------
        public int Total { get; private set; }
        public int Hadir { get; private set; }
        public int Izin { get; private set; }
        public int Sakit { get; private set; }
        public int Alpha { get; private set; }
        public string PersentaseHadir => Total == 0 ? "0%" : $"{Hadir * 100.0 / Total:0.#}%";

        // ---------- Baris terpilih ----------
        private Attendance? _selectedRecord;
        public Attendance? SelectedRecord
        {
            get => _selectedRecord;
            set
            {
                if (!SetField(ref _selectedRecord, value)) return;
                if (value != null)
                {
                    SelectedStudentInput = Students.FirstOrDefault(s => s.Id == value.StudentId);
                    MataKuliahInput = value.MataKuliah;
                    TanggalInput = value.Tanggal;
                    StatusInput = value.Status;
                    KeteranganInput = value.Keterangan;
                }
                OnPropertyChanged(nameof(FormTitle));
            }
        }

        public string FormTitle => SelectedRecord == null ? "Catat Absensi" : "Ubah Absensi";

        // ---------- Command ----------
        public ICommand SaveCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand ClearFilterCommand { get; }

        public AttendanceViewModel(ObservableCollection<Student> students)
        {
            Students = students;
            FilterMatkulList = new List<string> { SemuaMatkul };
            FilterMatkulList.AddRange(MataKuliahList);

            RecordsView = CollectionViewSource.GetDefaultView(Records);
            RecordsView.Filter = FilterRecord;

            SaveCommand = new RelayCommand(_ => SaveRecord(), _ => SelectedRecord == null);
            UpdateCommand = new RelayCommand(_ => UpdateRecord(), _ => SelectedRecord != null);
            DeleteCommand = new RelayCommand(_ => DeleteRecord());
            ResetCommand = new RelayCommand(_ => ResetForm());
            ClearFilterCommand = new RelayCommand(_ =>
            {
                FilterMatkul = SemuaMatkul;
                FilterTanggal = null;
            });
        }

        private bool FilterRecord(object obj)
        {
            if (obj is not Attendance a) return false;
            if (FilterMatkul != SemuaMatkul && a.MataKuliah != FilterMatkul) return false;
            if (FilterTanggal.HasValue && a.Tanggal.Date != FilterTanggal.Value.Date) return false;
            return true;
        }

        public bool LoadData()
        {
            try
            {
                var data = _repo.GetAll();
                Records.Clear();
                foreach (var a in data) Records.Add(a);
                RefreshView();
                return true;
            }
            catch (MySqlException ex)
            {
                DialogService.Error("Gagal memuat data absensi dari database.\n\n" + ex.Message);
                return false;
            }
        }

        /// <summary>Terapkan filter lalu hitung ulang rekap berdasarkan data yang tampil.</summary>
        private void RefreshView()
        {
            RecordsView.Refresh();
            var visible = RecordsView.Cast<Attendance>().ToList();

            Total = visible.Count;
            Hadir = visible.Count(a => a.Status == "Hadir");
            Izin = visible.Count(a => a.Status == "Izin");
            Sakit = visible.Count(a => a.Status == "Sakit");
            Alpha = visible.Count(a => a.Status == "Alpha");

            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(Hadir));
            OnPropertyChanged(nameof(Izin));
            OnPropertyChanged(nameof(Sakit));
            OnPropertyChanged(nameof(Alpha));
            OnPropertyChanged(nameof(PersentaseHadir));
        }

        /// <summary>Validasi input. Mengembalikan pesan error atau null bila valid.</summary>
        private string? Validate(int excludeId)
        {
            if (SelectedStudentInput == null) return "Pilih mahasiswa terlebih dahulu.";
            if (string.IsNullOrEmpty(MataKuliahInput)) return "Pilih mata kuliah terlebih dahulu.";
            if (TanggalInput == null) return "Tanggal absensi harus diisi.";
            if (TanggalInput.Value.Date > DateTime.Today) return "Tanggal absensi tidak boleh melebihi hari ini.";
            if (string.IsNullOrEmpty(StatusInput)) return "Pilih status kehadiran.";
            if (StatusInput is "Izin" or "Sakit" && string.IsNullOrWhiteSpace(KeteranganInput))
                return $"Keterangan wajib diisi untuk status {StatusInput}.";
            if (KeteranganInput.Trim().Length > 200) return "Keterangan maksimal 200 karakter.";

            if (_repo.Exists(SelectedStudentInput.Id, MataKuliahInput, TanggalInput.Value, excludeId))
                return $"{SelectedStudentInput.Nama} sudah memiliki absensi {MataKuliahInput} " +
                       $"pada {TanggalInput.Value:dd MMM yyyy}.";

            return null;
        }

        private Attendance BuildFromForm(int id = 0) => new()
        {
            Id = id,
            StudentId = SelectedStudentInput!.Id,
            MataKuliah = MataKuliahInput!,
            Tanggal = TanggalInput!.Value.Date,
            Status = StatusInput,
            Keterangan = KeteranganInput.Trim()
        };

        private void SaveRecord()
        {
            try
            {
                var error = Validate(0);
                if (error != null) { DialogService.Warning(error); return; }

                _repo.Insert(BuildFromForm());
                AfterChange("Absensi berhasil disimpan.");
            }
            catch (MySqlException ex)
            {
                DialogService.Error("Gagal menyimpan absensi.\n\n" + ex.Message);
            }
        }

        private void UpdateRecord()
        {
            if (SelectedRecord == null) return;
            try
            {
                var error = Validate(SelectedRecord.Id);
                if (error != null) { DialogService.Warning(error); return; }

                _repo.Update(BuildFromForm(SelectedRecord.Id));
                AfterChange("Absensi berhasil diubah.");
            }
            catch (MySqlException ex)
            {
                DialogService.Error("Gagal mengubah absensi.\n\n" + ex.Message);
            }
        }

        private void DeleteRecord()
        {
            if (SelectedRecord == null)
            {
                DialogService.Warning("Pilih data absensi yang ingin dihapus.");
                return;
            }

            var r = SelectedRecord;
            var msg = $"Hapus absensi berikut?\n\n{r.Nrp} - {r.Nama}\n{r.MataKuliah}\n" +
                      $"{r.Tanggal:dd MMM yyyy} ({r.Status})";
            if (!DialogService.Confirm(msg)) return;

            try
            {
                _repo.Delete(r.Id);
                AfterChange("Absensi berhasil dihapus.");
            }
            catch (MySqlException ex)
            {
                DialogService.Error("Gagal menghapus absensi.\n\n" + ex.Message);
            }
        }

        private void AfterChange(string message)
        {
            LoadData();
            ResetForm();
            DialogService.Info(message);
        }

        private void ResetForm()
        {
            SelectedRecord = null;
            SelectedStudentInput = null;
            MataKuliahInput = null;
            TanggalInput = DateTime.Today;
            StatusInput = string.Empty;
            KeteranganInput = string.Empty;
        }
    }
}
