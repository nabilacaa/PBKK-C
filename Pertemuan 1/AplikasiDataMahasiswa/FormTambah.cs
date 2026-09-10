using System;
using System.Drawing;
using System.Windows.Forms;

namespace DataMahasiswa
{
    public class FormTambah : Form
    {
        private TextBox txtNim, txtNama, txtProdi, txtIpk;

        public FormTambah()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Theme.SetupForm(this, 360, 340, "Tambah Mahasiswa");

            var panel = Theme.CreateBorderedPanel(320, 300);
            panel.Location = new Point(20, 20);
            this.Controls.Add(panel);

            var judul = Theme.CreateTitle("TAMBAH MAHASISWA");
            panel.Controls.Add(judul);
            judul.Location = new Point((panel.Width - judul.PreferredWidth) / 2, 15);

            var garis = Theme.CreateSeparator(panel.Width - 40);
            garis.Location = new Point(20, 45);
            panel.Controls.Add(garis);

            int y = 65;

            var lblNim = new Label { Text = "NIM", Location = new Point(20, y + 4), AutoSize = true };
            Theme.StyleLabel(lblNim);
            txtNim = new TextBox { Location = new Point(110, y), Width = 170 };
            Theme.StyleTextBox(txtNim);
            panel.Controls.Add(lblNim);
            panel.Controls.Add(txtNim);
            y += 38;

            var lblNama = new Label { Text = "Nama", Location = new Point(20, y + 4), AutoSize = true };
            Theme.StyleLabel(lblNama);
            txtNama = new TextBox { Location = new Point(110, y), Width = 170 };
            Theme.StyleTextBox(txtNama);
            panel.Controls.Add(lblNama);
            panel.Controls.Add(txtNama);
            y += 38;

            var lblProdi = new Label { Text = "Program Studi", Location = new Point(20, y + 4), AutoSize = true };
            Theme.StyleLabel(lblProdi);
            txtProdi = new TextBox { Location = new Point(110, y), Width = 170 };
            Theme.StyleTextBox(txtProdi);
            panel.Controls.Add(lblProdi);
            panel.Controls.Add(txtProdi);
            y += 38;

            var lblIpk = new Label { Text = "IPK", Location = new Point(20, y + 4), AutoSize = true };
            Theme.StyleLabel(lblIpk);
            txtIpk = new TextBox { Location = new Point(110, y), Width = 170 };
            Theme.StyleTextBox(txtIpk);
            panel.Controls.Add(lblIpk);
            panel.Controls.Add(txtIpk);
            y += 38;

            var btnSimpan = Theme.CreateButton("Simpan", Theme.BadgeColors[3], Color.White);
            btnSimpan.Location = new Point(110, y + 10);
            btnSimpan.Width = 80;
            btnSimpan.Click += BtnSimpan_Click;
            panel.Controls.Add(btnSimpan);

            var btnBatal = Theme.CreateButton("Batal", Theme.BadgeColors[4], Color.White);
            btnBatal.Location = new Point(200, y + 10);
            btnBatal.Width = 80;
            btnBatal.Click += (s, e) => this.Close();
            panel.Controls.Add(btnBatal);
        }

        private void BtnSimpan_Click(object sender, EventArgs e)
        {
            string nim = txtNim.Text.Trim();
            string nama = txtNama.Text.Trim();
            string prodi = txtProdi.Text.Trim();
            string ipkText = txtIpk.Text.Trim();

            if (string.IsNullOrEmpty(nim) || string.IsNullOrEmpty(nama) ||
                string.IsNullOrEmpty(prodi) || string.IsNullOrEmpty(ipkText))
            {
                MessageBox.Show("Semua data harus diisi!", "Validasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!double.TryParse(ipkText, out double ipk) || ipk < 0 || ipk > 4)
            {
                MessageBox.Show("IPK harus berupa angka 0 - 4.", "Validasi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataStore.DaftarMahasiswa.Add(new Mahasiswa(nim, nama, prodi, ipk));

            MessageBox.Show("Data mahasiswa berhasil ditambahkan.", "Sukses",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            txtNim.Clear();
            txtNama.Clear();
            txtProdi.Clear();
            txtIpk.Clear();
            txtNim.Focus();
        }
    }
}