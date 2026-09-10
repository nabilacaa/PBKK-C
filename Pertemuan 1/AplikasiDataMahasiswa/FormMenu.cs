using System;
using System.Drawing;
using System.Windows.Forms;

namespace DataMahasiswa
{
    public class FormMenu : Form
    {
        private TextBox txtPilihan;

        public FormMenu()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Theme.SetupForm(this, 1000, 650, "DataMahasiswa");
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;

            var panel = Theme.CreateBorderedPanel(380, 470);
            this.Controls.Add(panel);
            this.Resize += (s, e) => PosisikanTengah(panel);
            PosisikanTengah(panel);

            int y = 25;

            var iconTopi = new Label
            {
                Text = "\U0001F393",
                Font = new Font("Segoe UI Emoji", 20),
                AutoSize = true,
                ForeColor = Theme.AccentCyan
            };
            panel.Controls.Add(iconTopi);
            iconTopi.Location = new Point((panel.Width - iconTopi.PreferredWidth) / 2, y);
            y += 45;

            var judul = Theme.CreateTitle("SISTEM DATA MAHASISWA");
            panel.Controls.Add(judul);
            judul.Location = new Point((panel.Width - judul.PreferredWidth) / 2, y);
            y += 26;

            var subjudul = Theme.CreateSubtitle("KELOLA DATA MAHASISWA DENGAN MUDAH");
            panel.Controls.Add(subjudul);
            subjudul.Location = new Point((panel.Width - subjudul.PreferredWidth) / 2, y);
            y += 24;

            var garis1 = Theme.CreateSeparator(panel.Width - 40);
            garis1.Location = new Point(20, y);
            panel.Controls.Add(garis1);
            y += 20;

            string[] labelMenu = { "Tambah Mahasiswa", "Tampilkan Mahasiswa", "Cari Mahasiswa", "Hapus Mahasiswa", "Keluar" };
            string[] ikonMenu = { "+", "\u2261", "\U0001F50D", "\U0001F5D1", "\u23CF" };

            for (int i = 0; i < labelMenu.Length; i++)
            {
                int nomor = i + 1;

                var badge = Theme.CreateBadge(nomor, Theme.BadgeColors[i]);
                badge.Location = new Point(25, y);
                panel.Controls.Add(badge);

                var ikon = new Label
                {
                    Text = ikonMenu[i],
                    ForeColor = Theme.BadgeColors[i],
                    Font = new Font("Segoe UI Emoji", 11),
                    AutoSize = true,
                    Location = new Point(60, y + 3)
                };
                panel.Controls.Add(ikon);

                var teks = new Label
                {
                    Text = labelMenu[i],
                    ForeColor = Theme.TextLight,
                    Font = new Font("Segoe UI", 10),
                    AutoSize = true,
                    Location = new Point(90, y + 4)
                };
                panel.Controls.Add(teks);

                badge.Cursor = ikon.Cursor = teks.Cursor = Cursors.Hand;
                EventHandler klik = (s, e) => JalankanPilihan(nomor);
                badge.Click += klik;
                ikon.Click += klik;
                teks.Click += klik;

                y += 34;
            }

            y += 6;
            var garis2 = Theme.CreateSeparator(panel.Width - 40);
            garis2.Location = new Point(20, y);
            panel.Controls.Add(garis2);
            y += 20;

            var lblPilihan = new Label
            {
                Text = "Pilihan:",
                ForeColor = Theme.TextLight,
                AutoSize = true,
                Location = new Point(25, y + 4)
            };
            panel.Controls.Add(lblPilihan);

            txtPilihan = new TextBox { Location = new Point(90, y), Width = 60 };
            Theme.StyleTextBox(txtPilihan);
            txtPilihan.KeyDown += TxtPilihan_KeyDown;
            panel.Controls.Add(txtPilihan);
        }

        private void PosisikanTengah(Panel panel)
        {
            panel.Location = new Point(
                (this.ClientSize.Width - panel.Width) / 2,
                (this.ClientSize.Height - panel.Height) / 2);
        }

        private void TxtPilihan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (int.TryParse(txtPilihan.Text.Trim(), out int pilihan))
                {
                    JalankanPilihan(pilihan);
                }
                else
                {
                    MessageBox.Show("Masukkan angka 1 - 5.", "Pilihan tidak valid",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void JalankanPilihan(int pilihan)
        {
            switch (pilihan)
            {
                case 1:
                    new FormTambah().ShowDialog();
                    break;
                case 2:
                    new FormTampilkan().ShowDialog();
                    break;
                case 3:
                    new FormCari().ShowDialog();
                    break;
                case 4:
                    new FormHapus().ShowDialog();
                    break;
                case 5:
                    Application.Exit();
                    break;
                default:
                    MessageBox.Show("Pilihan tidak tersedia!", "Info",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }
            txtPilihan.Clear();
        }
    }
}