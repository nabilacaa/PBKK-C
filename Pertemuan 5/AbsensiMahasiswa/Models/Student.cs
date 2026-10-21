namespace AbsensiMahasiswa.Models
{
    /// <summary>Satu record mahasiswa (tabel students).</summary>
    public class Student
    {
        public int Id { get; set; }
        public string Nrp { get; set; } = string.Empty;
        public string Nama { get; set; } = string.Empty;
        public string Jurusan { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        /// <summary>Teks yang tampil di ComboBox pemilihan mahasiswa.</summary>
        public string Display => $"{Nrp} - {Nama}";
    }
}
