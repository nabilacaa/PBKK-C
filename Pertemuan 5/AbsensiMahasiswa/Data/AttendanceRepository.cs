using AbsensiMahasiswa.Models;
using MySqlConnector;

namespace AbsensiMahasiswa.Data
{
    /// <summary>Operasi CRUD untuk tabel attendances.</summary>
    public class AttendanceRepository
    {
        public List<Attendance> GetAll()
        {
            var list = new List<Attendance>();
            using var conn = Database.OpenConnection();
            const string sql =
                "SELECT a.id, a.student_id, s.nrp, s.nama, a.mata_kuliah, a.tanggal, a.status, a.keterangan " +
                "FROM attendances a JOIN students s ON s.id = a.student_id " +
                "ORDER BY a.tanggal DESC, s.nrp";
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new Attendance
                {
                    Id = reader.GetInt32("id"),
                    StudentId = reader.GetInt32("student_id"),
                    Nrp = reader.GetString("nrp"),
                    Nama = reader.GetString("nama"),
                    MataKuliah = reader.GetString("mata_kuliah"),
                    Tanggal = reader.GetDateTime("tanggal"),
                    Status = reader.GetString("status"),
                    Keterangan = reader.GetString("keterangan")
                });
            }
            return list;
        }

        /// <summary>
        /// Cek absensi ganda: mahasiswa, mata kuliah, dan tanggal yang sama.
        /// excludeId dipakai saat mengubah data agar record itu sendiri tidak terhitung.
        /// </summary>
        public bool Exists(int studentId, string mataKuliah, DateTime tanggal, int excludeId = 0)
        {
            using var conn = Database.OpenConnection();
            const string sql =
                "SELECT COUNT(*) FROM attendances " +
                "WHERE student_id=@sid AND mata_kuliah=@mk AND tanggal=@tgl AND id <> @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@sid", studentId);
            cmd.Parameters.AddWithValue("@mk", mataKuliah);
            cmd.Parameters.AddWithValue("@tgl", tanggal.Date);
            cmd.Parameters.AddWithValue("@id", excludeId);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public void Insert(Attendance a)
        {
            using var conn = Database.OpenConnection();
            const string sql =
                "INSERT INTO attendances (student_id, mata_kuliah, tanggal, status, keterangan) " +
                "VALUES (@sid, @mk, @tgl, @status, @ket)";
            using var cmd = new MySqlCommand(sql, conn);
            AddParameters(cmd, a);
            cmd.ExecuteNonQuery();
        }

        public void Update(Attendance a)
        {
            using var conn = Database.OpenConnection();
            const string sql =
                "UPDATE attendances SET student_id=@sid, mata_kuliah=@mk, tanggal=@tgl, " +
                "status=@status, keterangan=@ket WHERE id=@id";
            using var cmd = new MySqlCommand(sql, conn);
            AddParameters(cmd, a);
            cmd.Parameters.AddWithValue("@id", a.Id);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = Database.OpenConnection();
            const string sql = "DELETE FROM attendances WHERE id=@id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        private static void AddParameters(MySqlCommand cmd, Attendance a)
        {
            cmd.Parameters.AddWithValue("@sid", a.StudentId);
            cmd.Parameters.AddWithValue("@mk", a.MataKuliah);
            cmd.Parameters.AddWithValue("@tgl", a.Tanggal.Date);
            cmd.Parameters.AddWithValue("@status", a.Status);
            cmd.Parameters.AddWithValue("@ket", a.Keterangan);
        }
    }
}
