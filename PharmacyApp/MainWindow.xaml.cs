using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PharmacyApp.Common.Repositories;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Products_Catalogue;
using PharmacyApp.Models;
using System;

namespace PharmacyApp
{
    public sealed partial class MainWindow : Window
    {
        private ProductCatalogueService productService;
        private OrderService orderService;
        private BasketService basketService;

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                IItemsRepository repo = new SQLItemsRepository();
                productService = new ProductCatalogueService(repo);
                orderService = new OrderService();
                basketService = new BasketService();

                Features.Accounts.Views.LoginView.UserLoggedIn += () =>
                {
                    UpdateUI();
                };

                Features.Accounts.Views.RegisterView.UserRegistered += () =>
                {
                    UpdateUI();
                };

                MainFrame.Navigate(typeof(Features.Products_Catalogue.HomePage));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("MainWindow startup crash:");
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        private void OnHomeClicked(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(typeof(Features.Products_Catalogue.HomePage));
        }

        private void OnProductsClicked(object sender, RoutedEventArgs e)
        {
            User? currentuser = ServiceWrapper.UserAccountService.CurrentUser;
            MainFrame.Navigate(typeof(Features.Products_Catalogue.CatalogPage), (productService, currentuser, basketService));
        }

        private void OnCartClicked(object sender, RoutedEventArgs e)
        {
            if (ServiceWrapper.UserAccountService.CurrentUser == null)
            {
                MainFrame.Navigate(typeof(Features.Accounts.Views.LoginView));
            }
            else
            {
                MainFrame.Navigate(typeof(Features.Orders.Views.BasketPage), basketService);
            }
        }

        private void OnAccountClicked(object sender, RoutedEventArgs e)
        {
            if (ServiceWrapper.UserAccountService.CurrentUser == null)
            {
                MainFrame.Navigate(typeof(Features.Accounts.Views.LoginView));
            }
            else
            {
                MainFrame.Navigate(typeof(Features.Accounts.Views.ProfileManagementView));
            }
        }

        private void OnAdminClicked(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(typeof(Features.Pharmacy_Management.EditPage));
        }

        private void OnPeriodTrackerClicked(object sender, RoutedEventArgs e)
        {
            if (ServiceWrapper.UserAccountService.CurrentUser == null)
            {
                MainFrame.Navigate(typeof(Features.Accounts.Views.LoginView));
            }
            else
            {
                MainFrame.Navigate(typeof(Features.Period_Tracker.Views.PeriodTrackerPage));
            }
        }

        private void UpdateUI()
        {
            var user = ServiceWrapper.UserAccountService.CurrentUser;

            if (user != null && user.IsAdmin)
            {
                AdminButton.Visibility = Visibility.Visible;
                AdminUsersButton.Visibility = Visibility.Visible;
            }
            else
            {
                AdminButton.Visibility = Visibility.Collapsed;
                AdminUsersButton.Visibility = Visibility.Collapsed;
            }

            if (user != null)
            {
                RegisterButton.Visibility = Visibility.Collapsed;
                LoginButton.Visibility = Visibility.Collapsed;
                AccountButton.Visibility = Visibility.Visible;
            }
            else
            {
                RegisterButton.Visibility = Visibility.Visible;
                LoginButton.Visibility = Visibility.Visible;
                AccountButton.Visibility = Visibility.Collapsed;
            }
        }

        private void OnAdminUsersClicked(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(typeof(Features.Accounts.Views.AdminAccountsManagementView));
        }

        private void OnRegisterClicked(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(typeof(Features.Accounts.Views.RegisterView));
        }

        private void OnLoginClicked(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(typeof(Features.Accounts.Views.LoginView));
        }

        private void OnNotificationsClicked(object sender, RoutedEventArgs e)
        {
            if (ServiceWrapper.UserAccountService.CurrentUser == null)
            {
                MainFrame.Navigate(typeof(Features.Accounts.Views.LoginView));
                return;
            }

            MainFrame.Navigate(typeof(Features.Pharmacy_Management.Notifications));
        }
    }
}