using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PosApp.Views
{
    public partial class ZReportCardDialog : Window
    {
        private Models.ZReportHistory _report;

        public ZReportCardDialog(Models.ZReportHistory report)
        {
            InitializeComponent();
            _report = report;
            LoadReportData();
        }

        private void LoadReportData()
        {
            if (_report == null) return;

            TxtReportNo.Text = $"Rapor #{_report.Id}";
            TxtReportDate.Text = $"Tarih: {_report.ReportDateString}";

            TxtCashSales.Text = _report.CashSales.ToString("C2");
            TxtCreditSales.Text = _report.CreditSales.ToString("C2");
            TxtTotalSales.Text = (_report.CashSales + _report.CreditSales).ToString("C2");

            TxtCashRefunds.Text = "- " + _report.CashRefunds.ToString("C2");
            TxtCreditRefunds.Text = "- " + _report.CreditRefunds.ToString("C2");
            TxtExpenses.Text = "- " + _report.Expenses.ToString("C2");

            TxtOpeningCash.Text = _report.OpeningCash.ToString("C2");
            TxtExpectedCash.Text = _report.ExpectedCash.ToString("C2");
            TxtCountedCash.Text = _report.CountedCash.ToString("C2");

            TxtDifference.Text = _report.CashDifference.ToString("C2");

            if (_report.CashDifference > 0)
            {
                BadgeDifference.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#dcfce7")); // Green
                TxtDifference.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#166534"));
                TxtDifference.Text = "+" + _report.CashDifference.ToString("C2") + " (Fazla)";
            }
            else if (_report.CashDifference < 0)
            {
                BadgeDifference.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fee2e2")); // Red
                TxtDifference.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#991b1b"));
                TxtDifference.Text = _report.CashDifference.ToString("C2") + " (Açık)";
            }
            else
            {
                BadgeDifference.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f1f5f9")); // Gray
                TxtDifference.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155"));
                TxtDifference.Text = "Denk (₺0.00)";
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var db = new DatabaseManager();
                var sm = new SettingsManager(db);
                var rm = new ReportManager(db);
                string text = rm.FormatZReportFromHistory(_report);
                var pm = new PrinterManager(sm.GetSetting("PrinterName", "POS-58"));
                pm.PrintReceipt(text);
                
                PosApp.Views.ModernMessageBox.Show("Rapor yazdırıldı.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(Exception ex)
            {
                PosApp.Views.ModernMessageBox.Show("Yazdırılırken hata oluştu: " + ex.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
