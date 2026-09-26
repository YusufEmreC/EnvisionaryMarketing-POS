using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PosApp.Models;

namespace PosApp.Views
{
    public partial class QuantityDialog : Window
    {
        public double SelectedQuantity { get; private set; } = 1.0;
        private Product _product;

        public QuantityDialog(Product product)
        {
            InitializeComponent();
            _product = product;
            TxtProductName.Text = $"{product.Name} (Birim Fiyatı: {product.Price:C2})";
            
            // İmleci odakla
            Loaded += (s, e) => { TxtQuantity.Focus(); };
        }

        private void Numpad_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                string content = btn.Content.ToString();
                
                // İlk başta sıfır varsa veya virgül değilse üzerine yaz
                if (TxtQuantity.Text == "0" && content != ",")
                {
                    TxtQuantity.Text = content;
                }
                else if (content == ",")
                {
                    if (!TxtQuantity.Text.Contains(","))
                    {
                        if (string.IsNullOrEmpty(TxtQuantity.Text)) TxtQuantity.Text = "0,";
                        else TxtQuantity.Text += ",";
                    }
                }
                else
                {
                    TxtQuantity.Text += content;
                }
                
                TxtQuantity.Focus();
                TxtQuantity.CaretIndex = TxtQuantity.Text.Length;
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (TxtQuantity.Text.Length > 0)
            {
                TxtQuantity.Text = TxtQuantity.Text.Substring(0, TxtQuantity.Text.Length - 1);
            }
            if (TxtQuantity.Text.Length == 0)
            {
                TxtQuantity.Text = "0";
            }
            TxtQuantity.Focus();
            TxtQuantity.CaretIndex = TxtQuantity.Text.Length;
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(TxtQuantity.Text.Replace(".", ","), out double quantity))
            {
                if (quantity <= 0)
                {
                    PosApp.Views.ModernMessageBox.Show("Miktar sıfırdan büyük olmalıdır.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                SelectedQuantity = quantity;
                DialogResult = true;
                Close();
            }
            else
            {
                PosApp.Views.ModernMessageBox.Show("Lütfen geçerli bir miktar giriniz.", "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TxtQuantity_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Sadece sayı ve virgül kontrolü XAML tarafında PreviewTextInput ile daha iyi yapılabilir
            // ancak basit bir doğrulama eklenebilir.
        }

        private void TxtQuantity_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnOK_Click(null, null);
            }
        }
    }
}
