using System.Windows;

namespace PosApp.Views
{
    public partial class ImportSummaryDialog : Window
    {
        public ImportSummaryDialog(ImportAnalysisResult result)
        {
            InitializeComponent();
            
            TxtTotal.Text = $"Okunan Toplam Ürün: {result.TotalRows}";
            TxtNew.Text = $"{result.NewProductsCount} adet tamamen yeni ürün eklenecek.";
            TxtUpdated.Text = $"{result.UpdatedProductsCount} adet ürün sistemde mevcut, stokları üzerine eklenecek.";

            if (result.MissingExpirationCount > 0)
            {
                WarningBox.Visibility = Visibility.Visible;
                TxtWarning.Text = $"DİKKAT: Yüklediğiniz listedeki {result.MissingExpirationCount} üründe Son Kullanım Tarihi (SKT) girilmemiş. (Eğer bu ürünler temizlik malzemesi, peçete vb. ise sorun yoktur, işleme devam edebilirsiniz. Ancak gıda ürünü iseler işlemi iptal edip Excel'i düzeltmeniz tavsiye edilir.)";
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnApprove_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
