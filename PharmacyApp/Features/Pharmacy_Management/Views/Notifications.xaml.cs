using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PharmacyApp.Features.Pharmacy_Management.ViewModels;
using PharmacyApp.Features.Products_Catalogue;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.
namespace PharmacyApp.Features.Pharmacy_Management
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
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
