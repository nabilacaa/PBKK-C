using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using KalkulatorSaintifik.Core;

namespace KalkulatorSaintifik;

/// <summary>
/// Code-behind hanya meneruskan input (tombol/keyboard) ke <see cref="MesinKalkulator"/>
/// lalu menyegarkan tampilan. Tidak ada logika perhitungan di sini.
/// </summary>
public partial class MainWindow : Window
{
    private readonly MesinKalkulator _mesin = new();

    public MainWindow()
    {
        InitializeComponent();

        // Tunnel: window menerima input keyboard lebih dulu dibanding kontrol anak.
        AddHandler(KeyDownEvent, OnWindowKeyDown, RoutingStrategies.Tunnel);
        AddHandler(TextInputEvent, OnWindowTextInput, RoutingStrategies.Tunnel);

        PerbaruiTampilan();
    }

    // ============================================================ tombol

    private void OnAngkaClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string digit } && digit.Length == 1)
            Jalankan(() => _mesin.MasukkanDigit(digit[0]));
    }

    private void OnOperatorClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string tag } && Enum.TryParse<OperasiBiner>(tag, out var op))
            Jalankan(() => _mesin.PilihOperator(op));
    }

    private void OnFungsiClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string tag } && Enum.TryParse<FungsiUnary>(tag, out var f))
            Jalankan(() => _mesin.TerapkanFungsi(f));
    }

    private void OnKonstantaClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string tag }) return;
        double nilai = tag == "pi" ? Math.PI : Math.E;
        Jalankan(() => _mesin.MasukkanKonstanta(nilai));
    }

    private void OnTitikClick(object? sender, RoutedEventArgs e) => Jalankan(_mesin.MasukkanTitik);
    private void OnSamaDenganClick(object? sender, RoutedEventArgs e) => Jalankan(_mesin.Hitung);
    private void OnClearClick(object? sender, RoutedEventArgs e) => Jalankan(_mesin.Clear);
    private void OnClearEntryClick(object? sender, RoutedEventArgs e) => Jalankan(_mesin.ClearEntry);
    private void OnBackspaceClick(object? sender, RoutedEventArgs e) => Jalankan(_mesin.HapusKarakter);
    private void OnGantiTandaClick(object? sender, RoutedEventArgs e) => Jalankan(_mesin.GantiTanda);
    private void OnPersenClick(object? sender, RoutedEventArgs e) => Jalankan(_mesin.Persen);
    private void OnModeClick(object? sender, RoutedEventArgs e) => Jalankan(_mesin.GantiModeSudut);
    private void OnHapusRiwayatClick(object? sender, RoutedEventArgs e) => Jalankan(_mesin.HapusRiwayat);

    /// <summary>Klik item riwayat: hasilnya dipakai sebagai input baru.</summary>
    private void OnRiwayatTapped(object? sender, TappedEventArgs e)
    {
        if (ListRiwayat.SelectedItem is not string baris) return;
        int idx = baris.LastIndexOf(" = ", StringComparison.Ordinal);
        if (idx >= 0) Jalankan(() => _mesin.GunakanNilai(baris[(idx + 3)..]));
        ListRiwayat.SelectedItem = null;
        Focus(); // kembalikan fokus ke window agar keyboard tetap berfungsi
    }

    /// <summary>Jendela sempit: panel riwayat disembunyikan otomatis (bisa dibuka lagi lewat tombol Riwayat).</summary>
    private const double LebarMinimumRiwayat = 640;
    private bool? _sebelumnyaLebar;

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        bool lebar = e.NewSize.Width >= LebarMinimumRiwayat;
        if (_sebelumnyaLebar != lebar)
        {
            BtnTampilRiwayat.IsChecked = lebar;
            _sebelumnyaLebar = lebar;
        }
    }

    // ========================================================== keyboard

    /// <summary>Karakter yang diketik: angka, titik/koma, operator, %, ^, =.</summary>
    private void OnWindowTextInput(object? sender, TextInputEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Text)) return;
        char c = e.Text[0];

        Action? aksi = c switch
        {
            >= '0' and <= '9' => () => _mesin.MasukkanDigit(c),
            '.' or ','        => _mesin.MasukkanTitik,
            '+'               => () => _mesin.PilihOperator(OperasiBiner.Tambah),
            '-'               => () => _mesin.PilihOperator(OperasiBiner.Kurang),
            '*' or 'x' or 'X' => () => _mesin.PilihOperator(OperasiBiner.Kali),
            '/' or ':'        => () => _mesin.PilihOperator(OperasiBiner.Bagi),
            '^'               => () => _mesin.PilihOperator(OperasiBiner.Pangkat),
            '%'               => _mesin.Persen,
            '!'               => () => _mesin.TerapkanFungsi(FungsiUnary.Faktorial),
            '='               => _mesin.Hitung,
            'p' or 'P'        => () => _mesin.MasukkanKonstanta(Math.PI),
            _ => null
        };

        if (aksi is null) return;
        Jalankan(aksi);
        e.Handled = true;
    }

    /// <summary>Tombol khusus yang tidak menghasilkan karakter.</summary>
    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        Action? aksi = e.Key switch
        {
            Key.Enter      => _mesin.Hitung,
            Key.Back       => _mesin.HapusKarakter,
            Key.Delete     => _mesin.ClearEntry,
            Key.Escape     => _mesin.Clear,
            _ => null
        };

        if (aksi is null) return;
        Jalankan(aksi);
        e.Handled = true;
    }

    // ========================================================== tampilan

    private void Jalankan(Action aksi)
    {
        aksi();
        PerbaruiTampilan();
    }

    private void PerbaruiTampilan()
    {
        TxtLayar.Text = _mesin.TeksLayar;
        TxtLayar.Foreground = _mesin.AdaError ? Brush.Parse("#FF6B6B") : Brushes.White;
        TxtEkspresi.Text = _mesin.TeksEkspresi;

        TxtError.Text = _mesin.AdaError ? "Tekan C atau ketik angka untuk melanjutkan." : "";

        string mode = _mesin.ModeDerajat ? "DEG" : "RAD";
        BtnMode.Content = mode;
        TxtMode.Text = mode;
        TxtMode.Foreground = Brush.Parse(_mesin.ModeDerajat ? "#7CC4FA" : "#FFB74D");
        BadgeMode.Background = Brush.Parse(_mesin.ModeDerajat ? "#1E3A52" : "#4A3418");

        ListRiwayat.ItemsSource = _mesin.Riwayat.ToList();
        TxtRiwayatKosong.IsVisible = _mesin.Riwayat.Count == 0;
    }
}
