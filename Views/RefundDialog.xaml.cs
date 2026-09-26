using System;
using System.Windows;
using System.Windows.Input;
using PosApp.Models;

namespace PosApp.Views
{
    public partial class RefundDialog : Window
    {
        private ProductManager _productManager;
        public Product RefundedProduct { get; private set; }
        public string RefundPaymentType { get; private set; }
        public double RefundQuantity { get; private set; } = 1.0;

        public RefundDialog(ProductManager productManager)
        {
            InitializeComponent();
            _productManager = productManager;
            TxtRefundBarcode.Focus();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnFindProduct_Click(object sender, RoutedEventArgs e)
        {
            SearchProduct();
        }

        private void TxtRefundBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchProduct();
            }
        }

        private void TxtRefundQuantity_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateInfo();
        }

        private void TxtRefundBarcode_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_productManager == null) return;
            string query = TxtRefundBarcode.Text.Trim();
            if (query.Length >= 2)
            {
                var results = _productManager.SearchProducts(query);
                if (results.Count > 0)
                {
                    LbxSuggestions.ItemsSource = results;
                    SuggestionsPopup.IsOpen = true;
                }
                else SuggestionsPopup.IsOpen = false;
            }
            else SuggestionsPopup.IsOpen = false;
        }

        private void LbxSuggestions_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (LbxSuggestions.SelectedItem is Product selectedProduct)
            {
                TxtRefundBarcode.Text = selectedProduct.Barcode;
                SuggestionsPopup.IsOpen = false;
                BtnFindProduct_Click(null, null); // Aramayı tetikle
            }
            LbxSuggestions.SelectedItem = null;
        }

        private void SearchProduct()
        {
            SuggestionsPopup.IsOpen = false;
            string barcode = TxtRefundBarcode.Text.Trim();
            var product = _productManager.GetProductByBarcode(barcode);

            if (product != null)
            {
                RefundedProduct = product;
                if (product.IsWeighed)
                {
                    if (LblQuantityTitle != null) LblQuantityTitle.Text = "İADE MİKTARI (KG)";
                    TxtRefundQuantity.Focus();
                    TxtRefundQuantity.SelectAll();
                }
                else
                {
                    if (LblQuantityTitle != null) LblQuantityTitle.Text = "İADE ADEDİ";
                }
            }
            else
            {
                RefundedProduct = null;
            }
            UpdateInfo();
        }

        private void UpdateInfo()
        {
            if (TxtProductInfo == null || BtnConfirmNakit == null || BtnConfirmKart == null || TxtRefundBarcode == null) return;

            if (RefundedProduct != null)
            {
                if (double.TryParse(TxtRefundQuantity.Text.Replace(".", ","), out double qty) && qty > 0)
                {
                    RefundQuantity = qty;
                    TxtProductInfo.Text = $"Ürün: {RefundedProduct.Name}\nMiktar: {qty}\nİade Tutarı: {RefundedProduct.Price * (decimal)qty:C2}";
                    TxtProductInfo.Foreground = System.Windows.Media.Brushes.DarkGreen;
                    BtnConfirmNakit.IsEnabled = true;
                    BtnConfirmKart.IsEnabled = true;
                }
                else
                {
                    TxtProductInfo.Text = "Lütfen geçerli bir iade adedi girin!";
                    TxtProductInfo.Foreground = System.Windows.Media.Brushes.Red;
                    BtnConfirmNakit.IsEnabled = false;
                    BtnConfirmKart.IsEnabled = false;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(TxtRefundBarcode.Text))
                    TxtProductInfo.Text = "";
                else
                    TxtProductInfo.Text = "Ürün bulunamadı! Lütfen barkodu kontrol edin.";
                TxtProductInfo.Foreground = System.Windows.Media.Brushes.Red;
                BtnConfirmNakit.IsEnabled = false;
                BtnConfirmKart.IsEnabled = false;
            }
        }



        private void BtnConfirmNakit_Click(object sender, RoutedEventArgs e)
        {

            if (RefundedProduct != null)
            {
                RefundPaymentType = "İade - Nakit";
                DialogResult = true;
            }
        }

        private void BtnConfirmKart_Click(object sender, RoutedEventArgs e)
        {

            if (RefundedProduct != null)
            {
                RefundPaymentType = "İade - Kredi Kartı";
                DialogResult = true;
            }
        }
    }
}
