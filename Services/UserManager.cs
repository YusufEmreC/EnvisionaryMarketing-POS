using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;
using PosApp.Models;

namespace PosApp.Services
{
    public class UserManager
    {
        private readonly DatabaseManager _dbManager;

        public UserManager(DatabaseManager dbManager)
        {
            _dbManager = dbManager;
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        public List<User> GetAllUsers()
        {
            var users = new List<User>();
            using (var conn = _dbManager.GetConnection())
            {
                string query = "SELECT Id, Username, Role FROM Users";
                using (var cmd = new SQLiteCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Username = reader["Username"].ToString(),
                            Role = reader["Role"].ToString()
                        });
                    }
                }
            }
            return users;
        }

        public bool IsUsernameTaken(string username, int? excludeId = null)
        {
            using (var conn = _dbManager.GetConnection())
            {
                string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                if (excludeId.HasValue)
                {
                    query += " AND Id != @Id";
                }

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    if (excludeId.HasValue) cmd.Parameters.AddWithValue("@Id", excludeId.Value);

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        public void AddCashier(string username, string passwordHash)
        {
            using (var conn = _dbManager.GetConnection())
            {
                string query = "INSERT INTO Users (Username, PasswordHash, Role) VALUES (@Username, @PasswordHash, 'Cashier')";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteUser(int id)
        {
            using (var conn = _dbManager.GetConnection())
            {
                string query = "DELETE FROM Users WHERE Id = @Id";
                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateUser(int id, string username, string passwordHash = null)
        {
            using (var conn = _dbManager.GetConnection())
            {
                string query;
                if (string.IsNullOrEmpty(passwordHash))
                {
                    query = "UPDATE Users SET Username = @Username WHERE Id = @Id";
                }
                else
                {
                    query = "UPDATE Users SET Username = @Username, PasswordHash = @PasswordHash WHERE Id = @Id";
                }

                using (var cmd = new SQLiteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    if (!string.IsNullOrEmpty(passwordHash))
                    {
                        cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    }
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
