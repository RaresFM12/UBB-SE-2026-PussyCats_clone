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
using PharmacyApp.Models; // Added so it knows what an OrderItem is!
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
    public sealed partial class ModifyIncompleteOrderPage : Page
    {
        IOrderService orderServ;
        public ModifyIncompleteOrderViewModel ViewModel { get; set; }

        public ModifyIncompleteOrderPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var extractedArgs = (Tuple<IOrderService, int>)(e.Parameter);

            orderServ = extractedArgs.Item1;
            int orderID = extractedArgs.Item2;
            ViewModel = new ModifyIncompleteOrderViewModel(orderServ, orderID);
            DataContext = ViewModel;

            base.OnNavigatedTo(e);
        }

        private void SetPickUpDate(object sender, RoutedEventArgs e)
        {
            // TODO idk if this is good, to default to noon, but I can't create
            // it otherwise
            DateTimeOffset chosenPickUpDate = new System.DateTimeOffset(
                ViewModel.PickUpDate,
                new TimeOnly(12, 0),
                new TimeSpan(12, 0, 0)
            );
            PickUpDateSelector.SelectedDates.Add(chosenPickUpDate);
        }

        private void CheckUnselectedDate(CalendarView sender, CalendarViewSelectedDatesChangedEventArgs e)
        {
            if (PickUpDateSelector.SelectedDates.Count == 0)
            {
                DateTimeOffset chosenPickUpDate = new System.DateTimeOffset(
                    ViewModel.PickUpDate,
                    new TimeOnly(12, 0),
                    new TimeSpan(12, 0, 0)
                );
                PickUpDateSelector.SelectedDates.Add(chosenPickUpDate);
            }
        }

        private void CancelChanges(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PharmacyApp.Features.Orders.Views.OrderHistoryPage), orderServ);
        }

        private async void ModifyOrder(object sender, RoutedEventArgs e)
        {
            // --- REPLACED TUPLE WITH OrderItem ---
            Dictionary<int, OrderItem> updatedQuantities = new Dictionary<int, OrderItem>();

            foreach (var entry in ViewModel.OrderItems)
            {
                // Passing all 3 required arguments to fix that CS7036 error!
                updatedQuantities.Add(entry.ItemID, new OrderItem(entry.ItemID, entry.ItemQuantity, entry.ItemFinalPrice));
            }

            DateOnly selectedDate = DateOnly.FromDateTime(PickUpDateSelector.SelectedDates[0].Date);

            // TODO not get the function directly from the user service
            // maybe get it through the view model? but na, no time
            try
            {
                orderServ.ModifyIncompleteOrder(ViewModel.currentOrderID, updatedQuantities, selectedDate);

                ContentDialog confirmationMessage = new ContentDialog();

                confirmationMessage.XamlRoot = this.XamlRoot;
                confirmationMessage.Title = "Order#" + ViewModel.currentOrderID + " was successfully modified";
                confirmationMessage.CloseButtonText = "Ok";

                // TODO rewrite the parameter, so that it's connected nicely
                Frame.Navigate(typeof(PharmacyApp.Features.Orders.Views.OrderHistoryPage), orderServ);
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
    }
}