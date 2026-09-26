using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using PosApp.Services;
using PosApp.ViewModels;
using PosApp.Infrastructure;

namespace PosApp.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        public void Initialize(SettingsManager sm, DatabaseManager dbm, IDialogService dialogService)
        {
            var vm = new SettingsViewModel(sm, dbm, dialogService);
            DataContext = vm;
            vm.LoadSettingsCommand.Execute(null);
        }

        private void TxtAutoBackupPath_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (DataContext is SettingsViewModel vm && System.IO.Directory.Exists(vm.AutoBackupPath))
                {
                    System.Diagnostics.Process.Start("explorer.exe", vm.AutoBackupPath);
                }
            }
            catch { /* Ignore */ }
        }

        private void BtnBackup_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel vm)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Veritabanı Yedeği (*.bak)|*.bak|Tüm Dosyalar (*.*)|*.*";
                sfd.FileName = $"PosApp_Yedek_{System.DateTime.Now:yyyyMMdd_HHmm}.bak";
                sfd.Title = "Yedeği Nereye Kaydetmek İstiyorsunuz? (Örn: Flash Bellek)";

                if (sfd.ShowDialog() == true)
                {
                    vm.BackupDatabase(sfd.FileName);
                }
            }
        }

        private void BtnRestore_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsViewModel vm)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "Veritabanı Yedeği (*.bak;*.sqlite)|*.bak;*.sqlite|Tüm Dosyalar (*.*)|*.*";
                ofd.Title = "Geri Yüklenecek Yedek Dosyasını Seçin";

                if (ofd.ShowDialog() == true)
                {
                    vm.RestoreDatabase(ofd.FileName);
                }
            }
        }
    }
}
