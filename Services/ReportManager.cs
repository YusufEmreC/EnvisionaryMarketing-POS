using System;
using System.Data.SQLite;

namespace PosApp.Services
{
    public class ReportManager
    {
        private readonly DatabaseManager _dbManager;

        public ReportManager(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
        }

        public string GenerateXReport()
        {
            using (var connection = _dbManager.GetConnection())
            {
                string query = @"
                    SELECT 
                        SUM(CASE WHEN PaymentType = 'Nakit' THEN TotalAmount ELSE 0 END) as CashSales,
                        SUM(CASE WHEN PaymentType = 'Kredi Kartı' THEN TotalAmount ELSE 0 END) as CreditSales,
                        SUM(CASE WHEN PaymentType = 'İade - Nakit' THEN TotalAmount ELSE 0 END) as CashRefunds,
                        SUM(CASE WHEN PaymentType = 'İade - Kredi Kartı' THEN TotalAmount ELSE 0 END) as CreditRefunds,
                        COUNT(CASE WHEN PaymentType NOT LIKE 'İade%' THEN Id END) as ReceiptCount
                    FROM Sales 
                    WHERE ZReportId IS NULL;";

                using (var cmd = new SQLiteCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        double cashSales = reader["CashSales"] != DBNull.Value ? Convert.ToDouble(reader["CashSales"]) : 0;
                        double creditSales = reader["CreditSales"] != DBNull.Value ? Convert.ToDouble(reader["CreditSales"]) : 0;
                        double cashRefunds = reader["CashRefunds"] != DBNull.Value ? Convert.ToDouble(reader["CashRefunds"]) : 0;
                        double creditRefunds = reader["CreditRefunds"] != DBNull.Value ? Convert.ToDouble(reader["CreditRefunds"]) : 0;
                        int receiptCount = reader["ReceiptCount"] != DBNull.Value ? Convert.ToInt32(reader["ReceiptCount"]) : 0;
                        
                        double totalSales = cashSales + creditSales;
                        double totalRefunds = cashRefunds + creditRefunds;
                        double netSales = totalSales - totalRefunds;

                        var sm = new SettingsManager(_dbManager);
                        string companyName = sm.GetSetting("CompanyName", "ENVISIONARY MARKETING").ToUpper();

                        string CenterText(string text) {
                            int w = 32;
                            if (text == null) return "";
                            if (text.Length >= w) return text.Substring(0, w);
                            return text.PadLeft(text.Length + (w - text.Length) / 2);
                        }

                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        sb.AppendLine(CenterText(companyName));
                        sb.AppendLine(CenterText("X RAPORU"));
                        sb.AppendLine("--------------------------------");
                        sb.AppendLine($"Tarih : {DateTime.Now:dd.MM.yyyy HH:mm}");
                        sb.AppendLine("--------------------------------");
                        sb.AppendLine("SATIS BILGILERI");
                        sb.AppendLine("Toplam Satis Tutari: " + totalSales.ToString("C2").PadLeft(11));
                        sb.AppendLine("Toplam Iade Tutari : " + ("-" + totalRefunds.ToString("C2")).PadLeft(11));
                        sb.AppendLine("Net Satis          : " + netSales.ToString("C2").PadLeft(11));
                        sb.AppendLine("");
                        sb.AppendLine("ODEME DETAYLARI");
                        sb.AppendLine("Nakit              : " + cashSales.ToString("C2").PadLeft(11));
                        sb.AppendLine("Kredi Karti        : " + creditSales.ToString("C2").PadLeft(11));
                        sb.AppendLine("--------------------------------");
                        sb.AppendLine("Istatistikler:");
                        sb.AppendLine($"Kesilen Fis Sayisi : {receiptCount,11}");
                        sb.AppendLine("--------------------------------");
                        sb.AppendLine(CenterText("Mali Degeri Yoktur."));
                        sb.AppendLine(CenterText("(Bilgi Amaclidir)"));

                        return sb.ToString();
                    }
                }
            }
            return "X-Raporu oluşturulamadı.";
        }

        public double GetDailyRefunds()
        {
            using (var connection = _dbManager.GetConnection())
            {
                string sumQuery = "SELECT SUM(TotalAmount) FROM Sales WHERE ZReportId IS NULL AND PaymentType LIKE 'İade%';";
                using (var cmd = new SQLiteCommand(sumQuery, connection))
                {
                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value) return Convert.ToDouble(result);
                }
            }
            return 0;
        }

        public bool HasZReportForToday()
        {
            using (var connection = _dbManager.GetConnection())
            {
                string query = "SELECT COUNT(*) FROM Z_Reports WHERE date(ReportDate) = date(@Today) OR date(ReportDate, 'localtime') = date(@Today);";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Today", DateTime.Now.ToString("yyyy-MM-dd"));
                    var count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        // 5. KURAL: Z-Raporu (Günü resmi olarak sonlandırır ve tüm işlemleri kilitler)
        public string GenerateZReport(double openingCash, double expenses, double countedCash)
        {
            using (var connection = _dbManager.GetConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    double cashSales = 0, creditSales = 0, cashRefunds = 0, creditRefunds = 0;
                    int receiptCount = 0;

                    string query = @"
                        SELECT 
                            SUM(CASE WHEN PaymentType = 'Nakit' THEN TotalAmount ELSE 0 END) as CashSales,
                            SUM(CASE WHEN PaymentType = 'Kredi Kartı' THEN TotalAmount ELSE 0 END) as CreditSales,
                            SUM(CASE WHEN PaymentType = 'İade - Nakit' THEN TotalAmount ELSE 0 END) as CashRefunds,
                            SUM(CASE WHEN PaymentType = 'İade - Kredi Kartı' THEN TotalAmount ELSE 0 END) as CreditRefunds,
                            COUNT(CASE WHEN PaymentType NOT LIKE 'İade%' THEN Id END) as ReceiptCount
                        FROM Sales 
                        WHERE ZReportId IS NULL;";

                    using (var cmd = new SQLiteCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            cashSales = reader["CashSales"] != DBNull.Value ? Convert.ToDouble(reader["CashSales"]) : 0;
                            creditSales = reader["CreditSales"] != DBNull.Value ? Convert.ToDouble(reader["CreditSales"]) : 0;
                            cashRefunds = reader["CashRefunds"] != DBNull.Value ? Convert.ToDouble(reader["CashRefunds"]) : 0;
                            creditRefunds = reader["CreditRefunds"] != DBNull.Value ? Convert.ToDouble(reader["CreditRefunds"]) : 0;
                            receiptCount = reader["ReceiptCount"] != DBNull.Value ? Convert.ToInt32(reader["ReceiptCount"]) : 0;
                        }
                    }

                    // 2. Beklenen Nakit Formülü: Açılış + Nakit Satışlar - Nakit İadeler - Giderler
                    double expectedCash = openingCash + cashSales - cashRefunds - expenses;
                    
                    // Kasa Farkı (Sayılan Nakit - Beklenen Nakit)
                    double cashDifference = countedCash - expectedCash;

                    // 3. Z-Raporu Kaydını Oluştur ve ID'sini al
                    long zReportId = 0;
                    string insertZReport = @"
                        INSERT INTO Z_Reports 
                        (OpeningCash, CashSales, CreditSales, CashRefunds, CreditRefunds, Expenses, ExpectedCash, CountedCash, CashDifference, IsClosed)
                        VALUES 
                        (@Open, @CSales, @CrSales, @CRefunds, @CrRefunds, @Exp, @Expected, @Counted, @Diff, 1);
                        SELECT last_insert_rowid();";

                    using (var cmd = new SQLiteCommand(insertZReport, connection))
                    {
                        cmd.Parameters.AddWithValue("@Open", openingCash);
                        cmd.Parameters.AddWithValue("@CSales", cashSales);
                        cmd.Parameters.AddWithValue("@CrSales", creditSales);
                        cmd.Parameters.AddWithValue("@CRefunds", cashRefunds);
                        cmd.Parameters.AddWithValue("@CrRefunds", creditRefunds);
                        cmd.Parameters.AddWithValue("@Exp", expenses);
                        cmd.Parameters.AddWithValue("@Expected", expectedCash);
                        cmd.Parameters.AddWithValue("@Counted", countedCash);
                        cmd.Parameters.AddWithValue("@Diff", cashDifference);
                        zReportId = (long)cmd.ExecuteScalar();
                    }

                    // 4. Güncel Satışları Kilitle (ZReportId atayarak)
                    string lockSales = "UPDATE Sales SET ZReportId = @ZId WHERE ZReportId IS NULL;";
                    using (var cmd = new SQLiteCommand(lockSales, connection))
                    {
                        cmd.Parameters.AddWithValue("@ZId", zReportId);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();

                    double totalSales = cashSales + creditSales;
                    double totalRefunds = cashRefunds + creditRefunds;
                    double netSales = totalSales - totalRefunds;

                    var sm = new SettingsManager(_dbManager);
                    string companyName = sm.GetSetting("CompanyName", "ENVISIONARY MARKETING").ToUpper();

                    string CenterText(string text) {
                        int w = 32;
                        if (text == null) return "";
                        if (text.Length >= w) return text.Substring(0, w);
                        return text.PadLeft(text.Length + (w - text.Length) / 2);
                    }

                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    sb.AppendLine(CenterText(companyName));
                    sb.AppendLine(CenterText("Z RAPORU (GUN SONU RAPORU)"));
                    sb.AppendLine("--------------------------------");
                    sb.AppendLine($"Tarih : {DateTime.Now:dd.MM.yyyy HH:mm}");
                    sb.AppendLine($"Z No  : Z-{zReportId:D5}");
                    sb.AppendLine("--------------------------------");
                    sb.AppendLine("SATIS BILGILERI");
                    sb.AppendLine("Brut Satis         : " + totalSales.ToString("C2").PadLeft(11));
                    sb.AppendLine("Iadeler            : " + ("-" + totalRefunds.ToString("C2")).PadLeft(11));
                    sb.AppendLine("Net Satis          : " + netSales.ToString("C2").PadLeft(11));
                    sb.AppendLine("");
                    sb.AppendLine("TAHSILAT BILGILERI");
                    sb.AppendLine("Nakit              : " + cashSales.ToString("C2").PadLeft(11));
                    sb.AppendLine("Kredi Karti        : " + creditSales.ToString("C2").PadLeft(11));
                    sb.AppendLine("TOPLAM TAHSILAT    : " + totalSales.ToString("C2").PadLeft(11));
                    sb.AppendLine("--------------------------------");
                    sb.AppendLine("KASA DURUMU");
                    sb.AppendLine("Acilis Kasasi      : " + openingCash.ToString("C2").PadLeft(11));
                    sb.AppendLine("Beklenen Nakit     : " + expectedCash.ToString("C2").PadLeft(11));
                    sb.AppendLine("Sayilan Nakit      : " + countedCash.ToString("C2").PadLeft(11));
                    sb.AppendLine("KASA FARKI         : " + cashDifference.ToString("C2").PadLeft(11));
                    sb.AppendLine("--------------------------------");
                    sb.AppendLine("FIS ISTATISTIKLERI");
                    sb.AppendLine($"Toplam Fis Sayisi  : {receiptCount,11}");
                    sb.AppendLine("--------------------------------");
                    sb.AppendLine(CenterText("Teslim Eden: ....."));
                    sb.AppendLine(CenterText("Teslim Alan: ....."));

                    return sb.ToString();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public Models.DashboardStats GetDashboardStats(DateTime date)
        {
            var stats = new Models.DashboardStats();
            using (var connection = _dbManager.GetConnection())
            {
                string query = @"
                    SELECT 
                        SUM(s.TotalAmount) as TotalRev,
                        SUM(CASE WHEN s.PaymentType = 'Nakit' OR s.PaymentType = 'İade - Nakit' THEN s.TotalAmount ELSE 0 END) as CashVol,
                        SUM(CASE WHEN s.PaymentType = 'Kredi Kartı' OR s.PaymentType = 'İade - Kredi Kartı' THEN s.TotalAmount ELSE 0 END) as CardVol,
                        COUNT(DISTINCT s.Id) as TxCount,
                        (SELECT SUM(Quantity * (UnitPrice - UnitBuyPrice)) FROM SalesItems si JOIN Sales ss ON si.SaleId = ss.Id WHERE date(ss.SaleDate) = date(@Date)) as Profit
                    FROM Sales s
                    WHERE date(s.SaleDate) = date(@Date)";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            stats.TotalRevenue     = reader["TotalRev"] != DBNull.Value ? Convert.ToDecimal(reader["TotalRev"]) : 0m;
                            stats.CashVolume       = reader["CashVol"]  != DBNull.Value ? Convert.ToDecimal(reader["CashVol"])  : 0m;
                            stats.CardVolume       = reader["CardVol"]  != DBNull.Value ? Convert.ToDecimal(reader["CardVol"])  : 0m;
                            stats.TransactionCount = reader["TxCount"]  != DBNull.Value ? Convert.ToInt32(reader["TxCount"])    : 0;
                            stats.NetProfit        = reader["Profit"]   != DBNull.Value ? Convert.ToDecimal(reader["Profit"])   : 0m;
                        }
                    }
                }
            }
            return stats;
        }

        public System.Collections.Generic.List<Models.TopProduct> GetTopProducts(DateTime date, int limit = 10)
        {
            var list = new System.Collections.Generic.List<Models.TopProduct>();
            using (var connection = _dbManager.GetConnection())
            {
                string query = @"
                    SELECT si.Barcode, si.ProductName, SUM(si.Quantity) as TotalQty, SUM(si.Quantity * si.UnitPrice) as TotalRev
                    FROM SalesItems si
                    JOIN Sales s ON si.SaleId = s.Id
                    WHERE date(s.SaleDate) = date(@Date)
                    GROUP BY si.Barcode, si.ProductName
                    ORDER BY TotalQty DESC
                    LIMIT @Limit";

                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Date", date.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@Limit", limit);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Models.TopProduct
                            {
                                Barcode = reader["Barcode"].ToString(),
                                Name = reader["ProductName"].ToString(),
                                TotalSold    = Convert.ToInt32(reader["TotalQty"]),
                                TotalRevenue = Convert.ToDecimal(reader["TotalRev"])
                            });
                        }
                    }
                }
            }
            return list;
        }

        public System.Collections.Generic.List<Models.DailyPerformance> GetLast30DaysPerformance()
        {
            var list = new System.Collections.Generic.List<Models.DailyPerformance>();
            using (var connection = _dbManager.GetConnection())
            {
                string query = @"
                    SELECT 
                        date(SaleDate) as Dt,
                        COUNT(Id) as Rcpt,
                        SUM(TotalAmount) as TotalRev,
                        SUM(CASE WHEN PaymentType = 'Nakit' OR PaymentType = 'İade - Nakit' THEN TotalAmount ELSE 0 END) as Cash,
                        SUM(CASE WHEN PaymentType = 'Kredi Kartı' OR PaymentType = 'İade - Kredi Kartı' THEN TotalAmount ELSE 0 END) as Card
                    FROM Sales
                    WHERE SaleDate >= date('now', '-30 days')
                    GROUP BY date(SaleDate)
                    ORDER BY date(SaleDate) DESC";

                using (var cmd = new SQLiteCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Models.DailyPerformance
                        {
                            DateString = reader["Dt"].ToString(),
                            ReceiptCount = Convert.ToInt32(reader["Rcpt"]),
                            TotalRevenue = Convert.ToDecimal(reader["TotalRev"]),
                            CashVolume   = Convert.ToDecimal(reader["Cash"]),
                            CardVolume   = Convert.ToDecimal(reader["Card"])
                        });
                    }
                }
            }
            return list;
        }

        private bool HasColumn(SQLiteDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public System.Collections.Generic.List<Models.ZReportHistory> GetZReportHistory()
        {
            var list = new System.Collections.Generic.List<Models.ZReportHistory>();
            using (var connection = _dbManager.GetConnection())
            {
                string query = @"
                    SELECT *
                    FROM Z_Reports
                    ORDER BY Id DESC
                    LIMIT 30"; 

                using (var cmd = new SQLiteCommand(query, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Models.ZReportHistory
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            ReportDateString = Convert.ToDateTime(reader["ReportDate"]).ToString("dd.MM.yyyy HH:mm"),
                            OpeningCash    = Convert.ToDecimal(reader["OpeningCash"]),
                            CashSales      = Convert.ToDecimal(reader["CashSales"]),
                            CreditSales    = HasColumn(reader, "CreditSales")    && reader["CreditSales"]    != DBNull.Value ? Convert.ToDecimal(reader["CreditSales"])    : 0m,
                            CashRefunds    = Convert.ToDecimal(reader["CashRefunds"]),
                            CreditRefunds  = HasColumn(reader, "CreditRefunds")  && reader["CreditRefunds"]  != DBNull.Value ? Convert.ToDecimal(reader["CreditRefunds"])  : 0m,
                            Expenses       = Convert.ToDecimal(reader["Expenses"]),
                            ExpectedCash   = Convert.ToDecimal(reader["ExpectedCash"]),
                            CountedCash    = HasColumn(reader, "CountedCash")    && reader["CountedCash"]    != DBNull.Value ? Convert.ToDecimal(reader["CountedCash"])    : 0m,
                            CashDifference = HasColumn(reader, "CashDifference") && reader["CashDifference"] != DBNull.Value ? Convert.ToDecimal(reader["CashDifference"]) : 0m,
                            IsClosed = Convert.ToBoolean(reader["IsClosed"])
                        });
                    }
                }
            }
            return list;
        }

        public string FormatZReportFromHistory(Models.ZReportHistory report)
        {
            decimal totalSales  = report.CashSales  + report.CreditSales;
            decimal totalRefunds = report.CashRefunds + report.CreditRefunds;
            decimal netSales     = totalSales - totalRefunds;

            var sm = new SettingsManager(_dbManager);
            string companyName = sm.GetSetting("CompanyName", "ENVISIONARY MARKETING").ToUpper();

            string CenterText(string text) {
                int w = 32;
                if (text == null) return "";
                if (text.Length >= w) return text.Substring(0, w);
                return text.PadLeft(text.Length + (w - text.Length) / 2);
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine(CenterText(companyName));
            sb.AppendLine(CenterText("Z RAPORU (ARSIV KOPYASI)"));
            sb.AppendLine("--------------------------------");
            sb.AppendLine($"Tarih : {report.ReportDateString}");
            sb.AppendLine($"Z No  : Z-{report.Id:D5}");
            sb.AppendLine("--------------------------------");
            sb.AppendLine("SATIS BILGILERI");
            sb.AppendLine("Brut Satis         : " + totalSales.ToString("C2").PadLeft(11));
            sb.AppendLine("Iadeler            : " + ("-" + totalRefunds.ToString("C2")).PadLeft(11));
            sb.AppendLine("Net Satis          : " + netSales.ToString("C2").PadLeft(11));
            sb.AppendLine("");
            sb.AppendLine("TAHSILAT BILGILERI");
            sb.AppendLine("Nakit              : " + report.CashSales.ToString("C2").PadLeft(11));
            sb.AppendLine("Kredi Karti        : " + report.CreditSales.ToString("C2").PadLeft(11));
            sb.AppendLine("TOPLAM TAHSILAT    : " + totalSales.ToString("C2").PadLeft(11));
            sb.AppendLine("--------------------------------");
            sb.AppendLine("KASA DURUMU");
            sb.AppendLine("Acilis Kasasi      : " + report.OpeningCash.ToString("C2").PadLeft(11));
            sb.AppendLine("Beklenen Nakit     : " + report.ExpectedCash.ToString("C2").PadLeft(11));
            sb.AppendLine("Sayilan Nakit      : " + report.CountedCash.ToString("C2").PadLeft(11));
            sb.AppendLine("KASA FARKI         : " + report.CashDifference.ToString("C2").PadLeft(11));
            sb.AppendLine("--------------------------------");
            sb.AppendLine(CenterText("(Arsivden Yazdirilmistir)"));

            return sb.ToString();
        }

        public void ClearSalesHistory(DateTime? beforeDate = null)
        {
            using (var connection = _dbManager.GetConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    if (beforeDate.HasValue)
                    {
                        string dateStr = beforeDate.Value.ToString("yyyy-MM-dd");
                        
                        string deleteSalesItems = "DELETE FROM SalesItems WHERE SaleId IN (SELECT Id FROM Sales WHERE date(SaleDate) < date(@Date));";
                        using (var cmd = new SQLiteCommand(deleteSalesItems, connection))
                        {
                            cmd.Parameters.AddWithValue("@Date", dateStr);
                            cmd.ExecuteNonQuery();
                        }

                        string deleteSales = "DELETE FROM Sales WHERE date(SaleDate) < date(@Date);";
                        using (var cmd = new SQLiteCommand(deleteSales, connection))
                        {
                            cmd.Parameters.AddWithValue("@Date", dateStr);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        new SQLiteCommand("DELETE FROM SalesItems;", connection).ExecuteNonQuery();
                        new SQLiteCommand("DELETE FROM Sales;", connection).ExecuteNonQuery();
                        new SQLiteCommand("DELETE FROM sqlite_sequence WHERE name='SalesItems';", connection).ExecuteNonQuery();
                        new SQLiteCommand("DELETE FROM sqlite_sequence WHERE name='Sales';", connection).ExecuteNonQuery();
                    }
                    
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void ClearZReportHistory(DateTime? beforeDate = null)
        {
            using (var connection = _dbManager.GetConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    if (beforeDate.HasValue)
                    {
                        string dateStr = beforeDate.Value.ToString("yyyy-MM-dd");
                        
                        string deleteZReports = "DELETE FROM Z_Reports WHERE date(ReportDate) < date(@Date);";
                        using (var cmd = new SQLiteCommand(deleteZReports, connection))
                        {
                            cmd.Parameters.AddWithValue("@Date", dateStr);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        new SQLiteCommand("DELETE FROM Z_Reports;", connection).ExecuteNonQuery();
                        new SQLiteCommand("DELETE FROM sqlite_sequence WHERE name='Z_Reports';", connection).ExecuteNonQuery();
                    }
                    
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
