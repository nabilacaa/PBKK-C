using Microsoft.Data.SqlClient;

namespace StudentManagementApp.Data;

/// <summary>
/// Menerjemahkan nomor error SQL Server menjadi pesan yang mudah dibaca.
/// 2627/2601 = pelanggaran UNIQUE, 547 = pelanggaran FOREIGN KEY atau CHECK.
/// </summary>
public static class SqlErrorTranslator
{
    public static string Translate(SqlException ex)
    {
        var msg = ex.Message;
        return ex.Number switch
        {
            2627 or 2601 when msg.Contains("UQ_Students_NRP") => "NRP tersebut sudah terdaftar.",
            2627 or 2601 when msg.Contains("UQ_Students_Email") => "Email tersebut sudah dipakai mahasiswa lain.",
            2627 or 2601 when msg.Contains("UQ_Enrollments") =>
                "Mahasiswa sudah mengambil mata kuliah ini pada periode yang sama.",
            2627 or 2601 => "Data yang sama sudah ada di database.",
            547 when msg.Contains("FK_Enrollments_Students") =>
                "Mahasiswa ini masih memiliki data KRS. Hapus KRS-nya terlebih dahulu.",
            547 when msg.Contains("CK_") => $"Data ditolak oleh aturan CHECK database: {msg}",
            547 => $"Data ditolak karena melanggar relasi antar-tabel: {msg}",
            -1 or 2 or 53 or 4060 or 18456 =>
                "Tidak dapat terhubung ke SQL Server. Periksa connection string di appsettings.json " +
                "dan pastikan database StudentManagementDB sudah dibuat.",
            _ => $"SQL Server error {ex.Number}: {msg}"
        };
    }
}
