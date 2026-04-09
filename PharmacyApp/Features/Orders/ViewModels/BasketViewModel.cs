using Microsoft.UI.Xaml;
using PharmacyApp.Common.Commands;
using PharmacyApp.Common.Repositories;
using PharmacyApp.Common.Services;
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

    // TODO maybe refactor access modifiers
    public class BasketItem : INotifyPropertyChanged, IEquatable<BasketItem>
    {
        float finalPrice;
        float priceDiscountsApplied;
        int quantity;

        public int ItemId { get; private set; }
        public string ItemThumbnailImagePath { get; private set; }
        public string ItemName { get; private set; }
        public string ItemProducer { get; private set; }
        public int ItemQuantityInBasket 
        { 
            get { return quantity; } 
            set { 
                quantity = value; 
                OnPropertyChanged();
                CalculateFinalPrices();
            } 
        }
        public float ItemActiveDiscount { get; private set; }
        public float ItemActiveUserDiscount { get; private set; }
        public float InitialPricePerBox { get; private set; }
        public float FinalPriceBeforeDiscount 
        { 
            get { return finalPrice; }
            private set {
                finalPrice = value;
                OnPropertyChanged();
            }
        }
        public float FinalPriceAfterDiscount
        { 
            get { return priceDiscountsApplied; }
            private set {
                priceDiscountsApplied = value;
                OnPropertyChanged();
            }
        }


        // These properties were written because AFAIK there's no way
        // to for ex. set the text in a TextBox to a concatenation of two strings which
        // are bound to the properties in this class (I mean to use x:Bind multiple times between quotes)
        //
        // calling OnPropertyChanged on the underlying numerical properties
        public string ItemDescription 
        { 
            get { return ItemName + " - " + ItemProducer; } 
            set { } 
        }
        public string ItemQuantityString
        {
            get { return "Quantity: " + ItemQuantityInBasket; }
            set { }
        }
        public string ItemDiscountString 
        { 
            get { return "-" + (int)(ItemActiveDiscount * 100) + "%"; } 
            set { } 
        }
        public string ItemUserDiscountString { 
            get { return "-" + (int)(ItemActiveUserDiscount * 100) + "%"; } 
            set { }
        }
        private string finalPriceString;
        public string ItemFinalPriceString
        {
            get { return finalPriceString; }
            set { finalPriceString = value; OnPropertyChanged(); }
        }
        private string finalDiscountedPriceString;
        public string ItemFinalDiscountedPriceString
        {
            get { return finalDiscountedPriceString; }
            set { finalDiscountedPriceString = value; OnPropertyChanged(); }
        }


        private void CalculateFinalPrices()
        {
            //FinalPriceBeforeDiscount = InitialPricePerBox * ItemQuantityInBasket;

            //priceDiscountsApplied = FinalPriceBeforeDiscount * (1 - ItemActiveDiscount) * (1 - ItemActiveUserDiscount);
            //decimal temp = Math.Truncate((decimal)priceDiscountsApplied * 100) / 100;
            //FinalPriceAfterDiscount = (float)temp;

            finalPrice = InitialPricePerBox * ItemQuantityInBasket;

            priceDiscountsApplied = finalPrice * (1 - ItemActiveDiscount) * (1 - ItemActiveUserDiscount);
            decimal temp = Math.Truncate((decimal)priceDiscountsApplied * 100) / 100;
            priceDiscountsApplied = (float)temp;

            // we have to update the strings as well
            // this is convoluted I know...
            // but right now I don't know any other way to chain together the changes
            // from quantity to price calculation
            // it does price calculation automatically under the hood, it's just that the UI
            // elements that display the updated prices (discounted and not) don't bind to the numbers themselves
            ItemFinalPriceString = FinalPriceBeforeDiscount.ToString("0.00") + " RON";
            ItemFinalDiscountedPriceString = FinalPriceAfterDiscount.ToString("0.00") + " RON";
        }


        public BasketItem(int itemId, string imagePath, string name, string producer, int quantity,
                          float activeDiscount, float userDiscount, float initialPrice)
        {
            ItemId = itemId;
            ItemThumbnailImagePath = imagePath;
            ItemName = name;
            ItemProducer = producer;
            ItemQuantityInBasket = quantity;
            ItemActiveDiscount = activeDiscount;
            ItemActiveUserDiscount = userDiscount;
            InitialPricePerBox = initialPrice;

            // TODO rewrite this (and in the getter as well) because it doesn't seem elegant
            CalculateFinalPrices();
        }


        public bool Equals(BasketItem other)
        {
            if (other is null) return false;
            return this.ItemId == other.ItemId;
        }


        // for INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] String propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public class BasketViewModel : INotifyPropertyChanged
    {

        OrderService orderService;

        public ICommand RemoveItemCommand { get; set; }

        public ObservableCollection<BasketItem> BasketItems { get; private set; }


        string totalPriceBeforeDiscount;
        public string TotalPriceString
        {
            get { return totalPriceBeforeDiscount; }
            set { totalPriceBeforeDiscount = value; OnPropertyChanged(); }
        }

        string totalPriceAfterDiscount;
        public string TotalDiscountedPriceString
        {
            get { return totalPriceAfterDiscount; }
            set { totalPriceAfterDiscount = value; OnPropertyChanged(); }
        }


        public BasketViewModel(OrderService newOrderService)
        {
            orderService = newOrderService;
            RemoveItemCommand = new RelayCommandWithOneParameter<BasketItem>(RemoveItemFromBasket);

            // get the info for every item (from Items, Users) inside a wrapper class
            Dictionary<int,int> itemsInBasket = newOrderService.ActiveUser.Basket;
            BasketItems = new();

            foreach (KeyValuePair<int,int> item in itemsInBasket)
            {
                Item currentItem = newOrderService.ItemsRepository.GetItem(item.Key);

                float userDiscount;
                if (newOrderService.ActiveUser.UserDiscounts.ContainsKey(currentItem.Id))
                    userDiscount = newOrderService.ActiveUser.UserDiscounts[currentItem.Id];
                else
                    userDiscount = 0f;

                // TODO figure out, why does the image in XAML take FORWARD slashes
                // instead of BACKWARD slashes, like everything else in Windows
                string alteredImagePath;
                if (currentItem.ImagePath.StartsWith("ms-appx://"))
                {
                    // Already correct format
                    alteredImagePath = currentItem.ImagePath;
                }
                else
                {
                    // Convert from Windows path to ms-appx:// (juste added ms-appx:// in the alteredImagePath)
                    int startingIndexOfImagePathSubstring = currentItem.ImagePath.IndexOf("\\Assets");
                    if (startingIndexOfImagePathSubstring!=-1)
                    {
                        string backwardSlashedImagePath = currentItem.ImagePath.Substring(startingIndexOfImagePathSubstring);
                        alteredImagePath = "ms-appx://" + backwardSlashedImagePath.Replace("\\", "/");
                    }
                    else
                    {
                        alteredImagePath = "ms-appx:///Assets/logo.png";
                    }

                }
                //modified by Isac

                BasketItem basketItem = new(
                    currentItem.Id,
                    alteredImagePath,
                    currentItem.Name,
                    currentItem.Producer,
                    item.Value,  // the quantity inside the basket for said item
                    currentItem.DiscountPercentage,
                    userDiscount,
                    currentItem.Price);

                basketItem.PropertyChanged += UpdateItemInBasket;
                BasketItems.Add(basketItem);
            }

            UpdateTotalPrices();
        }


        private void RemoveItemFromBasket(BasketItem itemToRemove)
        {
            orderService.RemoveFromBasket(itemToRemove.ItemId);
            BasketItems.Remove(itemToRemove);

            OnBasketQuantityRemoved();
            UpdateTotalPrices();
        }

        // TODO maybe leave the removing part in the service to RemoveItemFromBasket from here?
        private void UpdateItemInBasket(object item, PropertyChangedEventArgs e)
        {
            BasketItem itemToUpdate = (BasketItem)item;
            orderService.UpdateBasketItemQuantity(itemToUpdate.ItemId, itemToUpdate.ItemQuantityInBasket);

            if (itemToUpdate.ItemQuantityInBasket <= 0)
                BasketItems.Remove(itemToUpdate);

            OnBasketQuantityRemoved();
            UpdateTotalPrices();
        }

        private void UpdateTotalPrices()
        {
            float newTotalPrice = 0f;
            float newTotalDiscountedPrice = 0f;

            foreach (BasketItem item in BasketItems)
            {
                newTotalPrice += item.FinalPriceBeforeDiscount;
                newTotalDiscountedPrice += item.FinalPriceAfterDiscount;
            }

            TotalPriceString = newTotalPrice.ToString("0.00") + " RON";
            TotalDiscountedPriceString = newTotalDiscountedPrice.ToString("0.00") + " RON";
        }

        public void GetPrescription(string prescriptionId)
        {
            Dictionary<int, int> prescriptionItems = orderService.FillBasketFromPrescription(prescriptionId);

            if (prescriptionItems.Count == 0)
                throw new ArgumentException("Medicine couldn't be retrieved");

            // same procedure as in the constructor

            foreach (KeyValuePair<int, int> itemEntry in prescriptionItems)
            {
                Item currentItem = orderService.ItemsRepository.GetItem(itemEntry.Key);

                float userDiscount;
                if (orderService.ActiveUser.UserDiscounts.ContainsKey(currentItem.Id))
                    userDiscount = orderService.ActiveUser.UserDiscounts[currentItem.Id];
                else
                    userDiscount = 0f;

                // TODO figure out, why does the image in XAML take FORWARD slashes
                // instead of BACKWARD slashes, like everything else in Windows

                string alteredImagePath = "ms-appx:///Assets/logo.png";

                BasketItem newBasketItem = new(
                    currentItem.Id,
                    alteredImagePath,
                    currentItem.Name,
                    currentItem.Producer,
                    itemEntry.Value,  // the quantity inside the basket for said item
                    currentItem.DiscountPercentage,
                    userDiscount,
                    currentItem.Price);

                // we have to verify if the item doesn't already exist in the basket
                if (BasketItems.Contains(newBasketItem))
                {
                    foreach (BasketItem currentBasketItem in BasketItems)
                    {
                        if (newBasketItem.Equals(currentBasketItem))
                        {
                            currentBasketItem.ItemQuantityInBasket += itemEntry.Value;
                            break;
                        }
                    }
                }
                else
                {
                    newBasketItem.PropertyChanged += UpdateItemInBasket;
                    orderService.AddToBasket(itemEntry.Key, itemEntry.Value);
                    BasketItems.Add(newBasketItem);
                }
            }

            UpdateTotalPrices();
        }


        // for INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] String propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        // custom delegate and event for handling disabled state for checkout button
        public delegate void QuantityChanged(int quantity);

        public event QuantityChanged BasketQuantityRemoved;

        // TODO maybe a bad idea to expose the function like this
        public virtual void OnBasketQuantityRemoved()
        {
            int totalQuantity = 0;
            foreach (BasketItem basketEntry in BasketItems)
                totalQuantity += basketEntry.ItemQuantityInBasket;
            BasketQuantityRemoved?.Invoke(totalQuantity);
        }
    }
}
