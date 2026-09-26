using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using PosApp.Services;
using PosApp.ViewModels;
using PosApp.Infrastructure;

namespace PosApp.Views
{
    public partial class ReportsView : UserControl
    {
        public ReportsView()
        {
            InitializeComponent();
        }

        public void Initialize(ReportManager reportManager, IDialogService dialogService)
        {
            var vm = new ReportsViewModel(reportManager, dialogService);
            vm.PropertyChanged += Vm_PropertyChanged;
            DataContext = vm;
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ReportsViewModel.DailyPerformances) && DataContext is ReportsViewModel vm)
            {
                if (vm.DailyPerformances != null)
                {
                    DrawChart(vm.DailyPerformances.ToList());
                }
            }
        }

        public void RefreshData()
        {
            if (DataContext is ReportsViewModel vm)
            {
                vm.LoadDataCommand.Execute(null);
            }
        }

        private void DrawChart(System.Collections.Generic.List<Models.DailyPerformance> performanceList)
        {
            ChartBarsPanel.Children.Clear();
            if (performanceList == null || performanceList.Count == 0) return;

            var list = new System.Collections.Generic.List<Models.DailyPerformance>(performanceList);
            list.Reverse();

            decimal maxRev = 0m;
            foreach (var item in list) if (item.TotalRevenue > maxRev) maxRev = item.TotalRevenue;
            
            if (maxRev == 0m) maxRev = 1m;
            double maxHeight = 100; // WPF Height double alır

            foreach (var item in list)
            {
                // decimal hesap → WPF double Height'a dönüştür
                double height = maxRev > 0m ? (double)(item.TotalRevenue / maxRev) * maxHeight : 0;
                if (height < 5) height = 5;

                // Date Label
                string shortDate = "";
                if (DateTime.TryParse(item.DateString, out DateTime dt))
                    shortDate = dt.ToString("dd MMM");
                else
                    shortDate = item.DateString;

                var container = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(6, 0, 6, 0),
                    VerticalAlignment = VerticalAlignment.Bottom
                };

                // Amount Label
                var amountLabel = new TextBlock
                {
                    Text = item.TotalRevenue.ToString("N0") + "₺",
                    FontSize = 10,
                    Foreground = (System.Windows.Media.Brush)FindResource("ColorOnSurfaceVariant"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 4)
                };
                if (item.TotalRevenue == 0) amountLabel.Opacity = 0;

                // The Bar
                var border = new Border
                {
                    Width = 32,
                    Height = height,
                    CornerRadius = new CornerRadius(4, 4, 0, 0),
                    Background = (System.Windows.Media.Brush)FindResource("ColorPrimary"),
                    ToolTip = $"{item.DateString}\nCiro: {item.TotalRevenue:C2}\nFiş: {item.ReceiptCount}"
                };

                // Date Label
                var dateLabel = new TextBlock
                {
                    Text = shortDate,
                    FontSize = 10,
                    Foreground = (System.Windows.Media.Brush)FindResource("ColorOnSurfaceVariant"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 4, 0, 0)
                };

                container.Children.Add(amountLabel);
                container.Children.Add(border);
                container.Children.Add(dateLabel);

                ChartBarsPanel.Children.Add(container);
            }
        }

        private void BtnClearData_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ClearHistoryDialog("🗑️ Satış/Analiz Verilerini Sil", "30 Günden Eski Satışları Sil (Son 1 Ayı Koru)", "Tüm Satış Geçmişini Sil (Sıfırla)") { Owner = Application.Current.MainWindow };
            if (dialog.ShowDialog() == true)
            {
                if (DataContext is ReportsViewModel vm)
                {
                    vm.ClearDataCommand.Execute(dialog.DeleteAll);
                }
            }
        }
    }
}
