using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Products_Catalogue.Service;
using PharmacyApp.Features.Products_Catalogue.ViewModels;
using PharmacyApp.Models;

namespace PharmacyApp.Features.Products_Catalogue
{
    public sealed partial class CatalogPage : Page
    {
        public ICatalogPageViewModel ViewModel { get; }

        public CatalogPage()
        {
            InitializeComponent();

            ViewModel = new CatalogPageViewModel();

            DataContext = ViewModel;
            ViewModel.NavigateRequested += OnViewModelNavigateRequested;
        }

        private void OnViewModelNavigateRequested(object sender, Type pageType)
        {
            Frame.Navigate(pageType);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ValueTuple<ProductCatalogueService, User, IOrderService> tuple)
            {
                ViewModel.Initialize(tuple.Item1, tuple.Item2, tuple.Item3);
            }
            else if (e.Parameter is ValueTuple<IProductCatalogueService, User, IOrderService> interfaceTuple)
            {
                ViewModel.Initialize(interfaceTuple.Item1, interfaceTuple.Item2, interfaceTuple.Item3);
            }
        }

        // We keep this click handler because opening a details page is a purely UI-driven navigation event
        private void OnProductClicked(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var uiItem = button?.DataContext as UIItem;
            if (uiItem?.OriginalItem == null)
            {
                return;
            }

            Frame.Navigate(
                typeof(ProductDetailsPage),
                (uiItem.OriginalItem, ViewModel.CurrentUser, ViewModel.OrderService));
        }
    }
}