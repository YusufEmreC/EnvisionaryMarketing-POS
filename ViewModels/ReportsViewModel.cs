using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using PosApp.Models;
using PosApp.Services;
using PosApp.Infrastructure;

namespace PosApp.ViewModels
{
    public class ReportsViewModel : ObservableObject
    {
        private readonly ReportManager _reportManager;
        private readonly IDialogService _dialogService;

        private DateTime? _selectedDate = DateTime.Today;
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    LoadDataCommand.Execute(null);
                }
            }
        }

        private DashboardStats _stats;
        public DashboardStats Stats
        {
            get => _stats;
            set => SetProperty(ref _stats, value);
        }

        public ObservableCollection<TopProduct> TopProducts { get; } = new ObservableCollection<TopProduct>();
        
        private ObservableCollection<DailyPerformance> _dailyPerformances;
        public ObservableCollection<DailyPerformance> DailyPerformances
        {
            get => _dailyPerformances;
            set => SetProperty(ref _dailyPerformances, value);
        }

        public ICommand LoadDataCommand { get; }
        public ICommand ClearDataCommand { get; }

        public ReportsViewModel(ReportManager reportManager, IDialogService dialogService)
        {
            _reportManager = reportManager;
            _dialogService = dialogService;
            
            _dailyPerformances = new ObservableCollection<DailyPerformance>();

            LoadDataCommand = new RelayCommand(_ => LoadDashboardData());
            ClearDataCommand = new RelayCommand<bool>(deleteAll => ClearSalesHistory(deleteAll));
        }

        private void LoadDashboardData()
        {
            if (_reportManager == null || !SelectedDate.HasValue) return;

            DateTime targetDate = SelectedDate.Value;

            try
            {
                Stats = _reportManager.GetDashboardStats(targetDate);

                var topProducts = _reportManager.GetTopProducts(targetDate, 10);
                TopProducts.Clear();
                foreach(var p in topProducts) TopProducts.Add(p);

                var weeklyPerformance = _reportManager.GetLast30DaysPerformance();
                
                // Reassign collection to trigger PropertyChanged for the chart drawer in View
                var newPerformances = new ObservableCollection<DailyPerformance>();
                foreach(var wp in weeklyPerformance) newPerformances.Add(wp);
                
                DailyPerformances = newPerformances;
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage("Rapor verileri yüklenirken hata oluştu: " + ex.Message, "Hata", "OK", "Error");
            }
        }

        private void ClearSalesHistory(bool deleteAll)
        {
            try
            {
                if (deleteAll)
                {
                    _reportManager.ClearSalesHistory();
                    _dialogService.ShowMessage("Tüm satış verileri ve analizler başarıyla silindi. Z-Raporlarınız korunuyor.", "Başarılı", "OK", "Information");
                }
                else
                {
                    DateTime thirtyDaysAgo = DateTime.Now.AddDays(-30);
                    _reportManager.ClearSalesHistory(thirtyDaysAgo);
                    _dialogService.ShowMessage("30 günden eski satış verileri başarıyla silindi. Z-Raporlarınız korunuyor.", "Başarılı", "OK", "Information");
                }
                LoadDashboardData();
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage("Veriler silinirken hata oluştu: " + ex.Message, "Hata", "OK", "Error");
            }
        }
    }
}
