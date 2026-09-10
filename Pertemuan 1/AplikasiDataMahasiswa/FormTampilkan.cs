using System.Drawing;
using System.Windows.Forms;

namespace DataMahasiswa
{
    public class FormTampilkan : Form
    {
        private DataGridView dgvMahasiswa;

        public FormTampilkan()
        {
            InitializeComponent();
            MuatData();
        }

        private void InitializeComponent()
        {
            Theme.SetupForm(this, 560, 380, "Tampilkan Mahasiswa");

            var judul = new Label
            {
                Text = "DAFTAR MAHASISWA",
                ForeColor = Theme.TextLight,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 45,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var garis = Theme.CreateSeparator(this.Width);
            garis.Dock = DockStyle.Top;

            dgvMahasiswa = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Theme.PanelColor,
                BorderStyle = BorderStyle.None,
                GridColor = Theme.AccentCyan,
                EnableHeadersVisualStyles = false,
                RowHeadersVisible = false
            };
            dgvMahasiswa.DefaultCellStyle.BackColor = Theme.PanelColor;
            dgvMahasiswa.DefaultCellStyle.ForeColor = Theme.TextLight;
            dgvMahasiswa.DefaultCellStyle.SelectionBackColor = Theme.AccentCyan;
            dgvMahasiswa.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvMahasiswa.ColumnHeadersDefaultCellStyle.BackColor = Theme.BackgroundColor;
            dgvMahasiswa.ColumnHeadersDefaultCellStyle.ForeColor = Theme.TextLight;
            dgvMahasiswa.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);

            // Urutan Add penting: Fill ditambahkan dulu baru Top, supaya layout benar
            this.Controls.Add(dgvMahasiswa);
            this.Controls.Add(garis);
            this.Controls.Add(judul);
        }

        private void MuatData()
        {
            dgvMahasiswa.DataSource = null;
            dgvMahasiswa.DataSource = DataStore.DaftarMahasiswa;

            if (dgvMahasiswa.Columns["IPK"] != null)
            {
                dgvMahasiswa.Columns["IPK"].DefaultCellStyle.Format = "F2";
            }
        }
    }
}   