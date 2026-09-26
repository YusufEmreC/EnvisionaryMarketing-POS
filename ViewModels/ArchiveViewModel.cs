using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using PosApp.Models;
using PosApp.Services;
using PosApp.Infrastructure;

namespace PosApp.ViewModels
{
    public class ArchiveViewModel : ObservableObject
    {
        private readonly ReportManager _reportManager;
        private readonly IDialogService _dialogService;

        public ObservableCollection<ZReportHistory> Reports { get; } = new ObservableCollection<ZReportHistory>();

        public ICommand LoadCommand { get; }
        public ICommand ClearDataCommand { get; }

        public ArchiveViewModel(ReportManager reportManager, IDialogService dialogService)
        {
            _reportManager = reportManager;
            _dialogService = dialogService;

            LoadCommand = new RelayCommand(_ => LoadZReportHistory());
            ClearDataCommand = new RelayCommand<bool>(deleteAll => ClearZReportHistory(deleteAll));
        }

        public void LoadZReportHistory()
        {
            try
            {
                var history = _reportManager.GetZReportHistory();
                Reports.Clear();
                foreach (var report in history)
                {
                    Reports.Add(report);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage("Geçmiş raporlar yüklenirken hata oluştu: " + ex.Message, "Hata", "OK", "Error");
            }
        }

        private void ClearZReportHistory(bool deleteAll)
        {
            try
            {
                if (deleteAll)
                {
                    _reportManager.ClearZReportHistory();
                    _dialogService.ShowMessage("Tüm arşiv verileri tamamen silindi.", "Başarılı", "OK", "Information");
                }
                else
                {
                    DateTime threshold = DateTime.Now.AddDays(-30);
                    _reportManager.ClearZReportHistory(threshold);
                    _dialogService.ShowMessage("Son 30 gün hariç tüm arşiv verileri silindi.", "Başarılı", "OK", "Information");
                }
                LoadZReportHistory();
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage("Veriler silinirken hata oluştu: " + ex.Message, "Hata", "OK", "Error");
            }
        }
    }
}
