using System.ComponentModel;
using System.Runtime.CompilerServices;
using PharmacyApp.Models;

namespace PharmacyApp.Features.Products_Catalogue.ViewModels
{
    public class HomePageViewModel : INotifyPropertyChanged
    {
        private User currentUser;
        private const string ProductsPage = "Products";
        private const string HomePage = "Home";
        private const string LoginPage = "Login";
        private const string RegisterPage = "Products";
        private const string ProductDetailsPage = "Products";
        private const string LoginView = "LoginView";
        public User CurrentUser
        {
            get => currentUser;
            private set
            {
                currentUser = value;
                OnPropertyChanged();
            }
        }

        public void Initialize(User user)
        {
            CurrentUser = user;

            OnPropertyChanged(nameof(IsAdminDashboardVisible));
            OnPropertyChanged(nameof(IsMyAccountVisible));
            OnPropertyChanged(nameof(IsLoginVisible));
            OnPropertyChanged(nameof(IsRegisterVisible));
        }

        public bool IsAdminDashboardVisible => CurrentUser != null && CurrentUser.IsAdmin;

        public bool IsMyAccountVisible => CurrentUser == null;
        public bool IsLoginVisible => CurrentUser != null;
        public bool IsRegisterVisible => CurrentUser != null;

        public string HandleNavigationRequest(string requestedDestination)
        {
            if (CurrentUser == null)
            {
                bool isAllowed = requestedDestination == ProductsPage ||
                                 requestedDestination == HomePage ||
                                 requestedDestination == LoginPage ||
                                 requestedDestination == RegisterPage ||
                                 requestedDestination == ProductDetailsPage;

                if (!isAllowed)
                {
                    return LoginView;
                }
            }

            return requestedDestination;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}