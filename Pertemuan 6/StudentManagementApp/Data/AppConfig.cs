using System;
using System.IO;
using System.Text.Json;

namespace StudentManagementApp.Data;

/// <summary>
/// Membaca connection string dari environment variable STUDENTDB_CONNECTION,
/// lalu dari appsettings.json. Kredensial tidak ditulis di source code.
/// </summary>
public static class AppConfig
{
    public const string EnvironmentKey = "STUDENTDB_CONNECTION";

    private const string Fallback =
        @"Server=localhost\SQLEXPRESS;Database=StudentManagementDB;Trusted_Connection=True;TrustServerCertificate=True;";

    public static string ConnectionString { get; } = Load();

    private static string Load()
    {
        var fromEnv = Environment.GetEnvironmentVariable(EnvironmentKey);
        if (!string.IsNullOrWhiteSpace(fromEnv))
            return fromEnv;

        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (File.Exists(path))
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            if (doc.RootElement.TryGetProperty("ConnectionStrings", out var cs) &&
                cs.TryGetProperty("Default", out var value) &&
                !string.IsNullOrWhiteSpace(value.GetString()))
            {
                return value.GetString()!;
            }
        }

        return Fallback;
    }
}
