namespace PosApp.Infrastructure
{
    public interface IDialogService
    {
        void ShowMessage(string message, string title, string buttonType = "OK", string imageType = "Information");
        bool ShowConfirmation(string message, string title);
        
        // Özel diyaloglar
        void ShowReportPreview(string title, string reportText, System.Action printCallback);
        (bool IsSuccess, decimal OpeningCash, decimal Expenses, decimal CountedCash) ShowCashCountDialog();
        
        // Checkout diyalogları
        (bool IsSuccess, string PaymentMethod) ShowPaymentDialog(decimal totalAmount);
        void ShowSuccessDialog(string title, string message, string amount, string paymentType, string receiptNo);
        (bool IsSuccess, PosApp.Models.Product RefundedProduct, string RefundPaymentType, double RefundQuantity) ShowRefundDialog(PosApp.Services.ProductManager productManager);
        (bool IsSuccess, double SelectedQuantity) ShowQuantityDialog(PosApp.Models.Product product);
    }
}
