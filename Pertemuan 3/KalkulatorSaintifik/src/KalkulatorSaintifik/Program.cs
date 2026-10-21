using Avalonia;

namespace KalkulatorSaintifik;

internal static class Program
{
    // Titik awal aplikasi. Jangan memakai API Avalonia sebelum AppMain dipanggil.
    [STAThread]
    public static void Main(string[] args) =>
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    // Juga dipakai oleh visual designer di IDE.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
