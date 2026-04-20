using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Products_Catalogue.Service;
using PharmacyApp.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PharmacyApp.Features.Products_Catalogue.ViewModels
{
    /// <summary>
    /// UI representation of a catalogue Item, used by CatalogPage and ProductDetailsPage bindings.
    /// </summary>
    public class UIItem : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Image { get; set; }

        // BUG FIX: Discount is stored as a fraction (e.g. 0.20 = 20%).
        // FinalPrice must be OldPrice * (1 - Discount), not OldPrice * Discount.
        public float Discount { get; set; }
        public int Quantity { get; set; }
        public float OldPrice { get; set; }

        public float FinalPrice => OldPrice * (1 - Discount);

        public string OldPriceDisplay => $"{OldPrice:F2} lei";
        public string FinalPriceDisplay => $"{FinalPrice:F2} lei";

        // BUG FIX: DiscountDisplay should only appear when there IS a discount.
        // Previously it was always shown (just at 0%), now it returns empty string when no discount.
        public string DiscountDisplay => HasDiscount ? $"-{Discount * 100}%" : string.Empty;

        public string StockText =>
            Quantity == 0 ? "Out of stock" :
            Quantity < ProductCatalogueService.LowStockThreshold ? $"Only {Quantity} in stock" :
            "In stock";

        public SolidColorBrush StockColor =>
            Quantity == 0 ? new SolidColorBrush(Colors.Red) :
            Quantity < ProductCatalogueService.LowStockThreshold ? new SolidColorBrush(Colors.Orange) :
            new SolidColorBrush(Colors.Green);

        public bool CanAddToCart => Quantity > 0;

        public bool HasDiscount => Discount > 0;

        /// <summary>
        /// Used to control the Opacity of the old-price and discount-badge TextBlocks.
        /// Returns 1.0 when discounted, 0.0 when not – keeps them invisible but layout-stable.
        /// </summary>
        public double DiscountOpacity => HasDiscount ? 1.0 : 0.0;

        public Item OriginalItem { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    /// ViewModel for CatalogPage. Holds all state, filter/sort logic and paging.
    /// The View binds to properties here; no business logic lives in the code-behind.
    /// </summary>
    public class CatalogPageViewModel : INotifyPropertyChanged
    {
        // ── dependencies ──────────────────────────────────────────────────────────
        private ProductCatalogueService _productService;
        public User CurrentUser { get; set; }
        public IOrderService OrderService { get; set; }

        // ── paging ────────────────────────────────────────────────────────────────
        private int _currentPage = 0;
        private int _pageSize = 10;
        private int _totalItemsOnPage = 0;

        // ── filter / sort state ──────────────────────────────────────────────────
        public string SearchText { get; set; } = string.Empty;

        public bool FilterMedicine { get; set; }
        public bool FilterSupplements { get; set; }
        public bool FilterWellness { get; set; }

        public bool FilterPrice0_49 { get; set; }
        public bool FilterPrice50_99 { get; set; }
        public bool FilterPrice100_199 { get; set; }
        public bool FilterPrice200_499 { get; set; }
        public bool FilterPrice500Plus { get; set; }

        // Stock: null = All, "in_stock", "low_stock"
        public string StockFilter { get; set; } = null;

        // BUG FIX: Requirements (F4.3) require BOTH "Discounted" and "Non-discounted"
        // options. Previously only a single DiscountCheck existed, which could only
        // filter FOR discounted items – there was no way to filter non-discounted items.
        // Solution: use a nullable bool, driven by three radio buttons in the View:
        //   All (null) / Discounted (true) / Non-discounted (false)
        public bool? DiscountFilter { get; set; } = null;

        public string SortBy { get; set; } = null;          // null = default (by ID)
        public bool SortAscending { get; set; } = true;

        // ── observable collection ─────────────────────────────────────────────────
        private ObservableCollection<UIItem> _products = new ObservableCollection<UIItem>();
        public ObservableCollection<UIItem> Products
        {
            get => _products;
            private set { _products = value; OnPropertyChanged(); }
        }

        private string _pageText = "Page 1";
        public string PageText
        {
            get => _pageText;
            private set { _pageText = value; OnPropertyChanged(); }
        }

        private string _emptyMessage = string.Empty;
        public string EmptyMessage
        {
            get => _emptyMessage;
            private set { _emptyMessage = value; OnPropertyChanged(); }
        }

        private bool _isEmptyMessageVisible = false;
        public bool IsEmptyMessageVisible
        {
            get => _isEmptyMessageVisible;
            private set { _isEmptyMessageVisible = value; OnPropertyChanged(); }
        }

        // ── initialisation ────────────────────────────────────────────────────────
        public void Initialize(ProductCatalogueService service, User user, IOrderService orderService)
        {
            _productService = service;
            CurrentUser = user;
            OrderService = orderService;
            LoadProducts();
        }

        // ── public commands (called from code-behind event handlers) ──────────────
        public void Search()
        {
            _currentPage = 0;
            ApplyFilters();
        }

        public void ApplyFilters()
        {
            if (_productService == null) return;

            var categories = BuildCategoryList();
            var priceRanges = BuildPriceRangeList();

            var items = _productService.GetItems(
                SearchText,
                categories.Any() ? categories : null,
                priceRanges.Any() ? priceRanges : null,
                StockFilter,
                DiscountFilter,       
                null,                 
                SortAscending,
                _currentPage,
                _pageSize,
                SortBy
            );

            var uiItems = items.Select(MapToUIItem).ToList();
            _totalItemsOnPage = uiItems.Count;

            Products = new ObservableCollection<UIItem>(uiItems);
            PageText = $"Page {_currentPage + 1}";

            if (uiItems.Count == 0)
            {
                EmptyMessage = !string.IsNullOrWhiteSpace(SearchText)
                    ? "No products found."
                    : "No products match the selected filters.";
                IsEmptyMessageVisible = true;
            }
            else
            {
                IsEmptyMessageVisible = false;
                EmptyMessage = string.Empty;
            }
        }

        public void NextPage()
        {
            if (_productService == null) return;
            if (_totalItemsOnPage == _pageSize)
            {
                _currentPage++;
                ApplyFilters();
            }
        }

        public void PreviousPage()
        {
            if (_productService == null) return;
            if (_currentPage > 0)
            {
                _currentPage--;
                ApplyFilters();
            }
        }

        // ── helpers ───────────────────────────────────────────────────────────────
        private List<string> BuildCategoryList()
        {
            var list = new List<string>();
            if (FilterMedicine) list.Add("Medicine");
            if (FilterSupplements) list.Add("Supplements");
            if (FilterWellness) list.Add("Wellness");
            return list;
        }

        private List<(float, float)> BuildPriceRangeList()
        {
            var list = new List<(float, float)>();
            if (FilterPrice0_49) list.Add((0, 49));
            if (FilterPrice50_99) list.Add((50, 99));
            if (FilterPrice100_199) list.Add((100, 199));
            if (FilterPrice200_499) list.Add((200, 499));
            if (FilterPrice500Plus) list.Add((500, float.MaxValue));
            return list;
        }

        private void LoadProducts()
        {
            var items = _productService.GetItems(null, page: _currentPage, pageSize: _pageSize);
            var uiItems = items.Select(MapToUIItem).ToList();
            _totalItemsOnPage = uiItems.Count;
            Products = new ObservableCollection<UIItem>(uiItems);
            PageText = $"Page {_currentPage + 1}";
        }

        private static UIItem MapToUIItem(Item item)
        {
            string cleanPath = item.ImagePath?.TrimStart('/') ?? string.Empty;
            string safeImagePath = cleanPath.StartsWith("ms-appx:///")
                ? cleanPath
                : $"ms-appx:///{cleanPath}";

            return new UIItem
            {
                Name = item.Name,
                OldPrice = item.Price,
                Discount = item.DiscountPercentage / 100,
                Quantity = item.Quantity,
                Image = safeImagePath,
                OriginalItem = item
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}