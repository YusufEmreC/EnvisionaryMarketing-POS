using System;
using System.Windows;

namespace PosApp.Views
{
    public partial class ReportPreviewDialog : Window
    {
        private Action _onPrintAction;

        public ReportPreviewDialog(string title, string content, Action onPrintAction)
        {
            InitializeComponent();
            TxtTitle.Text = title;
            TxtReportContent.Text = content;
            _onPrintAction = onPrintAction;
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            _onPrintAction?.Invoke();
            
            var success = new SuccessDialog("Başarılı", "Yazdırma İşlemi Başladı...", "", "", "");
            success.ShowDialog();
            
            DialogResult = true;
            Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
