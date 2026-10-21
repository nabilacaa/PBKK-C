namespace AbsensiMahasiswa.ViewModels
{
    /// <summary>ViewModel induk yang menyatukan kedua tab.</summary>
    public class MainViewModel : BaseViewModel
    {
        public StudentViewModel StudentVM { get; }
        public AttendanceViewModel AttendanceVM { get; }

        public MainViewModel()
        {
            StudentVM = new StudentViewModel();
            AttendanceVM = new AttendanceViewModel(StudentVM.Students);

            // Data mahasiswa berubah (mis. dihapus) -> absensi dimuat ulang
            StudentVM.DataChanged += () => AttendanceVM.LoadData();
        }

        /// <summary>Dipanggil saat window selesai dimuat.</summary>
        public void Initialize()
        {
            // Bila koneksi gagal, cukup tampilkan satu pesan error
            if (StudentVM.LoadData())
                AttendanceVM.LoadData();
        }
    }
}
