using System.Globalization;

namespace KalkulatorSaintifik.Core;

/// <summary>Operasi yang membutuhkan dua operand (a ○ b).</summary>
public enum OperasiBiner { Tambah, Kurang, Kali, Bagi, Pangkat }

/// <summary>Operasi yang hanya membutuhkan satu operand f(x).</summary>
public enum FungsiUnary { Sin, Cos, Tan, Kuadrat, Akar, Ln, Log, Kebalikan, Faktorial, Mutlak }

/// <summary>
/// Seluruh logika kalkulator, tanpa ketergantungan pada UI.
/// UI cukup memanggil method publik lalu membaca <see cref="TeksLayar"/>,
/// <see cref="TeksEkspresi"/>, dan <see cref="Riwayat"/>.
/// Pemisahan ini membuat logika dapat diuji dengan unit test.
/// </summary>
public class MesinKalkulator
{
    public const int MaksDigit = 15;

    private string _input = "";            // angka yang sedang diketik
    private double _operandKiri;           // angka pertama
    private OperasiBiner? _operasi;        // operator yang menunggu angka kedua
    private bool _baruSelesai;             // true setelah "=" atau fungsi unary
    private readonly List<string> _riwayat = new();

    public bool ModeDerajat { get; private set; } = true;
    public string? PesanError { get; private set; }
    public bool AdaError => PesanError is not null;

    /// <summary>Teks utama di layar (input, hasil, atau pesan error).</summary>
    public string TeksLayar => PesanError ?? (_input.Length == 0 ? "0" : _input);

    /// <summary>Baris kecil di atas layar, mis. "12 + " atau "12 + 8 =".</summary>
    public string TeksEkspresi { get; private set; } = "";

    /// <summary>Riwayat perhitungan, item terbaru di indeks 0.</summary>
    public IReadOnlyList<string> Riwayat => _riwayat;

    /// <summary>Nilai numerik yang sedang tampil.</summary>
    public double NilaiSekarang =>
        _input.Length == 0 || _input == "-" ? 0 : double.Parse(_input, CultureInfo.InvariantCulture);

    // ------------------------------------------------------------------ input

    public void MasukkanDigit(char digit)
    {
        if (!char.IsAsciiDigit(digit)) return;
        MulaiInputBaruJikaPerlu();

        if (HitungDigit(_input) >= MaksDigit) return;

        if (_input == "0") _input = digit.ToString();
        else if (_input == "-0") _input = "-" + digit;
        else _input += digit;
    }

    public void MasukkanTitik()
    {
        MulaiInputBaruJikaPerlu();
        if (_input.Contains('.')) return;
        _input = _input is "" or "-" ? _input + "0." : _input + ".";
    }

    /// <summary>Tombol ±.</summary>
    public void GantiTanda()
    {
        if (AdaError) return;
        if (_input.Length == 0 || (NilaiSekarang == 0 && !_input.Contains('.'))) return;
        _input = _input.StartsWith('-') ? _input[1..] : "-" + _input;
    }

    /// <summary>Tombol ⌫ (backspace).</summary>
    public void HapusKarakter()
    {
        if (AdaError) { Clear(); return; }
        if (_baruSelesai) return; // hasil perhitungan tidak diedit per karakter
        if (_input.Length == 0) return;
        _input = _input[..^1];
        if (_input == "-") _input = "";
    }

    /// <summary>Tombol C: kembali ke kondisi awal (riwayat tetap disimpan).</summary>
    public void Clear()
    {
        _input = "";
        _operandKiri = 0;
        _operasi = null;
        _baruSelesai = false;
        PesanError = null;
        TeksEkspresi = "";
    }

    /// <summary>Tombol CE: hanya menghapus angka yang sedang diketik.</summary>
    public void ClearEntry()
    {
        if (AdaError) { Clear(); return; }
        _input = "";
        _baruSelesai = false;
    }

    public void HapusRiwayat() => _riwayat.Clear();

    public void GantiModeSudut() => ModeDerajat = !ModeDerajat;

    public void MasukkanKonstanta(double nilai)
    {
        MulaiInputBaruJikaPerlu();
        _input = Format(nilai);
        _baruSelesai = true; // angka berikutnya mengganti konstanta, bukan menyambung
    }

    /// <summary>Memasukkan nilai dari riwayat sebagai input.</summary>
    public void GunakanNilai(string teksAngka)
    {
        if (!double.TryParse(teksAngka, NumberStyles.Float, CultureInfo.InvariantCulture, out var v)) return;
        MasukkanKonstanta(v);
    }

    // -------------------------------------------------------------- operasi

    public void PilihOperator(OperasiBiner op)
    {
        if (AdaError) return;

        // Operator ditekan dua kali berturut-turut: ganti operatornya saja.
        if (_operasi is not null && _input.Length == 0)
        {
            _operasi = op;
            TeksEkspresi = $"{Format(_operandKiri)} {Simbol(op)}";
            return;
        }

        // Operasi berantai: 10 + 5 + ... → hitung 10 + 5 dulu.
        if (_operasi is not null)
        {
            if (!CobaHitung(_operandKiri, _operasi.Value, NilaiSekarang, out var antara)) return;
            _operandKiri = antara;
        }
        else
        {
            _operandKiri = NilaiSekarang;
        }

        _operasi = op;
        _input = "";
        _baruSelesai = false;
        TeksEkspresi = $"{Format(_operandKiri)} {Simbol(op)}";
    }

    /// <summary>Tombol =.</summary>
    public void Hitung()
    {
        if (AdaError || _operasi is null) return;

        if (_input.Length == 0)
        {
            SetError("Angka kedua kosong");
            return;
        }

        double kanan = NilaiSekarang;
        string ekspresi = $"{Format(_operandKiri)} {Simbol(_operasi.Value)} {Format(kanan)}";
        if (!CobaHitung(_operandKiri, _operasi.Value, kanan, out var hasil)) return;

        SelesaikanDengan(ekspresi, hasil);
        _operasi = null;
    }

    /// <summary>
    /// Tombol %. Jika ada + atau − yang tertunda, x% berarti x persen dari angka pertama
    /// (200 + 10% = 220). Selain itu x% = x / 100.
    /// </summary>
    public void Persen()
    {
        if (AdaError) return;
        double x = NilaiSekarang;
        double hasil = _operasi is OperasiBiner.Tambah or OperasiBiner.Kurang
            ? _operandKiri * x / 100
            : x / 100;
        _input = Format(hasil);
        _baruSelesai = false;
    }

    public void TerapkanFungsi(FungsiUnary f)
    {
        if (AdaError) return;
        double x = NilaiSekarang;
        string deg = ModeDerajat ? "°" : "";
        string label;
        double hasil;

        switch (f)
        {
            case FungsiUnary.Sin:
                label = $"sin({Format(x)}{deg})";
                hasil = BersihkanNol(Math.Sin(KeRadian(x)));
                break;
            case FungsiUnary.Cos:
                label = $"cos({Format(x)}{deg})";
                hasil = BersihkanNol(Math.Cos(KeRadian(x)));
                break;
            case FungsiUnary.Tan:
                label = $"tan({Format(x)}{deg})";
                if (TanTakTerdefinisi(x)) { SetError("Domain error"); return; }
                hasil = BersihkanNol(Math.Tan(KeRadian(x)));
                break;
            case FungsiUnary.Kuadrat:
                label = $"sqr({Format(x)})";
                hasil = x * x;
                break;
            case FungsiUnary.Akar:
                label = $"√({Format(x)})";
                if (x < 0) { SetError("Input tidak valid"); return; }
                hasil = Math.Sqrt(x);
                break;
            case FungsiUnary.Ln:
                label = $"ln({Format(x)})";
                if (x <= 0) { SetError("Input tidak valid"); return; }
                hasil = Math.Log(x);
                break;
            case FungsiUnary.Log:
                label = $"log({Format(x)})";
                if (x <= 0) { SetError("Input tidak valid"); return; }
                hasil = Math.Log10(x);
                break;
            case FungsiUnary.Kebalikan:
                label = $"1/({Format(x)})";
                if (x == 0) { SetError("Tidak bisa dibagi nol"); return; }
                hasil = 1 / x;
                break;
            case FungsiUnary.Faktorial:
                label = $"{Format(x)}!";
                if (x < 0 || x != Math.Floor(x)) { SetError("Input tidak valid"); return; }
                if (x > 170) { SetError("Overflow"); return; }
                hasil = Faktorial((int)x);
                break;
            case FungsiUnary.Mutlak:
                label = $"|{Format(x)}|";
                hasil = Math.Abs(x);
                break;
            default:
                return;
        }

        if (!HasilValid(hasil)) return;

        // Fungsi diterapkan pada angka kedua: tampilkan "12 + sin(30°)" di ekspresi,
        // operasi tertunda tetap menunggu tombol "=".
        if (_operasi is not null)
        {
            TeksEkspresi = $"{Format(_operandKiri)} {Simbol(_operasi.Value)} {label}";
            _input = Format(hasil);
            _baruSelesai = true;
            return;
        }

        SelesaikanDengan(label, hasil);
    }

    // ------------------------------------------------------------- helper

    public static double Faktorial(int n)
    {
        double hasil = 1;
        for (int i = 2; i <= n; i++) hasil *= i;
        return hasil;
    }

    /// <summary>Format angka untuk layar: maksimal 15 digit signifikan, notasi ilmiah bila ekstrem.</summary>
    public static string Format(double v)
    {
        if (double.IsNaN(v)) return "NaN";
        if (double.IsInfinity(v)) return "Overflow";
        if (v == 0) return "0"; // juga menghilangkan "-0"

        double abs = Math.Abs(v);
        if (abs >= 1e15 || abs < 1e-7)
            return v.ToString("0.##########E+0", CultureInfo.InvariantCulture);
        return v.ToString("G15", CultureInfo.InvariantCulture);
    }

    public static string Simbol(OperasiBiner op) => op switch
    {
        OperasiBiner.Tambah => "+",
        OperasiBiner.Kurang => "−",
        OperasiBiner.Kali => "×",
        OperasiBiner.Bagi => "÷",
        OperasiBiner.Pangkat => "^",
        _ => "?"
    };

    private bool CobaHitung(double a, OperasiBiner op, double b, out double hasil)
    {
        hasil = 0;
        switch (op)
        {
            case OperasiBiner.Tambah: hasil = a + b; break;
            case OperasiBiner.Kurang: hasil = a - b; break;
            case OperasiBiner.Kali: hasil = a * b; break;
            case OperasiBiner.Bagi:
                if (b == 0) { SetError("Tidak bisa dibagi nol"); return false; }
                hasil = a / b;
                break;
            case OperasiBiner.Pangkat: hasil = Math.Pow(a, b); break;
        }
        return HasilValid(hasil);
    }

    private bool HasilValid(double v)
    {
        if (double.IsNaN(v)) { SetError("Hasil tidak terdefinisi"); return false; }
        if (double.IsInfinity(v)) { SetError("Overflow"); return false; }
        return true;
    }

    private void SelesaikanDengan(string ekspresi, double hasil)
    {
        string teksHasil = Format(hasil);
        TeksEkspresi = ekspresi + " =";
        _riwayat.Insert(0, $"{ekspresi} = {teksHasil}");
        _input = teksHasil;
        _baruSelesai = true;
    }

    private void SetError(string pesan)
    {
        PesanError = pesan;
        _input = "";
        _operasi = null;
        _baruSelesai = true;
    }

    /// <summary>Setelah hasil/ error, angka baru memulai input dari awal.</summary>
    private void MulaiInputBaruJikaPerlu()
    {
        if (AdaError) Clear();
        if (_baruSelesai)
        {
            _input = "";
            _baruSelesai = false;
            if (_operasi is null) TeksEkspresi = "";
        }
    }

    private double KeRadian(double x) => ModeDerajat ? x * Math.PI / 180.0 : x;

    private bool TanTakTerdefinisi(double x)
    {
        if (ModeDerajat)
        {
            double sisa = ((x % 180) + 180) % 180;
            return Math.Abs(sisa - 90) < 1e-9;
        }
        return Math.Abs(Math.Cos(x)) < 1e-15;
    }

    /// <summary>sin(180°) secara floating point = 1.2e-16; bulatkan ke 0.</summary>
    private static double BersihkanNol(double v) => Math.Abs(v) < 1e-12 ? 0 : v;

    private static int HitungDigit(string s) => s.Count(char.IsAsciiDigit);
}
