using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PharmacyApp.Features.Accounts.ViewModels
{
    public class AdminAccountsManagementViewModel : INotifyPropertyChanged
    {
        private readonly IUserAccountService _userService;

        private string searchQuery;
        private string errorMessage;

        public ObservableCollection<UserItemViewModel> Users { get; set; }

        public AdminAccountsManagementViewModel(IUserAccountService userService)
        {
            _userService = userService;
            Users = new ObservableCollection<UserItemViewModel>();

            LoadUsers();
        }

        public string SearchQuery
        {
            get => searchQuery;
            set
            {
                searchQuery = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                errorMessage = value;
                OnPropertyChanged();
            }
        }

        public void LoadUsers()
        {
            try
            {
                ErrorMessage = null;
                var users = _userService.SearchUsers("");
                UpdateUsers(users);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public void Search()
        {
            try
            {
                ErrorMessage = null;
                var result = _userService.SearchUsers(SearchQuery ?? "");
                UpdateUsers(result);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public void Promote(UserItemViewModel userItem)
        {
            try
            {
                ErrorMessage = null;
                _userService.PromoteToAdmin(userItem.User);
                Refresh();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        public void Disable(UserItemViewModel userItem)
        {
            try
            {
                ErrorMessage = null;
                _userService.DisableAccount(userItem.User);
                Refresh();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        private void Refresh()
        {
            Search();
        }

        private void UpdateUsers(List<User> users)
        {
            Users.Clear();
            foreach (var user in users)
                Users.Add(new UserItemViewModel(user));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}