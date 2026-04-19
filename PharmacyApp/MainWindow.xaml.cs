using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PharmacyApp.Common.Repositories;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Period_Tracker.Logic;
using PharmacyApp.Features.Period_Tracker.ViewModels;
using PharmacyApp.Features.Products_Catalogue;
using PharmacyApp.Models;
using System;

namespace PharmacyApp
{
    public sealed partial class MainWindow : Window
    {
        private readonly ProductCatalogueService productService;
        private readonly IOrderService orderService;
        private readonly ICurrentUserService currentUserService;
        private readonly IPeriodTrackerServiceFactory periodTrackerServiceFactory;

        public MainWindow()
            : this(
                new ProductCatalogueService(new SQLItemsRepository()),
                new OrderService(),
                new CurrentUserServiceAdapter(),
                new PeriodTrackerServiceFactory(
                    new SQLUsersRepository(),
                    new SQLItemsRepository(),
                    new CurrentUserServiceAdapter(),
                    new OrderService()))
        {
        }

        public MainWindow(
            ProductCatalogueService productService,
            IOrderService orderService,
            ICurrentUserService currentUserService,
            IPeriodTrackerServiceFactory periodTrackerServiceFactory)
        {
            try
            {
                InitializeComponent();

                this.productService = productService;
                this.orderService = orderService;
                this.currentUserService = currentUserService;
                this.periodTrackerServiceFactory = periodTrackerServiceFactory;

                Features.Accounts.Views.LoginView.UserLoggedIn += HandleUserLoggedIn;
                Features.Accounts.Views.RegisterView.UserRegistered += HandleUserRegistered;

                MainFrame.Navigate(typeof(Features.Products_Catalogue.HomePage));
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine("MainWindow startup crash:");
                System.Diagnostics.Debug.WriteLine(exception.ToString());
                throw;
            }
        }

        private void HandleUserLoggedIn()
        {
            UpdateUserInterface();
        }

        private void HandleUserRegistered()
        {
            UpdateUserInterface();
        }

        private void OnHomeClicked(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(typeof(Features.Products_Catalogue.HomePage));
        }

        private void OnProductsClicked(object sender, RoutedEventArgs e)
        {
            User currentUser = currentUserService.CurrentUser;
            MainFrame.Navigate(typeof(Features.Products_Catalogue.CatalogPage), (productService, currentUser, orderService));
        }

        private void OnCartClicked(object sender, RoutedEventArgs e)
        {
            if (currentUserService.CurrentUser == null)
            {
                MainFrame.Navigate(typeof(Features.Accounts.Views.LoginView));
                return;
            }

            MainFrame.Navigate(typeof(Features.Orders.Views.BasketPage), orderService);
        }

        private void OnAccountClicked(object sender, RoutedEventArgs e)
        {
            if (currentUserService.CurrentUser == null)
            {
                MainFrame.Navigate(typeof(Features.Accounts.Views.LoginView));
                return;
            }

            MainFrame.Navigate(typeof(Features.Accounts.Views.ProfileManagementView));
        }

        private void OnAdminClicked(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(typeof(Features.Pharmacy_Management.EditPage));
        }

        private void OnPeriodTrackerClicked(object sender, RoutedEventArgs e)
        {
            if (currentUserService.CurrentUser == null)
            {
                MainFrame.Navigate(typeof(Features.Accounts.Views.LoginView));
                return;
            }

            PeriodTrackerViewModel periodTrackerViewModel = new PeriodTrackerViewModel(
                periodTrackerServiceFactory.CreatePeriodTrackerService(),
                periodTrackerServiceFactory.CreateWellnessItemsService(),
                periodTrackerServiceFactory.CreateBasketService());

            MainFrame.Navigate(
                typeof(Features.Period_Tracker.Views.PeriodTrackerPage),
                periodTrackerViewModel);
        }

        private void UpdateUserInterface()
        {
            User currentUser = currentUserService.CurrentUser;

            if (currentUser != null && currentUser.IsAdmin)
            {
                AdminButton.Visibility = Visibility.Visible;
                AdminUsersButton.Visibility = Visibility.Visible;
            }
            else
            {
                AdminButton.Visibility = Visibility.Collapsed;
                AdminUsersButton.Visibility = Visibility.Collapsed;
            }

            if (currentUser != null)
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
            if (currentUserService.CurrentUser == null)
            {
                MainFrame.Navigate(typeof(Features.Accounts.Views.LoginView));
                return;
            }

            MainFrame.Navigate(typeof(Features.Pharmacy_Management.Notifications));
        }
    }
}