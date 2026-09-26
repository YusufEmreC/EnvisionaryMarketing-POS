using System;
using System.Windows;

namespace PosApp.Views
{
    public partial class ClearHistoryDialog : Window
    {
        public bool DeleteAll { get; private set; }

        public ClearHistoryDialog(string title, string keepOption, string deleteAllOption)
        {
            InitializeComponent();
            TxtTitle.Text = title;
            RbKeepOneMonth.Content = keepOption;
            RbDeleteAll.Content = deleteAllOption;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            DeleteAll = RbDeleteAll.IsChecked == true;
            DialogResult = true;
            Close();
        }
    }
}
