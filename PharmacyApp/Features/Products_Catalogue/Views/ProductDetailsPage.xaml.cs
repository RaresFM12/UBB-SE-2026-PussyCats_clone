using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using PharmacyApp.Features.Accounts.Views;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Products_Catalogue.ViewModels;
using PharmacyApp.Models;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PharmacyApp.Features.Products_Catalogue
{
    /// <summary>
    /// Code-behind for ProductDetailsPage (F4.5).
    /// Responsibilities limited to: wiring ViewModel, handling navigation, loading the image
    /// (BitmapImage construction stays here because it depends on UI infrastructure).
    /// All display logic and validation live in ProductDetailsPageViewModel.
    /// </summary>
    public sealed partial class ProductDetailsPage : Page
    {
        public IProductDetailsPageViewModel ViewModel { get; }

        public ProductDetailsPage()
        {
            InitializeComponent();
            ViewModel = new ProductDetailsPageViewModel();
            DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // BUG FIX: original code expected OrderService as concrete OrderService type
            // (not the IOrderService interface), which would crash at runtime if a different
            // implementation was injected. Changed to IOrderService.
            if (e.Parameter is ValueTuple<Item, User, IOrderService> tuple)
            {
                ViewModel.Initialize(tuple.Item1, tuple.Item2, tuple.Item3);
                LoadProductImage(tuple.Item1.ImagePath);
            }
        }

        private void LoadProductImage(string imagePath)
{
    if (!string.IsNullOrWhiteSpace(imagePath))
    {
        // 1. Clean up any leading slashes just in case
        string cleanPath = imagePath.TrimStart('/');
        
        // 2. Ensure the path has the ms-appx:/// prefix that WinUI requires
        string fullPath = cleanPath.StartsWith("ms-appx:///") 
            ? cleanPath 
            : $"ms-appx:///{cleanPath}";
            
        // 3. Create the URI with the safe full path
        ProductImage.Source = new BitmapImage(new Uri(fullPath));
    }
}

        private void OnAddToBasket(object sender, RoutedEventArgs e)
        {
            var (success, navigateToLogin) = ViewModel.TryAddToBasket(QuantityBox.Text);

            if (navigateToLogin)
                Frame.Navigate(typeof(LoginView));
        }
    }
}