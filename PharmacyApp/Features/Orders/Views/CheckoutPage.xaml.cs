using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Orders.ViewModels;
using PharmacyApp.Common.Repositories;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PharmacyApp.Features.Orders.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CheckoutPage : Page
    {
        private CheckoutViewModel viewModel;
        private IOrderService currentOrderService;

        public CheckoutPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            currentOrderService = (IOrderService)e.Parameter;
            viewModel = new CheckoutViewModel(currentOrderService);

            viewModel.OrderPlacedSuccessfully += OnOrderSuccess;
            viewModel.OrderPlacementFailed += OnOrderFailure;

            this.DataContext = viewModel;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (viewModel != null)
            {
                viewModel.OrderPlacedSuccessfully -= OnOrderSuccess;
                viewModel.OrderPlacementFailed -= OnOrderFailure;
            }
        }

        private void SetDefaultPickUpDate(object sender, RoutedEventArgs e)
        {
            PickUpDateSelector.MinDate = new DateTimeOffset(DateTime.Now.Date.AddDays(1));
            PickUpDateSelector.SelectedDates.Add(PickUpDateSelector.MinDate);
        }

        private void CheckUnselectedDate(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs e)
        {
            if (PickUpDateSelector.SelectedDates.Count == 0)
            {
                PickUpDateSelector.SelectedDates.Add(PickUpDateSelector.MinDate);
            }
        }

        private void PlaceOrder(object sender, RoutedEventArgs e)
        {
            // TODO not get the function directly from the user service
            // maybe get it through the view model? but na, no time
            if (PickUpDateSelector.SelectedDates.Count > 0)
            {
                if (viewModel.PlaceOrderCommand.CanExecute(PickUpDateSelector.SelectedDates[0]))
                {
                    viewModel.PlaceOrderCommand.Execute(PickUpDateSelector.SelectedDates[0]);
                }
            }
        }

        private async void OnOrderSuccess()
        {
            ContentDialog confirmationMessage = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = "Your order was placed",
                CloseButtonText = "Ok"
            };

            // TODO rewrite the parameter, so that it's connected nicely
            Frame.Navigate(typeof(Products_Catalogue.HomePage), new Products_Catalogue.ProductCatalogueService(new SQLItemsRepository()));
            await confirmationMessage.ShowAsync();
        }

        private async void OnOrderFailure(string errorMessage)
        {
            ContentDialog causeOfErrorDialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = "Error",
                Content = errorMessage,
                CloseButtonText = "Ok"
            };

            await causeOfErrorDialog.ShowAsync();
        }

        private void NavigateToBasket(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BasketPage), currentOrderService);
        }
    }
}