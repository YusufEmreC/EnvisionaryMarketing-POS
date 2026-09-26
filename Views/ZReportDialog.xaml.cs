using System.Windows;

namespace PosApp.Views
{
    public partial class ZReportDialog : Window
    {
        public double OpeningCash { get; private set; }
        public double Refunds { get; private set; }
        public double Expenses { get; private set; }

        public ZReportDialog(double defaultOpeningCash, double dailyRefunds)
        {
            InitializeComponent();
            TxtOpeningCash.Text = defaultOpeningCash.ToString("0.00");
            
            // KULLANICI TALEBİ: İadeler veritabanından çekilerek salt-okunur olarak yazılır
            TxtRefunds.Text = dailyRefunds.ToString("0.00");
            TxtRefunds.IsReadOnly = true;
            TxtRefunds.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#EAEAEA"));
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            // Başlangıç parasını al
            double.TryParse(TxtOpeningCash.Text, out double opening);
            OpeningCash = opening;

            double.TryParse(TxtExpenses.Text.Replace(".", ","), out double expenses);
            Expenses = expenses;

            DialogResult = true; // Pencereyi onayla kapat
        }
    }
}
