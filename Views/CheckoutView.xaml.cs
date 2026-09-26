using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PosApp.Models;
using PosApp.Services;
using PosApp.ViewModels;
using PosApp.Infrastructure;

namespace PosApp.Views
{
    public partial class CheckoutView : UserControl
    {
        public event Action<string> RequestAddProduct;

        public CheckoutView()
        {
            InitializeComponent();
        }

        public void Initialize(ProductManager pm, CartManager cm, PrinterManager printer, SettingsManager sm, IDialogService dialogService)
        {
            var vm = new CheckoutViewModel(pm, cm, printer, sm, dialogService);
            vm.RequestAddProduct += (barcode) => RequestAddProduct?.Invoke(barcode);
            DataContext = vm;
            vm.Initialize();
        }

        public void FocusBarcodeTextBox()
        {
            Dispatcher.BeginInvoke(new Action(() => 
            {
                TxtManualBarcode.Focus();
                TxtManualBarcode.SelectAll();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            FocusBarcodeTextBox();
        }

        private void TxtManualBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DataContext is CheckoutViewModel vm)
                {
                    vm.ProcessBarcodeCommand.Execute(null);
                    FocusBarcodeTextBox();
                }
            }
        }
    }
}
