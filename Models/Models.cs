using System;

namespace PosApp.Models
{
    // Veritabanı ve Arayüz (WPF) arasında veri taşıyacak temel sınıflarımız
    public class Product
    {
        public int Id { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        // Para alanları decimal: floating-point yuvarlama hatalarını önler
        public decimal BuyPrice { get; set; }
        public decimal Price { get; set; }
        public double StockQuantity { get; set; }
        public string Category { get; set; } = string.Empty;
        public decimal KdvRate { get; set; } = 20.0m;
        public DateTime? ExpirationDate { get; set; }
        public bool IsQuickProduct { get; set; }
        public bool IsWeighed { get; set; }
        
        // UI için seçim özelliği (Veritabanına kaydedilmez)
        public bool IsSelected { get; set; }
        
        // Kritik Stok kontrolü
        public bool IsCriticalStock => StockQuantity <= 4;
        
        // SKT Yaklaşan kontrolü (15 gün ve altı, geçmiş tarihler de dahil)
        public bool IsExpiringSoon => ExpirationDate.HasValue && (ExpirationDate.Value.Date - DateTime.Now.Date).TotalDays <= 15;
        
        // Kalan gün sayısını metin olarak gösteren özellik
        public string ExpirationStatusText
        {
            get
            {
                if (!ExpirationDate.HasValue) return string.Empty;
                int days = (int)(ExpirationDate.Value.Date - DateTime.Now.Date).TotalDays;
                if (days < 0) return $"{Math.Abs(days)} Gün Geçti!";
                if (days == 0) return "Bugün Bitiyor!";
                return $"{days} Gün Kaldı";
            }
        }
    }

    public class SaleItem : System.ComponentModel.INotifyPropertyChanged
    {
        public Product Product { get; set; }
        
        private double _quantity;
        public double Quantity 
        { 
            get => _quantity; 
            set 
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                    OnPropertyChanged(nameof(SubTotal));
                }
            } 
        }
        
        // Ara toplam: UI tarafında anlık güncellenecek (decimal ile yuvarlama hatası yok)
        public decimal SubTotal => Product.Price * (decimal)Quantity; 

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

    public class InventoryStats
    {
        public int TotalProducts { get; set; }
        public int CriticalStock { get; set; }
        public int Categories { get; set; }
        public int ExpiringSoon { get; set; }
    }

    public class DashboardStats
    {
        public decimal TotalRevenue { get; set; }
        public decimal CashVolume { get; set; }
        public decimal CardVolume { get; set; }
        public decimal NetProfit { get; set; }
        public int TransactionCount { get; set; }
    }

    public class TopProduct
    {
        public string Barcode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class DailyPerformance
    {
        public string DateString { get; set; } = string.Empty;
        public int ReceiptCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal CashVolume { get; set; }
        public decimal CardVolume { get; set; }
    }

    public class ZReportHistory
    {
        public int Id { get; set; }
        public string ReportDateString { get; set; } = string.Empty;
        public decimal OpeningCash { get; set; }
        public decimal CashSales { get; set; }
        public decimal CreditSales { get; set; }
        public decimal CashRefunds { get; set; }
        public decimal CreditRefunds { get; set; }
        public decimal Expenses { get; set; }
        public decimal ExpectedCash { get; set; }
        public decimal CountedCash { get; set; }
        public decimal CashDifference { get; set; }
        public bool IsClosed { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Cashier"; // Admin, Cashier
        public string RecoveryWordHash { get; set; } = string.Empty;
    }
}
