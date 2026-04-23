using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PharmacyApp.Features.Products_Catalogue;
using PharmacyApp.Features.Pharmacy_Management.ViewModels;

namespace PharmacyApp.Features.Pharmacy_Management
{
    public sealed partial class Notifications : Page
    {
        private NotificationsViewModel ViewModel { get; } = new NotificationsViewModel();
        public Notifications()
        {
            ViewModel.PopulateNotifications();

            InitializeComponent();
        }

        private void OnNotificationButtonClicked(object sender, RoutedEventArgs e)
        {
            string buttonContent = (string)((Button)sender).Content;
            if (buttonContent == "Go to products")
            {
                Frame.Navigate(typeof(HomePage));
            }

            if (buttonContent == "Go fix it")
            {
                Frame.Navigate(typeof(EditPage));
            }
        }
    }
}
