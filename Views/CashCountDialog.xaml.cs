using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace PosApp.Views
{
    public partial class CashCountDialog : Window
    {
        public double OpeningCash { get; private set; }
        public double Expenses { get; private set; }
        public double CountedCash { get; private set; }

        public CashCountDialog()
        {
            InitializeComponent();
            TxtOpeningCash.Text = "0.00";
            TxtExpenses.Text = "0.00";
            TxtCountedCash.Text = "0.00";
            
            TxtOpeningCash.PreviewTextInput += NumberValidationTextBox;
            
            TxtExpenses.PreviewTextInput += NumberValidationTextBox;
            TxtCountedCash.PreviewTextInput += NumberValidationTextBox;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Sadece sayı ve virgül girişine izin ver
            e.Handled = !System.Text.RegularExpressions.Regex.IsMatch(e.Text, "[0-9,]");
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string openText = TxtOpeningCash.Text.Replace(".", ",");
                string expText = TxtExpenses.Text.Replace(".", ",");
                string cashText = TxtCountedCash.Text.Replace(".", ",");

                if (double.TryParse(openText, out double open))
                    OpeningCash = open;

                if (double.TryParse(expText, out double exp))
                    Expenses = exp;

                if (double.TryParse(cashText, out double cash))
                    CountedCash = cash;

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                PosApp.Views.ModernMessageBox.Show("Lütfen geçerli sayısal değerler giriniz. Hata: " + ex.Message, "Giriş Hatası", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.Dispatcher.BeginInvoke(new Action(() => tb.SelectAll()));
                
                if (tb.Text == "0" || tb.Text == "0,00" || tb.Text == "0.00")
                {
                    tb.Text = "";
                }
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    tb.Text = "0,00";
                }
            }
        }
    }
}
