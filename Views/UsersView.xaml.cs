using System.Windows;
using System.Windows.Controls;
using PosApp.Models;
using PosApp.Services;
using PosApp.ViewModels;
using PosApp.Infrastructure;

namespace PosApp.Views
{
    public partial class UsersView : UserControl
    {
        public UsersView()
        {
            InitializeComponent();
        }

        public void Initialize(UserManager userManager, IDialogService dialogService)
        {
            var vm = new UsersViewModel(userManager, dialogService);
            DataContext = vm;
            vm.LoadUsersCommand.Execute(null);
        }

        private void BtnAddCashier_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is UsersViewModel vm)
            {
                vm.AddCashierCommand.Execute(TxtNewCashierPassword.Password);
                TxtNewCashierPassword.Clear();
            }
        }

        private void BtnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is User user)
            {
                if (DataContext is UsersViewModel vm)
                {
                    vm.DeleteUserCommand.Execute(user);
                }
            }
        }

        private void BtnEditUser_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is User user)
            {
                if (DataContext is UsersViewModel vm)
                {
                    vm.StartEditCommand.Execute(user);
                    TxtEditPassword.Clear();
                }
            }
        }

        private void BtnSaveEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is UsersViewModel vm)
            {
                vm.SaveEditCommand.Execute(TxtEditPassword.Password);
                TxtEditPassword.Clear();
            }
        }
    }
}
