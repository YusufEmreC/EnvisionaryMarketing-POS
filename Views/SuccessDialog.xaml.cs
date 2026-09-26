using System.Windows;

namespace PosApp.Views
{
    public partial class SuccessDialog : Window
    {
        public SuccessDialog(string title, string message, string amount, string paymentType, string receiptNo)
        {
            InitializeComponent();

            if (Application.Current != null)
            {
                var activeWindow = System.Linq.Enumerable.FirstOrDefault(System.Linq.Enumerable.OfType<Window>(Application.Current.Windows), x => x.IsActive);
                if (activeWindow != null && activeWindow != this)
                {
                    this.Owner = activeWindow;
                }
                else if (Application.Current.MainWindow != null && Application.Current.MainWindow != this)
                {
                    this.Owner = Application.Current.MainWindow;
                }
            }
            
            TxtTitle.Text = title;
            TxtMessage.Text = message;
            
            // Eğer detaylar boş verilirse gizle
            if (string.IsNullOrEmpty(amount) && string.IsNullOrEmpty(paymentType) && string.IsNullOrEmpty(receiptNo))
            {
                GridDetails.Visibility = Visibility.Collapsed;
            }
            else
            {
                TxtDetailAmount.Text = amount;
                TxtDetailPayment.Text = paymentType;
                TxtDetailReceipt.Text = receiptNo;
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
