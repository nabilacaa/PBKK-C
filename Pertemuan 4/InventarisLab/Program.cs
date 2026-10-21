using Avalonia;
using System;

namespace InventarisLab;

class Program
{
    // Titik masuk aplikasi. Jangan memakai API Avalonia sebelum AppMain dipanggil.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Konfigurasi Avalonia, juga dipakai oleh visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
