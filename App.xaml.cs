using System.Windows;
using System.Globalization;
using System.Threading;
using System.Windows.Markup;

namespace PosApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.DispatcherUnhandledException += (s, args) =>
            {
                System.IO.File.WriteAllText("PosAppCrashLog.txt", args.Exception.ToString());
                MessageBox.Show("KRİTİK HATA! Lütfen masaüstündeki PosAppCrashLog.txt dosyasına bakın.\n\n" + args.Exception.Message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
                Shutdown();
            };

            // KULLANICI TALEBİ: Fiyatların '$' yerine '₺' (TL) olarak görünmesi için Türkiye formatını zorluyoruz
            CultureInfo cultureInfo = new CultureInfo("tr-TR");
            
            // Kod tarafındaki ToString("C2") formatlarını TL yapar
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;

            // XAML (Arayüz) tarafındaki StringFormat=C2 bağlamalarını TL yapar
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(cultureInfo.IetfLanguageTag)));

            // İlk olarak AuthWindow'u başlat
            PosApp.Views.AuthWindow authWindow = new PosApp.Views.AuthWindow();
            authWindow.Show();
        }
    }
}
