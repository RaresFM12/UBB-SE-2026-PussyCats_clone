using PharmacyApp.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
// Assuming you have a Views namespace for LoginView, HomePage, etc.
// using PharmacyApp.Features.Accounts.Views; 

namespace PharmacyApp.Features.Products_Catalogue.ViewModels
{
    public class HomePageViewModel : INotifyPropertyChanged
    {
        private User _currentUser;
        public User CurrentUser
        {
            get => _currentUser;
            private set { _currentUser = value; OnPropertyChanged(); }
        }

        public void Initialize(User user)
        {
            CurrentUser = user;

            // Notify UI that visibility states might have changed
            OnPropertyChanged(nameof(IsAdminDashboardVisible));
            OnPropertyChanged(nameof(IsMyAccountVisible));
            OnPropertyChanged(nameof(IsLoginVisible));
            OnPropertyChanged(nameof(IsRegisterVisible));
        }

        // F4.6 Validation 1: Admin Dashboard visible only to admins
        public bool IsAdminDashboardVisible => CurrentUser != null && CurrentUser.IsAdmin;

        // F4.6 Validation 2: For users not logged in, display My Account and hide Login/Register
        public bool IsMyAccountVisible => CurrentUser == null;
        public bool IsLoginVisible => CurrentUser != null;
        public bool IsRegisterVisible => CurrentUser != null;

        // F4.6 Validation 3: Non-authenticated user redirection
        public string HandleNavigationRequest(string requestedDestination)
        {
            if (CurrentUser == null)
            {
                bool isAllowed = requestedDestination == "Products" ||
                                 requestedDestination == "Home" ||
                                 requestedDestination == "Login" ||
                                 requestedDestination == "Register" ||
                                 requestedDestination == "ProductDetails";

                if (!isAllowed)
                {
                    return "LoginView"; // Redirect to Login
                }
            }

            return requestedDestination; // Allow normal navigation
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}