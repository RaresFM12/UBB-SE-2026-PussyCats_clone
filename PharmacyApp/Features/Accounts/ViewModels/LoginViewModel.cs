using PharmacyApp.Common.Commands;
using PharmacyApp.Features.Accounts.Logic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace PharmacyApp.Features.Accounts.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private IUserAccountService _userAccountService;
        private string email;
        private string password;
        private string errorMessage;


        public event Action LoginSucceded;
        public event PropertyChangedEventHandler PropertyChanged;
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
        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                errorMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoginCommand { get; set; }

        public LoginViewModel(IUserAccountService userAccountService)
        {
            _userAccountService = userAccountService;

            LoginCommand = (ICommand)new RelayCommand(Login);
        }

        public void Login()
        {
            try
            {

                _userAccountService.Login(Email, Password);
                LoginSucceded?.Invoke();
            }
            catch (Exception ex) {
                ErrorMessage = ex.Message;
            }
            
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
