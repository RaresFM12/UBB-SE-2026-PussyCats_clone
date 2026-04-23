using PharmacyApp.Common.Commands;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PharmacyApp.Features.Orders.ViewModels
{
    public class ModifyIncompleteOrderViewModel : INotifyPropertyChanged
    {
        private readonly IOrderService orderService;
        public int currentOrderIdentifier; // Redenumit din currentOrderID

        public ICommand RemoveItemCommand { get; set; }
        public ObservableCollection<ItemDetail> OrderItems { get; set; }

        private string totalPriceString;
        public string TotalPriceString
        {
            get { return totalPriceString; }
            set { totalPriceString = value; OnPropertyChanged(); }
        }

        public DateOnly PickUpDate { get; private set; }

        public ModifyIncompleteOrderViewModel(IOrderService orderService, int currentOrderIdentifier)
        {
            this.orderService = orderService;
            this.currentOrderIdentifier = currentOrderIdentifier;

            // Folosim nume complete în expresia lambda
            RemoveItemCommand = new RelayCommandWithOneParameter<ItemDetail>(itemDetail => RemoveItemFromUnsavedOrder(itemDetail));

            // IMPORTANT: Am presupus că IOrderService are o metodă GetOrderById
            // pentru a nu accesa repository-ul direct din ViewModel
            Order currentOrder = orderService.GetOrderById(currentOrderIdentifier);

            Dictionary<int, OrderItem> itemsInOrder = currentOrder.OrderedItems;
            OrderItems = new ObservableCollection<ItemDetail>();
            float totalCalculatedPrice = 0f;

            foreach (KeyValuePair<int, OrderItem> orderItemEntry in itemsInOrder)
            {
                Item currentItem = orderService.GetItemById(orderItemEntry.Key);

                string itemImagePath = currentItem.ImagePath;
                string itemDescription = currentItem.Name + " - " + currentItem.Producer;

                int itemQuantity = orderItemEntry.Value.Quantity;
                float itemTotalFinalPrice = orderItemEntry.Value.FinalPrice;

                OrderItems.Add(
                    new ItemDetail(currentItem.Id, itemImagePath, itemDescription,
                                    itemQuantity, itemTotalFinalPrice)
                );

                totalCalculatedPrice += itemTotalFinalPrice;
            }

            TotalPriceString = totalCalculatedPrice.ToString("0.00") + " Romanian Leu";
            PickUpDate = currentOrder.PickUpDate;
        }

        private void RemoveItemFromUnsavedOrder(ItemDetail itemToRemove)
        {
            OrderItems.Remove(itemToRemove);
            UpdateTotalPrice();
        }

        private void UpdateTotalPrice()
        {
            float newTotalCalculatedPrice = 0f;

            foreach (ItemDetail itemDetail in OrderItems)
            {
                newTotalCalculatedPrice += itemDetail.ItemFinalPrice;
            }

            TotalPriceString = newTotalCalculatedPrice.ToString("0.00") + " Romanian Leu";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}