namespace AbsensiMahasiswa.Models
{
    /// <summary>Satu record absensi (tabel attendances, di-JOIN dengan students).</summary>
    public class Attendance
    {
        public int Id { get; set; }
        public int StudentId { get; set; }

        // Kolom hasil JOIN, hanya untuk tampilan
        public string Nrp { get; set; } = string.Empty;
        public string Nama { get; set; } = string.Empty;

        public string MataKuliah { get; set; } = string.Empty;
        public DateTime Tanggal { get; set; } = DateTime.Today;
        public string Status { get; set; } = string.Empty;
        public string Keterangan { get; set; } = string.Empty;
    }
}
