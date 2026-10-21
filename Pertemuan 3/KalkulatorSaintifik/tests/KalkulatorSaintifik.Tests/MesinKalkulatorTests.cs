using KalkulatorSaintifik.Core;
using Xunit;

namespace KalkulatorSaintifik.Tests;

public class MesinKalkulatorTests
{
    /// <summary>
    /// Helper: menjalankan urutan "tombol" sederhana.
    /// Angka/titik diketik per karakter; token lain: + - * / ^ = % C CE BS NEG
    /// atau nama FungsiUnary (Sin, Akar, ...).
    /// </summary>
    private static MesinKalkulator Tekan(params string[] tombol)
    {
        var m = new MesinKalkulator();
        foreach (var t in tombol)
        {
            switch (t)
            {
                case "+": m.PilihOperator(OperasiBiner.Tambah); break;
                case "-": m.PilihOperator(OperasiBiner.Kurang); break;
                case "*": m.PilihOperator(OperasiBiner.Kali); break;
                case "/": m.PilihOperator(OperasiBiner.Bagi); break;
                case "^": m.PilihOperator(OperasiBiner.Pangkat); break;
                case "=": m.Hitung(); break;
                case "%": m.Persen(); break;
                case "C": m.Clear(); break;
                case "CE": m.ClearEntry(); break;
                case "BS": m.HapusKarakter(); break;
                case "NEG": m.GantiTanda(); break;
                case "RAD": m.GantiModeSudut(); break;
                default:
                    if (char.IsLetter(t[0]) && Enum.TryParse<FungsiUnary>(t, out var f)) { m.TerapkanFungsi(f); break; }
                    foreach (var c in t)
                        if (c == '.') m.MasukkanTitik(); else m.MasukkanDigit(c);
                    break;
            }
        }
        return m;
    }

    // ---- Skenario pengujian dari modul -----------------------------------

    [Theory]
    [InlineData("30", "10", "+", "20", "=")]
    [InlineData("18", "30", "-", "12", "=")]
    [InlineData("42", "6", "*", "7", "=")]
    [InlineData("25", "100", "/", "4", "=")]
    [InlineData("10", "2.5", "*", "4", "=")]
    public void OperasiDasar(string harapan, params string[] tombol) =>
        Assert.Equal(harapan, Tekan(tombol).TeksLayar);

    [Fact]
    public void BagiNol_Error()
    {
        var m = Tekan("10", "/", "0", "=");
        Assert.True(m.AdaError);
        Assert.Equal("Tidak bisa dibagi nol", m.TeksLayar);
    }

    [Fact]
    public void Clear_KembaliKeNol()
    {
        var m = Tekan("123", "+", "4", "C");
        Assert.Equal("0", m.TeksLayar);
        Assert.Equal("", m.TeksEkspresi);
    }

    [Fact]
    public void AngkaKeduaKosong_Error()
    {
        var m = Tekan("5", "+", "=");
        Assert.True(m.AdaError);
        Assert.Equal("Angka kedua kosong", m.TeksLayar);
    }

    // ---- Perilaku input ---------------------------------------------------

    [Fact]
    public void Floating_0_1_plus_0_2() => Assert.Equal("0.3", Tekan("0.1", "+", "0.2", "=").TeksLayar);

    [Fact]
    public void TitikHanyaSekali() => Assert.Equal("1.23", Tekan("1.2.3").TeksLayar);

    [Fact]
    public void TitikDiAwal_Menjadi0Koma() => Assert.Equal("0.5", Tekan(".5").TeksLayar);

    [Fact]
    public void NolDiDepanDihapus() => Assert.Equal("5", Tekan("0005").TeksLayar);

    [Fact]
    public void MaksimalLimaBelasDigit() =>
        Assert.Equal("123456789012345", Tekan("1234567890123456789").TeksLayar);

    [Fact]
    public void Backspace() => Assert.Equal("12", Tekan("123", "BS").TeksLayar);

    [Fact]
    public void ClearEntry_HanyaHapusAngkaKedua() =>
        Assert.Equal("15", Tekan("10", "+", "99", "CE", "5", "=").TeksLayar);

    [Fact]
    public void GantiTanda() => Assert.Equal("-2", Tekan("5", "NEG", "+", "3", "=").TeksLayar);

    [Fact]
    public void OperasiBerantai() => Assert.Equal("18", Tekan("10", "+", "5", "+", "3", "=").TeksLayar);

    [Fact]
    public void OperasiBerantai_UrutanKiriKeKanan() => Assert.Equal("20", Tekan("2", "+", "3", "*", "4", "=").TeksLayar);

    [Fact]
    public void OperatorDobel_YangTerakhirDipakai() => Assert.Equal("6", Tekan("2", "+", "*", "3", "=").TeksLayar);

    [Fact]
    public void AngkaSetelahHasil_MulaiBaru() => Assert.Equal("7", Tekan("2", "+", "3", "=", "7").TeksLayar);

    [Fact]
    public void HasilBisaDipakaiLagi() => Assert.Equal("10", Tekan("2", "+", "3", "=", "*", "2", "=").TeksLayar);

    [Fact]
    public void SetelahError_AngkaBaruMengosongkanError()
    {
        var m = Tekan("1", "/", "0", "=", "8");
        Assert.False(m.AdaError);
        Assert.Equal("8", m.TeksLayar);
    }

    // ---- Persen dan pangkat ----------------------------------------------

    [Fact] public void Persen_Tunggal() => Assert.Equal("0.5", Tekan("50", "%").TeksLayar);
    [Fact] public void Persen_DariAngkaPertama() => Assert.Equal("220", Tekan("200", "+", "10", "%", "=").TeksLayar);
    [Fact] public void Persen_Perkalian() => Assert.Equal("25", Tekan("50", "*", "50", "%", "=").TeksLayar);
    [Fact] public void Pangkat() => Assert.Equal("1024", Tekan("2", "^", "10", "=").TeksLayar);

    [Fact]
    public void PangkatTakTerdefinisi_Error() =>
        Assert.True(Tekan("8", "NEG", "^", "0.5", "=").AdaError);

    // ---- Fungsi saintifik ---------------------------------------------------

    [Theory]
    [InlineData("30", "Sin", "0.5")]
    [InlineData("180", "Sin", "0")]
    [InlineData("60", "Cos", "0.5")]
    [InlineData("90", "Cos", "0")]
    [InlineData("45", "Tan", "1")]
    [InlineData("25", "Akar", "5")]
    [InlineData("12", "Kuadrat", "144")]
    [InlineData("1000", "Log", "3")]
    [InlineData("1", "Ln", "0")]
    [InlineData("4", "Kebalikan", "0.25")]
    [InlineData("5", "Faktorial", "120")]
    [InlineData("0", "Faktorial", "1")]
    public void FungsiUnary_Benar(string input, string fungsi, string harapan) =>
        Assert.Equal(harapan, Tekan(input, fungsi).TeksLayar);

    [Fact]
    public void Sin_ModeRadian()
    {
        var m = new MesinKalkulator();
        m.GantiModeSudut();
        m.MasukkanKonstanta(Math.PI / 2);
        m.TerapkanFungsi(FungsiUnary.Sin);
        Assert.Equal("1", m.TeksLayar);
    }

    [Theory]
    [InlineData("90", "Tan")]
    [InlineData("270", "Tan")]
    [InlineData("0", "Ln")]
    [InlineData("0", "Log")]
    [InlineData("0", "Kebalikan")]
    [InlineData("2.5", "Faktorial")]
    [InlineData("171", "Faktorial")]
    public void FungsiUnary_Error(string input, string fungsi) =>
        Assert.True(Tekan(input, fungsi).AdaError);

    [Fact]
    public void AkarNegatif_Error() => Assert.True(Tekan("4", "NEG", "Akar").AdaError);

    [Fact]
    public void Faktorial170_TidakOverflow() =>
        Assert.False(Tekan("170", "Faktorial").AdaError);

    [Fact]
    public void FungsiPadaAngkaKedua() =>
        Assert.Equal("14", Tekan("10", "+", "16", "Akar", "=").TeksLayar);

    // ---- Riwayat dan format ---------------------------------------------------

    [Fact]
    public void Riwayat_TerbaruDiAtas()
    {
        var m = Tekan("1", "+", "1", "=", "3", "*", "3", "=");
        Assert.Equal(2, m.Riwayat.Count);
        Assert.Equal("3 × 3 = 9", m.Riwayat[0]);
        Assert.Equal("1 + 1 = 2", m.Riwayat[1]);
    }

    [Fact]
    public void GunakanNilaiRiwayat()
    {
        var m = new MesinKalkulator();
        m.GunakanNilai("42");
        m.PilihOperator(OperasiBiner.Tambah);
        m.MasukkanDigit('8');
        m.Hitung();
        Assert.Equal("50", m.TeksLayar);
    }

    [Theory]
    [InlineData(1e20, "1E+20")]
    [InlineData(0.00000001, "1E-8")]
    [InlineData(-0.0, "0")]
    [InlineData(1234.5, "1234.5")]
    public void Format(double v, string harapan) => Assert.Equal(harapan, MesinKalkulator.Format(v));
}
