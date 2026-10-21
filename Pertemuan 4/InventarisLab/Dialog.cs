using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace InventarisLab;

/// <summary>
/// Dialog pesan dan konfirmasi buatan sendiri, karena Avalonia
/// tidak menyediakan MessageBox seperti WPF.
/// </summary>
public static class Dialog
{
    public enum Jenis { Info, Peringatan, Sukses }

    /// <summary>Menampilkan pesan dengan satu tombol OK.</summary>
    public static async Task Pesan(Window pemilik, string judul, string isi, Jenis jenis = Jenis.Info)
    {
        await Buat(pemilik, judul, isi, jenis, konfirmasi: false);
    }

    /// <summary>Menampilkan konfirmasi Ya/Tidak. Mengembalikan true jika memilih Ya.</summary>
    public static async Task<bool> Konfirmasi(Window pemilik, string judul, string isi)
    {
        return await Buat(pemilik, judul, isi, Jenis.Peringatan, konfirmasi: true);
    }

    private static async Task<bool> Buat(Window pemilik, string judul, string isi, Jenis jenis, bool konfirmasi)
    {
        string aksen = jenis switch
        {
            Jenis.Sukses => "#047857",
            Jenis.Peringatan => "#B45309",
            _ => "#0E7490"
        };

        var win = new Window
        {
            Title = judul,
            Width = 380,
            SizeToContent = SizeToContent.Height,
            CanResize = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Background = Brushes.White,
            ShowInTaskbar = false
        };

        var tombol = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Spacing = 8,
            Margin = new Thickness(0, 20, 0, 0)
        };

        if (konfirmasi)
        {
            var btnTidak = BuatTombol("Tidak", "#FFFFFF", "#334155", "#CBD5E1");
            btnTidak.Click += (_, _) => win.Close(false);
            tombol.Children.Add(btnTidak);

            var btnYa = BuatTombol("Ya, Hapus", "#B91C1C", "#FFFFFF", "#B91C1C");
            btnYa.Click += (_, _) => win.Close(true);
            tombol.Children.Add(btnYa);
        }
        else
        {
            var btnOk = BuatTombol("OK", "#0F172A", "#FFFFFF", "#0F172A");
            btnOk.Click += (_, _) => win.Close(true);
            tombol.Children.Add(btnOk);
        }

        win.Content = new Border
        {
            BorderBrush = Brush.Parse(aksen),
            BorderThickness = new Thickness(0, 4, 0, 0),
            Padding = new Thickness(24, 20),
            Child = new StackPanel
            {
                Children =
                {
                    new TextBlock
                    {
                        Text = judul,
                        FontSize = 16,
                        FontWeight = FontWeight.SemiBold,
                        Foreground = Brush.Parse("#0F172A")
                    },
                    new TextBlock
                    {
                        Text = isi,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 13,
                        Foreground = Brush.Parse("#475569"),
                        Margin = new Thickness(0, 8, 0, 0)
                    },
                    tombol
                }
            }
        };

        return await win.ShowDialog<bool>(pemilik);
    }

    private static Button BuatTombol(string teks, string bg, string fg, string border) => new()
    {
        Content = teks,
        MinWidth = 84,
        Height = 34,
        HorizontalContentAlignment = HorizontalAlignment.Center,
        VerticalContentAlignment = VerticalAlignment.Center,
        CornerRadius = new CornerRadius(6),
        Background = Brush.Parse(bg),
        Foreground = Brush.Parse(fg),
        BorderBrush = Brush.Parse(border),
        BorderThickness = new Thickness(1),
        FontWeight = FontWeight.SemiBold
    };
}
