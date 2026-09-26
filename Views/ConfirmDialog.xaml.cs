using System.Windows;

namespace PosApp.Views
{
    public partial class ConfirmDialog : Window
    {
        public ConfirmDialog(string title, string message)
        {
            InitializeComponent();
            
            TxtTitle.Text = title;
            TxtMessage.Text = message;
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
