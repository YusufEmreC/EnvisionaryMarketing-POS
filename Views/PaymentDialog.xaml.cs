using System.Windows;
using System.Windows.Controls;

namespace PosApp.Views
{
    public partial class PaymentDialog : Window
    {
        public string SelectedPaymentMethod { get; private set; } = string.Empty;
        private decimal _totalAmount;

        public PaymentDialog(decimal totalAmount)
        {
            InitializeComponent();
            _totalAmount = totalAmount;
            TxtTotalAmount.Text = totalAmount.ToString("C2");
            TxtHeaderTitle.Text = "Ödeme Yöntemi Seçimi";
        }

        private void BtnMethodKart_Click(object sender, RoutedEventArgs e)
        {
            // Direct payment via Credit Card
            SelectedPaymentMethod = "Kredi Kartı";
            DialogResult = true;
            Close();
        }

        private void BtnMethodNakit_Click(object sender, RoutedEventArgs e)
        {
            // Transition to Step 2 (Cash details)
            Step1Grid.Visibility = Visibility.Collapsed;
            Step2Grid.Visibility = Visibility.Visible;
            TxtHeaderTitle.Text = "Nakit Ödeme Tahsilatı";
            
            TxtGivenAmount.Text = _totalAmount.ToString("F2");
            TxtGivenAmount.Focus();
            TxtGivenAmount.SelectAll();
        }

        private void BtnBackToMethods_Click(object sender, RoutedEventArgs e)
        {
            // Back to Step 1
            Step2Grid.Visibility = Visibility.Collapsed;
            Step1Grid.Visibility = Visibility.Visible;
            TxtHeaderTitle.Text = "Ödeme Yöntemi Seçimi";
        }

        private void TxtGivenAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TxtChangeAmount == null || BtnCompleteCash == null) return;
            
            if (decimal.TryParse(TxtGivenAmount.Text, out decimal given))
            {
                decimal change = given - _totalAmount;
                if (change < 0)
                {
                    TxtChangeAmount.Text = "Yetersiz Tutar";
                    TxtChangeAmount.Foreground = System.Windows.Media.Brushes.Red;
                    BtnCompleteCash.IsEnabled = false;
                }
                else
                {
                    TxtChangeAmount.Text = change.ToString("C2");
                    TxtChangeAmount.Foreground = (System.Windows.Media.Brush)FindResource("ColorSuccess");
                    BtnCompleteCash.IsEnabled = true;
                }
            }
            else
            {
                TxtChangeAmount.Text = "Geçersiz";
                TxtChangeAmount.Foreground = System.Windows.Media.Brushes.Red;
                BtnCompleteCash.IsEnabled = false;
            }
        }

        private void BtnCompleteCash_Click(object sender, RoutedEventArgs e)
        {
            SelectedPaymentMethod = "Nakit";
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
