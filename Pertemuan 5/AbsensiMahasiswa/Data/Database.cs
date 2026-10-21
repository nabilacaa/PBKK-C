using MySqlConnector;

namespace AbsensiMahasiswa.Data
{
    /// <summary>Titik tunggal pembuatan koneksi ke MySQL.</summary>
    public static class Database
    {
        // Sesuaikan dengan konfigurasi MySQL di komputer masing-masing.
        // Jangan commit password asli ke repository.
        private const string ConnectionString =
            "Server=127.0.0.1;Port=3306;Database=absensi_db;User ID=root;Password=root;";

        public static MySqlConnection OpenConnection()
        {
            var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }
    }
}
