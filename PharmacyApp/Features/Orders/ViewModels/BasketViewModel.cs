using PharmacyApp.Common.Commands;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace PharmacyApp.Features.Orders.ViewModels
{
    public class BasketItem : INotifyPropertyChanged, IEquatable<BasketItem>
    {
        private float finalPriceBeforeDiscount;
        private float finalPriceAfterDiscount;
        private int quantity;

        public int ItemId { get; }
        public string ItemThumbnailImagePath { get; }
        public string ItemName { get; }
        public string ItemProducer { get; }
        public float InitialPricePerBox { get; }

        public float BaseItemDiscount { get; }
        public float ExtraItemDiscount { get; }
        public float ItemActiveDiscount => 1 - ((1 - BaseItemDiscount) * (1 - ExtraItemDiscount));
        public float ItemActiveUserDiscount { get; }

        public int ItemQuantityInBasket
        {
            get => quantity;
            set
            {
                int safeValue = Math.Max(0, value);

                if (quantity == safeValue)
                    return;

                quantity = safeValue;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ItemQuantityString));
                CalculateFinalPrices();
            }
        }

        public float FinalPriceBeforeDiscount
        {
            get => finalPriceBeforeDiscount;
            private set
            {
                if (Math.Abs(finalPriceBeforeDiscount - value) < 0.0001f)
                    return;

                finalPriceBeforeDiscount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ItemFinalPriceString));
            }
        }

        public float FinalPriceAfterDiscount
        {
            get => finalPriceAfterDiscount;
            private set
            {
                if (Math.Abs(finalPriceAfterDiscount - value) < 0.0001f)
                    return;

                finalPriceAfterDiscount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ItemFinalDiscountedPriceString));
            }
        }

        public string ItemDescription => $"{ItemName} - {ItemProducer}";
        public string ItemQuantityString => $"Quantity: {ItemQuantityInBasket}";
        public string ItemDiscountString => $"-{(int)Math.Round(ItemActiveDiscount * 100)}%";
        public string ItemUserDiscountString => $"-{(int)Math.Round(ItemActiveUserDiscount * 100)}%";
        public string ItemFinalPriceString => $"{FinalPriceBeforeDiscount:0.00} RON";
        public string ItemFinalDiscountedPriceString => $"{FinalPriceAfterDiscount:0.00} RON";

        public BasketItem(
            int itemId,
            string imagePath,
            string name,
            string producer,
            int quantity,
            float baseItemDiscount,
            float extraItemDiscount,
            float userDiscount,
            float initialPrice)
        {
            ItemId = itemId;
            ItemThumbnailImagePath = imagePath;
            ItemName = name;
            ItemProducer = producer;
            InitialPricePerBox = initialPrice;

            BaseItemDiscount = NormalizeDiscount(baseItemDiscount);
            ExtraItemDiscount = NormalizeDiscount(extraItemDiscount);
            ItemActiveUserDiscount = NormalizeDiscount(userDiscount);

            this.quantity = Math.Max(0, quantity);
            CalculateFinalPrices();
        }

        private static float NormalizeDiscount(float discount)
        {
            if (discount > 1f)
                discount /= 100f;

            if (discount < 0f)
                return 0f;

            if (discount > 1f)
                return 1f;

            return discount;
        }

        private void CalculateFinalPrices()
        {
            FinalPriceBeforeDiscount = RoundDownTo2Decimals(InitialPricePerBox * ItemQuantityInBasket);

            float discountedPrice = FinalPriceBeforeDiscount;
            discountedPrice *= (1 - BaseItemDiscount);
            discountedPrice *= (1 - ExtraItemDiscount);
            discountedPrice *= (1 - ItemActiveUserDiscount);

            FinalPriceAfterDiscount = RoundDownTo2Decimals(Math.Max(0f, discountedPrice));
        }

        private static float RoundDownTo2Decimals(float value)
        {
            decimal temp = Math.Truncate((decimal)value * 100m) / 100m;
            return (float)temp;
        }

        public bool Equals(BasketItem other)
        {
            if (other is null)
                return false;

            return ItemId == other.ItemId;
        }

        public override bool Equals(object obj) => Equals(obj as BasketItem);
        public override int GetHashCode() => ItemId.GetHashCode();

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BasketViewModel : INotifyPropertyChanged
    {
        private readonly OrderService orderService;
        private string totalPriceBeforeDiscount;
        private string totalPriceAfterDiscount;

        public ICommand RemoveItemCommand { get; }
        public ObservableCollection<BasketItem> BasketItems { get; }

        public string TotalPriceString
        {
            get => totalPriceBeforeDiscount;
            set
            {
                if (totalPriceBeforeDiscount == value)
                    return;

                totalPriceBeforeDiscount = value;
                OnPropertyChanged();
            }
        }

        public string TotalDiscountedPriceString
        {
            get => totalPriceAfterDiscount;
            set
            {
                if (totalPriceAfterDiscount == value)
                    return;

                totalPriceAfterDiscount = value;
                OnPropertyChanged();
            }
        }

        public BasketViewModel(OrderService newOrderService)
        {
            orderService = newOrderService;
            RemoveItemCommand = new RelayCommandWithOneParameter<BasketItem>(RemoveItemFromBasket);
            BasketItems = new ObservableCollection<BasketItem>();

            LoadBasketItems();
            UpdateTotalPrices();
        }

        private void LoadBasketItems()
        {
            Dictionary<int, BasketEntry> itemsInBasket = orderService.ActiveUser.Basket;

            foreach (KeyValuePair<int, BasketEntry> item in itemsInBasket)
            {
                Item currentItem = orderService.ItemsRepository.GetItem(item.Key);

                float userDiscount = 0f;
                if (orderService.ActiveUser.UserDiscounts.ContainsKey(currentItem.Id))
                    userDiscount = orderService.ActiveUser.UserDiscounts[currentItem.Id];

                string imagePath = BuildImagePath(currentItem.ImagePath);

                BasketItem basketItem = new BasketItem(
                    currentItem.Id,
                    imagePath,
                    currentItem.Name,
                    currentItem.Producer,
                    item.Value.Quantity,
                    currentItem.DiscountPercentage,
                    item.Value.ExtraDiscountPercentage,
                    userDiscount,
                    currentItem.Price);

                basketItem.PropertyChanged += UpdateItemInBasket;
                BasketItems.Add(basketItem);
            }
        }

        private static string BuildImagePath(string originalPath)
        {
            if (string.IsNullOrWhiteSpace(originalPath))
                return "ms-appx:///Assets/logo.png";

            if (originalPath.StartsWith("ms-appx://", StringComparison.OrdinalIgnoreCase))
                return originalPath;

            int assetsIndex = originalPath.IndexOf("\\Assets", StringComparison.OrdinalIgnoreCase);
            if (assetsIndex != -1)
            {
                string backwardSlashedPath = originalPath.Substring(assetsIndex);
                return "ms-appx://" + backwardSlashedPath.Replace("\\", "/");
            }

            return "ms-appx:///Assets/logo.png";
        }

        private void RemoveItemFromBasket(BasketItem itemToRemove)
        {
            if (itemToRemove == null)
                return;

            orderService.RemoveFromBasket(itemToRemove.ItemId);
            itemToRemove.PropertyChanged -= UpdateItemInBasket;
            BasketItems.Remove(itemToRemove);

            OnBasketQuantityRemoved();
            UpdateTotalPrices();
        }

        private void UpdateItemInBasket(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(BasketItem.ItemQuantityInBasket))
                return;

            BasketItem itemToUpdate = (BasketItem)sender;

            if (itemToUpdate.ItemQuantityInBasket <= 0)
            {
                orderService.RemoveFromBasket(itemToUpdate.ItemId);
                itemToUpdate.PropertyChanged -= UpdateItemInBasket;
                BasketItems.Remove(itemToUpdate);
            }
            else
            {
                orderService.UpdateBasketItemQuantity(itemToUpdate.ItemId, itemToUpdate.ItemQuantityInBasket);
            }

            OnBasketQuantityRemoved();
            UpdateTotalPrices();
        }

        private void UpdateTotalPrices()
        {
            float totalBefore = BasketItems.Sum(item => item.FinalPriceBeforeDiscount);
            float totalAfter = BasketItems.Sum(item => item.FinalPriceAfterDiscount);

            TotalPriceString = $"{totalBefore:0.00} RON";
            TotalDiscountedPriceString = $"{totalAfter:0.00} RON";
        }

        public void GetPrescription(string prescriptionId)
        {
            Dictionary<int, int> prescriptionItems = orderService.FillBasketFromPrescription(prescriptionId);

            if (prescriptionItems.Count == 0)
                throw new ArgumentException("Medicine couldn't be retrieved");

            foreach (KeyValuePair<int, int> itemEntry in prescriptionItems)
            {
                Item currentItem = orderService.ItemsRepository.GetItem(itemEntry.Key);
                string imagePath = BuildImagePath(currentItem.ImagePath);

                float userDiscount = 0f;
                if (orderService.ActiveUser.UserDiscounts.ContainsKey(currentItem.Id))
                    userDiscount = orderService.ActiveUser.UserDiscounts[currentItem.Id];

                BasketItem existingItem = BasketItems.FirstOrDefault(x => x.ItemId == itemEntry.Key);

                if (existingItem != null)
                {
                    existingItem.ItemQuantityInBasket += itemEntry.Value;
                }
                else
                {
                    BasketItem newBasketItem = new BasketItem(
                        currentItem.Id,
                        imagePath,
                        currentItem.Name,
                        currentItem.Producer,
                        itemEntry.Value,
                        currentItem.DiscountPercentage,
                        0f,
                        userDiscount,
                        currentItem.Price);

                    newBasketItem.PropertyChanged += UpdateItemInBasket;
                    orderService.AddToBasket(itemEntry.Key, itemEntry.Value, 0f);
                    BasketItems.Add(newBasketItem);
                }
            }

            UpdateTotalPrices();
            OnBasketQuantityRemoved();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public delegate void QuantityChanged(int quantity);
        public event QuantityChanged BasketQuantityRemoved;

        public virtual void OnBasketQuantityRemoved()
        {
            int totalQuantity = BasketItems.Sum(item => item.ItemQuantityInBasket);
            BasketQuantityRemoved?.Invoke(totalQuantity);
        }
    }
}