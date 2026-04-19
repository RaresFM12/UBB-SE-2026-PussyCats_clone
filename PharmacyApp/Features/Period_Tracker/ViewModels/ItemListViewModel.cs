using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PharmacyApp.Features.Period_Tracker.ViewModels
{
    public class ItemListViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private ObservableCollection<ItemViewModel> items;
        public ObservableCollection<ItemViewModel> Items
        {
            get => items;
            set
            {
                if (items == value)
                {
                    return;
                }

                items = value;
                OnPropertyChanged();
            }
        }

        public ItemListViewModel()
        {
            Items = new ObservableCollection<ItemViewModel>();
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}