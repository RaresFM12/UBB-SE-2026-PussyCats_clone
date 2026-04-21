using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PharmacyApp.Features.Products_Catalogue
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public class UIItem
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public float Discount { get; set; }
        public int Quantity { get; set; }
        public float OldPrice { get; set; }
        public float FinalPrice => OldPrice * (1 - Discount);
        public string OldPriceDisplay => $"{OldPrice:F2} lei";
        public string FinalPriceDisplay => $"{FinalPrice:F2} lei";
        public string DiscountDisplay => $"-{Discount * 100:F0}%";
        public string StockText =>
        Quantity == 0 ? "Out of stock" :
        Quantity < ProductCatalogueService.LowStockThreshold ? $"Only {Quantity} in stock" :
        "In stock";
        public SolidColorBrush StockColor =>
            Quantity == 0 ? new SolidColorBrush(Colors.Red) :
            Quantity < 10 ? new SolidColorBrush(Colors.Orange) :
            new SolidColorBrush(Colors.Green);
        public bool CanAddToCart => Quantity > 0;
        public bool HasDiscount => Discount > 0;
        public double DiscountFloat =>
            HasDiscount is false ? 0.00 :
            1.00;
        public Item OriginalItem { get; set; }


    }
    public sealed partial class CatalogPage : Page
    {
        private int currentPage = 0;
        private int pageSize = 10;
        private int totalItems = 0;
        private ProductCatalogueService productService;
        public CatalogPage()
        {
            InitializeComponent();
        }

        private void OnSearchClicked(object sender, RoutedEventArgs e)
        {
            currentPage = 0;
            ApplyFilters();
        }
        User currentUser;
        OrderService OrderService;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ValueTuple<ProductCatalogueService, User, OrderService> tuple)
            {
                productService = tuple.Item1;
                currentUser = tuple.Item2;
                OrderService = tuple.Item3;
            }


            LoadProducts();
        }
        private void LoadProducts()
        {
            var items = productService.GetItems(null, page: currentPage, pageSize: pageSize);
            var uiItems = items.Select(item => new UIItem
            {
                Name = item.Name,
                OldPrice = item.Price,
                Discount = item.DiscountPercentage,
                Quantity = item.Quantity,
                Image = item.ImagePath,
                OriginalItem = item
            }).ToList();
            totalItems = uiItems.Count;

            ProductsList.ItemsSource = uiItems;
            PageText.Text = $"Page {currentPage + 1}";
        }
        //private void LoadProducts()
        //{
        //    var items = new List<UIItem>
        //    {
        //        new UIItem { Name = "Paracetamol", Price = "5.99 lei", Image = "ms-appx:///Assets/logo.png" },
        //        new UIItem { Name = "Ibuprofen", Price = "7.49 lei", Image = "ms-appx:///Assets/logo.png" },
        //        new UIItem { Name = "Vitamin C", Price = "9.99 lei", Image = "ms-appx:///Assets/logo.png" },
        //        new UIItem { Name = "Cough Syrup", Price = "6.75 lei", Image = "ms-appx:///Assets/logo.png" },
        //        new UIItem { Name = "Aspirin", Price = "4.50 lei", Image = "ms-appx:///Assets/logo.png" },
        //        new UIItem { Name = "Magnesium", Price = "11.25 lei", Image = "ms-appx:///Assets/logo.png" }
        //    };

        //    ProductsList.ItemsSource = items;
        //}
        private void OnNextClick(object sender, RoutedEventArgs e)
        {
            if (productService == null) return;
            if (totalItems == pageSize)
                currentPage++;
            ApplyFilters();
        }

        private void OnPreviousClick(object sender, RoutedEventArgs e)
        {
            if (productService == null) return;
            if (currentPage > 0)
            {
                currentPage--;
                ApplyFilters();
            }
        }
        private void OnFilterClicked(object sender, RoutedEventArgs e)
        {
            currentPage = 0;
            ApplyFilters();
        }

        private string? currentSearch;
        private List<string>? currentCategories;
        private List<(float, float)>? currentPriceRanges;
        private string? currentStock;
        private bool? currentDiscount;
        private bool ascending = true;
        private void ApplyFilters()
        {
            if (productService == null) return;

            //search
            string searchText = SearchBox.Text;

            // categories
            var categories = new List<string>();
            if (MedicineCheck.IsChecked == true) categories.Add("Medicine");
            if (SupplementsCheck.IsChecked == true) categories.Add("Supplements");
            if (WellnessCheck.IsChecked == true) categories.Add("Wellness");
            currentCategories = categories.Any() ? categories : null;

            // price
            var priceRanges = new List<(float, float)>();
            if (Price0_49Check.IsChecked == true) priceRanges.Add((0, 49));
            if (Price50_99Check.IsChecked == true) priceRanges.Add((50, 99));
            if (Price100_199Check.IsChecked == true) priceRanges.Add((100, 199));
            if (Price200_499Check.IsChecked == true) priceRanges.Add((200, 499));
            if (Price500PlusCheck.IsChecked == true) priceRanges.Add((500, float.MaxValue));
            currentPriceRanges = priceRanges.Any() ? priceRanges : null;

            // stock
            if (InStockRadio.IsChecked == true) currentStock = ProductCatalogueService.StockFilterInStock;
            else if (LowStockRadio.IsChecked == true) currentStock = ProductCatalogueService.StockFilterLowStock;
            else currentStock = null;

            // discount
            currentDiscount = DiscountCheck.IsChecked == true ? true : null;

            // sort
            string? sortBy = null;
            var selectedSort = (SortByBox.SelectedItem as ComboBoxItem).Content.ToString();
            if (selectedSort == "Price") sortBy = ProductCatalogueService.SortByPrice;
            if (selectedSort == "Newest") sortBy = ProductCatalogueService.SortByNewest;

            bool ascending = true;
            var selectedDirection = (SortAscendingBox.SelectedItem as ComboBoxItem).Content.ToString();
            if (selectedDirection == "Descending")
                ascending = false;
            var items = productService.GetItems(
                searchText,
                categories.Any() ? categories : null,
                priceRanges.Any() ? priceRanges : null,
                currentStock,
                currentDiscount,
                null, //substances
                ascending,
                currentPage,
                pageSize,
                sortBy
                );
            var uiItems = items.Select(item => new UIItem
            {
                Name = item.Name,
                OldPrice = item.Price,
                Discount = item.DiscountPercentage,
                Quantity = item.Quantity,
                Image = item.ImagePath,
                OriginalItem = item
            }).ToList();
            totalItems = uiItems.Count;

            ProductsList.ItemsSource = uiItems;
            PageText.Text = $"Page {currentPage + 1}";
            if (uiItems.Count == 0)
            {
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    EmptyMessage.Text = "No products found.";
                }
                else
                {
                    EmptyMessage.Text = "No products match the selected filters.";
                }

                EmptyMessage.Visibility = Visibility.Visible;
            }
            else
            {
                EmptyMessage.Visibility = Visibility.Collapsed;
            }
        }

        private void OnProductClicked(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var uiItem = button?.DataContext as UIItem;

            if (uiItem?.OriginalItem == null) return;

            Frame.Navigate(typeof(ProductDetailsPage), (uiItem.OriginalItem, currentUser, OrderService));
        }

        private void OnAddToCartClicked(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var uiItem = button?.DataContext as UIItem;

            if (uiItem?.OriginalItem == null) return;
            if (currentUser == null)
                Frame.Navigate(typeof(Features.Accounts.Views.LoginView));
            else {
                try
                {
                    OrderService.AddToBasket(uiItem.OriginalItem.Id, 1);
                }
                catch (ArgumentException ex)
                {}
            }
        }
    }
}
