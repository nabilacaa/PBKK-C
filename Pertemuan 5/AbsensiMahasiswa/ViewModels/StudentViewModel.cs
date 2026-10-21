using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Input;
using AbsensiMahasiswa.Data;
using AbsensiMahasiswa.Models;
using AbsensiMahasiswa.Services;
using MySqlConnector;

namespace AbsensiMahasiswa.ViewModels
{
    /// <summary>Logika tab "Data Mahasiswa": tambah, ubah, hapus, reset, cari.</summary>
    public class StudentViewModel : BaseViewModel
    {
        private readonly StudentRepository _repo = new();

        /// <summary>Dipicu setelah data mahasiswa berubah, agar tab Absensi ikut memuat ulang.</summary>
        public event Action? DataChanged;

        public ObservableCollection<Student> Students { get; } = new();
        public ICollectionView StudentsView { get; }

        public List<string> JurusanList { get; } = new()
        {
            "Teknik Informatika",
            "Sistem Informasi",
            "Teknologi Informasi",
            "Teknik Komputer",
            "Sains Data"
        };

        // ---------- Input form ----------
        private string _nrpInput = string.Empty;
        public string NrpInput { get => _nrpInput; set => SetField(ref _nrpInput, value); }

        private string _namaInput = string.Empty;
        public string NamaInput { get => _namaInput; set => SetField(ref _namaInput, value); }

        private string? _jurusanInput;
        public string? JurusanInput { get => _jurusanInput; set => SetField(ref _jurusanInput, value); }

        private string _emailInput = string.Empty;
        public string EmailInput { get => _emailInput; set => SetField(ref _emailInput, value); }

        // ---------- Pencarian ----------
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetField(ref _searchText, value))
                {
                    StudentsView.Refresh();
                    OnPropertyChanged(nameof(CountText));
                }
            }
        }

        public string CountText => $"{StudentsView.Cast<object>().Count()} dari {Students.Count} mahasiswa";

        // ---------- Baris terpilih ----------
        private Student? _selectedStudent;
        public Student? SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                if (!SetField(ref _selectedStudent, value)) return;
                if (value != null)
                {
                    NrpInput = value.Nrp;
                    NamaInput = value.Nama;
                    JurusanInput = value.Jurusan;
                    EmailInput = value.Email;
                }
                OnPropertyChanged(nameof(FormTitle));
            }
        }

        public string FormTitle => SelectedStudent == null ? "Tambah Mahasiswa" : "Ubah Mahasiswa";

        // ---------- Command ----------
        public ICommand AddCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand RefreshCommand { get; }

        public StudentViewModel()
        {
            StudentsView = CollectionViewSource.GetDefaultView(Students);
            StudentsView.Filter = FilterStudent;

            AddCommand = new RelayCommand(_ => AddStudent(), _ => SelectedStudent == null);
            UpdateCommand = new RelayCommand(_ => UpdateStudent(), _ => SelectedStudent != null);
            DeleteCommand = new RelayCommand(_ => DeleteStudent());
            ResetCommand = new RelayCommand(_ => ResetForm());
            RefreshCommand = new RelayCommand(_ => LoadData());
        }

        private bool FilterStudent(object obj)
        {
            if (obj is not Student s) return false;
            if (string.IsNullOrWhiteSpace(SearchText)) return true;
            var key = SearchText.Trim();
            return s.Nrp.Contains(key, StringComparison.OrdinalIgnoreCase)
                || s.Nama.Contains(key, StringComparison.OrdinalIgnoreCase)
                || s.Jurusan.Contains(key, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Memuat ulang data dari database. Mengembalikan false bila koneksi gagal.</summary>
        public bool LoadData()
        {
            try
            {
                var data = _repo.GetAll();
                Students.Clear();
                foreach (var s in data) Students.Add(s);
                OnPropertyChanged(nameof(CountText));
                return true;
            }
            catch (MySqlException ex)
            {
                DialogService.Error("Gagal memuat data mahasiswa dari database.\n\n" + ex.Message);
                return false;
            }
        }

        /// <summary>Validasi seluruh input. Mengembalikan pesan error atau null bila valid.</summary>
        private string? Validate(int excludeId)
        {
            var nrp = NrpInput.Trim();
            var nama = NamaInput.Trim();
            var email = EmailInput.Trim();

            if (string.IsNullOrEmpty(nrp)) return "NRP harus diisi.";
            if (!Regex.IsMatch(nrp, @"^\d+$")) return "NRP hanya boleh berisi angka.";
            if (nrp.Length != 10) return "NRP harus terdiri dari 10 digit.";

            if (string.IsNullOrEmpty(nama)) return "Nama harus diisi.";
            if (nama.Length < 3) return "Nama minimal 3 karakter.";
            if (!Regex.IsMatch(nama, @"^[A-Za-z .'\-]+$")) return "Nama hanya boleh berisi huruf, spasi, titik, petik, atau tanda hubung.";

            if (string.IsNullOrEmpty(JurusanInput)) return "Pilih jurusan terlebih dahulu.";

            if (string.IsNullOrEmpty(email)) return "Email harus diisi.";
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) return "Format email tidak valid.";

            if (_repo.NrpExists(nrp, excludeId)) return $"NRP {nrp} sudah terdaftar.";

            return null;
        }

        private Student BuildFromForm(int id = 0) => new()
        {
            Id = id,
            Nrp = NrpInput.Trim(),
            Nama = NamaInput.Trim(),
            Jurusan = JurusanInput ?? string.Empty,
            Email = EmailInput.Trim()
        };

        private void AddStudent()
        {
            try
            {
                var error = Validate(0);
                if (error != null) { DialogService.Warning(error); return; }

                _repo.Insert(BuildFromForm());
                AfterChange("Data mahasiswa berhasil ditambahkan.");
            }
            catch (MySqlException ex)
            {
                DialogService.Error("Gagal menyimpan data.\n\n" + ex.Message);
            }
        }

        private void UpdateStudent()
        {
            if (SelectedStudent == null) return;
            try
            {
                var error = Validate(SelectedStudent.Id);
                if (error != null) { DialogService.Warning(error); return; }

                _repo.Update(BuildFromForm(SelectedStudent.Id));
                AfterChange("Data mahasiswa berhasil diubah.");
            }
            catch (MySqlException ex)
            {
                DialogService.Error("Gagal mengubah data.\n\n" + ex.Message);
            }
        }

        private void DeleteStudent()
        {
            if (SelectedStudent == null)
            {
                DialogService.Warning("Pilih mahasiswa yang ingin dihapus.");
                return;
            }

            var msg = $"Hapus {SelectedStudent.Nama} ({SelectedStudent.Nrp})?\n" +
                      "Seluruh data absensi mahasiswa ini juga akan terhapus.";
            if (!DialogService.Confirm(msg)) return;

            try
            {
                _repo.Delete(SelectedStudent.Id);
                AfterChange("Data mahasiswa berhasil dihapus.");
            }
            catch (MySqlException ex)
            {
                DialogService.Error("Gagal menghapus data.\n\n" + ex.Message);
            }
        }

        private void AfterChange(string message)
        {
            LoadData();
            ResetForm();
            DataChanged?.Invoke();
            DialogService.Info(message);
        }

        private void ResetForm()
        {
            SelectedStudent = null;
            NrpInput = string.Empty;
            NamaInput = string.Empty;
            JurusanInput = null;
            EmailInput = string.Empty;
        }
    }
}
