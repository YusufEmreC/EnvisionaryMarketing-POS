using System;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;
using PosApp.Models;

namespace PosApp.Views
{
    public partial class AuthWindow : Window
    {
        private readonly DatabaseManager _dbManager;

        public AuthWindow()
        {
            InitializeComponent();

            _dbManager = new DatabaseManager();
            _dbManager.InitializeDatabase();

            CheckSetupStatus();
        }

        // ─────────────────────────────────────────────
        // Panel Yönetimi
        // ─────────────────────────────────────────────

        private void HideAllPanels()
        {
            LoginPanel.Visibility       = Visibility.Collapsed;
            SetupPanel.Visibility       = Visibility.Collapsed;
            WelcomePanel.Visibility     = Visibility.Collapsed;
            ForgotPassPanel.Visibility  = Visibility.Collapsed;
            SupportPanel.Visibility     = Visibility.Collapsed;
        }

        private void CheckSetupStatus()
        {
            HideAllPanels();
            if (!_dbManager.HasAdminUser())
            {
                WelcomePanel.Visibility = Visibility.Visible;
                TxtSubtitle.Text = "Uygulamamızı tercih ettiğiniz için teşekkürler! Kuruluma geçmeden önce kısa bir bilgilendirme...";
            }
            else
            {
                LoginPanel.Visibility = Visibility.Visible;
                TxtSubtitle.Text = "Uygulamaya hoş geldiniz! Lütfen devam etmek için giriş bilgilerinizi girin.";
            }
        }

        // ─────────────────────────────────────────────
        // Pencere Taşıma & Kapatma
        // ─────────────────────────────────────────────

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // ─────────────────────────────────────────────
        // İlk Kurulum
        // ─────────────────────────────────────────────

        private void BtnStartSetup_Click(object sender, RoutedEventArgs e)
        {
            HideAllPanels();
            SetupPanel.Visibility = Visibility.Visible;
            TxtSubtitle.Text = "İlk kuruluma hoş geldiniz! Sistemi kullanmaya başlamak için bilgileri doldurun.";
        }

        private void BtnSetup_Click(object sender, RoutedEventArgs e)
        {
            string companyName  = TxtSetupCompany.Text.Trim();
            string username     = TxtSetupUsername.Text.Trim();
            string password     = TxtSetupPassword.Password;
            string recoveryWord = TxtSetupRecoveryWord.Text.Trim();

            if (string.IsNullOrEmpty(companyName) || string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password)    || string.IsNullOrEmpty(recoveryWord))
            {
                ModernMessageBox.Show("Lütfen tüm alanları (Kurtarma Kelimesi dahil) doldurun.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                using var conn = _dbManager.GetConnection();

                string saveSettings = "INSERT OR REPLACE INTO Settings (Key, Value) VALUES ('CompanyName', @CompanyName)";
                using (var cmd = new SQLiteCommand(saveSettings, conn))
                {
                    cmd.Parameters.AddWithValue("@CompanyName", companyName);
                    cmd.ExecuteNonQuery();
                }

                string createUser = "INSERT INTO Users (Username, PasswordHash, Role, RecoveryWordHash) VALUES (@Username, @PasswordHash, 'Admin', @RecoveryWordHash)";
                using (var cmd = new SQLiteCommand(createUser, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@PasswordHash", HashPassword(password));
                    cmd.Parameters.AddWithValue("@RecoveryWordHash", HashPassword(recoveryWord.ToLowerInvariant()));
                    cmd.ExecuteNonQuery();
                }

                ModernMessageBox.Show("Kurulum başarıyla tamamlandı! Lütfen giriş yapın.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                CheckSetupStatus();
                TxtLoginUsername.Text = username;
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show($"Kurulum sırasında bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ─────────────────────────────────────────────
        // Normal Giriş
        // ─────────────────────────────────────────────

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtLoginUsername.Text.Trim();
            string password = TxtLoginPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModernMessageBox.Show("Kullanıcı adı ve şifre boş bırakılamaz.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string role = null;
                using (var conn = _dbManager.GetConnection())
                {
                    string query = "SELECT Role FROM Users WHERE Username = @Username AND PasswordHash = @PasswordHash";
                    using var cmd = new SQLiteCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@PasswordHash", HashPassword(password));
                    var result = cmd.ExecuteScalar();
                    if (result != null) role = result.ToString();
                }

                if (role != null)
                {
                    OpenMainWindow(new User { Username = username, Role = role });
                }
                else
                {
                    ModernMessageBox.Show("Kullanıcı adı veya şifre hatalı.", "Giriş Başarısız", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show($"Giriş yapılırken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ─────────────────────────────────────────────
        // Şifremi Unuttum
        // ─────────────────────────────────────────────

        private void LnkForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            HideAllPanels();
            ForgotPassPanel.Visibility = Visibility.Visible;
            TxtSubtitle.Text = "Şifrenizi sıfırlamak için kullanıcı adınızı ve kurtarma kelimenizi girin.";
        }

        private void LnkBackToLogin_Click(object sender, RoutedEventArgs e)
        {
            HideAllPanels();
            LoginPanel.Visibility = Visibility.Visible;
            TxtSubtitle.Text = "Uygulamaya hoş geldiniz! Lütfen devam etmek için giriş bilgilerinizi girin.";
        }

        private void BtnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            string username     = TxtForgotUsername.Text.Trim();
            string recoveryWord = TxtForgotRecoveryWord.Text.Trim();
            string newPassword  = TxtForgotNewPassword.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(recoveryWord) || string.IsNullOrEmpty(newPassword))
            {
                ModernMessageBox.Show("Lütfen tüm alanları doldurun.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                bool isValid = false;
                using (var conn = _dbManager.GetConnection())
                {
                    string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username AND RecoveryWordHash = @RecoveryWordHash";
                    using (var cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);
                        cmd.Parameters.AddWithValue("@RecoveryWordHash", HashPassword(recoveryWord.ToLowerInvariant()));
                        isValid = (long)cmd.ExecuteScalar() > 0;
                    }

                    if (isValid)
                    {
                        string updateQuery = "UPDATE Users SET PasswordHash = @NewPasswordHash WHERE Username = @Username";
                        using var updateCmd = new SQLiteCommand(updateQuery, conn);
                        updateCmd.Parameters.AddWithValue("@NewPasswordHash", HashPassword(newPassword));
                        updateCmd.Parameters.AddWithValue("@Username", username);
                        updateCmd.ExecuteNonQuery();
                    }
                }

                if (isValid)
                {
                    ModernMessageBox.Show("Şifreniz başarıyla sıfırlandı! Yeni şifrenizle giriş yapabilirsiniz.", "Başarılı", MessageBoxButton.OK, MessageBoxImage.Information);
                    LnkBackToLogin_Click(null, null);
                    TxtLoginUsername.Text = username;
                    TxtLoginPassword.Password = string.Empty;
                }
                else
                {
                    ModernMessageBox.Show("Kullanıcı adı veya Kurtarma Kelimesi hatalı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show($"Şifre sıfırlanırken hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ─────────────────────────────────────────────
        // Geliştirici Destek Girişi (RSA Challenge-Response)
        // Hardcoded şifre YOK. Private key yalnızca geliştiricidedir.
        // ─────────────────────────────────────────────

        private void LnkSupportAccess_Click(object sender, RoutedEventArgs e)
        {
            HideAllPanels();
            SupportPanel.Visibility = Visibility.Visible;
            TxtSubtitle.Text = "Geliştirici destek girişi — aşağıdaki kodu geliştiricinize bildirin.";

            // Bu cihaza özgü, günlük değişen destek kodu
            TxtChallengeCode.Text = SupportUnlockService.GenerateChallengeCode();
            TxtUnlockCode.Text    = string.Empty;
        }

        private void BtnSupportLogin_Click(object sender, RoutedEventArgs e)
        {
            string challenge   = TxtChallengeCode.Text;
            string unlockCode  = TxtUnlockCode.Text.Trim();

            if (string.IsNullOrEmpty(unlockCode))
            {
                ModernMessageBox.Show("Lütfen geliştiricinizden aldığınız Kilit Açma Kodunu girin.", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SupportUnlockService.VerifyUnlockCode(challenge, unlockCode))
            {
                // RSA imzası doğrulandı → Geçici süper admin girişi
                var superAdmin = new User
                {
                    Username = "Destek",
                    Role     = "Admin"
                };
                OpenMainWindow(superAdmin);
            }
            else
            {
                ModernMessageBox.Show(
                    "Kilit Açma Kodu geçersiz veya süresi dolmuş.\n\n" +
                    "• Kod günlük değişir, bugün üretilen kodu kullandığınızdan emin olun.\n" +
                    "• Kodu eksiksiz yapıştırdığınızdan emin olun.",
                    "Doğrulama Başarısız",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ─────────────────────────────────────────────
        // Yardımcı Metodlar
        // ─────────────────────────────────────────────

        private void OpenMainWindow(User user)
        {
            var mainWindow = new MainWindow(user);
            mainWindow.Show();
            this.Close();
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
