using ABI.System;
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
using PharmacyApp.Models; // Added this so it recognizes OrderItem!
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
    public sealed partial class EditableOrderDetailPage : Page
    {
        IOrderService orderServ;
        public EditDetailViewModel ViewModel { get; set; }

        public EditableOrderDetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var extractedArgs = (Tuple<IOrderService, int>)(e.Parameter);

            orderServ = extractedArgs.Item1;
            int orderID = extractedArgs.Item2;
            ViewModel = new EditDetailViewModel(orderServ, orderID);
            DataContext = ViewModel;

            base.OnNavigatedTo(e);
        }

        private async void CompleteOrder(object sender, RoutedEventArgs e)
        {
            int orderID = ViewModel.shownOrderID;

            // --- REPLACED TUPLE WITH OrderItem ---
            Dictionary<int, OrderItem> updatedQuantities = new Dictionary<int, OrderItem>();
            foreach (var entry in ViewModel.OrderItems)
            {
                updatedQuantities.Add(entry.ItemID, new OrderItem(entry.ItemID, entry.ItemQuantity, entry.ItemFinalPrice));
            }

            try
            {
                orderServ.CompleteOrder(orderID, updatedQuantities);

                ContentDialog confirmationMessage = new ContentDialog();

                confirmationMessage.XamlRoot = this.XamlRoot;
                confirmationMessage.Title = $"Order#{orderID} was completed";
                confirmationMessage.CloseButtonText = "Ok";

                // TODO rewrite the parameter, so that it's connected nicely
                Frame.Navigate(typeof(PharmacyApp.Features.Orders.Views.OrderManagementPage), orderServ);
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