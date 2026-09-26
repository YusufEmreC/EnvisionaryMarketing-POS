using System.Windows;
using System.Windows.Controls;
using PosApp.Models;
using PosApp.Services;
using PosApp.ViewModels;
using PosApp.Infrastructure;

namespace PosApp.Views
{
    public partial class ArchiveView : UserControl
    {
        public ArchiveView()
        {
            InitializeComponent();
        }

        public void Initialize(ReportManager reportManager, IDialogService dialogService)
        {
            var vm = new ArchiveViewModel(reportManager, dialogService);
            DataContext = vm;
        }

        public void RefreshData()
        {
            if (DataContext is ArchiveViewModel vm)
            {
                vm.LoadCommand.Execute(null);
            }
        }

        private void BtnRefreshHistory_Click(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }

        private void BtnViewReport_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is ZReportHistory report)
            {
                ShowArchivedReport(report);
            }
        }

        private void DgZReportHistory_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DgZReportHistory.SelectedItem is ZReportHistory report)
            {
                ShowArchivedReport(report);
            }
        }

        private void ShowArchivedReport(ZReportHistory report)
        {
            var cardDialog = new ZReportCardDialog(report) { Owner = Application.Current.MainWindow };
            cardDialog.ShowDialog();
        }

        private void BtnClearData_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ClearHistoryDialog("🗑️ Satış/Analiz Verilerini Sil", "Sadece 30 günden eskileri sil", "Tüm geçmişi tamamen sil") { Owner = Application.Current.MainWindow };
            if (dialog.ShowDialog() == true)
            {
                if (DataContext is ArchiveViewModel vm)
                {
                    vm.ClearDataCommand.Execute(dialog.DeleteAll);
                }
            }
        }
    }
}
