using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Printing;
using System.Windows.Input;
using PosApp.Infrastructure;
using PosApp.Services;

namespace PosApp.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private readonly SettingsManager _settingsManager;
        private readonly DatabaseManager _dbManager;
        private readonly IDialogService _dialogService;

        public ObservableCollection<string> Printers { get; } = new ObservableCollection<string>();

        private string _selectedPrinter;
        public string SelectedPrinter
        {
            get => _selectedPrinter;
            set => SetProperty(ref _selectedPrinter, value);
        }

        private string _autoBackupPath;
        public string AutoBackupPath
        {
            get => _autoBackupPath;
            set => SetProperty(ref _autoBackupPath, value);
        }

        private string _lastAutoBackupInfo;
        public string LastAutoBackupInfo
        {
            get => _lastAutoBackupInfo;
            set => SetProperty(ref _lastAutoBackupInfo, value);
        }

        private string _selectedCleanupMonths = "12";
        public string SelectedCleanupMonths
        {
            get => _selectedCleanupMonths;
            set => SetProperty(ref _selectedCleanupMonths, value);
        }

        public ICommand LoadSettingsCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand ClearDataCommand { get; }
        public ICommand FactoryResetCommand { get; }

        public SettingsViewModel(SettingsManager settingsManager, DatabaseManager dbManager, IDialogService dialogService)
        {
            _settingsManager = settingsManager;
            _dbManager = dbManager;
            _dialogService = dialogService;

            LoadSettingsCommand = new RelayCommand(_ => LoadSettings());
            SaveSettingsCommand = new RelayCommand(_ => SaveSettings());
            ClearDataCommand = new RelayCommand(_ => ClearData());
            FactoryResetCommand = new RelayCommand(_ => FactoryReset());
        }

        private void LoadSettings()
        {
            Printers.Clear();
            try
            {
                var printServer = new LocalPrintServer();
                foreach (var printQueue in printServer.GetPrintQueues())
                {
                    Printers.Add(printQueue.Name);
                }
            }
            catch { /* Ignore */ }

            string savedPrinter = _settingsManager.GetSetting("PrinterName", "Termal_Yazici");
            if (Printers.Contains(savedPrinter))
            {
                SelectedPrinter = savedPrinter;
            }
            else
            {
                SelectedPrinter = savedPrinter; // Can just be set to text if not in list
            }

            LoadBackupInfo();
        }

        private void LoadBackupInfo()
        {
            try
            {
                string dbPath = _dbManager.DbPath;
                string backupDir = Path.Combine(Path.GetDirectoryName(dbPath), "AutoBackups");
                
                AutoBackupPath = backupDir;

                if (Directory.Exists(backupDir))
                {
                    var dirInfo = new DirectoryInfo(backupDir);
                    var newestFile = dirInfo.GetFiles("backup_*.bak").OrderByDescending(f => f.CreationTime).FirstOrDefault();
                    
                    if (newestFile != null)
                    {
                        LastAutoBackupInfo = $"{newestFile.CreationTime:dd.MM.yyyy HH:mm} ( {newestFile.Length / 1024} KB )";
                    }
                    else
                    {
                        LastAutoBackupInfo = "Henüz yedek alınmamış.";
                    }
                }
                else
                {
                    LastAutoBackupInfo = "Henüz yedek alınmamış.";
                }
            }
            catch
            {
                LastAutoBackupInfo = "Bilgi alınamadı.";
                AutoBackupPath = "Bilinmiyor.";
            }
        }

        private void SaveSettings()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(SelectedPrinter))
                {
                    _settingsManager.SetSetting("PrinterName", SelectedPrinter);
                }
                _dialogService.ShowMessage("Ayarlar başarıyla kaydedildi! (Yazıcı değişikliği sonraki fişte geçerli olacaktır)", "Başarılı", "OK", "Information");
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage(ex.Message, "Hata", "OK", "Error");
            }
        }

        private void ClearData()
        {
            if (int.TryParse(SelectedCleanupMonths, out int months))
            {
                DateTime beforeDate;

                if (months == 999)
                {
                    beforeDate = DateTime.Now; // Delete everything
                }
                else
                {
                    beforeDate = DateTime.Now.AddMonths(-months);
                }

                string warningMsg = months == 999
                    ? "TÜM GEÇMİŞ VERİLERİ SİLMEK üzeresiniz!\n\nBu işlem geri alınamaz. Devam etmek istiyor musunuz?"
                    : $"{months} aydan daha eski tüm raporlar ve fişler SİLİNECEKTİR!\n\nBu işlem geri alınamaz. Devam etmek istiyor musunuz?";

                if (_dialogService.ShowConfirmation(warningMsg, "Kritik Uyarı"))
                {
                    try
                    {
                        int deletedZReports = _dbManager.ClearOldData(beforeDate);
                        _dialogService.ShowMessage($"İşlem başarılı!\nToplam {deletedZReports} adet gün sonu verisi (ve ilgili tüm satış detayları) temizlendi.", "Başarılı", "OK", "Information");
                    }
                    catch (Exception ex)
                    {
                        _dialogService.ShowMessage("Veri temizleme sırasında bir hata oluştu:\n" + ex.Message, "Hata", "OK", "Error");
                    }
                }
            }
        }

        private void FactoryReset()
        {
            if (_dialogService.ShowConfirmation("DİKKAT! Tüm verileriniz (ürünler, satışlar, raporlar, kullanıcılar) silinecektir.\n\nFabrika ayarlarına dönmek istediğinize EMIN MİSİNİZ?", "Kritik Uyarı"))
            {
                if (_dialogService.ShowConfirmation("Bu işlem GERİ ALINAMAZ!\n\nGerçekten tüm verileri silip sistemi sıfırlamak istiyor musunuz?", "Son Kararınız Mı?"))
                {
                    try
                    {
                        _dbManager.FactoryReset();
                        _dialogService.ShowMessage("Uygulama başarıyla sıfırlandı. Lütfen yeniden kurulum yapın.", "Sıfırlandı", "OK", "Information");

                        // We will broadcast an event or let View handle navigation/shutdown.
                        // For simplicity in MVVM, we can just trigger a restart directly from here or notify view.
                        // However, shutting down from ViewModel is acceptable.
                        System.Diagnostics.Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
                        System.Windows.Application.Current.Shutdown();
                    }
                    catch (Exception ex)
                    {
                        _dialogService.ShowMessage("Sıfırlama sırasında hata oluştu: " + ex.Message, "Hata", "OK", "Error");
                    }
                }
            }
        }

        public void BackupDatabase(string targetPath)
        {
            try
            {
                string sourceFile = _dbManager.DbPath;
                if (!File.Exists(sourceFile))
                {
                    _dialogService.ShowMessage("Veritabanı dosyası bulunamadı!", "Hata", "OK", "Error");
                    return;
                }

                File.Copy(sourceFile, targetPath, true);
                _dialogService.ShowMessage($"Veritabanı başarıyla yedeklendi!\n\nYedek Konumu: {targetPath}", "Başarılı", "OK", "Information");
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage("Yedekleme sırasında hata oluştu: " + ex.Message, "Hata", "OK", "Error");
            }
        }

        public void RestoreDatabase(string targetPath)
        {
            if (_dialogService.ShowConfirmation("DİKKAT! Yedekten geri yükleme işlemi yaparsanız ŞU ANKİ TÜM VERİLERİNİZ SİLİNİR ve seçtiğiniz dosyadaki eski verilere dönülür.\n\nDevam etmek istiyor musunuz?", "Riskli İşlem Onayı"))
            {
                if (_dialogService.ShowConfirmation("Son Kararınız Mı?\nSeçilen yedeği sisteme kurmak üzeresiniz. İşlemden sonra uygulama YENİDEN BAŞLATILACAKTIR.", "Son Onay"))
                {
                    try
                    {
                        string dbFile = _dbManager.DbPath;
                        
                        System.Data.SQLite.SQLiteConnection.ClearAllPools();
                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                        File.Copy(targetPath, dbFile, true);
                        
                        string walFile = dbFile + "-wal";
                        string shmFile = dbFile + "-shm";
                        if (File.Exists(walFile)) File.Delete(walFile);
                        if (File.Exists(shmFile)) File.Delete(shmFile);

                        _dialogService.ShowMessage("Yedek başarıyla geri yüklendi!\nDeğişikliklerin etkili olması için uygulama şimdi kapatılacaktır. Lütfen uygulamayı tekrar açın.", "Başarılı", "OK", "Information");

                        System.Windows.Application.Current.Shutdown();
                    }
                    catch (Exception ex)
                    {
                        _dialogService.ShowMessage("Geri yükleme işlemi BAŞARISIZ oldu! Lütfen hiçbir işlemin açık olmadığından emin olun.\n\nHata: " + ex.Message, "Kritik Hata", "OK", "Error");
                    }
                }
            }
        }
    }
}
