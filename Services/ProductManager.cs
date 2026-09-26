using System;
using System.Collections.Generic;
using System.Data.SQLite;
using PosApp.Models;

namespace PosApp.Services
{
    public class ProductManager
    {
        private readonly DatabaseManager _dbManager;

        public ProductManager(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
        }

        // ─── DRY FIX: Tek nokta reader → Product dönüşümü ───────────────────────
        private static Product MapProduct(SQLiteDataReader reader)
        {
            return new Product
            {
                Id             = Convert.ToInt32(reader["Id"]),
                Barcode        = reader["Barcode"].ToString() ?? string.Empty,
                Name           = reader["Name"].ToString()   ?? string.Empty,
                BuyPrice       = Convert.ToDecimal(reader["BuyPrice"]),
                Price          = Convert.ToDecimal(reader["Price"]),
                StockQuantity  = Convert.ToDouble(reader["StockQuantity"]),
                Category       = reader["Category"].ToString() ?? string.Empty,
                KdvRate        = Convert.ToDecimal(reader["KdvRate"]),
                ExpirationDate = reader["ExpirationDate"] != DBNull.Value
                                     ? Convert.ToDateTime(reader["ExpirationDate"])
                                     : (DateTime?)null,
                IsQuickProduct = reader["IsQuickProduct"] != DBNull.Value && Convert.ToBoolean(reader["IsQuickProduct"]),
                IsWeighed      = reader["IsWeighed"]      != DBNull.Value && Convert.ToBoolean(reader["IsWeighed"])
            };
        }
        // ─────────────────────────────────────────────────────────────────────────

        private const string SelectColumns =
            "SELECT Id, Barcode, Name, BuyPrice, Price, StockQuantity, Category, KdvRate, ExpirationDate, IsQuickProduct, IsWeighed FROM Products";

        public Product GetProductByBarcode(string barcode)
        {
            using var connection = _dbManager.GetConnection();
            string query = SelectColumns + " WHERE Barcode = @Barcode";
            using var cmd = new SQLiteCommand(query, connection);
            cmd.Parameters.AddWithValue("@Barcode", barcode);
            using var reader = cmd.ExecuteReader();
            if (reader.Read()) return MapProduct(reader);
            return null;
        }

        public List<Product> SearchProducts(string keyword)
        {
            var products = new List<Product>();
            using var connection = _dbManager.GetConnection();
            string query = SelectColumns + " WHERE Name LIKE @Keyword OR Barcode LIKE @Keyword";
            using var cmd = new SQLiteCommand(query, connection);
            cmd.Parameters.AddWithValue("@Keyword", $"%{keyword}%");
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) products.Add(MapProduct(reader));
            return products;
        }

        public List<Product> GetAllProducts()
        {
            var products = new List<Product>();
            using var connection = _dbManager.GetConnection();
            using var cmd    = new SQLiteCommand(SelectColumns, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) products.Add(MapProduct(reader));
            return products;
        }

        public List<Product> GetQuickProducts()
        {
            var products = new List<Product>();
            using var connection = _dbManager.GetConnection();
            string query = SelectColumns + " WHERE IsQuickProduct = 1";
            using var cmd    = new SQLiteCommand(query, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) products.Add(MapProduct(reader));
            return products;
        }

        public void AddOrUpdateProduct(Product product)
        {
            using var connection = _dbManager.GetConnection();

            string checkQuery = "SELECT COUNT(1) FROM Products WHERE Barcode = @Barcode";
            long count = 0;
            using (var checkCmd = new SQLiteCommand(checkQuery, connection))
            {
                checkCmd.Parameters.AddWithValue("@Barcode", product.Barcode);
                count = (long)checkCmd.ExecuteScalar();
            }

            if (count > 0)
            {
                string updateQuery = @"
                    UPDATE Products 
                    SET Name = @Name, BuyPrice = @BuyPrice, Price = @Price, 
                        StockQuantity = @StockQuantity, Category = @Category,
                        KdvRate = @KdvRate, ExpirationDate = @ExpirationDate,
                        IsQuickProduct = @IsQuickProduct, IsWeighed = @IsWeighed 
                    WHERE Barcode = @Barcode;";

                using var updateCmd = new SQLiteCommand(updateQuery, connection);
                BindProductParams(updateCmd, product);
                updateCmd.ExecuteNonQuery();
            }
            else
            {
                string insertQuery = @"
                    INSERT INTO Products (Barcode, Name, BuyPrice, Price, StockQuantity, Category, KdvRate, ExpirationDate, IsQuickProduct, IsWeighed) 
                    VALUES (@Barcode, @Name, @BuyPrice, @Price, @StockQuantity, @Category, @KdvRate, @ExpirationDate, @IsQuickProduct, @IsWeighed);";

                using var insertCmd = new SQLiteCommand(insertQuery, connection);
                BindProductParams(insertCmd, product);
                insertCmd.ExecuteNonQuery();
            }
        }

        // Ortak parametre bağlama — DRY
        private static void BindProductParams(SQLiteCommand cmd, Product product)
        {
            cmd.Parameters.AddWithValue("@Barcode",        product.Barcode);
            cmd.Parameters.AddWithValue("@Name",           product.Name);
            cmd.Parameters.AddWithValue("@BuyPrice",       product.BuyPrice);
            cmd.Parameters.AddWithValue("@Price",          product.Price);
            cmd.Parameters.AddWithValue("@StockQuantity",  product.StockQuantity);
            cmd.Parameters.AddWithValue("@Category",       product.Category ?? string.Empty);
            cmd.Parameters.AddWithValue("@KdvRate",        product.KdvRate);
            cmd.Parameters.AddWithValue("@ExpirationDate", product.ExpirationDate.HasValue
                                                               ? product.ExpirationDate.Value.ToString("yyyy-MM-dd HH:mm:ss")
                                                               : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@IsQuickProduct", product.IsQuickProduct ? 1 : 0);
            cmd.Parameters.AddWithValue("@IsWeighed",      product.IsWeighed      ? 1 : 0);
        }

        public void DeleteProduct(string barcode)
        {
            using var connection = _dbManager.GetConnection();
            string query = "DELETE FROM Products WHERE Barcode = @Barcode";
            using var cmd = new SQLiteCommand(query, connection);
            cmd.Parameters.AddWithValue("@Barcode", barcode);
            cmd.ExecuteNonQuery();
        }

        public InventoryStats GetInventoryStats(int criticalLimit = 5, int expiringDays = 15)
        {
            int total = 0, critical = 0, categories = 0, expiringSoon = 0;
            using var connection = _dbManager.GetConnection();

            using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Products;", connection))
                total = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);

            using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Products WHERE StockQuantity <= @Limit;", connection))
            {
                cmd.Parameters.AddWithValue("@Limit", criticalLimit);
                critical = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
            }

            using (var cmd = new SQLiteCommand("SELECT COUNT(DISTINCT Category) FROM Products WHERE Category IS NOT NULL AND Category != '';", connection))
                categories = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);

            using (var cmd = new SQLiteCommand("SELECT COUNT(*) FROM Products WHERE ExpirationDate IS NOT NULL AND (julianday(ExpirationDate) - julianday('now', 'localtime')) <= @Days;", connection))
            {
                cmd.Parameters.AddWithValue("@Days", expiringDays);
                expiringSoon = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
            }

            return new InventoryStats 
            { 
                TotalProducts = total, 
                CriticalStock = critical, 
                Categories = categories, 
                ExpiringSoon = expiringSoon 
            };
        }
    }
}
