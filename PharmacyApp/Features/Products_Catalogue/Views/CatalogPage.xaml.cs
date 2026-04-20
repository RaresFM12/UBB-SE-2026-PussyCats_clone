using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Products_Catalogue.Service;
using PharmacyApp.Features.Products_Catalogue.ViewModels;
using PharmacyApp.Models;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PharmacyApp.Features.Products_Catalogue
{
    /// <summary>
    /// Code-behind for CatalogPage. Responsibilities:
    ///   - Create and expose the ViewModel.
    ///   - Forward UI events to the ViewModel.
    ///   - Handle navigation (Frame.Navigate) which cannot live in the ViewModel.
    /// All state, filter logic and data mapping live in CatalogPageViewModel.
    /// </summary>
    public sealed partial class CatalogPage : Page
    {
        public CatalogPageViewModel ViewModel { get; } = new CatalogPageViewModel();

        public CatalogPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ValueTuple<ProductCatalogueService, User, IOrderService> tuple)
            {
                ViewModel.Initialize(tuple.Item1, tuple.Item2, tuple.Item3);
            }
        }

        // ── search ────────────────────────────────────────────────────────────────
        private void OnSearchClicked(object sender, RoutedEventArgs e)
        {
            ViewModel.SearchText = SearchBox.Text;
            ViewModel.Search();
        }

        // ── filter / sort ─────────────────────────────────────────────────────────
        private void OnFilterClicked(object sender, RoutedEventArgs e)
        {
            SyncFiltersToViewModel();
            ViewModel.ApplyFilters();
        }

        // ── paging ────────────────────────────────────────────────────────────────
        private void OnNextClick(object sender, RoutedEventArgs e)
        {
            SyncFiltersToViewModel();
            ViewModel.NextPage();
        }

        private void OnPreviousClick(object sender, RoutedEventArgs e)
        {
            SyncFiltersToViewModel();
            ViewModel.PreviousPage();
        }

        // ── product navigation ────────────────────────────────────────────────────
        private void OnProductClicked(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var uiItem = button?.DataContext as UIItem;
            if (uiItem?.OriginalItem == null) return;

            Frame.Navigate(
                typeof(ProductDetailsPage),
                (uiItem.OriginalItem, ViewModel.CurrentUser, ViewModel.OrderService));
        }

        // ── add to cart ───────────────────────────────────────────────────────────
        private void OnAddToCartClicked(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var uiItem = button?.DataContext as UIItem;
            if (uiItem?.OriginalItem == null) return;

            if (ViewModel.CurrentUser == null)
            {
                Frame.Navigate(typeof(Features.Accounts.Views.LoginView));
                return;
            }

            try
            {
                ViewModel.OrderService.AddToBasket(uiItem.OriginalItem.Id, 1);
            }
            catch (ArgumentException)
            {
                // Item already in basket – silently ignored on catalogue list view
            }
        }

        // ── helper: read all filter/sort controls into ViewModel ─────────────────
        private void SyncFiltersToViewModel()
        {
            // Search
            ViewModel.SearchText = SearchBox.Text;

            // Categories
            ViewModel.FilterMedicine = MedicineCheck.IsChecked == true;
            ViewModel.FilterSupplements = SupplementsCheck.IsChecked == true;
            ViewModel.FilterWellness = WellnessCheck.IsChecked == true;

            // Price ranges
            ViewModel.FilterPrice0_49 = Price0_49Check.IsChecked == true;
            ViewModel.FilterPrice50_99 = Price50_99Check.IsChecked == true;
            ViewModel.FilterPrice100_199 = Price100_199Check.IsChecked == true;
            ViewModel.FilterPrice200_499 = Price200_499Check.IsChecked == true;
            ViewModel.FilterPrice500Plus = Price500PlusCheck.IsChecked == true;

            // Stock
            if (InStockRadio.IsChecked == true)
                ViewModel.StockFilter = ProductCatalogueService.StockFilterInStock;
            else if (LowStockRadio.IsChecked == true)
                ViewModel.StockFilter = ProductCatalogueService.StockFilterLowStock;
            else
                ViewModel.StockFilter = null;

            // BUG FIX: Non-discounted option now wired up via DiscountedRadio / NonDiscountedRadio.
            // Previously only a single checkbox existed ("Discounted"), so it was impossible to
            // filter non-discounted products – requirement F4.3 was unmet.
            if (DiscountedRadio.IsChecked == true)
                ViewModel.DiscountFilter = true;
            else if (NonDiscountedRadio.IsChecked == true)
                ViewModel.DiscountFilter = false;
            else
                ViewModel.DiscountFilter = null;

            // Sort
            string sortContent = (SortByBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            ViewModel.SortBy = sortContent == "Price" ? ProductCatalogueService.SortByPrice
                             : sortContent == "Newest" ? ProductCatalogueService.SortByNewest
                             : null;

            string dirContent = (SortAscendingBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
            ViewModel.SortAscending = dirContent != "Descending";
        }
    }
}