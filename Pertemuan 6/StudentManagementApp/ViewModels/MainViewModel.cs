using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentManagementApp.Data;
using StudentManagementApp.Services;

namespace StudentManagementApp.ViewModels;

/// <summary>
/// ViewModel jendela utama: memegang ViewModel tiap tab dan status bar bersama.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _statusMessage = "Menyiapkan koneksi ke SQL Server...";

    [ObservableProperty]
    private bool _isSuccess = true;

    [ObservableProperty]
    private string _serverInfo = string.Empty;

    public StudentsViewModel Students { get; }
    public EnrollmentsViewModel Enrollments { get; }
    public SummaryViewModel Summary { get; }

    // Konstruktor untuk designer XAML.
    public MainViewModel() : this(new StudentRepository(), new EnrollmentRepository(), new AlwaysYesDialogService(), autoLoad: false)
    {
    }

    public MainViewModel(StudentRepository studentRepo, EnrollmentRepository enrollmentRepo,
                         IDialogService dialogs, bool autoLoad = true)
    {
        Students = new StudentsViewModel(studentRepo, dialogs, SetStatus);
        Enrollments = new EnrollmentsViewModel(enrollmentRepo, studentRepo, dialogs, SetStatus);
        Summary = new SummaryViewModel(studentRepo, SetStatus);

        // Data mahasiswa berubah -> daftar mahasiswa di tab KRS dan ringkasan ikut diperbarui.
        Students.DataChanged += async () =>
        {
            await Enrollments.ReloadMasterDataAsync();
            await Summary.LoadAsync();
        };

        ServerInfo = DescribeServer(AppConfig.ConnectionString);

        if (autoLoad)
            _ = LoadAllAsync();
    }

    [RelayCommand]
    private async Task LoadAllAsync()
    {
        SetStatus("Memuat data dari SQL Server...", true);
        await Students.InitializeAsync();
        await Enrollments.ReloadMasterDataAsync();
        await Summary.LoadAsync();

        if (IsSuccess)
            SetStatus($"Terhubung. {Students.Items.Count} mahasiswa dan {Enrollments.Items.Count} data KRS dimuat.", true);
    }

    private void SetStatus(string message, bool success)
    {
        StatusMessage = message;
        IsSuccess = success;
    }

    private static string DescribeServer(string connectionString)
    {
        try
        {
            var b = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
            return $"{b.DataSource} • {b.InitialCatalog}";
        }
        catch (Exception)
        {
            return "Connection string tidak valid";
        }
    }
}
