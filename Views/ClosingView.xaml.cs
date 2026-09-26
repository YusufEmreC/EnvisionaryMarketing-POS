using System.Windows.Controls;
using PosApp.Services;
using PosApp.ViewModels;
using PosApp.Infrastructure;

namespace PosApp.Views
{
    public partial class ClosingView : UserControl
    {
        public ClosingView()
        {
            InitializeComponent();
        }

        public void Initialize(ReportManager reportManager, PrinterManager printerManager, SettingsManager settingsManager, IDialogService dialogService)
        {
            // Set DataContext to ViewModel
            DataContext = new ClosingViewModel(reportManager, printerManager, dialogService);
        }
    }
}
