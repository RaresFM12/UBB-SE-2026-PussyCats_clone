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

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _priceString;
        public string PriceString
        {
            get { return _priceString; }
            set
            {
                _priceString = value;
                OnPropertyChanged();
            }
        }

        private string _priceDiscountedString;
        public string PriceDiscountedString
        {
            get { return _priceDiscountedString; }
            set
            {
                _priceDiscountedString = value;
                OnPropertyChanged();
            }
        }

        private string _priceColor;
        public string PriceColor
        {
            get { return _priceColor; }
            set
            {
                _priceColor = value;
                OnPropertyChanged();
            }
        }

        private string _finalPriceColor;
        public string FinalPriceColor
        {
            get { return _finalPriceColor; }
            set
            {
                _finalPriceColor = value;
                OnPropertyChanged();
            }
        }

        private string _imagePath;
        public string ImagePath
        {
            get { return _imagePath; }
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

            float originalPrice = item.Price;
            float finalPrice = originalPrice;

            if (item.DiscountPercentage > 0)
                finalPrice *= (1.0f - item.DiscountPercentage / 100.0f);

            if (extraDiscountPercentage > 0)
                finalPrice *= (1.0f - extraDiscountPercentage / 100.0f);

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