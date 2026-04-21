using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PharmacyApp.Common.Repositories;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Orders.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PharmacyApp.Features.Orders.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CheckoutPage : Page
    {
        OrderService basketServ;
        public CheckoutViewModel ViewModel { get; private set; }

        public CheckoutPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            basketServ = e.Parameter as OrderService ?? new OrderService();
            ViewModel = new CheckoutViewModel(basketServ);
            DataContext = ViewModel;
            base.OnNavigatedTo(e);
            Bindings?.Update();
        }

        private void SetDefaultPickUpDate(object sender, RoutedEventArgs e)
        {
            PickUpDateSelector.MinDate = new System.DateTimeOffset(DateTime.Now.Date.AddDays(1));
            PickUpDateSelector.SelectedDates.Add(PickUpDateSelector.MinDate);
        }

        private void CheckUnselectedDate(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs e)
        {
            if (PickUpDateSelector.SelectedDates.Count == 0)
                PickUpDateSelector.SelectedDates.Add(PickUpDateSelector.MinDate);
        }

        private async void PlaceOrder(object sender, RoutedEventArgs e)
        {
            DateOnly selectedDate = DateOnly.FromDateTime(PickUpDateSelector.SelectedDates[0].Date);

            // TODO not get the function directly from the user service
            // maybe get it through the view model? but na, no time
            try
            {
                IOrderService orderService = new OrderService();
                orderService.PlaceOrderFromBasket(selectedDate);

                ContentDialog confirmationMessage = new ContentDialog();

                confirmationMessage.XamlRoot = this.XamlRoot;
                confirmationMessage.Title = "Your order was placed";
                confirmationMessage.CloseButtonText = "Ok";

                // TODO rewrite the parameter, so that it's connected nicely
                Frame.Navigate(typeof(Products_Catalogue.HomePage), new Products_Catalogue.ProductCatalogueService(new SQLItemsRepository()));
                var result = await confirmationMessage.ShowAsync();
            }
            catch (ArgumentException exception)
            {
                ContentDialog causeOfErrorDialog = new ContentDialog();

                causeOfErrorDialog.XamlRoot = this.XamlRoot;
                causeOfErrorDialog.Title = "Error";
                causeOfErrorDialog.Content = exception.Message;
                causeOfErrorDialog.CloseButtonText = "Ok";

                var result = await causeOfErrorDialog.ShowAsync();
            }
        }

        private void NavigateToBasket(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(BasketPage), basketServ);
        }
    }
}
