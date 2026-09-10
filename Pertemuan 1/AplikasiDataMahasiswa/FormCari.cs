using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DataMahasiswa
{
    public class FormCari : Form
    {
        private TextBox txtNim;
        private Label lblHasil;

        public FormCari()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Theme.SetupForm(this, 360, 260, "Cari Mahasiswa");

            var panel = Theme.CreateBorderedPanel(320, 220);
            panel.Location = new Point(20, 20);
            this.Controls.Add(panel);

            var judul = Theme.CreateTitle("CARI MAHASISWA");
            panel.Controls.Add(judul);
            judul.Location = new Point((panel.Width - judul.PreferredWidth) / 2, 15);

            var garis = Theme.CreateSeparator(panel.Width - 40);
            garis.Location = new Point(20, 45);
            panel.Controls.Add(garis);

            var lblNim = new Label { Text = "NIM", Location = new Point(20, 62), AutoSize = true };
            Theme.StyleLabel(lblNim);
            panel.Controls.Add(lblNim);

            txtNim = new TextBox { Location = new Point(70, 58), Width = 150 };
            Theme.StyleTextBox(txtNim);
            panel.Controls.Add(txtNim);

            var btnCari = Theme.CreateButton("Cari", Theme.BadgeColors[2], Color.Black);
            btnCari.Location = new Point(230, 57);
            btnCari.Width = 70;
            btnCari.Click += BtnCari_Click;
            panel.Controls.Add(btnCari);

            lblHasil = new Label
            {
                Location = new Point(20, 100),
                Size = new Size(280, 110),
                Font = new Font("Consolas", 9),
                ForeColor = Theme.AccentCyan
            };
            panel.Controls.Add(lblHasil);
        }

        private void BtnCari_Click(object sender, EventArgs e)
        {
            string nimCari = txtNim.Text.Trim();

            Mahasiswa ditemukan = DataStore.DaftarMahasiswa
                .FirstOrDefault(m => m.NIM.Equals(nimCari, StringComparison.OrdinalIgnoreCase));

            if (ditemukan != null)
            {
                lblHasil.Text =
                    "Data ditemukan!\n" +
                    "NIM   : " + ditemukan.NIM + "\n" +
                    "Nama  : " + ditemukan.Nama + "\n" +
                    "Prodi : " + ditemukan.Prodi + "\n" +
                    "IPK   : " + ditemukan.IPK.ToString("F2");
            }
            else
            {
                lblHasil.Text = "Mahasiswa dengan NIM tersebut tidak ditemukan.";
            }
        }
    }
}