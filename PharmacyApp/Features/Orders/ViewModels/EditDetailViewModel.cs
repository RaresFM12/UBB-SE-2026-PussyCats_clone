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
    public class EditDetailViewModel : INotifyPropertyChanged
    {
        
        OrderService orderServ;

        public ICommand RemoveItemCommand { get; set; }

        public ObservableCollection<ItemDetail> OrderItems { get; private set; }


        string totalPriceString;
        public string TotalPriceString
        {
            get { return totalPriceString; }
            set { totalPriceString = value; OnPropertyChanged(); }
        }

        public string StatusString { get; private set; }
        public DateOnly PickUpDate { get; private set; }
        public string PickUpDateString { get { return PickUpDate.ToString("yyyy.MM.dd"); } }

        public int shownOrderID;

        public EditDetailViewModel(OrderService oService, int orderID)
        {
            shownOrderID = orderID;
            orderServ = oService;
            RemoveItemCommand = new RelayCommandWithOneParameter<ItemDetail>(RemoveItemFromUnsavedOrder);

            Order currOrder = orderServ.OrdersRepository.GetOrder(orderID);
            Dictionary<int, Tuple<int, float>> itemsInOrder = currOrder.ItemQuantitiesWithFinalPrice;
            OrderItems = new();
            float totalPrice = 0f;

            foreach (KeyValuePair<int, Tuple<int, float>> orderEntry in itemsInOrder)
            {
                Item currentItem = oService.ItemsRepository.GetItemById(orderEntry.Key);
                string alteredImagePath=currentItem.ImagePath;



                string itemDescription = currentItem.Name + " - " + currentItem.Producer;
                int itemQuantity = orderEntry.Value.Item1;
                float itemTotalPrice = orderEntry.Value.Item2;

                OrderItems.Add(
                    new ItemDetail(currentItem.Id, alteredImagePath, itemDescription,
                                    itemQuantity, itemTotalPrice)
                );

                totalPrice += itemTotalPrice;
            }

            TotalPriceString = totalPrice.ToString("0.00") + " RON";

            if (!currOrder.IsExpired && !currOrder.IsCompleted)
                StatusString = "Incomplete";
            else if (currOrder.IsExpired)
                StatusString = "Expired";
            else
                StatusString = "Complete";

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
