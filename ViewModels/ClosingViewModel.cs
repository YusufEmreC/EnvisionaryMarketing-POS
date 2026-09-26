using System;
using System.Windows.Input;
using PosApp.Infrastructure;
using PosApp.Services;

namespace PosApp.ViewModels
{
    public class ClosingViewModel : ObservableObject
    {
        private readonly ReportManager _reportManager;
        private readonly PrinterManager _printerManager;
        private readonly IDialogService _dialogService;

        public ICommand GenerateXReportCommand { get; }
        public ICommand GenerateZReportCommand { get; }

        public ClosingViewModel(ReportManager reportManager, PrinterManager printerManager, IDialogService dialogService)
        {
            _reportManager = reportManager;
            _printerManager = printerManager;
            _dialogService = dialogService;

            GenerateXReportCommand = new RelayCommand(_ => ExecuteGenerateXReport());
            GenerateZReportCommand = new RelayCommand(_ => ExecuteGenerateZReport());
        }

        private void ExecuteGenerateXReport()
        {
            try
            {
                string reportText = _reportManager.GenerateXReport();
                _dialogService.ShowReportPreview("📄 X-Raporu Önizleme", reportText, () =>
                {
                    _printerManager.PrintReceipt(reportText);
                });
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage("X-Raporu alınırken hata oluştu: " + ex.Message, "Hata", "OK", "Error");
            }
        }

        private void ExecuteGenerateZReport()
        {
            if (_reportManager != null && _reportManager.HasZReportForToday())
            {
                bool proceed = _dialogService.ShowConfirmation(
                    "Bugün için zaten bir Z-Raporu alınmış!\n\nYine de yeni bir kapanış (ikinci Z-Raporu) yapmak istiyor musunuz?",
                    "Uyarı");
                
                if (!proceed)
                {
                    return;
                }
            }

            var result = _dialogService.ShowCashCountDialog();
            if (result.IsSuccess)
            {
                try
                {
                    // Note: GenerateZReport was taking doubles, but ReportManager has been modified to use decimals where possible.
                    // Wait, let's verify if ReportManager.GenerateZReport uses double or decimal for these. We'll pass double for now, 
                    // or decimal if we updated it. We passed decimal in our implementation plan if we refactored ReportManager.
                    string reportText = _reportManager.GenerateZReport((double)result.OpeningCash, (double)result.Expenses, (double)result.CountedCash);
                    
                    _dialogService.ShowReportPreview("📄 Z-Raporu (Gün Sonu) Önizleme", reportText, () =>
                    {
                        _printerManager.PrintReceipt(reportText);
                    });
                }
                catch (Exception ex)
                {
                    _dialogService.ShowMessage("Z-Raporu alınırken hata oluştu: " + ex.Message, "Kritik Hata", "OK", "Error");
                }
            }
        }
    }
}
