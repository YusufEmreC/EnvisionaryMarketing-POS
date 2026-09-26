using System;
using System.Windows;
using System.Windows.Controls;

namespace PosApp.Views
{
    public partial class MainWindow : Window
    {
        private DatabaseManager _dbManager;
        private ProductManager _productManager;
        private CartManager _cartManager;
        private PrinterManager _printerManager;
        private ReportManager _reportManager;
        private BarcodeListener _barcodeListener;
        private SettingsManager _settingsManager;
        private UserManager _userManager;
        private PosApp.Infrastructure.IDialogService _dialogService;
        private System.Windows.Threading.DispatcherTimer _timer;

        // View Modülleri
        private CheckoutView _checkoutView;
        private InventoryView _inventoryView;
        private ReportsView _reportsView;
        private ArchiveView _archiveView;
        private ClosingView _closingView;
        private SettingsView _settingsView;
        private UsersView _usersView;
        
        private User _currentUser;

        public MainWindow(User user = null)
        {
            _currentUser = user;
            InitializeComponent();
            InitializeBackend();
            StartClockTimer();
        }

        private void StartClockTimer()
        {
            _timer = new System.Windows.Threading.DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) =>
            {
                TxtClockTime.Text = DateTime.Now.ToString("HH:mm");
                TxtClockDate.Text = DateTime.Now.ToString("dd MMMM yyyy");
            };
            _timer.Start();
            
            // İlk değeri hemen yaz
            TxtClockTime.Text = DateTime.Now.ToString("HH:mm");
            TxtClockDate.Text = DateTime.Now.ToString("dd MMMM yyyy");
        }

        private void InitializeBackend()
        {
            _dbManager = new DatabaseManager();
            _dbManager.InitializeDatabase(); 
            
            _settingsManager = new SettingsManager(_dbManager);
            _productManager = new ProductManager(_dbManager);
            _userManager = new UserManager(_dbManager);
            


            _cartManager = new CartManager(_dbManager);
            _reportManager = new ReportManager(_dbManager);
            _dialogService = new PosApp.Infrastructure.DialogService();
            
            // Ayarlardan yazıcı ismini çek, bulamazsa varsayılanı kullan
            string printerName = _settingsManager.GetSetting("PrinterName", "Termal_Yazici");
            _printerManager = new PrinterManager(printerName); 

            // Sayfaları ayağa kaldır ve bağımlılıkları enjekte et
            _checkoutView = new CheckoutView();
            _checkoutView.Initialize(_productManager, _cartManager, _printerManager, _settingsManager, _dialogService);
            _checkoutView.RequestAddProduct += (barcode) =>
            {
                NavInventory_Click(null, null);
                _inventoryView.PrefillNewProduct(barcode);
            };

            _inventoryView = new InventoryView();
            _inventoryView.Initialize(_dbManager, _productManager, _dialogService);

            _reportsView = new ReportsView();
            _reportsView.Initialize(_reportManager, _dialogService);

            _archiveView = new ArchiveView();
            _archiveView.Initialize(_reportManager, _dialogService);

            _closingView = new ClosingView();
            _closingView.Initialize(_reportManager, _printerManager, _settingsManager, _dialogService);

            _settingsView = new SettingsView();
            _settingsView.Initialize(_settingsManager, _dbManager, _dialogService);

            _usersView = new UsersView();
            _usersView.Initialize(_userManager, _dialogService);

            // Firma ismini güncelle
            TxtCompanyName.Text = _settingsManager.GetSetting("CompanyName", "Firma Adı");

            // Oturum açan kullanıcının rolünü/adını yazdır
            if (_currentUser != null)
            {
                TxtCurrentUser.Text = _currentUser.Role == "Admin" ? "Yönetici" : "Kasiyer";
            }
            else
            {
                TxtCurrentUser.Text = "Kasiyer"; // Varsayılan oturum
            }

            // Role Dayalı Kısıtlamalar (RBAC)
            if (_currentUser != null && _currentUser.Role == "Admin")
            {
                // Admin: Kasa menüsüne ihtiyacı yok
                BtnNavCheckout.Visibility = Visibility.Collapsed;
                
                // Başlangıç ekranı Raporlar (Performans ve Analiz) olsun
                _reportsView.RefreshData();
                MainContent.Content = _reportsView;
                SetActiveTab(BtnNavReports);
            }
            else
            {
                // Kasiyer (veya varsayılan): Raporlar, Ayarlar ve Kullanıcılar görünmesin
                BtnNavReports.Visibility = Visibility.Collapsed;
                BtnNavArchive.Visibility = Visibility.Collapsed;
                BtnNavSettings.Visibility = Visibility.Collapsed;
                BtnNavUsers.Visibility = Visibility.Collapsed;
                
                // Başlangıç ekranı Kasa olsun
                MainContent.Content = _checkoutView;
                SetActiveTab(BtnNavCheckout);
            }

            // Arka plandaki barkod dinleyiciyi başlat
            _barcodeListener = new BarcodeListener();
            _barcodeListener.BarcodeScanned += OnBarcodeScanned;
        }

        private void OnBarcodeScanned(object sender, string barcode)
        {
            Dispatcher.Invoke(() =>
            {
                // Barkod kasa ekranındayken sepete işlemeli, envanter ekranındayken ürün eklemeye yazılmalı
                if (MainContent.Content == _checkoutView)
                {
                    (_checkoutView.DataContext as PosApp.ViewModels.CheckoutViewModel)?.ProcessBarcodeCommand.Execute(barcode);
                }
                else if (MainContent.Content == _inventoryView)
                {
                    _inventoryView.ProcessBarcode(barcode);
                }
                else 
                {
                    ModernMessageBox.Show($"Kasa veya Envanter ekranında değilsiniz.\nOkutulan Barkod: {barcode}", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            });
        }

        private void SetActiveTab(Button activeBtn)
        {
            if (BtnNavCheckout == null) return; // Henüz yüklenmediyse

            var buttons = new[] { BtnNavCheckout, BtnNavInventory, BtnNavReports, BtnNavArchive, BtnNavClosing, BtnNavUsers, BtnNavSettings };
            foreach (var btn in buttons)
            {
                if (btn == activeBtn)
                {
                    btn.SetResourceReference(StyleProperty, "SidebarActiveButtonStyle");
                }
                else
                {
                    btn.SetResourceReference(StyleProperty, "SidebarInactiveButtonStyle");
                }
            }
        }

        private void NavCheckout_Click(object sender, RoutedEventArgs e)
        {
            (_checkoutView.DataContext as PosApp.ViewModels.CheckoutViewModel)?.Initialize();
            MainContent.Content = _checkoutView;
            SetActiveTab(BtnNavCheckout);
        }

        private void NavInventory_Click(object sender, RoutedEventArgs e)
        {
            _inventoryView.Initialize(_dbManager, _productManager, _dialogService); // Stok verisini yenile
            MainContent.Content = _inventoryView;
            SetActiveTab(BtnNavInventory);
        }

        private void NavReports_Click(object sender, RoutedEventArgs e)
        {
            _reportsView.RefreshData(); // Sayfa açılırken verileri yenile
            MainContent.Content = _reportsView;
            SetActiveTab(BtnNavReports);
        }

        private void NavArchive_Click(object sender, RoutedEventArgs e)
        {
            _archiveView.RefreshData(); // Arşivi yenile
            MainContent.Content = _archiveView;
            SetActiveTab(BtnNavArchive);
        }

        private void NavClosing_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = _closingView;
            SetActiveTab(BtnNavClosing);
        }

        private void NavSettings_Click(object sender, RoutedEventArgs e)
        {
            _settingsView.Initialize(_settingsManager, _dbManager, _dialogService);
            MainContent.Content = _settingsView;
            SetActiveTab(BtnNavSettings);
        }

        private void NavUsers_Click(object sender, RoutedEventArgs e)
        {
            _usersView.Initialize(_userManager, _dialogService);
            MainContent.Content = _usersView;
            SetActiveTab(BtnNavUsers);
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            AuthWindow authWindow = new AuthWindow();
            authWindow.Show();
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _barcodeListener?.Dispose();
            base.OnClosed(e);
        }
    }
}
