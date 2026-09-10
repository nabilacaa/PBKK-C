using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DataMahasiswa
{
    public class FormHapus : Form
    {
        private TextBox txtNim;

        public FormHapus()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Theme.SetupForm(this, 360, 220, "Hapus Mahasiswa");

            var panel = Theme.CreateBorderedPanel(320, 180);
            panel.Location = new Point(20, 20);
            this.Controls.Add(panel);

            var judul = Theme.CreateTitle("HAPUS MAHASISWA");
            panel.Controls.Add(judul);
            judul.Location = new Point((panel.Width - judul.PreferredWidth) / 2, 15);

            var garis = Theme.CreateSeparator(panel.Width - 40);
            garis.Location = new Point(20, 45);
            panel.Controls.Add(garis);

            var lblNim = new Label { Text = "NIM", Location = new Point(20, 65), AutoSize = true };
            Theme.StyleLabel(lblNim);
            panel.Controls.Add(lblNim);

            txtNim = new TextBox { Location = new Point(70, 61), Width = 230 };
            Theme.StyleTextBox(txtNim);
            panel.Controls.Add(txtNim);

            var btnHapus = Theme.CreateButton("Hapus", Theme.BadgeColors[0], Color.White);
            btnHapus.Location = new Point(70, 110);
            btnHapus.Width = 100;
            btnHapus.Click += BtnHapus_Click;
            panel.Controls.Add(btnHapus);

            var btnBatal = Theme.CreateButton("Batal", Theme.BadgeColors[4], Color.White);
            btnBatal.Location = new Point(190, 110);
            btnBatal.Width = 100;
            btnBatal.Click += (s, e) => this.Close();
            panel.Controls.Add(btnBatal);
        }

        private void BtnHapus_Click(object sender, EventArgs e)
        {
            string nimHapus = txtNim.Text.Trim();

            Mahasiswa ditemukan = DataStore.DaftarMahasiswa
                .FirstOrDefault(m => m.NIM.Equals(nimHapus, StringComparison.OrdinalIgnoreCase));

            if (ditemukan != null)
            {
                var konfirmasi = MessageBox.Show(
                    "Yakin ingin menghapus data " + ditemukan.Nama + "?",
                    "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (konfirmasi == DialogResult.Yes)
                {
                    DataStore.DaftarMahasiswa.Remove(ditemukan);
                    MessageBox.Show("Data mahasiswa berhasil dihapus.", "Sukses",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtNim.Clear();
                }
            }
            else
            {
                MessageBox.Show("Data mahasiswa tidak ditemukan.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}