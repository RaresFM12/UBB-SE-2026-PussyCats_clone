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
        IOrderService orderService;
        public int currentOrderID;

        public ICommand RemoveItemCommand { get; set; }

        public ObservableCollection<ItemDetail> OrderItems { get; set; }

        string totalPriceString;
        public string TotalPriceString
        {
            get { return totalPriceString; }
            set { totalPriceString = value; OnPropertyChanged(); }
        }
        public DateOnly PickUpDate { get; private set; }

        public ModifyIncompleteOrderViewModel(IOrderService orderServ, int currOrderID)
        {
            orderService = orderServ;
            currentOrderID = currOrderID;
            RemoveItemCommand = new RelayCommandWithOneParameter<ItemDetail>(RemoveItemFromUnsavedOrder);

            Order currOrder = orderServ.OrdersRepository.GetOrder(currentOrderID);

            // --- MODIFIED TO USE OrderItem ---
            Dictionary<int, OrderItem> itemsInOrder = currOrder.OrderedItems;

            OrderItems = new ObservableCollection<ItemDetail>();
            float totalPrice = 0f;

            foreach (KeyValuePair<int, OrderItem> orderEntry in itemsInOrder)
            {
                Item currentItem = orderService.ItemsRepository.GetItem(orderEntry.Key);

                string alteredImagePath = currentItem.ImagePath;
                string itemDescription = currentItem.Name + " - " + currentItem.Producer;

                // --- EXTRACTING FROM OrderItem ---
                int itemQuantity = orderEntry.Value.Quantity;
                float itemTotalPrice = orderEntry.Value.FinalPrice;

                OrderItems.Add(
                    new ItemDetail(currentItem.Id, alteredImagePath, itemDescription,
                                    itemQuantity, itemTotalPrice)
                );

                totalPrice += itemTotalPrice;
            }

            TotalPriceString = totalPrice.ToString("0.00") + " RON";
            PickUpDate = currOrder.PickUpDate;
        }

        // "unsaved" because these changes (removing items) are not saved
        // immediately, only after validating and completing the order
        private void RemoveItemFromUnsavedOrder(ItemDetail itemToRemove)
        {
            OrderItems.Remove(itemToRemove);
            UpdateTotalPrice();
        }

        private void UpdateTotalPrice()
        {
            float newTotalPrice = 0f;

            foreach (ItemDetail item in OrderItems)
            {
                newTotalPrice += item.ItemFinalPrice;
            }

            TotalPriceString = newTotalPrice.ToString("0.00") + " RON";
        }

        // for INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] String propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}