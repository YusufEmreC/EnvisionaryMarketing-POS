using System;
using System.Windows;
using System.Windows.Controls;
using PosApp.Models;
using PosApp.ViewModels;
using PosApp.Services;
using PosApp.Infrastructure;
using System.Linq;

namespace PosApp.Views
{
    public partial class InventoryView : UserControl
    {
        public InventoryView()
        {
            InitializeComponent();
        }

        public void Initialize(DatabaseManager dbManager, ProductManager productManager, IDialogService dialogService)
        {
            var vm = new InventoryViewModel(dbManager, productManager, dialogService);
            DataContext = vm;
            vm.Initialize();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is InventoryViewModel vm)
            {
                vm.RefreshData();
            }
        }

        private void BtnExcelImport_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is InventoryViewModel vm)
            {
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Filter = "CSV Dosyaları (*.csv)|*.csv|Excel Dosyaları (*.xlsx)|*.xlsx|Tüm Dosyalar (*.*)|*.*";

                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        var importer = vm.GetProductImporter();
                        var analysisResult = importer.AnalyzeExcelFile(openFileDialog.FileName);
                        
                        if (analysisResult.TotalRows == 0)
                        {
                            PosApp.Views.ModernMessageBox.Show("Excel dosyasında okunacak ürün bulunamadı.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        var dialog = new ImportSummaryDialog(analysisResult);
                        var window = Window.GetWindow(this);
                        if (window != null)
                        {
                            dialog.Owner = window;
                        }

                        if (dialog.ShowDialog() == true)
                        {
                            int importedCount = importer.ExecuteImport(analysisResult);
                            PosApp.Views.ModernMessageBox.Show($"{importedCount} ürün başarıyla içe aktarıldı/güncellendi.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                            vm.RefreshData();
                        }
                    }
                    catch (Exception ex)
                    {
                        PosApp.Views.ModernMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ChkSelectAll_Click(object sender, RoutedEventArgs e)
        {
            var isChecked = (sender as CheckBox)?.IsChecked == true;
            if (DataContext is InventoryViewModel vm)
            {
                foreach (var product in vm.Products)
                {
                    product.IsSelected = isChecked;
                }
                ProductsGrid.Items.Refresh();
            }
        }

        private void BorderCritical_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is InventoryViewModel vm)
            {
                vm.ToggleFilterCommand.Execute("Critical");
                CardExpiringSoon.Background = System.Windows.Media.Brushes.White;
                if (sender is Border b)
                {
                    b.Background = vm.IsCriticalFilterActive ? new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FADBD8")) : System.Windows.Media.Brushes.White;
                }
            }
        }

        private void BorderExpiringSoon_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is InventoryViewModel vm)
            {
                vm.ToggleFilterCommand.Execute("Expiring");
                CardCriticalStock.Background = System.Windows.Media.Brushes.White;
                if (sender is Border b)
                {
                    b.Background = vm.IsExpiringFilterActive ? new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#FFE0B2")) : System.Windows.Media.Brushes.White;
                }
            }
        }

        private void BorderTotal_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is InventoryViewModel vm)
            {
                vm.ToggleFilterCommand.Execute("All");
                CardCriticalStock.Background = System.Windows.Media.Brushes.White;
                CardExpiringSoon.Background = System.Windows.Media.Brushes.White;
            }
        }

        public void ProcessBarcode(string barcode)
        {
            if (DataContext is InventoryViewModel vm)
            {
                Dispatcher.BeginInvoke(new Action(() => 
                {
                    vm.FormBarcode = barcode;
                    TxtFormBarcode.CaretIndex = barcode.Length;
                    TxtFormName.Focus();
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.Dispatcher.BeginInvoke(new Action(() => tb.SelectAll()));
                
                if (tb.Text == "0" || tb.Text == "0,00" || tb.Text == "0.00" || tb.Text == "20")
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
                    if (tb.Name.Contains("Price")) tb.Text = "0,00";
                    else if (tb.Name.Contains("Stock")) tb.Text = "0";
                    else if (tb.Name.Contains("Kdv")) tb.Text = "20";
                }
            }
        }

        public void PrefillNewProduct(string barcode)
        {
            if (DataContext is InventoryViewModel vm)
            {
                vm.ClearForm();
                vm.FormBarcode = barcode;
                TxtFormName.Focus();
            }
        }
    }
}
