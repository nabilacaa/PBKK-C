using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.SqlClient;
using StudentManagementApp.Data;
using StudentManagementApp.Models;

namespace StudentManagementApp.ViewModels;

/// <summary>
/// Tab "Ringkasan": hasil query GROUP BY + COUNT jumlah mahasiswa per program studi.
/// </summary>
public partial class SummaryViewModel : ViewModelBase
{
    private readonly StudentRepository _repo;
    private readonly Action<string, bool> _setStatus;

    public ObservableCollection<ProgramSummary> Items { get; } = new();

    [ObservableProperty] private int _totalStudents;

    public SummaryViewModel(StudentRepository repo, Action<string, bool> setStatus)
    {
        _repo = repo;
        _setStatus = setStatus;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            var list = await _repo.GetProgramSummaryAsync();
            Items.Clear();
            foreach (var row in list) Items.Add(row);
            TotalStudents = Items.Sum(i => i.StudentCount);
        }
        catch (SqlException ex)
        {
            _setStatus(SqlErrorTranslator.Translate(ex), false);
        }
    }
}
