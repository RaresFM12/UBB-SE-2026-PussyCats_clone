using PharmacyApp.Features.Period_Tracker.Logic;
using PharmacyApp.Models;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Syncfusion.UI.Xaml.Core;

namespace PharmacyApp.Features.Period_Tracker.ViewModels
{
    public class ItemViewModel : INotifyPropertyChanged
    {
        private readonly IBasketService basketService;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int Id { get; }

        public ICommand AddToBasketCommand { get; }

        public float ExtraDiscountPercentage { get; }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private string priceString;
        public string PriceString
        {
            get => priceString;
            set
            {
                priceString = value;
                OnPropertyChanged();
            }
        }

        private string priceDiscountedString;
        public string PriceDiscountedString
        {
            get => priceDiscountedString;
            set
            {
                priceDiscountedString = value;
                OnPropertyChanged();
            }
        }

        private string priceColor;
        public string PriceColor
        {
            get => priceColor;
            set
            {
                priceColor = value;
                OnPropertyChanged();
            }
        }

        private string finalPriceColor;
        public string FinalPriceColor
        {
            get => finalPriceColor;
            set
            {
                finalPriceColor = value;
                OnPropertyChanged();
            }
        }

        private string imagePath;
        public string ImagePath
        {
            get => imagePath;
            set
            {
                imagePath = value;
                OnPropertyChanged();
            }
        }

        public ItemViewModel(Item item, float extraDiscountPercentage, IBasketService basketService)
        {
            this.basketService = basketService;

            Id = item.Id;
            Name = item.Name;
            ExtraDiscountPercentage = extraDiscountPercentage;

            float originalPrice = item.Price;
            float finalPrice = originalPrice;

            if (item.DiscountPercentage > 0)
            {
                finalPrice *= (1.0f - item.DiscountPercentage / 100.0f);
            }

            if (ExtraDiscountPercentage > 0)
            {
                finalPrice *= (1.0f - ExtraDiscountPercentage / 100.0f);
            }

            if (finalPrice < originalPrice)
            {
                PriceDiscountedString = originalPrice.ToString("C", CultureInfo.CurrentCulture);
                PriceColor = "Gray";
                FinalPriceColor = "Green";
            }
            else
            {
                PriceDiscountedString = "";
                PriceColor = "Transparent";
                FinalPriceColor = "Black";
            }

            PriceString = finalPrice.ToString("C", CultureInfo.CurrentCulture);
            ImagePath = BuildImagePath(item.ImagePath);

            AddToBasketCommand = new DelegateCommand(_ => this.basketService.AddToBasket(Id, 1, ExtraDiscountPercentage));
        }

        private static string BuildImagePath(string rawPath)
        {
            if (string.IsNullOrWhiteSpace(rawPath))
            {
                return "ms-appx:///Assets/placeholder.png";
            }

            if (rawPath.StartsWith("ms-appx://"))
            {
                return rawPath;
            }

            string normalizedPath = rawPath.Replace("\\", "/").TrimStart('.');
            if (!normalizedPath.StartsWith("/"))
            {
                normalizedPath = "/" + normalizedPath;
            }

            return "ms-appx://" + normalizedPath;
        }
    }
}