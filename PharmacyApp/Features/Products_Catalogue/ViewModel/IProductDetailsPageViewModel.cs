using System.ComponentModel;
using Microsoft.UI.Xaml.Media;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Models;

namespace PharmacyApp.Features.Products_Catalogue.ViewModels
{
    public interface IProductDetailsPageViewModel : INotifyPropertyChanged
    {
        // Initialization
        void Initialize(Item item, User user, IOrderService orderService);

        // Actions
        (bool success, bool navigateToLogin) TryAddToBasket(string quantityText);

        // Data Properties
        string ProductName { get; }
        string FinalPriceDisplay { get; }
        string OldPriceDisplay { get; }
        string DiscountDisplay { get; }
        bool HasDiscount { get; }
        string StockText { get; }
        StockLevel CurrentStockLevel { get; }
        bool IsAddToCartEnabled { get; }
        bool IsQuantityBoxEnabled { get; }
        string DescriptionText { get; }
        string LabelText { get; }
        string ProducerText { get; }
        string CategoryText { get; }
        string PillsText { get; }
        string ImagePath { get; }
        string ErrorText { get; }
    }
}