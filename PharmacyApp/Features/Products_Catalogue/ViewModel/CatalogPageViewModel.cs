using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input; // Required for ICommand
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Products_Catalogue.Service;
using PharmacyApp.Features.Products_Catalogue.ViewModels;
using PharmacyApp.Models;

namespace PharmacyApp.Features.Products_Catalogue.ViewModels
{
    // ── MVVM RELAY COMMAND ────────────────────────────────────────────────────
    // This allows the View to trigger methods in the ViewModel via Bindings.
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        public RelayCommand(Action execute) => _execute = execute;
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _execute();
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        public RelayCommand(Action<T> execute) => _execute = execute;
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _execute((T)parameter);
    }

    // ── UI ITEM ───────────────────────────────────────────────────────────────
    public class UIItem : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public float Discount { get; set; }
        public int Quantity { get; set; }
        public float OldPrice { get; set; }

        public float FinalPrice => OldPrice * (1 - Discount);
        public string OldPriceDisplay => $"{OldPrice:F2} lei";
        public string FinalPriceDisplay => $"{FinalPrice:F2} lei";
        public string DiscountDisplay => HasDiscount ? $"-{Discount * 100}%" : string.Empty;

        public string StockText =>
            Quantity == 0 ? "Out of stock" :
            Quantity < ProductCatalogueService.LowStockThreshold ? $"Only {Quantity} in stock" : "In stock";

        // To strictly separate UI from ViewModel, Color logic usually relies on Converters or enums,
        // but returning color strings here is an acceptable MVVM compromise for simplicity.
        public string StockColor => Quantity == 0 ? "Red" : Quantity < ProductCatalogueService.LowStockThreshold ? "Orange" : "Green";

        public bool CanAddToCart => Quantity > 0;
        public bool HasDiscount => Discount > 0;
        public double DiscountOpacity => HasDiscount ? 1.0 : 0.0;
        public Item OriginalItem { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // ── VIEW MODEL ────────────────────────────────────────────────────────────
    public class CatalogPageViewModel : ICatalogPageViewModel
    {
        private IProductCatalogueService _productService;
        public User CurrentUser { get; set; }
        public IOrderService OrderService { get; set; }

        // Event to tell the code-behind to perform UI Navigation
        public event EventHandler<Type> NavigateRequested;

        // ── Commands ──
        public ICommand SearchCommand { get; }
        public ICommand ApplyFiltersCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand AddToCartCommand { get; }

        // ── Paging ──
        private int _currentPage = 0;
        private int _pageSize = 10;
        private int _totalItemsOnPage = 0;

        // ── Bindable State (Two-Way) ──
        public string SearchText { get; set; } = string.Empty;

        public bool FilterMedicine { get; set; }
        public bool FilterSupplements { get; set; }
        public bool FilterWellness { get; set; }

        public bool FilterPrice0_49 { get; set; }
        public bool FilterPrice50_99 { get; set; }
        public bool FilterPrice100_199 { get; set; }
        public bool FilterPrice200_499 { get; set; }
        public bool FilterPrice500Plus { get; set; }

        // ── RadioButton Bindings (Stock) ──
        public string StockFilter { get; private set; } = null;
        private bool _isStockAll = true;
        public bool IsStockAll { get => _isStockAll; set { _isStockAll = value; if (value) StockFilter = null; OnPropertyChanged(); } }
        private bool _isStockIn;
        public bool IsStockIn { get => _isStockIn; set { _isStockIn = value; if (value) StockFilter = ProductCatalogueService.StockFilterInStock; OnPropertyChanged(); } }
        private bool _isStockLow;
        public bool IsStockLow { get => _isStockLow; set { _isStockLow = value; if (value) StockFilter = ProductCatalogueService.StockFilterLowStock; OnPropertyChanged(); } }

        // ── RadioButton Bindings (Discount) ──
        public bool? DiscountFilter { get; private set; } = null;
        private bool _isDiscountAll = true;
        public bool IsDiscountAll { get => _isDiscountAll; set { _isDiscountAll = value; if (value) DiscountFilter = null; OnPropertyChanged(); } }
        private bool _isDiscountYes;
        public bool IsDiscountYes { get => _isDiscountYes; set { _isDiscountYes = value; if (value) DiscountFilter = true; OnPropertyChanged(); } }
        private bool _isDiscountNo;
        public bool IsDiscountNo { get => _isDiscountNo; set { _isDiscountNo = value; if (value) DiscountFilter = false; OnPropertyChanged(); } }

        // ── Dropdown Bindings (Sorting) ──
        public List<string> SortOptions { get; } = new List<string> { "Default", "Price", "Newest" };
        private string _selectedSortOption = "Default";
        public string SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                _selectedSortOption = value;
                OnPropertyChanged();
            }
        }

        public List<string> SortDirections { get; } = new List<string> { "Ascending", "Descending" };
        private string _selectedSortDirection = "Ascending";
        public string SelectedSortDirection
        {
            get => _selectedSortDirection;
            set
            {
                _selectedSortDirection = value;
                OnPropertyChanged();
            }
        }

        // ── Collections & UI Messages ──
        private ObservableCollection<UIItem> _products = new ObservableCollection<UIItem>();
        public ObservableCollection<UIItem> Products { get => _products; private set { _products = value; OnPropertyChanged(); } }

        private string _pageText = "Page 1";
        public string PageText { get => _pageText; private set { _pageText = value; OnPropertyChanged(); } }

        private string _emptyMessage = string.Empty;
        public string EmptyMessage { get => _emptyMessage; private set { _emptyMessage = value; OnPropertyChanged(); } }

        private bool _isEmptyMessageVisible = false;
        public bool IsEmptyMessageVisible { get => _isEmptyMessageVisible; private set { _isEmptyMessageVisible = value; OnPropertyChanged(); } }

        // ── Initialization ──
        public CatalogPageViewModel()
        {
            SearchCommand = new RelayCommand(Search);
            ApplyFiltersCommand = new RelayCommand(ApplyFilters);
            NextPageCommand = new RelayCommand(NextPage);
            PreviousPageCommand = new RelayCommand(PreviousPage);
            AddToCartCommand = new RelayCommand<UIItem>(AddToCart);
        }

        public void Initialize(IProductCatalogueService service, User user, IOrderService orderService)
        {
            _productService = service;
            CurrentUser = user;
            OrderService = orderService;
            LoadProducts();
        }

        // ── Command Actions ──
        private void Search()
        {
            _currentPage = 0;
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_productService == null) return;

            var categories = BuildCategoryList();
            var priceRanges = BuildPriceRangeList();

            // Resolve the SortBy logic right before querying
            string sortByParam = SelectedSortOption == "Price" ? ProductCatalogueService.SortByPrice
                               : SelectedSortOption == "Newest" ? ProductCatalogueService.SortByNewest : null;
            bool sortAscendingParam = SelectedSortDirection == "Ascending";

            var items = _productService.GetItems(
                SearchText,
                categories.Any() ? categories : null,
                priceRanges.Any() ? priceRanges : null,
                StockFilter,
                DiscountFilter,
                null,
                sortAscendingParam,
                _currentPage,
                _pageSize,
                sortByParam
            );

            var uiItems = items.Select(MapToUIItem).ToList();
            _totalItemsOnPage = uiItems.Count;

            Products = new ObservableCollection<UIItem>(uiItems);
            PageText = $"Page {_currentPage + 1}";

            IsEmptyMessageVisible = uiItems.Count == 0;
            EmptyMessage = uiItems.Count == 0 ? (!string.IsNullOrWhiteSpace(SearchText) ? "No products found." : "No products match the selected filters.") : string.Empty;
        }

        private void NextPage()
        {
            if (_productService == null) return;
            if (_totalItemsOnPage == _pageSize)
            {
                _currentPage++;
                ApplyFilters();
            }
        }

        private void PreviousPage()
        {
            if (_productService == null) return;
            if (_currentPage > 0)
            {
                _currentPage--;
                ApplyFilters();
            }
        }

        private void AddToCart(UIItem item)
        {
            if (item?.OriginalItem == null) return;

            if (CurrentUser == null)
            {
                // Trigger the event so the View can navigate
                NavigateRequested?.Invoke(this, typeof(Features.Accounts.Views.LoginView));
                return;
            }

            try { OrderService.AddToBasket(item.OriginalItem.Id, 1); }
            catch (ArgumentException) { /* Ignored on catalog view */ }
        }

        // ── Helpers ──
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
            if (FilterPrice0_49) list.Add((0f, 49.99f));
            if (FilterPrice50_99) list.Add((50f, 99.99f));
            if (FilterPrice100_199) list.Add((100f, 199.99f));
            if (FilterPrice200_499) list.Add((200f, 499.99f));
            if (FilterPrice500Plus) list.Add((500f, float.MaxValue));
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
            return new UIItem
            {
                Name = item.Name,
                OldPrice = item.Price,
                Discount = item.DiscountPercentage / 100,
                Quantity = item.Quantity,
                Image = cleanPath.StartsWith("ms-appx:///") ? cleanPath : $"ms-appx:///{cleanPath}",
                OriginalItem = item
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}