using System;
using Microsoft.UI.Xaml.Controls;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Features.Accounts.ViewModels;

namespace PharmacyApp.Features.Accounts.Views
{
    public sealed partial class ChangePasswordView : ContentDialog
    {
        private UserAccountService accountService;
        public ChangePasswordViewModel ViewModel { get; }

        public ChangePasswordView(UserAccountService service)
        {
            this.InitializeComponent();

            accountService = service;
            ViewModel = new ChangePasswordViewModel(service);

            this.DataContext = ViewModel;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            try
            {
                ViewModel.OldPassword = OldPasswordBox.Password;
                ViewModel.NewPassword = NewPasswordBox.Password;
                ViewModel.ConfirmPassword = ConfirmPasswordBox.Password;

                ViewModel.ErrorMessage = null;

                ViewModel.ChangePasswordCommand.Execute(null);
            }
            catch (Exception ex)
            {
                args.Cancel = true;
                ViewModel.ErrorMessage = ex.Message;
            }
        }
    }
}
