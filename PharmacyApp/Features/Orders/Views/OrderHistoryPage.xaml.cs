using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PharmacyApp.Models;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Orders.ViewModels;
using System;

namespace PharmacyApp.Features.Orders.Views
{
    public sealed partial class OrderHistoryPage : Page
    {
        private IOrderService currentOrderService;
        public OrderHistoryViewModel ViewModel { get; private set; }

        public OrderHistoryPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            currentOrderService = (IOrderService)e.Parameter;
            ViewModel = new OrderHistoryViewModel(currentOrderService);
            DataContext = ViewModel;

            ViewModel.RedirectToDetailRequested += RedirectToDetailPage;
            ViewModel.CancelConfirmationRequested += AskCancelOrderConfirmation;
            ViewModel.RedirectToResubmitRequested += RedirectToResubmitPage;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (ViewModel != null)
            {
                ViewModel.RedirectToDetailRequested -= RedirectToDetailPage;
                ViewModel.CancelConfirmationRequested -= AskCancelOrderConfirmation;
                ViewModel.RedirectToResubmitRequested -= RedirectToResubmitPage;
            }
        }

        private void RedirectToDetailPage(int orderId)
        {
            Frame.Navigate(typeof(PharmacyApp.Features.Orders.Views.ModifyIncompleteOrderPage),
                        new Tuple<IOrderService, int>(currentOrderService, orderId));
        }

        private async void AskCancelOrderConfirmation(Order currOrder)
        {
            ContentDialog dialog = new ContentDialog
            {
                XamlRoot = this.XamlRoot,
                Title = "Cancel?",
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No",
                DefaultButton = ContentDialogButton.None,
                Content = $"Do you want to cancel Order#{currOrder.Id}?"
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                ViewModel.CancelOrder(currOrder);
            }
        }

        private void RedirectToResubmitPage(int orderId)
        {
            Frame.Navigate(typeof(PharmacyApp.Features.Orders.Views.ResubmitOrderPage),
                        new Tuple<IOrderService, int>(currentOrderService, orderId));
        }
    }

    public class OrderTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CompletedTemplate { get; set; }
        public DataTemplate IncompletedTemplate { get; set; }
        public DataTemplate ExpiredTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            Order currentOrder = (Order)item;

            if (currentOrder.IsCompleted)
                return CompletedTemplate;
            if (currentOrder.IsExpired)
                return ExpiredTemplate;
            return IncompletedTemplate;
        }
    }
}