using PharmacyApp.Models;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PharmacyApp.Features.Period_Tracker.ViewModels
{
    public class ItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int Id { get; set; }
        public int ItemIndex { get; set; }
        public ICommand AddToBasketCommand { get; set; }

        public float ExtraDiscountPercentage { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _priceString;
        public string PriceString
        {
            get => _priceString;
            set
            {
                _priceString = value;
                OnPropertyChanged();
            }
        }

        private string _priceDiscountedString;
        public string PriceDiscountedString
        {
            get => _priceDiscountedString;
            set
            {
                _priceDiscountedString = value;
                OnPropertyChanged();
            }
        }

        private string _priceColor;
        public string PriceColor
        {
            get => _priceColor;
            set
            {
                _priceColor = value;
                OnPropertyChanged();
            }
        }

        private string _finalPriceColor;
        public string FinalPriceColor
        {
            get => _finalPriceColor;
            set
            {
                _finalPriceColor = value;
                OnPropertyChanged();
            }
        }

        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                OnPropertyChanged();
            }
        }

        public ItemViewModel(Item item, float extraDiscountPercentage = 0)
        {
            Id = item.Id;
            Name = item.Name;
            ExtraDiscountPercentage = extraDiscountPercentage;

            float originalPrice = item.Price;
            float finalPrice = originalPrice;

            if (item.DiscountPercentage > 0)
                finalPrice *= (1.0f - item.DiscountPercentage / 100.0f);

            if (ExtraDiscountPercentage > 0)
                finalPrice *= (1.0f - ExtraDiscountPercentage / 100.0f);

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
            ImagePath = "\\Assets\\placeholder.png";
        }
    }
}