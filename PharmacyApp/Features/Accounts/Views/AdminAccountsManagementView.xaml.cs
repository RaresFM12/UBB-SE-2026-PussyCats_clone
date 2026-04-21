using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Features.Accounts.ViewModels;
using System;

namespace PharmacyApp.Features.Accounts.Views
{
    public sealed partial class AdminAccountsManagementView : Page
    {
        private IUserAccountService _accountService;
        public AdminAccountsManagementViewModel ViewModel { get; private set; }

        public AdminAccountsManagementView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _accountService = (IUserAccountService)e.Parameter;
            ViewModel = new AdminAccountsManagementViewModel(_accountService);

            this.DataContext = ViewModel;
        }

        private void OnSearchClick(object sender, RoutedEventArgs e)
        {
            ViewModel.Search();
        }

        private async void OnPromoteClick(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.DataContext is UserItemViewModel userItem)
            {
                var dialog = new ContentDialog
                {
                    Title = "Warning",
                    Content = "This action cannot be undone. Proceed?",
                    PrimaryButtonText = "Proceed",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    ViewModel.Promote(userItem);
                }
                else
                {
                    cb.IsChecked = false;
                }
            }
        }

        private async void OnDisableClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is UserItemViewModel userItem)
            {
                var dialog = new ContentDialog
                {
                    Title = "Confirm",
                    Content = "Disable this account?",
                    PrimaryButtonText = "Yes",
                    CloseButtonText = "No",
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    ViewModel.Disable(userItem);
                }
            }
        }
    }
}