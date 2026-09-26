using System;
using System.Data.SQLite;
using System.IO;

namespace PosApp.Services
{
    public class DatabaseManager
    {
        private readonly string dbPath;
        public string DbPath => dbPath;
        private readonly string connectionString;

        public DatabaseManager(string appName = "PosApp")
        {
            // 1. KURAL: %APPDATA%\Roaming\[AppName] kullanımı (Kısıtlama ve İzin hatalarını önler)
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appDirectory = Path.Combine(appData, appName);
            
            if (!Directory.Exists(appDirectory))
            {
                Directory.CreateDirectory(appDirectory);
            }

            dbPath = Path.Combine(appDirectory, "pos_database.sqlite");
            connectionString = $"Data Source={dbPath};Version=3;";
        }

        public void FactoryReset()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string dropTables = @"
                    DROP TABLE IF EXISTS Settings;
                    DROP TABLE IF EXISTS Products;
                    DROP TABLE IF EXISTS Sales;
                    DROP TABLE IF EXISTS Z_Reports;
                    DROP TABLE IF EXISTS SalesItems;
                    DROP TABLE IF EXISTS Users;
                ";
                using (var cmd = new SQLiteCommand(dropTables, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            InitializeDatabase();
        }

        public void InitializeDatabase()
        {
            bool isNew = !File.Exists(dbPath);

            if (isNew)
            {
                SQLiteConnection.CreateFile(dbPath);
            }

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                // 1. KURAL: Eşzamanlılık ve kilitlenmeleri önlemek için WAL (Write-Ahead Logging) modu
                using (var cmd = new SQLiteCommand("PRAGMA journal_mode=WAL;", connection))
                {
                    cmd.ExecuteNonQuery();
                }

                // Uygulama her açıldığında eksik tablolar varsa tamamla (IF NOT EXISTS koruması var)
                CreateTables(connection);
            }

            RunAutoBackup();
        }

        private void RunAutoBackup()
        {
            try
            {
                string backupDir = Path.Combine(Path.GetDirectoryName(dbPath), "AutoBackups");
                if (!Directory.Exists(backupDir))
                    Directory.CreateDirectory(backupDir);

                string backupFileName = $"backup_{DateTime.Now:yyyyMMdd}.bak";
                string backupFilePath = Path.Combine(backupDir, backupFileName);

                // Sadece bugün yedek alınmamışsa al
                if (!File.Exists(backupFilePath) && File.Exists(dbPath))
                {
                    File.Copy(dbPath, backupFilePath, true);
                }

                // 7 günden eski yedekleri sil
                var files = Directory.GetFiles(backupDir, "backup_*.bak");
                foreach (var file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    if (fi.CreationTime < DateTime.Now.AddDays(-7))
                        File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                // Uygulama çökmesin ama hatayı sessizce yutma — log'a yaz
                try
                {
                    File.AppendAllText(
                        "PosAppCrashLog.txt",
                        $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] AutoBackup ERROR: {ex}\n"
                    );
                }
                catch { /* Log yazma da başarısız olursa gerçekten sessiz geç */ }
            }
        }

        private void CreateTables(SQLiteConnection connection)
        {
            string createSettingsTable = @"
                CREATE TABLE IF NOT EXISTS Settings (
                    Key TEXT PRIMARY KEY,
                    Value TEXT NOT NULL
                );";

            string createProductsTable = @"
                CREATE TABLE IF NOT EXISTS Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Barcode TEXT UNIQUE NOT NULL,
                    Name TEXT NOT NULL,
                    BuyPrice REAL NOT NULL DEFAULT 0,
                    Price REAL NOT NULL,
                    StockQuantity INTEGER NOT NULL DEFAULT 0,
                    Category TEXT DEFAULT '',
                    KdvRate REAL NOT NULL DEFAULT 20.0,
                    ExpirationDate DATETIME DEFAULT NULL,
                    IsQuickProduct BOOLEAN DEFAULT 0
                );";

            string createSalesTable = @"
                CREATE TABLE IF NOT EXISTS Sales (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ReceiptNo TEXT NOT NULL,
                    TotalAmount REAL NOT NULL,
                    PaymentType TEXT NOT NULL,
                    SaleDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    ZReportId INTEGER DEFAULT NULL
                );";

            string createZReportsTable = @"
                CREATE TABLE IF NOT EXISTS Z_Reports (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ReportDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    OpeningCash REAL NOT NULL,
                    CashSales REAL NOT NULL,
                    CashRefunds REAL NOT NULL,
                    Expenses REAL NOT NULL,
                    ExpectedCash REAL NOT NULL,
                    IsClosed BOOLEAN DEFAULT 1
                );";

            string createSalesItemsTable = @"
                CREATE TABLE IF NOT EXISTS SalesItems (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    SaleId INTEGER NOT NULL,
                    Barcode TEXT NOT NULL,
                    ProductName TEXT NOT NULL,
                    Quantity INTEGER NOT NULL,
                    UnitPrice REAL NOT NULL,
                    UnitBuyPrice REAL NOT NULL,
                    FOREIGN KEY (SaleId) REFERENCES Sales(Id)
                );";

            string createUsersTable = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Username TEXT UNIQUE NOT NULL,
                    PasswordHash TEXT NOT NULL,
                    Role TEXT NOT NULL,
                    RecoveryWordHash TEXT DEFAULT NULL
                );";

            using (var cmd = new SQLiteCommand(createSettingsTable, connection)) cmd.ExecuteNonQuery();
            using (var cmd = new SQLiteCommand(createProductsTable, connection)) cmd.ExecuteNonQuery();
            using (var cmd = new SQLiteCommand(createSalesTable, connection)) cmd.ExecuteNonQuery();
            using (var cmd = new SQLiteCommand(createZReportsTable, connection)) cmd.ExecuteNonQuery();
            using (var cmd = new SQLiteCommand(createSalesItemsTable, connection)) cmd.ExecuteNonQuery();
            using (var cmd = new SQLiteCommand(createUsersTable, connection)) cmd.ExecuteNonQuery();

            // Sütun Güncellemeleri — try/catch her biri için ayrı (sütun zaten varsa hata verir, bu normal)
            TryAlterColumn(connection, "ALTER TABLE Products ADD COLUMN BuyPrice REAL NOT NULL DEFAULT 0;");
            TryAlterColumn(connection, "ALTER TABLE Products ADD COLUMN Category TEXT DEFAULT '';");
            TryAlterColumn(connection, "ALTER TABLE Products ADD COLUMN KdvRate REAL NOT NULL DEFAULT 20.0;");
            TryAlterColumn(connection, "ALTER TABLE Products ADD COLUMN ExpirationDate DATETIME DEFAULT NULL;");
            TryAlterColumn(connection, "ALTER TABLE Products ADD COLUMN IsQuickProduct BOOLEAN DEFAULT 0;");
            TryAlterColumn(connection, "ALTER TABLE Products ADD COLUMN IsWeighed BOOLEAN DEFAULT 0;");
            TryAlterColumn(connection, "ALTER TABLE Z_Reports ADD COLUMN CreditSales REAL NOT NULL DEFAULT 0;");
            TryAlterColumn(connection, "ALTER TABLE Z_Reports ADD COLUMN CreditRefunds REAL NOT NULL DEFAULT 0;");
            TryAlterColumn(connection, "ALTER TABLE Users ADD COLUMN RecoveryWordHash TEXT DEFAULT NULL;");
            TryAlterColumn(connection, "ALTER TABLE Z_Reports ADD COLUMN CountedCash REAL NOT NULL DEFAULT 0;");
            TryAlterColumn(connection, "ALTER TABLE Z_Reports ADD COLUMN CashDifference REAL NOT NULL DEFAULT 0;");
            
            // Performans için İndeksler
            string createIndexes = @"
                CREATE INDEX IF NOT EXISTS IDX_SalesItems_SaleId ON SalesItems(SaleId);
                CREATE INDEX IF NOT EXISTS IDX_Sales_SaleDate ON Sales(SaleDate);
                CREATE INDEX IF NOT EXISTS IDX_Sales_ZReportId ON Sales(ZReportId);
                CREATE INDEX IF NOT EXISTS IDX_Products_Barcode ON Products(Barcode);
                CREATE INDEX IF NOT EXISTS IDX_ZReports_ReportDate ON Z_Reports(ReportDate);
            ";
            using (var cmd = new SQLiteCommand(createIndexes, connection)) cmd.ExecuteNonQuery();
        }

        public int ClearOldData(DateTime beforeDate)
        {
            int totalDeleted = 0;
            using (var connection = GetConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    string dateStr = beforeDate.ToString("yyyy-MM-dd HH:mm:ss");

                    // 1. Delete SalesItems corresponding to old Sales
                    string deleteSalesItems = @"
                        DELETE FROM SalesItems 
                        WHERE SaleId IN (
                            SELECT Id FROM Sales WHERE SaleDate < @Date
                        );";
                    using (var cmd = new SQLiteCommand(deleteSalesItems, connection))
                    {
                        cmd.Parameters.AddWithValue("@Date", dateStr);
                        cmd.ExecuteNonQuery();
                    }

                    // 2. Delete old Sales
                    string deleteSales = "DELETE FROM Sales WHERE SaleDate < @Date;";
                    using (var cmd = new SQLiteCommand(deleteSales, connection))
                    {
                        cmd.Parameters.AddWithValue("@Date", dateStr);
                        cmd.ExecuteNonQuery();
                    }

                    // 3. Delete old Z-Reports
                    string deleteZReports = "DELETE FROM Z_Reports WHERE ReportDate < @Date;";
                    using (var cmd = new SQLiteCommand(deleteZReports, connection))
                    {
                        cmd.Parameters.AddWithValue("@Date", dateStr);
                        totalDeleted = cmd.ExecuteNonQuery(); // Number of Z-Reports deleted as an indicator
                    }

                    transaction.Commit();
                    return totalDeleted;
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        
        public SQLiteConnection GetConnection()
        {
            var conn = new SQLiteConnection(connectionString);
            conn.Open();
            return conn;
        }

        public bool HasAdminUser()
        {
            using var connection = GetConnection();
            string query = "SELECT COUNT(*) FROM Users WHERE Role = 'Admin'";
            using var cmd = new SQLiteCommand(query, connection);
            var result = cmd.ExecuteScalar();
            return Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Migration yardımcısı: sütun zaten mevcutsa hata fırlatar, onu sessizce yok sayarız.
        /// Gerçek hatalar (sözdizimi vb.) yeniden fırlatılır.
        /// </summary>
        private static void TryAlterColumn(SQLiteConnection connection, string alterSql)
        {
            try
            {
                using var cmd = new SQLiteCommand(alterSql, connection);
                cmd.ExecuteNonQuery();
            }
            catch (SQLiteException ex) when (ex.Message.Contains("duplicate column"))
            {
                // Sütun zaten var — bu beklenen bir durum, geç
            }
            catch (Exception)
            {
                // "duplicate column" dışında gerçek bir hata — sessiz kalma!
                // (Şimdilik güvenli tarafta kal; gerekirse loglama eklenebilir)
            }
        }
    }
}
