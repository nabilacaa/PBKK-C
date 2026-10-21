using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.SqlClient;
using StudentManagementApp.Data;
using StudentManagementApp.Models;
using StudentManagementApp.Services;

namespace StudentManagementApp.ViewModels;

/// <summary>
/// Tab "Mahasiswa": CRUD lengkap tabel Students.
/// </summary>
public partial class StudentsViewModel : ViewModelBase
{
    private static readonly Regex EmailPattern = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    private readonly StudentRepository _repo;
    private readonly IDialogService _dialogs;
    private readonly Action<string, bool> _setStatus;

    public event Func<Task>? DataChanged;

    public ObservableCollection<Student> Items { get; } = new();
    public ObservableCollection<StudyProgram> Programs { get; } = new();
    public string[] Genders { get; } = { "L", "P" };

    // ----- Form -----
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditing), nameof(FormTitle))]
    private int _studentId;

    [ObservableProperty] private string _nrp = string.Empty;
    [ObservableProperty] private string _fullName = string.Empty;
    [ObservableProperty] private string _gender = "L";
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _entryYear = DateTime.Now.Year.ToString();
    [ObservableProperty] private string _phoneNumber = string.Empty;
    [ObservableProperty] private StudyProgram? _selectedProgram;

    [ObservableProperty] private Student? _selectedStudent;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private bool _isBusy;

    public bool IsEditing => StudentId > 0;
    public string FormTitle => IsEditing ? $"Edit Mahasiswa #{StudentId}" : "Mahasiswa Baru";

    public StudentsViewModel(StudentRepository repo, IDialogService dialogs, Action<string, bool> setStatus)
    {
        _repo = repo;
        _dialogs = dialogs;
        _setStatus = setStatus;
    }

    public async Task InitializeAsync()
    {
        try
        {
            Programs.Clear();
            foreach (var p in await _repo.GetProgramsAsync())
                Programs.Add(p);
            SelectedProgram ??= Programs.FirstOrDefault();
        }
        catch (SqlException ex)
        {
            _setStatus(SqlErrorTranslator.Translate(ex), false);
            return;
        }
        await LoadAsync();
    }

    partial void OnSelectedStudentChanged(Student? value)
    {
        if (value is null) return;

        StudentId = value.StudentId;
        Nrp = value.Nrp;
        FullName = value.FullName;
        Gender = value.Gender;
        Email = value.Email;
        EntryYear = value.EntryYear.ToString();
        PhoneNumber = value.PhoneNumber ?? string.Empty;
        SelectedProgram = Programs.FirstOrDefault(p => p.ProgramId == value.ProgramId);
        _setStatus($"'{value.FullName}' dipilih. Ubah data lalu klik Update, atau klik Hapus.", true);
    }

    [RelayCommand]
    private async Task SearchAsync() => await LoadAsync();

    [RelayCommand]
    private async Task LoadAsync() => await RunAsync(() => ReloadAsync(announce: true));

    private async Task ReloadAsync(bool announce)
    {
        var list = await _repo.GetStudentsAsync(SearchText);
        Items.Clear();
        foreach (var s in list) Items.Add(s);

        if (announce)
        {
            var filter = string.IsNullOrWhiteSpace(SearchText) ? "" : $" untuk kata kunci '{SearchText.Trim()}'";
            _setStatus($"Memuat {Items.Count} mahasiswa{filter}.", true);
        }
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        if (!TryBuildStudent(out var student)) return;

        await RunAsync(async () =>
        {
            await _repo.InsertStudentAsync(student);
            _setStatus($"Mahasiswa {student.FullName} ({student.Nrp}) berhasil ditambahkan.", true);
            await AfterChangeAsync();
        });
    }

    [RelayCommand]
    private async Task UpdateAsync()
    {
        if (!IsEditing)
        {
            _setStatus("Pilih mahasiswa pada tabel terlebih dahulu untuk di-update.", false);
            return;
        }
        if (!TryBuildStudent(out var student)) return;
        student.StudentId = StudentId;

        await RunAsync(async () =>
        {
            var rows = await _repo.UpdateStudentAsync(student);
            if (rows == 0)
            {
                _setStatus($"Data #{student.StudentId} tidak ditemukan (mungkin sudah dihapus).", false);
                return;
            }
            _setStatus($"Data {student.FullName} berhasil diperbarui.", true);
            await AfterChangeAsync();
        });
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (!IsEditing)
        {
            _setStatus("Pilih mahasiswa pada tabel terlebih dahulu untuk dihapus.", false);
            return;
        }

        var id = StudentId;
        var name = FullName;
        var ok = await _dialogs.ConfirmAsync("Hapus Mahasiswa",
            $"Hapus data {name} ({Nrp})?\nTindakan ini tidak dapat dibatalkan.");
        if (!ok)
        {
            _setStatus("Penghapusan dibatalkan.", true);
            return;
        }

        await RunAsync(async () =>
        {
            var rows = await _repo.DeleteStudentAsync(id);
            _setStatus(rows > 0 ? $"Data {name} berhasil dihapus." : $"Data #{id} tidak ditemukan.", rows > 0);
            await AfterChangeAsync();
        });
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedStudent = null;
        StudentId = 0;
        Nrp = FullName = Email = PhoneNumber = string.Empty;
        Gender = "L";
        EntryYear = DateTime.Now.Year.ToString();
        SelectedProgram = Programs.FirstOrDefault();
    }

    /// <summary>Setelah INSERT/UPDATE/DELETE: kosongkan form dan muat ulang tanpa menimpa pesan sukses.</summary>
    private async Task AfterChangeAsync()
    {
        ClearForm();
        await ReloadAsync(announce: false);
        if (DataChanged is not null) await DataChanged.Invoke();
    }

    private bool TryBuildStudent(out Student student)
    {
        student = new Student();
        string? error = null;

        var nrp = Nrp.Trim();
        var email = Email.Trim();

        if (nrp.Length != 10 || !nrp.All(char.IsDigit))
            error = "NRP harus 10 digit angka.";
        else if (string.IsNullOrWhiteSpace(FullName))
            error = "Nama lengkap wajib diisi.";
        else if (!EmailPattern.IsMatch(email))
            error = "Format email tidak valid (contoh: nama@student.its.ac.id).";
        else if (!short.TryParse(EntryYear, out var year) || year < 2000 || year > 2100)
            error = "Angkatan harus berupa tahun antara 2000 dan 2100.";
        else if (SelectedProgram is null)
            error = "Pilih program studi.";
        else
        {
            student = new Student
            {
                Nrp = nrp,
                FullName = FullName.Trim(),
                Gender = Gender,
                Email = email,
                EntryYear = year,
                PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim(),
                ProgramId = SelectedProgram.ProgramId
            };
        }

        if (error is not null)
        {
            _setStatus("Validasi gagal: " + error, false);
            return false;
        }
        return true;
    }

    private async Task RunAsync(Func<Task> action)
    {
        try
        {
            IsBusy = true;
            await action();
        }
        catch (SqlException ex)
        {
            _setStatus(SqlErrorTranslator.Translate(ex), false);
        }
        catch (Exception ex)
        {
            _setStatus($"Terjadi kesalahan: {ex.Message}", false);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
