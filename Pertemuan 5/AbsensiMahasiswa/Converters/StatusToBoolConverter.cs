using System.Globalization;
using System.Windows.Data;

namespace AbsensiMahasiswa.Converters
{
    /// <summary>
    /// Menghubungkan RadioButton dengan satu property string (Status).
    /// RadioButton tercentang bila Status == ConverterParameter.
    /// </summary>
    public class StatusToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return string.Equals(value?.ToString(), parameter?.ToString(), StringComparison.Ordinal);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Hanya RadioButton yang baru dicentang yang mengubah Status.
            return value is true ? parameter?.ToString() ?? string.Empty : Binding.DoNothing;
        }
    }
}
