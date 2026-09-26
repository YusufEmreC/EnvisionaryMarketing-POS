using System.Windows;
using PosApp.Views;

namespace PosApp.Infrastructure
{
    public class DialogService : IDialogService
    {
        public void ShowMessage(string message, string title, string buttonType = "OK", string imageType = "Information")
        {
            MessageBoxButton btn = MessageBoxButton.OK;
            if (buttonType == "YesNo") btn = MessageBoxButton.YesNo;
            
            MessageBoxImage img = MessageBoxImage.Information;
            if (imageType == "Warning") img = MessageBoxImage.Warning;
            else if (imageType == "Error") img = MessageBoxImage.Error;
            else if (imageType == "Question") img = MessageBoxImage.Question;

            ModernMessageBox.Show(message, title, btn, img);
        }

        public bool ShowConfirmation(string message, string title)
        {
            var result = ModernMessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        public void ShowReportPreview(string title, string reportText, System.Action printCallback)
        {
            var previewDialog = new ReportPreviewDialog(title, reportText, printCallback) { Owner = Application.Current.MainWindow };
            previewDialog.ShowDialog();
        }

        public (bool IsSuccess, decimal OpeningCash, decimal Expenses, decimal CountedCash) ShowCashCountDialog()
        {
            var dialog = new CashCountDialog() { Owner = Application.Current.MainWindow };
            if (dialog.ShowDialog() == true)
            {
                // Note: CashCountDialog is currently returning double. We'll cast it to decimal for now, 
                // and update CashCountDialog separately.
                return (true, (decimal)dialog.OpeningCash, (decimal)dialog.Expenses, (decimal)dialog.CountedCash);
            }
            return (false, 0m, 0m, 0m);
        }

        public (bool IsSuccess, string PaymentMethod) ShowPaymentDialog(decimal totalAmount)
        {
            var dialog = new PaymentDialog(totalAmount) { Owner = Application.Current.MainWindow };
            if (dialog.ShowDialog() == true)
            {
                return (true, dialog.SelectedPaymentMethod);
            }
            return (false, null);
        }

        public void ShowSuccessDialog(string title, string message, string amount, string paymentType, string receiptNo)
        {
            var dialog = new SuccessDialog(title, message, amount, paymentType, receiptNo) { Owner = Application.Current.MainWindow };
            dialog.ShowDialog();
        }

        public (bool IsSuccess, PosApp.Models.Product RefundedProduct, string RefundPaymentType, double RefundQuantity) ShowRefundDialog(PosApp.Services.ProductManager productManager)
        {
            var dialog = new RefundDialog(productManager) { Owner = Application.Current.MainWindow };
            if (dialog.ShowDialog() == true)
            {
                return (true, dialog.RefundedProduct, dialog.RefundPaymentType, dialog.RefundQuantity);
            }
            return (false, null, null, 0);
        }

        public (bool IsSuccess, double SelectedQuantity) ShowQuantityDialog(PosApp.Models.Product product)
        {
            var dialog = new QuantityDialog(product) { Owner = Application.Current.MainWindow };
            if (dialog.ShowDialog() == true)
            {
                return (true, dialog.SelectedQuantity);
            }
            return (false, 0);
        }
    }
}
