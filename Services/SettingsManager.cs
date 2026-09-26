using System.Data.SQLite;

namespace PosApp.Services
{
    public class SettingsManager
    {
        private readonly DatabaseManager _dbManager;

        public SettingsManager(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
        }

        public string GetSetting(string key, string defaultValue = "")
        {
            using (var connection = _dbManager.GetConnection())
            {
                string query = "SELECT Value FROM Settings WHERE Key = @Key";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Key", key);
                    var result = cmd.ExecuteScalar();
                    if (result != null) return result.ToString();
                }
            }
            return defaultValue;
        }

        public void SetSetting(string key, string value)
        {
            using (var connection = _dbManager.GetConnection())
            {
                string query = @"
                    INSERT INTO Settings (Key, Value) 
                    VALUES (@Key, @Value) 
                    ON CONFLICT(Key) DO UPDATE SET Value = @Value;";
                using (var cmd = new SQLiteCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Key", key);
                    cmd.Parameters.AddWithValue("@Value", value);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
