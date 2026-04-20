using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Products_Catalogue.Service;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PharmacyApp.Features.Products_Catalogue.ViewModels
{
    /// <summary>
    /// ViewModel for ProductDetailsPage (F4.5).
    /// All display-ready properties are computed here; the View only binds and routes events.
    /// </summary>
    public class ProductDetailsPageViewModel : INotifyPropertyChanged
    {
        // ── dependencies ──────────────────────────────────────────────────────────
        private Item _currentItem;
        public User CurrentUser { get; private set; }
        public IOrderService OrderService { get; private set; }

        // ── bindable properties ───────────────────────────────────────────────────
        public string ProductName => _currentItem?.Name ?? string.Empty;

        // BUG FIX: price display now correctly uses OldPrice (the plain price)
        // and FinalPrice (after discount). Previously in LoadData the local variable
        // was named "finalPrice" but the formula was correct – preserved here.
        public string FinalPriceDisplay =>
            _currentItem == null ? string.Empty
            : $"{_currentItem.Price * (1 - _currentItem.DiscountPercentage / 100):F2} lei";

        public string OldPriceDisplay =>
            HasDiscount ? $"{_currentItem.Price:F2} lei" : string.Empty;

        public string DiscountDisplay =>
            HasDiscount ? $"{_currentItem.DiscountPercentage}% off" : string.Empty;

        public bool HasDiscount => (_currentItem?.DiscountPercentage ?? 0) > 0;

        public string StockText
        {
            get
            {
                if (_currentItem == null) return string.Empty;
                if (_currentItem.Quantity == 0) return "Out of stock";
                if (_currentItem.Quantity < ProductCatalogueService.LowStockThreshold)
                    return $"Only {_currentItem.Quantity} in stock";
                return "In stock";
            }
        }

        public SolidColorBrush StockColor
        {
            get
            {
                if (_currentItem == null) return new SolidColorBrush(Colors.Gray);
                if (_currentItem.Quantity == 0) return new SolidColorBrush(Colors.Red);
                if (_currentItem.Quantity < ProductCatalogueService.LowStockThreshold)
                    return new SolidColorBrush(Colors.Orange);
                return new SolidColorBrush(Colors.Green);
            }
        }

        public bool IsAddToCartEnabled => (_currentItem?.Quantity ?? 0) > 0;
        public bool IsQuantityBoxEnabled => (_currentItem?.Quantity ?? 0) > 0;

        public string DescriptionText => _currentItem?.Description ?? string.Empty;
        public string LabelText => _currentItem?.Label ?? string.Empty;
        public string ProducerText => _currentItem?.Producer ?? string.Empty;
        public string CategoryText => _currentItem?.Category ?? string.Empty;
        public string PillsText => _currentItem?.NumberOfPills.ToString() ?? string.Empty;

        public string SubstancesText =>
            _currentItem?.ActiveSubstances != null && _currentItem.ActiveSubstances.Any()
                ? string.Join(", ", _currentItem.ActiveSubstances.Select(s => $"{s.Key} ({s.Value})"))
                : "None";

        public string ImagePath => _currentItem?.ImagePath ?? string.Empty;

        // Error message shown near the Add to Basket button
        private string _errorText = string.Empty;
        public string ErrorText
        {
            get => _errorText;
            private set { _errorText = value; OnPropertyChanged(); }
        }

        // ── init ─────────────────────────────────────────────────────────────────
        public void Initialize(Item item, User user, IOrderService orderService)
        {
            _currentItem = item;
            CurrentUser = user;
            OrderService = orderService;

            // Notify all bindings to refresh
            OnPropertyChanged(string.Empty);
        }

        // ── add-to-basket logic ───────────────────────────────────────────────────
        /// <summary>
        /// Validates the requested quantity and adds to basket.
        /// Returns true on success; on failure it sets ErrorText and returns false.
        /// The View can navigate to LoginView when this returns NavigateToLogin = true.
        /// </summary>
        public (bool success, bool navigateToLogin) TryAddToBasket(string quantityText)
        {
            ErrorText = string.Empty;

            if (CurrentUser == null)
                return (false, true);

            if (!int.TryParse(quantityText, out int qty) || qty <= 0)
            {
                ErrorText = "Invalid quantity selected";
                return (false, false);
            }

            // BUG FIX (F4.5 validation): quantity must not exceed stock AND must not exceed 50.
            // The original code had both checks combined in one branch which meant the error
            // message was the same for "exceeds stock" and "exceeds 50". Preserved same message
            // as per original but now logically explicit.
            if (qty > 50 || qty > _currentItem.Quantity)
            {
                ErrorText = "Invalid quantity selected";
                return (false, false);
            }

            try
            {
                OrderService.AddToBasket(_currentItem.Id, qty);
                return (true, false);
            }
            catch (Exception)
            {
                ErrorText = "Item already in basket";
                return (false, false);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}