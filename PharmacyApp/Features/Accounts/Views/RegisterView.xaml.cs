using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Features.Accounts.ViewModels;

namespace PharmacyApp.Features.Accounts.Views
{
    public sealed partial class RegisterView : Page
    {
        public static event Action UserRegistered;
        public RegisterView()
        {
            this.InitializeComponent();
            var viewModel = new RegisterViewModel(ServiceWrapper.UserAccountService);
            viewModel.RegisterSucceded += OnRegisterSucceded;
            this.DataContext = viewModel;
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var vm = (RegisterViewModel)this.DataContext;
            vm.Password = PasswordBox.Password;
        }
        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var vm = (RegisterViewModel)this.DataContext;
            vm.ConfirmPassword = ConfirmPasswordBox.Password;
        }
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            (this.Parent as Frame)?.Navigate(typeof(LoginView));
        }
        private void OnRegisterSucceded()
        {
            UserRegistered?.Invoke();
            (this.Parent as Frame)?.Navigate(typeof(Features.Accounts.Views.ProfileManagementView));
        }
    }
}
