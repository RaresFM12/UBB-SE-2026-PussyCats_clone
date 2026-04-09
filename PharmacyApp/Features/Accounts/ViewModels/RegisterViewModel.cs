using PharmacyApp.Common.Commands;
using PharmacyApp.Features.Accounts.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace PharmacyApp.Features.Accounts.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private UserAccountService _userAccountService;
        public event Action RegisterSucceded;
        private string email;
        private string password;
        private string confirmPassword;
        private string username;
        private string phoneNumber;


        public string Email
        {
            get => email;
            set
            {
                email = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => password;
            set
            {
                password = value;
                OnPropertyChanged();
            }
        }
        public string ConfirmPassword
        {
            get => confirmPassword;
            set
            {
                confirmPassword = value;
                OnPropertyChanged();
            }
        }


        public string Username
        {
            get => username;
            set
            {
                username = value;
                OnPropertyChanged();
            }
        }
        public string PhoneNumber
        {
            get => phoneNumber;
            set
            {
                phoneNumber = value;
                OnPropertyChanged();
            }
        }
        private string errorMessage;
        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                errorMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand RegisterCommand { get; }
        public RegisterViewModel(UserAccountService userAccountService)
        {
            _userAccountService = userAccountService;
            RegisterCommand = new RelayCommand(Register);
        }
        private void Register()
        {
            try
            {
                _userAccountService.Register(
                    Email,
                    Password,
                    ConfirmPassword,
                    Username,
                    PhoneNumber
                );

                RegisterSucceded?.Invoke();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
