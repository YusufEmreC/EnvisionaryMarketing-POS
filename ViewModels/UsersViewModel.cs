using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using PosApp.Models;
using PosApp.Services;
using PosApp.Infrastructure;

namespace PosApp.ViewModels
{
    public class UsersViewModel : ObservableObject
    {
        private readonly UserManager _userManager;
        private readonly IDialogService _dialogService;

        public ObservableCollection<User> UsersList { get; } = new ObservableCollection<User>();

        private User _selectedEditUser;
        public User SelectedEditUser
        {
            get => _selectedEditUser;
            set => SetProperty(ref _selectedEditUser, value);
        }

        private bool _isEditPanelVisible;
        public bool IsEditPanelVisible
        {
            get => _isEditPanelVisible;
            set => SetProperty(ref _isEditPanelVisible, value);
        }

        private string _newUsername;
        public string NewUsername
        {
            get => _newUsername;
            set => SetProperty(ref _newUsername, value);
        }

        private string _editUsername;
        public string EditUsername
        {
            get => _editUsername;
            set => SetProperty(ref _editUsername, value);
        }

        public ICommand LoadUsersCommand { get; }
        public ICommand AddCashierCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand StartEditCommand { get; }
        public ICommand SaveEditCommand { get; }
        public ICommand CancelEditCommand { get; }

        public UsersViewModel(UserManager userManager, IDialogService dialogService)
        {
            _userManager = userManager;
            _dialogService = dialogService;

            LoadUsersCommand = new RelayCommand(_ => LoadUsers());
            AddCashierCommand = new RelayCommand<string>(ExecuteAddCashier);
            DeleteUserCommand = new RelayCommand<User>(ExecuteDeleteUser);
            StartEditCommand = new RelayCommand<User>(ExecuteStartEdit);
            SaveEditCommand = new RelayCommand<string>(ExecuteSaveEdit);
            CancelEditCommand = new RelayCommand(_ => ExecuteCancelEdit());
        }

        public void LoadUsers()
        {
            try
            {
                var users = _userManager.GetAllUsers();
                UsersList.Clear();
                foreach (var user in users)
                {
                    UsersList.Add(user);
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Kullanıcılar yüklenirken hata oluştu: {ex.Message}", "Hata", "OK", "Error");
            }
        }

        private void ExecuteAddCashier(string password)
        {
            if (string.IsNullOrEmpty(NewUsername) || string.IsNullOrEmpty(password))
            {
                _dialogService.ShowMessage("Kullanıcı adı ve şifre boş bırakılamaz.", "Uyarı", "OK", "Warning");
                return;
            }

            try
            {
                if (_userManager.IsUsernameTaken(NewUsername))
                {
                    _dialogService.ShowMessage("Bu kullanıcı adı sistemde zaten kayıtlı.", "Hata", "OK", "Error");
                    return;
                }

                _userManager.AddCashier(NewUsername, _userManager.HashPassword(password));
                
                NewUsername = string.Empty; // Clear field
                LoadUsers();
                _dialogService.ShowMessage("Yeni Kasiyer hesabı başarıyla eklendi.", "Başarılı", "OK", "Information");
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Kasiyer eklenirken bir hata oluştu: {ex.Message}", "Hata", "OK", "Error");
            }
        }

        private void ExecuteDeleteUser(User user)
        {
            if (user == null) return;

            if (user.Role == "Admin")
            {
                _dialogService.ShowMessage("Admin hesabını silemezsiniz. Lütfen önce başka bir Admin atayın veya destek ekibine başvurun.", "İşlem Engellendi", "OK", "Warning");
                return;
            }

            if (_dialogService.ShowConfirmation($"'{user.Username}' adlı kasiyer hesabını tamamen silmek istediğinize emin misiniz?", "Kullanıcı Silinecek"))
            {
                try
                {
                    _userManager.DeleteUser(user.Id);
                    LoadUsers();
                }
                catch (Exception ex)
                {
                    _dialogService.ShowMessage($"Silme işlemi sırasında hata oluştu: {ex.Message}", "Hata", "OK", "Error");
                }
            }
        }

        private void ExecuteStartEdit(User user)
        {
            if (user == null) return;
            
            SelectedEditUser = user;
            EditUsername = user.Username;
            IsEditPanelVisible = true;
        }

        private void ExecuteCancelEdit()
        {
            IsEditPanelVisible = false;
            SelectedEditUser = null;
        }

        private void ExecuteSaveEdit(string newPassword)
        {
            if (SelectedEditUser == null) return;

            if (string.IsNullOrEmpty(EditUsername))
            {
                _dialogService.ShowMessage("Kullanıcı adı boş bırakılamaz.", "Uyarı", "OK", "Warning");
                return;
            }

            try
            {
                if (_userManager.IsUsernameTaken(EditUsername, SelectedEditUser.Id))
                {
                    _dialogService.ShowMessage("Bu kullanıcı adı başka bir hesap tarafından kullanılıyor.", "Hata", "OK", "Error");
                    return;
                }

                string hash = string.IsNullOrEmpty(newPassword) ? null : _userManager.HashPassword(newPassword);
                _userManager.UpdateUser(SelectedEditUser.Id, EditUsername, hash);

                _dialogService.ShowMessage($"'{EditUsername}' hesabı başarıyla güncellendi.", "Başarılı", "OK", "Information");
                IsEditPanelVisible = false;
                SelectedEditUser = null;
                LoadUsers();
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Kullanıcı güncellenirken bir hata oluştu: {ex.Message}", "Hata", "OK", "Error");
            }
        }
    }
}
