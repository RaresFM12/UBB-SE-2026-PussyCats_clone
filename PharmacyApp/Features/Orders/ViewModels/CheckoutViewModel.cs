using PharmacyApp.Common.Commands;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace PharmacyApp.Features.Orders.ViewModels
{
    public class CheckoutViewModel
    {
        private readonly IOrderService orderService;

        public List<BasketItem> BasketItems { get; private set; }
        public string TotalPriceString { get; private set; }

        public ICommand PlaceOrderCommand { get; }

        public event Action OrderPlacedSuccessfully;
        public event Action<string> OrderPlacementFailed;

        public CheckoutViewModel(IOrderService injectedOrderService)
        {
            orderService = injectedOrderService;
            BasketItems = new List<BasketItem>();
            PlaceOrderCommand = new RelayCommandWithOneParameter<DateTimeOffset>(ExecutePlaceOrder);

            LoadBasketItems();
        }

        private void LoadBasketItems()
        {
            Dictionary<int, BasketEntry> itemsInBasket = orderService.ActiveUser.Basket;
            float totalPrice = 0f;

            foreach (KeyValuePair<int, BasketEntry> item in itemsInBasket)
            {
                Item currentItem = orderService.ItemsRepository.GetItem(item.Key);

                float userDiscount = 0f;
                if (orderService.ActiveUser.UserDiscounts.ContainsKey(currentItem.Id))
                {
                    userDiscount = orderService.ActiveUser.UserDiscounts[currentItem.Id];
                }

                string alteredImagePath = BuildImagePath(currentItem.ImagePath);

                BasketItem basketItem = new BasketItem(
                    currentItem.Id,
                    alteredImagePath,
                    currentItem.Name,
                    currentItem.Producer,
                    item.Value.Quantity,
                    currentItem.DiscountPercentage,
                    item.Value.ExtraDiscountPercentage,
                    userDiscount,
                    currentItem.Price);

                BasketItems.Add(basketItem);
                totalPrice += basketItem.FinalPriceAfterDiscount;
            }

            TotalPriceString = $"{totalPrice:0.00} RON";
        }

        private string BuildImagePath(string originalPath)
        {
            if (originalPath.StartsWith("ms-appx://"))
                return originalPath;

            int startingIndexOfImagePathSubstring = originalPath.IndexOf("\\Assets");
            if (startingIndexOfImagePathSubstring != -1)
            {
                string backwardSlashedImagePath = originalPath.Substring(startingIndexOfImagePathSubstring);
                return "ms-appx://" + backwardSlashedImagePath.Replace("\\", "/");
            }

            return "ms-appx:///Assets/logo.png";
        }

        private void ExecutePlaceOrder(DateTimeOffset selectedDateOffset)
        {
            try
            {
                DateOnly selectedDate = DateOnly.FromDateTime(selectedDateOffset.Date);
                orderService.PlaceOrderFromBasket(selectedDate);

                OrderPlacedSuccessfully?.Invoke();
            }
            catch (ArgumentException exception)
            {
                OrderPlacementFailed?.Invoke(exception.Message);
            }
        }
    }
}