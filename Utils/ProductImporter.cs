using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using PosApp.Models;

namespace PosApp.Utils
{
    public class ImportAnalysisResult
    {
        public int TotalRows { get; set; }
        public int NewProductsCount { get; set; }
        public int UpdatedProductsCount { get; set; }
        public int MissingExpirationCount { get; set; }
        public List<Product> ParsedProducts { get; set; } = new List<Product>();
    }

    public class ProductImporter
    {
        private readonly DatabaseManager _dbManager;

        public ProductImporter(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
        }

        public ImportAnalysisResult AnalyzeExcelFile(string filePath)
        {
            if (!File.Exists(filePath)) throw new FileNotFoundException("Excel dosyası bulunamadı.");

            var result = new ImportAnalysisResult();

            using (var connection = _dbManager.GetConnection())
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // 1. satır başlık

                    string checkQuery = "SELECT COUNT(1) FROM Products WHERE Barcode = @Barcode";
                    using (var checkCmd = new SQLiteCommand(checkQuery, connection))
                    {
                        foreach (var row in rows)
                        {
                            string barcode = row.Cell(1).GetString().Trim();
                            if (string.IsNullOrWhiteSpace(barcode)) continue;

                            result.TotalRows++;

                            string priceStr = row.Cell(3).GetString().Replace(".", ",");
                            decimal.TryParse(priceStr, out decimal price);

                            string stockStr = row.Cell(4).GetString().Replace(".", ",");
                            double.TryParse(stockStr, out double stock);

                            string category = row.Cell(5).GetString().Trim();
                            
                            string buyPriceStr = row.Cell(6).GetString().Replace(".", ",");
                            decimal.TryParse(buyPriceStr, out decimal buyPrice);

                            string kdvRateStr = row.Cell(7).GetString().Replace(".", ",");
                            decimal kdvRate = 20.0m;
                            if (!string.IsNullOrWhiteSpace(kdvRateStr)) {
                                decimal.TryParse(kdvRateStr, out kdvRate);
                            }

                            string expirationStr = row.Cell(8).GetString().Trim();
                            DateTime? expirationDate = null;
                            if (!string.IsNullOrWhiteSpace(expirationStr) && DateTime.TryParse(expirationStr, out DateTime parsedDate)) {
                                expirationDate = parsedDate;
                            }

                            if (!expirationDate.HasValue)
                            {
                                result.MissingExpirationCount++;
                            }

                            var product = new Product
                            {
                                Barcode = barcode,
                                Name = row.Cell(2).GetString().Trim(),
                                Price = price,
                                StockQuantity = (int)stock,
                                Category = category,
                                BuyPrice = buyPrice,
                                KdvRate = kdvRate,
                                ExpirationDate = expirationDate
                            };
                            
                            result.ParsedProducts.Add(product);

                            checkCmd.Parameters.Clear();
                            checkCmd.Parameters.AddWithValue("@Barcode", barcode);
                            long count = (long)checkCmd.ExecuteScalar();

                            if (count > 0)
                            {
                                result.UpdatedProductsCount++;
                                product.IsSelected = true; // Flag as existing (update)
                            }
                            else
                            {
                                result.NewProductsCount++;
                                product.IsSelected = false; // Flag as new (insert)
                            }
                        }
                    }
                }
            }

            return result;
        }

        public int ExecuteImport(ImportAnalysisResult analysisResult)
        {
            int importedCount = 0;

            using (var connection = _dbManager.GetConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    string updateQuery = @"
                        UPDATE Products SET 
                            Name = @Name, 
                            Price = @Price, 
                            StockQuantity = StockQuantity + @Stock,
                            Category = CASE WHEN @Category != '' THEN @Category ELSE Category END,
                            BuyPrice = CASE WHEN @BuyPrice > 0 THEN @BuyPrice ELSE BuyPrice END,
                            KdvRate = CASE WHEN @KdvRate >= 0 THEN @KdvRate ELSE KdvRate END,
                            ExpirationDate = CASE WHEN @ExpirationDate IS NOT NULL THEN @ExpirationDate ELSE ExpirationDate END
                        WHERE Barcode = @Barcode;";

                    string insertQuery = @"
                        INSERT INTO Products (Barcode, Name, Price, StockQuantity, Category, BuyPrice, KdvRate, ExpirationDate) 
                        VALUES (@Barcode, @Name, @Price, @Stock, @Category, @BuyPrice, @KdvRate, @ExpirationDate);";

                    using (var updateCmd = new SQLiteCommand(updateQuery, connection))
                    using (var insertCmd = new SQLiteCommand(insertQuery, connection))
                    {
                        foreach (var product in analysisResult.ParsedProducts)
                        {
                            if (product.IsSelected) // Existing (Update)
                            {
                                updateCmd.Parameters.Clear();
                                updateCmd.Parameters.AddWithValue("@Barcode", product.Barcode);
                                updateCmd.Parameters.AddWithValue("@Name", product.Name);
                                updateCmd.Parameters.AddWithValue("@Price", product.Price);
                                updateCmd.Parameters.AddWithValue("@Stock", product.StockQuantity);
                                updateCmd.Parameters.AddWithValue("@Category", product.Category);
                                updateCmd.Parameters.AddWithValue("@BuyPrice", product.BuyPrice);
                                updateCmd.Parameters.AddWithValue("@KdvRate", product.KdvRate);
                                updateCmd.Parameters.AddWithValue("@ExpirationDate", product.ExpirationDate.HasValue ? product.ExpirationDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : (object)DBNull.Value);
                                updateCmd.ExecuteNonQuery();
                            }
                            else // New (Insert)
                            {
                                insertCmd.Parameters.Clear();
                                insertCmd.Parameters.AddWithValue("@Barcode", product.Barcode);
                                insertCmd.Parameters.AddWithValue("@Name", product.Name);
                                insertCmd.Parameters.AddWithValue("@Price", product.Price);
                                insertCmd.Parameters.AddWithValue("@Stock", product.StockQuantity);
                                insertCmd.Parameters.AddWithValue("@Category", product.Category);
                                insertCmd.Parameters.AddWithValue("@BuyPrice", product.BuyPrice);
                                insertCmd.Parameters.AddWithValue("@KdvRate", product.KdvRate);
                                insertCmd.Parameters.AddWithValue("@ExpirationDate", product.ExpirationDate.HasValue ? product.ExpirationDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : (object)DBNull.Value);
                                insertCmd.ExecuteNonQuery();
                            }
                            importedCount++;
                        }
                    }
                    
                    transaction.Commit();
                    return importedCount;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("İçe aktarma sırasında hata oluştu: " + ex.Message);
                }
            }
        }
        
        public void DecreaseStock(string barcode, int quantity)
        {
            using (var connection = _dbManager.GetConnection())
            {
                string query = "UPDATE Products SET StockQuantity = StockQuantity - @Qty WHERE Barcode = @Barcode";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Qty", quantity);
                    cmd.Parameters.AddWithValue("@Barcode", barcode);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
