using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using PharmacyApp.Features.Accounts.Views;
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
    public sealed partial class ProductDetailsPage : Page
    {
        private Item currentItem;
        private User currentUser;
        private OrderService OrderService;
        public ProductDetailsPage()
        {
            InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ValueTuple<Item, User, OrderService> tuple)
            {
                currentItem = tuple.Item1;
                currentUser = tuple.Item2;
                OrderService = tuple.Item3;

            }

            LoadData();
        }
        private void LoadData()
        {
            if (currentItem == null) return;
            ProductImage.Source = new BitmapImage(new Uri(currentItem.ImagePath));
            ProductName.Text = currentItem.Name;
            float finalPrice = currentItem.Price * (1 - currentItem.DiscountPercentage);
            FinalPrice.Text = $"{finalPrice:F2} lei";
            if (currentItem.DiscountPercentage > 0)
            {
                OldPrice.Text = $"{currentItem.Price:F2} lei";
                Discount.Text = $"{currentItem.DiscountPercentage * 100:F0}% off";
            }
            else
            {
                OldPrice.Text = "";
                Discount.Text = "";
            }
            if (currentItem.Quantity == 0)
            {
                StockText.Text = "Out of stock";
                StockText.Foreground = new SolidColorBrush(Colors.Red);
            }
            else if (currentItem.Quantity < ProductCatalogueService.LowStockThreshold)
            {
                StockText.Text = $"Only {currentItem.Quantity} in stock";
                StockText.Foreground = new SolidColorBrush(Colors.Orange);
            }
            else
            {
                StockText.Text = "In stock";
                StockText.Foreground = new SolidColorBrush(Colors.Green);
            }
            if (currentItem.Quantity == 0)
            {
                AddToCartButton.IsEnabled = false;
                QuantityBox.IsEnabled = false;
                QuantityBox.Visibility = Visibility.Collapsed;
            }
            DescriptionText.Text = currentItem.Description;
            LabelText.Text = currentItem.Label;
            ProducerText.Text = currentItem.Producer;
            CategoryText.Text = currentItem.Category;
            PillsText.Text = currentItem.NumberOfPills.ToString();
            if (currentItem.ActiveSubstances != null && currentItem.ActiveSubstances.Any())
            {
                SubstancesText.Text = string.Join(", ",
                    currentItem.ActiveSubstances.Select(s => $"{s.Key} ({s.Value})"));
            }
            else
            {
                SubstancesText.Text = "None";
            }

        }

        private void OnAddToBasket(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";
            if (currentUser == null)
            {
                Frame.Navigate(typeof(LoginView));
            }
            else
            {
                if (!int.TryParse(QuantityBox.Text, out int qty))
                {
                    ErrorText.Text = "Invalid quantity selected";
                    return;
                }
                if (qty <= 0)
                {
                    ErrorText.Text = "Invalid quantity selected";
                    return;
                }
                if (qty > currentItem.Quantity || qty > 50)
                {
                    ErrorText.Text = "Invalid quantity selected";
                    return;
                }
                else
                {
                    try
                    {
                        OrderService.AddToBasket(currentItem.Id, qty);
                    }
                    catch (Exception ex) {
                        ErrorText.Text = "Item already in basket";
                    }
                }
            }

        }
    }
}
