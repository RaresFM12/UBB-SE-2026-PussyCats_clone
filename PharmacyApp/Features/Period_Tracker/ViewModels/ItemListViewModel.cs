using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PharmacyApp.Features.Orders.Logic;
using Syncfusion.UI.Xaml.Core;

namespace PharmacyApp.Features.Period_Tracker.ViewModels
{
    public class ItemListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand AddItemToBasket { get; set; }

        private ObservableCollection<ItemViewModel> _items;
        public ObservableCollection<ItemViewModel> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }

        public ItemListViewModel()
        {
            Items = new ObservableCollection<ItemViewModel>();
            AddItemToBasket = new DelegateCommand(OnAddItemToBasketCommand);
        }

        public void OnAddItemToBasketCommand(object obj)
        {
            if (obj == null)
                return;

            int itemIndex = int.Parse(obj.ToString());

            if (itemIndex < 0 || itemIndex >= Items.Count)
                return;

            int itemId = Items[itemIndex].Id;
            float extraDiscount = Items[itemIndex].ExtraDiscountPercentage;

            OrderService orderService = new OrderService();
            orderService.AddToBasket(itemId, 1, extraDiscount);
        }
    }
}