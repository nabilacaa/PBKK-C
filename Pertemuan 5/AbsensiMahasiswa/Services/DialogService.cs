using System.Windows;

namespace AbsensiMahasiswa.Services
{
    /// <summary>
    /// Pembungkus MessageBox agar ViewModel tidak memanggil MessageBox secara langsung.
    /// </summary>
    public static class DialogService
    {
        public static void Info(string message, string title = "Berhasil") =>
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

        public static void Warning(string message, string title = "Validasi") =>
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

        public static void Error(string message, string title = "Kesalahan") =>
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

        public static bool Confirm(string message, string title = "Konfirmasi") =>
            MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes;
    }
}
