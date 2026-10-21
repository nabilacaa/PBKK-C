using System.Threading.Tasks;

namespace StudentManagementApp.Services;

/// <summary>
/// Abstraksi dialog agar ViewModel bisa meminta konfirmasi tanpa mengenal kelas Window.
/// </summary>
public interface IDialogService
{
    Task<bool> ConfirmAsync(string title, string message);
}

/// <summary>Dipakai di designer/pengujian: selalu menjawab "Ya".</summary>
public class AlwaysYesDialogService : IDialogService
{
    public Task<bool> ConfirmAsync(string title, string message) => Task.FromResult(true);
}
