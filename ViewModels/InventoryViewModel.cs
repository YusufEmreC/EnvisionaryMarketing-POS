using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using PosApp.Models;
using PosApp.Services;
using PosApp.Infrastructure;

namespace PosApp.ViewModels
{
    public class InventoryViewModel : ObservableObject
    {
        private readonly DatabaseManager _dbManager;
        private readonly ProductManager _productManager;
        private readonly ProductImporter _productImporter;
        private readonly IDialogService _dialogService;

        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();

        private InventoryStats _stats;
        public InventoryStats Stats
        {
            get => _stats;
            set => SetProperty(ref _stats, value);
        }

        #region Filters

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    RefreshData();
                }
            }
        }

        private bool _isCriticalFilterActive;
        public bool IsCriticalFilterActive
        {
            get => _isCriticalFilterActive;
            set
            {
                if (SetProperty(ref _isCriticalFilterActive, value))
                {
                    if (value) IsExpiringFilterActive = false;
                    RefreshData();
                }
            }
        }

        private bool _isExpiringFilterActive;
        public bool IsExpiringFilterActive
        {
            get => _isExpiringFilterActive;
            set
            {
                if (SetProperty(ref _isExpiringFilterActive, value))
                {
                    if (value) IsCriticalFilterActive = false;
                    RefreshData();
                }
            }
        }

        #endregion

        #region Standard Product Form
        
        private string _formBarcode;
        public string FormBarcode { get => _formBarcode; set => SetProperty(ref _formBarcode, value); }
        
        private string _formName;
        public string FormName { get => _formName; set => SetProperty(ref _formName, value); }
        
        private string _formBuyPrice = "0,00";
        public string FormBuyPrice { get => _formBuyPrice; set => SetProperty(ref _formBuyPrice, value); }
        
        private string _formSellPrice = "0,00";
        public string FormSellPrice { get => _formSellPrice; set => SetProperty(ref _formSellPrice, value); }
        
        private string _formStock = "0";
        public string FormStock { get => _formStock; set => SetProperty(ref _formStock, value); }
        
        private string _formCategory;
        public string FormCategory { get => _formCategory; set => SetProperty(ref _formCategory, value); }
        
        private string _formKdvRate = "20";
        public string FormKdvRate { get => _formKdvRate; set => SetProperty(ref _formKdvRate, value); }
        
        private DateTime? _formExpirationDate;
        public DateTime? FormExpirationDate { get => _formExpirationDate; set => SetProperty(ref _formExpirationDate, value); }
        
        private bool _formIsWeighed;
        public bool FormIsWeighed { get => _formIsWeighed; set => SetProperty(ref _formIsWeighed, value); }

        #endregion

        #region Quick Product Form
        
        private string _quickName;
        public string QuickName { get => _quickName; set => SetProperty(ref _quickName, value); }
        
        private string _quickPrice = "0,00";
        public string QuickPrice { get => _quickPrice; set => SetProperty(ref _quickPrice, value); }
        
        private bool _quickIsWeighed;
        public bool QuickIsWeighed { get => _quickIsWeighed; set => SetProperty(ref _quickIsWeighed, value); }

        #endregion

        public ICommand RefreshDataCommand { get; }
        public ICommand SaveProductCommand { get; }
        public ICommand AddQuickProductCommand { get; }
        public ICommand SaveRowProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand DeleteSelectedCommand { get; }
        public ICommand ToggleFilterCommand { get; }

        public InventoryViewModel(DatabaseManager dbManager, ProductManager productManager, IDialogService dialogService)
        {
            _dbManager = dbManager;
            _productManager = productManager;
            _dialogService = dialogService;
            _productImporter = new ProductImporter(dbManager);

            RefreshDataCommand = new RelayCommand(_ => RefreshData());
            SaveProductCommand = new RelayCommand(_ => SaveProduct());
            AddQuickProductCommand = new RelayCommand(_ => AddQuickProduct());
            SaveRowProductCommand = new RelayCommand(p => SaveRowProduct(p as Product));
            DeleteProductCommand = new RelayCommand(p => DeleteProduct(p as Product));
            DeleteSelectedCommand = new RelayCommand(_ => DeleteSelected());
            ToggleFilterCommand = new RelayCommand(f => ToggleFilter(f as string));
        }

        public void Initialize()
        {
            RefreshData();
        }

        public void RefreshData()
        {
            var products = string.IsNullOrWhiteSpace(SearchText) ? _productManager.GetAllProducts() : _productManager.SearchProducts(SearchText);
            
            if (IsCriticalFilterActive)
            {
                products = products.Where(p => p.StockQuantity < 5).ToList();
            }
            if (IsExpiringFilterActive)
            {
                products = products.Where(p => p.IsExpiringSoon).ToList();
            }
            
            Products.Clear();
            foreach (var p in products)
            {
                Products.Add(p);
            }

            Stats = _productManager.GetInventoryStats(criticalLimit: 5, expiringDays: 15);
        }

        private void SaveProduct()
        {
            try
            {
                string barcode = FormBarcode?.Trim();
                string name = FormName?.Trim();

                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(barcode))
                {
                    _dialogService.ShowMessage("Barkod ve Ürün Adı zorunludur!", "Uyarı", "OK", "Warning");
                    return;
                }

                decimal.TryParse(FormBuyPrice?.Replace(".", ","), out decimal buyPrice);
                decimal.TryParse(FormSellPrice?.Replace(".", ","), out decimal sellPrice);
                double.TryParse(FormStock?.Replace(".", ","), out double stock);
                
                decimal kdvRate = 20m;
                if (!string.IsNullOrWhiteSpace(FormKdvRate))
                {
                    decimal.TryParse(FormKdvRate.Replace(".", ","), out kdvRate);
                }

                var product = new Product
                {
                    Barcode = barcode,
                    Name = name,
                    BuyPrice = buyPrice,
                    Price = sellPrice,
                    StockQuantity = stock,
                    Category = FormCategory?.Trim(),
                    KdvRate = kdvRate,
                    ExpirationDate = FormExpirationDate,
                    IsQuickProduct = false,
                    IsWeighed = FormIsWeighed
                };

                _productManager.AddOrUpdateProduct(product);
                RefreshData();
                ClearForm();
                
                _dialogService.ShowMessage($"'{product.Name}' başarıyla kaydedildi!", "Başarılı", "OK", "Information");
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message, "Hata", "OK", "Error");
            }
        }

        public void ClearForm()
        {
            FormBarcode = "";
            FormName = "";
            FormBuyPrice = "0,00";
            FormSellPrice = "0,00";
            FormStock = "0";
            FormCategory = "";
            FormKdvRate = "20";
            FormExpirationDate = null;
            FormIsWeighed = false;
        }

        private void AddQuickProduct()
        {
            try
            {
                string name = QuickName?.Trim();
                if (string.IsNullOrWhiteSpace(name))
                {
                    _dialogService.ShowMessage("Ürün Adı zorunludur!", "Uyarı", "OK", "Warning");
                    return;
                }

                decimal.TryParse(QuickPrice?.Replace(".", ","), out decimal sellPrice);

                string barcode = "_NOBARCODE_" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

                var product = new Product
                {
                    Barcode = barcode,
                    Name = name,
                    BuyPrice = 0,
                    Price = sellPrice,
                    StockQuantity = 0,
                    Category = "Hızlı Ürün",
                    KdvRate = 0,
                    ExpirationDate = null,
                    IsQuickProduct = true,
                    IsWeighed = QuickIsWeighed
                };

                _productManager.AddOrUpdateProduct(product);
                _dialogService.ShowMessage($"{name} hızlı ürün olarak eklendi.", "Başarılı", "OK", "Information");
                RefreshData();
                
                QuickName = "";
                QuickPrice = "0,00";
                QuickIsWeighed = false;
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage("Hızlı ürün eklenirken hata: " + ex.Message, "Hata", "OK", "Error");
            }
        }

        private void SaveRowProduct(Product product)
        {
            if (product != null)
            {
                if (_dialogService.ShowConfirmation($"{product.Name} ürününde yaptığınız değişiklikleri kaydetmek istiyor musunuz?", "Ürün Güncelleme Onayı"))
                {
                    _productManager.AddOrUpdateProduct(product);
                    _dialogService.ShowMessage("İşlem başarıyla gerçekleştirildi.", "Başarılı", "OK", "Information");
                    RefreshData();
                }
                else
                {
                    RefreshData();
                    _dialogService.ShowMessage("İşlem geri alındı. Değişiklikler kaydedilmedi.", "İptal Edildi", "OK", "Warning");
                }
            }
        }

        private void DeleteProduct(Product product)
        {
            if (product != null)
            {
                if (_dialogService.ShowConfirmation($"{product.Name} ürününü silmek istediğinize emin misiniz?", "Onay"))
                {
                    _productManager.DeleteProduct(product.Barcode);
                    RefreshData();
                }
            }
        }

        private void DeleteSelected()
        {
            var selectedProducts = Products.Where(p => p.IsSelected).ToList();
            
            if (selectedProducts.Count == 0)
            {
                _dialogService.ShowMessage("Lütfen silmek için en az bir ürün seçin.", "Uyarı", "OK", "Warning");
                return;
            }

            if (_dialogService.ShowConfirmation($"{selectedProducts.Count} ürünü silmek istediğinize emin misiniz?", "Toplu Silme Onayı"))
            {
                foreach (var product in selectedProducts)
                {
                    _productManager.DeleteProduct(product.Barcode);
                }
                RefreshData();
            }
        }

        private void ToggleFilter(string filter)
        {
            if (filter == "Critical")
            {
                IsCriticalFilterActive = !IsCriticalFilterActive;
            }
            else if (filter == "Expiring")
            {
                IsExpiringFilterActive = !IsExpiringFilterActive;
            }
            else if (filter == "All")
            {
                IsCriticalFilterActive = false;
                IsExpiringFilterActive = false;
            }
        }
        
        // Expose ProductImporter to let View handle FileDialog and SummaryDialog
        public ProductImporter GetProductImporter()
        {
            return _productImporter;
        }
    }
}
