using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DataMahasiswa
{
    // Kumpulan warna dan helper UI supaya tampilan konsisten di semua Form
    static class Theme
    {
        public static readonly Color BackgroundColor = ColorTranslator.FromHtml("#0B1220");
        public static readonly Color PanelColor = ColorTranslator.FromHtml("#0F1B2E");
        public static readonly Color AccentCyan = ColorTranslator.FromHtml("#29D3E5");
        public static readonly Color TextLight = Color.WhiteSmoke;
        public static readonly Color TextMuted = Color.FromArgb(150, 170, 190);
        public static readonly Color InputBack = ColorTranslator.FromHtml("#132238");

        // Warna badge nomor menu: merah, oranye, kuning, hijau, biru
        public static readonly Color[] BadgeColors =
        {
            ColorTranslator.FromHtml("#E5484D"),
            ColorTranslator.FromHtml("#F5A524"),
            ColorTranslator.FromHtml("#F5D90A"),
            ColorTranslator.FromHtml("#12B886"),
            ColorTranslator.FromHtml("#3B82F6")
        };

        // Lingkaran angka berwarna (badge nomor menu)
        public static Label CreateBadge(int nomor, Color warna)
        {
            var badge = new Label
            {
                Text = nomor.ToString(),
                Size = new Size(26, 26),
                BackColor = warna,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            var path = new GraphicsPath();
            path.AddEllipse(0, 0, badge.Width, badge.Height);
            badge.Region = new Region(path);
            return badge;
        }

        // Garis pembatas tipis warna cyan
        public static Panel CreateSeparator(int width)
        {
            return new Panel { Width = width, Height = 1, BackColor = AccentCyan };
        }

        // Pengaturan dasar Form: background gelap, ukuran, posisi tengah layar
        public static void SetupForm(Form form, int width, int height, string judul)
        {
            form.Text = judul;
            form.BackColor = BackgroundColor;
            form.Width = width;
            form.Height = height;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.MaximizeBox = false;
        }

        // Panel dengan garis tepi cyan tipis
        public static Panel CreateBorderedPanel(int width, int height)
        {
            var panel = new Panel { Width = width, Height = height, BackColor = PanelColor };
            panel.Paint += (s, e) =>
            {
                using var pen = new Pen(AccentCyan, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            };
            return panel;
        }

        public static Label CreateTitle(string teks)
        {
            return new Label
            {
                Text = teks,
                ForeColor = TextLight,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                AutoSize = true
            };
        }

        public static Label CreateSubtitle(string teks)
        {
            return new Label
            {
                Text = teks,
                ForeColor = TextMuted,
                Font = new Font("Segoe UI", 7.5f),
                AutoSize = true
            };
        }

        // Styling standar TextBox di tema gelap
        public static void StyleTextBox(TextBox txt)
        {
            txt.BackColor = InputBack;
            txt.ForeColor = TextLight;
            txt.BorderStyle = BorderStyle.FixedSingle;
        }

        // Styling standar Label biasa di tema gelap
        public static void StyleLabel(Label lbl)
        {
            lbl.ForeColor = TextLight;
        }

        // Tombol warna solid tanpa border 3D bawaan Windows
        public static Button CreateButton(string teks, Color warna, Color warnaTeks)
        {
            var btn = new Button
            {
                Text = teks,
                BackColor = warna,
                ForeColor = warnaTeks,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}