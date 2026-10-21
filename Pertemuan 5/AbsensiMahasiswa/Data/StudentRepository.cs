using AbsensiMahasiswa.Models;
using MySqlConnector;

namespace AbsensiMahasiswa.Data
{
    /// <summary>Operasi CRUD untuk tabel students.</summary>
    public class StudentRepository
    {
        public List<Student> GetAll()
        {
            var list = new List<Student>();
            using var conn = Database.OpenConnection();
            const string sql = "SELECT id, nrp, nama, jurusan, email FROM students ORDER BY nrp";
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new Student
                {
                    Id = reader.GetInt32("id"),
                    Nrp = reader.GetString("nrp"),
                    Nama = reader.GetString("nama"),
                    Jurusan = reader.GetString("jurusan"),
                    Email = reader.GetString("email")
                });
            }
            return list;
        }

        /// <summary>Cek apakah NRP sudah dipakai mahasiswa lain (excludeId untuk mode ubah).</summary>
        public bool NrpExists(string nrp, int excludeId = 0)
        {
            using var conn = Database.OpenConnection();
            const string sql = "SELECT COUNT(*) FROM students WHERE nrp = @nrp AND id <> @id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nrp", nrp);
            cmd.Parameters.AddWithValue("@id", excludeId);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public void Insert(Student s)
        {
            using var conn = Database.OpenConnection();
            const string sql = "INSERT INTO students (nrp, nama, jurusan, email) " +
                               "VALUES (@nrp, @nama, @jurusan, @email)";
            using var cmd = new MySqlCommand(sql, conn);
            AddParameters(cmd, s);
            cmd.ExecuteNonQuery();
        }

        public void Update(Student s)
        {
            using var conn = Database.OpenConnection();
            const string sql = "UPDATE students SET nrp=@nrp, nama=@nama, jurusan=@jurusan, email=@email " +
                               "WHERE id=@id";
            using var cmd = new MySqlCommand(sql, conn);
            AddParameters(cmd, s);
            cmd.Parameters.AddWithValue("@id", s.Id);
            cmd.ExecuteNonQuery();
        }

        /// <summary>Menghapus mahasiswa. Absensinya ikut terhapus (ON DELETE CASCADE).</summary>
        public void Delete(int id)
        {
            using var conn = Database.OpenConnection();
            const string sql = "DELETE FROM students WHERE id=@id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        private static void AddParameters(MySqlCommand cmd, Student s)
        {
            cmd.Parameters.AddWithValue("@nrp", s.Nrp);
            cmd.Parameters.AddWithValue("@nama", s.Nama);
            cmd.Parameters.AddWithValue("@jurusan", s.Jurusan);
            cmd.Parameters.AddWithValue("@email", s.Email);
        }
    }
}
