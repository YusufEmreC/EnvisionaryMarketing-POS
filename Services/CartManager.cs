using System;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Linq;
using PosApp.Models;

namespace PosApp.Services
{
    public class CartManager
    {
        private readonly DatabaseManager _dbManager;

        // WPF tarafında anlık güncellemeler (DataBinding) için ObservableCollection kullanıyoruz.
        public ObservableCollection<SaleItem> CurrentCart { get; private set; }

        // decimal: para hesabında yuvarlama hatası yok
        public decimal TotalAmount => CurrentCart.Sum(item => item.SubTotal);

        public CartManager(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
            CurrentCart = new ObservableCollection<SaleItem>();
        }

        public void AddToCart(Product product, double quantity = 1)
        {
            var existingItem = CurrentCart.FirstOrDefault(x => x.Product.Barcode == product.Barcode);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                CurrentCart.Add(new SaleItem { Product = product, Quantity = quantity });
            }
        }

        public void RemoveFromCart(string barcode)
        {
            var item = CurrentCart.FirstOrDefault(x => x.Product.Barcode == barcode);
            if (item != null) CurrentCart.Remove(item);
        }

        public void ClearCart()
        {
            CurrentCart.Clear();
        }

        public string ProcessRefund(Models.Product product, string refundType = "İade", double quantity = 1)
        {
            string receiptNo = "IADE" + DateTime.Now.ToString("yyyyMMddHHmmss");
            using var connection = _dbManager.GetConnection();
            using var transaction = connection.BeginTransaction();
            try
            {
                long saleId = 0;
                string insertSale = @"
                    INSERT INTO Sales (ReceiptNo, TotalAmount, PaymentType) VALUES (@Receipt, @Total, @Type);
                    SELECT last_insert_rowid();";
                using (var cmd = new SQLiteCommand(insertSale, connection))
                {
                    cmd.Parameters.AddWithValue("@Receipt", receiptNo);
                    cmd.Parameters.AddWithValue("@Total",   product.Price * (decimal)quantity);
                    cmd.Parameters.AddWithValue("@Type",    refundType);
                    saleId = (long)cmd.ExecuteScalar();
                }

                // İADE: eksi adet ile kayıt
                string insertSaleItem = @"
                    INSERT INTO SalesItems (SaleId, Barcode, ProductName, Quantity, UnitPrice, UnitBuyPrice)
                    VALUES (@SaleId, @Barcode, @Name, @Qty, @Price, @BuyPrice);";
                using (var cmdItem = new SQLiteCommand(insertSaleItem, connection))
                {
                    cmdItem.Parameters.AddWithValue("@SaleId",   saleId);
                    cmdItem.Parameters.AddWithValue("@Barcode",  product.Barcode);
                    cmdItem.Parameters.AddWithValue("@Name",     product.Name);
                    cmdItem.Parameters.AddWithValue("@Qty",      -quantity); // Eksi adet!
                    cmdItem.Parameters.AddWithValue("@Price",    product.Price);
                    cmdItem.Parameters.AddWithValue("@BuyPrice", product.BuyPrice);
                    cmdItem.ExecuteNonQuery();
                }

                // Stoğu Geri Ekle
                string updateStock = "UPDATE Products SET StockQuantity = StockQuantity + @Qty WHERE Id = @Id;";
                using (var cmd = new SQLiteCommand(updateStock, connection))
                {
                    cmd.Parameters.AddWithValue("@Id",  product.Id);
                    cmd.Parameters.AddWithValue("@Qty", quantity);
                    cmd.ExecuteNonQuery();
                }

                transaction.Commit();
                return receiptNo;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public string CompleteSale(string paymentType, decimal finalTotal = -1m)
        {
            if (CurrentCart.Count == 0) throw new InvalidOperationException("Sepet boş!");

            decimal saleTotal = finalTotal >= 0m ? finalTotal : TotalAmount;
            string receiptNo  = "FIS" + DateTime.Now.ToString("yyyyMMddHHmmss");

            using var connection = _dbManager.GetConnection();
            using var transaction = connection.BeginTransaction();
            try
            {
                // 1. Satışı kaydet
                long saleId = 0;
                string insertSale = @"
                    INSERT INTO Sales (ReceiptNo, TotalAmount, PaymentType) VALUES (@Receipt, @Total, @Payment);
                    SELECT last_insert_rowid();";
                using (var cmd = new SQLiteCommand(insertSale, connection))
                {
                    cmd.Parameters.AddWithValue("@Receipt", receiptNo);
                    cmd.Parameters.AddWithValue("@Total",   saleTotal);
                    cmd.Parameters.AddWithValue("@Payment", paymentType);
                    saleId = (long)cmd.ExecuteScalar();
                }

                // 2. SalesItems kaydet
                string insertSaleItem = @"
                    INSERT INTO SalesItems (SaleId, Barcode, ProductName, Quantity, UnitPrice, UnitBuyPrice)
                    VALUES (@SaleId, @Barcode, @Name, @Qty, @Price, @BuyPrice);";
                using (var cmdItem = new SQLiteCommand(insertSaleItem, connection))
                {
                    foreach (var item in CurrentCart)
                    {
                        cmdItem.Parameters.Clear();
                        cmdItem.Parameters.AddWithValue("@SaleId",   saleId);
                        cmdItem.Parameters.AddWithValue("@Barcode",  item.Product.Barcode);
                        cmdItem.Parameters.AddWithValue("@Name",     item.Product.Name);
                        cmdItem.Parameters.AddWithValue("@Qty",      item.Quantity);
                        cmdItem.Parameters.AddWithValue("@Price",    item.Product.Price);
                        cmdItem.Parameters.AddWithValue("@BuyPrice", item.Product.BuyPrice);
                        cmdItem.ExecuteNonQuery();
                    }
                }

                // 3. Stokları Düş
                string updateStock = "UPDATE Products SET StockQuantity = StockQuantity - @Qty WHERE Id = @Id;";
                using (var cmd = new SQLiteCommand(updateStock, connection))
                {
                    foreach (var item in CurrentCart)
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@Qty", item.Quantity);
                        cmd.Parameters.AddWithValue("@Id",  item.Product.Id);
                        cmd.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                ClearCart();
                return receiptNo;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
