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
/// Tab "KRS": transaksi pengambilan mata kuliah (tabel penghubung Enrollments).
/// </summary>
public partial class EnrollmentsViewModel : ViewModelBase
{
    public const string NoGrade = "Belum ada";
    private static readonly Regex AcademicYearPattern = new(@"^\d{4}/\d{4}$", RegexOptions.Compiled);

    private readonly EnrollmentRepository _repo;
    private readonly StudentRepository _studentRepo;
    private readonly IDialogService _dialogs;
    private readonly Action<string, bool> _setStatus;

    public ObservableCollection<Student> Students { get; } = new();
    public ObservableCollection<Course> Courses { get; } = new();
    public ObservableCollection<EnrollmentDetail> Items { get; } = new();

    public string[] Terms { get; } = { "Gasal", "Genap" };
    public string[] Grades { get; } = { NoGrade, "A", "AB", "B", "BC", "C", "D", "E" };

    // ----- Form -----
    [ObservableProperty] private Student? _selectedStudent;
    [ObservableProperty] private Course? _selectedCourse;
    [ObservableProperty] private string _academicYear = DefaultAcademicYear();
    [ObservableProperty] private string _term = DateTime.Now.Month >= 8 ? "Gasal" : "Genap";
    [ObservableProperty] private string _grade = NoGrade;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelection))]
    private EnrollmentDetail? _selectedEnrollment;

    // Filter tabel hanya untuk mahasiswa terpilih
    [ObservableProperty] private bool _filterBySelectedStudent;

    // ----- Ringkasan -----
    [ObservableProperty] private int _totalCourses;
    [ObservableProperty] private int _totalCredits;
    [ObservableProperty] private string _gpaText = "-";
    [ObservableProperty] private string _summaryCaption = "Semua mahasiswa";

    public bool HasSelection => SelectedEnrollment is not null;

    public EnrollmentsViewModel(EnrollmentRepository repo, StudentRepository studentRepo,
                                IDialogService dialogs, Action<string, bool> setStatus)
    {
        _repo = repo;
        _studentRepo = studentRepo;
        _dialogs = dialogs;
        _setStatus = setStatus;
    }

    private static string DefaultAcademicYear()
    {
        var now = DateTime.Now;
        var start = now.Month >= 8 ? now.Year : now.Year - 1;
        return $"{start}/{start + 1}";
    }

    /// <summary>Isi ulang ComboBox mahasiswa dan mata kuliah dari database (tanpa mengubah status).</summary>
    public async Task ReloadMasterDataAsync()
    {
        try
        {
            var keepStudent = SelectedStudent?.StudentId;
            var keepCourse = SelectedCourse?.CourseId;

            var students = await _studentRepo.GetStudentsAsync();
            var courses = await _studentRepo.GetCoursesAsync();

            Students.Clear();
            foreach (var s in students) Students.Add(s);
            Courses.Clear();
            foreach (var c in courses) Courses.Add(c);

            SelectedStudent = Students.FirstOrDefault(s => s.StudentId == keepStudent);
            SelectedCourse = Courses.FirstOrDefault(c => c.CourseId == keepCourse);

            await ReloadAsync();
        }
        catch (SqlException ex)
        {
            _setStatus(SqlErrorTranslator.Translate(ex), false);
        }
    }

    partial void OnSelectedStudentChanged(Student? value)
    {
        if (FilterBySelectedStudent) _ = RunAsync(ReloadAsync);
    }

    partial void OnFilterBySelectedStudentChanged(bool value) => _ = RunAsync(ReloadAsync);

    partial void OnSelectedEnrollmentChanged(EnrollmentDetail? value)
    {
        if (value is null) return;

        // Mengisi form dari baris yang dipilih, termasuk nilai yang bisa di-update.
        Grade = value.Grade ?? NoGrade;
        _setStatus($"KRS {value.Nrp} - {value.CourseCode} ({value.Period}) dipilih. " +
                   "Ubah nilai lalu klik Update Nilai, atau klik Hapus.", true);
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        await RunAsync(async () =>
        {
            await ReloadAsync();
            _setStatus($"Memuat {Items.Count} data KRS.", true);
        });
    }

    private async Task ReloadAsync()
    {
        int? studentId = FilterBySelectedStudent ? SelectedStudent?.StudentId : null;
        var list = await _repo.GetEnrollmentsAsync(studentId);

        Items.Clear();
        foreach (var e in list) Items.Add(e);

        // Ringkasan dihitung dari data yang sedang tampil.
        TotalCourses = Items.Count;
        TotalCredits = Items.Sum(e => e.Credits);

        var graded = Items.Where(e => e.GradePoint.HasValue).ToList();
        var gradedCredits = graded.Sum(e => e.Credits);
        GpaText = gradedCredits == 0
            ? "-"
            : (graded.Sum(e => e.GradePoint!.Value * e.Credits) / gradedCredits).ToString("0.00");

        SummaryCaption = studentId is null ? "Semua mahasiswa" : SelectedStudent!.Display;
    }

    [RelayCommand]
    private async Task AddAsync()
    {
        string? error = null;
        var year = AcademicYear.Trim();

        if (SelectedStudent is null) error = "Pilih mahasiswa.";
        else if (SelectedCourse is null) error = "Pilih mata kuliah.";
        else if (!AcademicYearPattern.IsMatch(year) ||
                 int.Parse(year[5..]) != int.Parse(year[..4]) + 1)
            error = "Tahun ajaran harus berformat YYYY/YYYY dan berurutan, contoh 2026/2027.";

        if (error is not null)
        {
            _setStatus("Validasi gagal: " + error, false);
            return;
        }

        var student = SelectedStudent!;
        var course = SelectedCourse!;

        await RunAsync(async () =>
        {
            await _repo.AddEnrollmentAsync(student.StudentId, course.CourseId, year, Term, ToDbGrade(Grade));
            await ReloadAsync();
            _setStatus($"{student.FullName} berhasil mengambil {course.CourseName} ({year} {Term}).", true);
        });
    }

    [RelayCommand]
    private async Task UpdateGradeAsync()
    {
        if (SelectedEnrollment is not { } target)
        {
            _setStatus("Pilih baris KRS pada tabel untuk mengubah nilai.", false);
            return;
        }

        var grade = ToDbGrade(Grade);
        await RunAsync(async () =>
        {
            var rows = await _repo.UpdateGradeAsync(target.EnrollmentId, grade);
            await ReloadAsync();
            _setStatus(rows > 0
                ? $"Nilai {target.Nrp} untuk {target.CourseCode} diubah menjadi {grade ?? "kosong"}."
                : "Data KRS tidak ditemukan.", rows > 0);
        });
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (SelectedEnrollment is not { } target)
        {
            _setStatus("Pilih baris KRS pada tabel terlebih dahulu untuk dihapus.", false);
            return;
        }

        var ok = await _dialogs.ConfirmAsync("Hapus KRS",
            $"Hapus {target.CourseName} dari KRS {target.FullName} ({target.Period})?");
        if (!ok)
        {
            _setStatus("Penghapusan dibatalkan.", true);
            return;
        }

        await RunAsync(async () =>
        {
            var rows = await _repo.DeleteEnrollmentAsync(target.EnrollmentId);
            await ReloadAsync();
            _setStatus(rows > 0 ? "Data KRS berhasil dihapus." : "Data KRS tidak ditemukan.", rows > 0);
        });
    }

    [RelayCommand]
    private void ClearForm()
    {
        SelectedEnrollment = null;
        SelectedCourse = null;
        Grade = NoGrade;
        AcademicYear = DefaultAcademicYear();
    }

    private static string? ToDbGrade(string grade) => grade == NoGrade ? null : grade;

    private async Task RunAsync(Func<Task> action)
    {
        try
        {
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
    }
}
