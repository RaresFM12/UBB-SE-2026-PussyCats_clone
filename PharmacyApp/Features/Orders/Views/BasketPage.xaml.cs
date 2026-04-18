using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Orders.ViewModels;
using System;

namespace PharmacyApp.Features.Orders.Views
{
    public sealed partial class BasketPage : Page
    {
        private OrderService orderServ;

        public BasketViewModel ViewModel { get; private set; }

        public BasketPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            orderServ = (OrderService)e.Parameter;
            ViewModel = new BasketViewModel(orderServ);
            DataContext = ViewModel;

            ViewModel.BasketQuantityRemoved -= HandleCheckoutButton;
            ViewModel.BasketQuantityRemoved += HandleCheckoutButton;

            Bindings.Update();
            ViewModel.OnBasketQuantityRemoved();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.BasketQuantityRemoved -= HandleCheckoutButton;

            base.OnNavigatedFrom(e);
        }

        private void NavigateToCheckout(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CheckoutPage), orderServ);
        }

        private void HandleCheckoutButton(int quantity)
        {
            CheckoutButton.Visibility = quantity > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void EnterPrescriptionID(object sender, RoutedEventArgs e)
        {
            PrescriptionWarning.Visibility = Visibility.Collapsed;

            string prescriptionId = PrescriptionInputBox.Text;

            try
            {
                ViewModel.GetPrescription(prescriptionId);
                Bindings.Update();
            }
            catch (ArgumentException exception)
            {
                PrescriptionWarning.Text = exception.Message;
                PrescriptionWarning.Visibility = Visibility.Visible;
            }
        }
    }
}