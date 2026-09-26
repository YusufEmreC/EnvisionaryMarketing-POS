using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using PosApp.Models;
using PosApp.Services;
using PosApp.Infrastructure;
using System.Collections.Generic;

namespace PosApp.ViewModels
{
    public class CheckoutViewModel : ObservableObject
    {
        private readonly ProductManager _productManager;
        private readonly CartManager _cartManager;
        private readonly PrinterManager _printerManager;
        private readonly SettingsManager _settingsManager;
        private readonly IDialogService _dialogService;

        public event Action<string> RequestAddProduct;

        // Observable collections bound directly to UI
        public ObservableCollection<SaleItem> Cart => _cartManager.CurrentCart;
        public ObservableCollection<Product> QuickProducts { get; } = new ObservableCollection<Product>();
        
        private List<Product> _searchSuggestions = new List<Product>();
        public List<Product> SearchSuggestions
        {
            get => _searchSuggestions;
            set => SetProperty(ref _searchSuggestions, value);
        }

        private bool _isSuggestionsOpen;
        public bool IsSuggestionsOpen
        {
            get => _isSuggestionsOpen;
            set => SetProperty(ref _isSuggestionsOpen, value);
        }

        private Product _selectedSuggestion;
        public Product SelectedSuggestion
        {
            get => _selectedSuggestion;
            set
            {
                if (SetProperty(ref _selectedSuggestion, value) && value != null)
                {
                    string barcodeToProcess = value.Barcode;
                    IsSuggestionsOpen = false;
                    ProcessBarcode(barcodeToProcess);
                    
                    // Reset selection so the same item can be clicked again later
                    _selectedSuggestion = null;
                    OnPropertyChanged(nameof(SelectedSuggestion));
                }
            }
        }

        private string _manualBarcode = "";
        public string ManualBarcode
        {
            get => _manualBarcode;
            set
            {
                if (SetProperty(ref _manualBarcode, value))
                {
                    UpdateSuggestions(value);
                }
            }
        }

        private string _discountPercent = "0";
        public string DiscountPercent
        {
            get => _discountPercent;
            set
            {
                if (SetProperty(ref _discountPercent, value))
                {
                    UpdateTotal();
                }
            }
        }

        private string _subTotal = "0,00 ₺";
        public string SubTotal
        {
            get => _subTotal;
            set => SetProperty(ref _subTotal, value);
        }

        private string _kdvTotal = "0,00 ₺";
        public string KdvTotal
        {
            get => _kdvTotal;
            set => SetProperty(ref _kdvTotal, value);
        }

        private string _discountTotal = "0,00 ₺";
        public string DiscountTotal
        {
            get => _discountTotal;
            set => SetProperty(ref _discountTotal, value);
        }

        private string _finalTotal = "0,00 ₺";
        public string FinalTotal
        {
            get => _finalTotal;
            set => SetProperty(ref _finalTotal, value);
        }

        public ICommand ProcessBarcodeCommand { get; }
        public ICommand AddQuickProductCommand { get; }
        public ICommand SelectSuggestionCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand RefundCommand { get; }
        public ICommand PayCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand DecreaseQtyCommand { get; }
        public ICommand IncreaseQtyCommand { get; }

        public CheckoutViewModel(ProductManager pm, CartManager cm, PrinterManager printer, SettingsManager sm, IDialogService dialogService)
        {
            _productManager = pm;
            _cartManager = cm;
            _printerManager = printer;
            _settingsManager = sm;
            _dialogService = dialogService;

            ProcessBarcodeCommand = new RelayCommand(p => 
            {
                if (p is string barcodeStr) ProcessBarcode(barcodeStr);
                else if (!string.IsNullOrWhiteSpace(ManualBarcode)) ProcessBarcode(ManualBarcode.ToUpper());
            });
            
            AddQuickProductCommand = new RelayCommand(p => {
                if (p is Product product) ProcessBarcode(product.Barcode, 1);
            });

            SelectSuggestionCommand = new RelayCommand(p => {
                if (p is Product product)
                {
                    ManualBarcode = product.Barcode;
                    IsSuggestionsOpen = false;
                    ProcessBarcode(ManualBarcode);
                }
            });

            ClearCartCommand = new RelayCommand(_ => {
                _cartManager.ClearCart();
                DiscountPercent = "0";
                UpdateTotal();
            });
            
            RefundCommand = new RelayCommand(_ => ProcessRefund());
            PayCommand = new RelayCommand(_ => InitiatePayment());
            RemoveItemCommand = new RelayCommand(p => {
                if (p is SaleItem item)
                {
                    _cartManager.RemoveFromCart(item.Product.Barcode);
                    UpdateTotal();
                }
            });
            DecreaseQtyCommand = new RelayCommand(p => {
                if (p is SaleItem item)
                {
                    if (item.Quantity > 1) { item.Quantity--; UpdateTotal(); }
                    else { _cartManager.RemoveFromCart(item.Product.Barcode); UpdateTotal(); }
                }
            });
            IncreaseQtyCommand = new RelayCommand(p => {
                if (p is SaleItem item)
                {
                    if (!item.Product.IsQuickProduct && item.Quantity + 1 > item.Product.StockQuantity)
                    {
                        _dialogService.ShowMessage($"Uyarı: Stok yetersiz! (Mevcut Stok: {item.Product.StockQuantity}). Daha fazla ekleyemezsiniz.", "Stok Yetersiz", "OK", "Warning");
                        return;
                    }
                    item.Quantity++;
                    UpdateTotal();
                }
            });
        }

        public void Initialize()
        {
            LoadQuickProducts();
            UpdateTotal();
        }

        public void LoadQuickProducts()
        {
            QuickProducts.Clear();
            var quickProducts = _productManager.GetQuickProducts();
            foreach (var q in quickProducts)
            {
                QuickProducts.Add(q);
            }
        }

        private void UpdateSuggestions(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                IsSuggestionsOpen = false;
                return;
            }
            var results = _productManager.SearchProducts(query);
            if (results.Count > 0)
            {
                SearchSuggestions = results.ToList();
                IsSuggestionsOpen = true;
            }
            else
            {
                IsSuggestionsOpen = false;
            }
        }

        private DateTime _lastProcessTime = DateTime.MinValue;
        private string _lastProcessedBarcode = string.Empty;

        public void ProcessBarcode(string barcode, double quantity = 1.0)
        {
            if (string.IsNullOrWhiteSpace(barcode)) return;

            if (barcode == _lastProcessedBarcode && (DateTime.Now - _lastProcessTime).TotalMilliseconds < 300)
            {
                ManualBarcode = "";
                return; // Ignore double fire for the same product within 300ms
            }
            _lastProcessedBarcode = barcode;
            _lastProcessTime = DateTime.Now;

            string originalBarcode = barcode;
            if (barcode.Length == 13 && (barcode.StartsWith("27") || barcode.StartsWith("28") || barcode.StartsWith("29")))
            {
                string productCode = barcode.Substring(2, 5); 
                string weightStr = barcode.Substring(7, 5); 

                if (double.TryParse(weightStr, out double weightGrams))
                {
                    quantity = weightGrams / 1000.0; 
                    barcode = productCode; 
                }
            }

            var product = _productManager.GetProductByBarcode(barcode);
            
            if (product == null && barcode.Length == 5)
            {
                product = _productManager.GetProductByBarcode("27" + barcode + "000000");
                if (product == null) product = _productManager.GetProductByBarcode("28" + barcode + "000000");
                if (product == null) product = _productManager.GetProductByBarcode("29" + barcode + "000000");
            }

            if (product != null)
            {
                if (product.IsWeighed && quantity == 1.0 && originalBarcode == barcode)
                {
                    var result = _dialogService.ShowQuantityDialog(product);
                    if (result.IsSuccess)
                    {
                        quantity = result.SelectedQuantity;
                    }
                    else
                    {
                        ManualBarcode = "";
                        return; // İptal edildiyse sepete ekleme
                    }
                }

                double currentInCart = 0;
                foreach (var item in _cartManager.CurrentCart)
                {
                    if (item.Product.Barcode == product.Barcode) currentInCart = item.Quantity;
                }

                if (!product.IsQuickProduct && currentInCart + quantity > product.StockQuantity)
                {
                    _dialogService.ShowMessage($"Uyarı: Stok yetersiz! (Mevcut Stok: {product.StockQuantity}). İşlem stok eksiye düşerek devam edecek.", "Stok Uyarısı", "OK", "Warning");
                }

                _cartManager.AddToCart(product, quantity);
                
                // Reset manual input after success
                ManualBarcode = "";
                IsSuggestionsOpen = false;
                
                UpdateTotal();
            }
            else
            {
                if (_dialogService.ShowConfirmation($"'{barcode}' barkodlu ürün bulunamadı.\n\nBu ürünü envantere eklemek ister misiniz?", "Ürün Bulunamadı"))
                {
                    RequestAddProduct?.Invoke(barcode);
                }
                ManualBarcode = "";
            }
        }

        private void InitiatePayment()
        {
            if (_cartManager.CurrentCart.Count == 0)
            {
                _dialogService.ShowMessage("Sepetiniz boş! Lütfen önce ürün ekleyin.", "Uyarı", "OK", "Warning");
                return;
            }

            var paymentDialogResult = _dialogService.ShowPaymentDialog(_cartManager.TotalAmount);
            if (paymentDialogResult.IsSuccess)
            {
                CompleteSale(paymentDialogResult.PaymentMethod);
            }
        }

        private void CompleteSale(string paymentType)
        {
            try
            {
                if (_cartManager.CurrentCart.Count == 0) return;

                if (paymentType == "Nakit") _printerManager.KickCashDrawer();

                decimal finalTotal = 0m;
                if (!string.IsNullOrWhiteSpace(FinalTotal))
                {
                    string totalStr = FinalTotal
                        .Replace("₺", string.Empty)
                        .Replace(" ", string.Empty)
                        .Trim();
                    decimal.TryParse(totalStr, out finalTotal);
                }

                string kdvSummary = "\nKDV BILGISI";
                var groupedKdv = _cartManager.CurrentCart.GroupBy(x => x.Product.KdvRate).OrderBy(g => g.Key);
                foreach (var group in groupedKdv)
                {
                    decimal groupTotal = group.Sum(x => x.Product.Price * (decimal)x.Quantity);
                    decimal rateDec    = group.Key / 100m;
                    decimal groupKdv   = groupTotal - (groupTotal / (1m + rateDec));
                    kdvSummary += $"\n% {group.Key} KDV: {groupKdv:C2}";
                }

                var cartSnapshot = _cartManager.CurrentCart.ToList();
                decimal saleTotal = finalTotal > 0m ? finalTotal : _cartManager.TotalAmount;
                string receiptNo  = _cartManager.CompleteSale(paymentType, saleTotal);

                string companyName = _settingsManager.GetSetting("CompanyName", "ENVISIONARY MARKETING").ToUpper();

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.AppendLine(CenterText(companyName));
                sb.AppendLine(CenterText("SATIS FISI"));
                sb.AppendLine("--------------------------------");
                sb.AppendLine($"Tarih : {DateTime.Now:dd.MM.yyyy HH:mm}");
                sb.AppendLine($"Fis No: {receiptNo}");
                sb.AppendLine("--------------------------------");
                sb.AppendLine("URUN                       TUTAR");

                foreach (var item in cartSnapshot)
                {
                    string pName = item.Product.Name.Length > 16
                        ? item.Product.Name.Substring(0, 16)
                        : item.Product.Name;

                    decimal lineTotal = item.Product.Price * (decimal)item.Quantity;
                    sb.AppendLine(pName.PadRight(16) + lineTotal.ToString("C2").PadLeft(16));

                    if (item.Quantity > 1)
                        sb.AppendLine($"  {item.Quantity}x {item.Product.Price:C2}");
                }

                sb.AppendLine("--------------------------------");
                sb.AppendLine("TOPLAM TUTAR:   " + saleTotal.ToString("C2").PadLeft(16));
                sb.AppendLine($"ODENEN ({paymentType}):".PadRight(16) + saleTotal.ToString("C2").PadLeft(16));
                sb.AppendLine(kdvSummary);
                sb.AppendLine("--------------------------------");
                sb.AppendLine(CenterText("MALI DEGERI YOKTUR"));
                sb.AppendLine(CenterText("Bizi Tercih Ettiginiz"));
                sb.AppendLine(CenterText("Icin Tesekkurler!"));

                _printerManager.PrintReceipt(sb.ToString());
                UpdateTotal();

                _dialogService.ShowSuccessDialog(
                    "Tahsilat Onaylandı",
                    "Satış işlemi başarıyla tamamlandı ve fiş yazdırıldı.",
                    finalTotal.ToString("C2"),
                    paymentType,
                    receiptNo
                );

                DiscountPercent = "0";
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message, "Satış Hatası", "OK", "Error");
            }
        }

        private void ProcessRefund()
        {
            var refundResult = _dialogService.ShowRefundDialog(_productManager);
            if (refundResult.IsSuccess && refundResult.RefundedProduct != null)
            {
                try
                {
                    string receiptNo = _cartManager.ProcessRefund(refundResult.RefundedProduct, refundResult.RefundPaymentType, refundResult.RefundQuantity);
                    decimal refundTotal = refundResult.RefundedProduct.Price * (decimal)refundResult.RefundQuantity;

                    string companyName = _settingsManager.GetSetting("CompanyName", "ENVISIONARY MARKETING").ToUpper();

                    System.Text.StringBuilder iadeSb = new System.Text.StringBuilder();
                    iadeSb.AppendLine(CenterText(companyName));
                    iadeSb.AppendLine(CenterText("IADE/IPTAL FISI"));
                    iadeSb.AppendLine("--------------------------------");
                    iadeSb.AppendLine($"Tarih : {DateTime.Now:dd.MM.yyyy HH:mm}");
                    iadeSb.AppendLine($"Islem No: {receiptNo}");
                    iadeSb.AppendLine("--------------------------------");
                    iadeSb.AppendLine("IADE EDILEN URUN           TUTAR");

                    string pName = refundResult.RefundedProduct.Name.Length > 16
                        ? refundResult.RefundedProduct.Name.Substring(0, 16)
                        : refundResult.RefundedProduct.Name;
                    iadeSb.AppendLine(pName.PadRight(16) + refundTotal.ToString("C2").PadLeft(16));

                    if (refundResult.RefundQuantity > 1)
                        iadeSb.AppendLine($"  {refundResult.RefundQuantity}x {refundResult.RefundedProduct.Price:C2}");

                    iadeSb.AppendLine("--------------------------------");
                    iadeSb.AppendLine("IADE TUTARI:    " + refundTotal.ToString("C2").PadLeft(16));
                    iadeSb.AppendLine($"ODEME SEKLI:".PadRight(16) + refundResult.RefundPaymentType.PadLeft(16));
                    iadeSb.AppendLine("--------------------------------");
                    iadeSb.AppendLine(CenterText("Bizi Tercih Ettiginiz"));
                    iadeSb.AppendLine(CenterText("Icin Tesekkurler!"));

                    _printerManager.PrintReceipt(iadeSb.ToString());

                    _dialogService.ShowSuccessDialog(
                        "İade Başarılı",
                        $"Ürün: {refundResult.RefundedProduct.Name}\nAdet: {refundResult.RefundQuantity}",
                        refundTotal.ToString("C2"),
                        refundResult.RefundPaymentType,
                        receiptNo
                    );
                }
                catch (Exception ex)
                {
                    _dialogService.ShowMessage(ex.Message, "İade Hatası", "OK", "Error");
                }
            }
        }

        private static string CenterText(string text)
        {
            const int w = 32;
            if (string.IsNullOrEmpty(text)) return string.Empty;
            if (text.Length >= w) return text.Substring(0, w);
            return text.PadLeft(text.Length + (w - text.Length) / 2);
        }

        public void UpdateTotal()
        {
            if (_cartManager == null) return;
            
            decimal subTotalValue = _cartManager.TotalAmount;

            decimal kdvAmount = 0m;
            foreach (var item in _cartManager.CurrentCart)
            {
                decimal productTotal = item.Product.Price * (decimal)item.Quantity;
                decimal kdvRateDec   = item.Product.KdvRate / 100m;
                decimal productKdv   = productTotal - (productTotal / (1m + kdvRateDec));
                kdvAmount += productKdv;
            }

            decimal discountRateValue = 0m;
            if (decimal.TryParse(DiscountPercent, out decimal parsedRate))
            {
                discountRateValue = parsedRate / 100m;
            }
            decimal discountAmountValue     = subTotalValue * discountRateValue;
            decimal finalTotalValue         = subTotalValue - discountAmountValue;
            decimal subTotalWithoutKdvValue = subTotalValue - kdvAmount;

            SubTotal = subTotalWithoutKdvValue.ToString("C2");
            KdvTotal = kdvAmount.ToString("C2");
            DiscountTotal = discountAmountValue.ToString("C2");
            FinalTotal = finalTotalValue.ToString("C2");
        }
    }
}
