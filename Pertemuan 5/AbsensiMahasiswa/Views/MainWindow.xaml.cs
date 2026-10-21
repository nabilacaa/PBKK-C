using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using AbsensiMahasiswa.ViewModels;

namespace AbsensiMahasiswa.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _vm;
        }

        // Event Loaded: data baru dimuat setelah window tampil
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _vm.Initialize();
        }

        // Event PreviewTextInput: TextBox NRP hanya menerima angka (urusan tampilan murni)
        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^\d+$");
        }

        // Event Closing: konfirmasi sebelum aplikasi ditutup
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var result = MessageBox.Show("Keluar dari aplikasi?", "Konfirmasi",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) e.Cancel = true;
        }
    }
}
