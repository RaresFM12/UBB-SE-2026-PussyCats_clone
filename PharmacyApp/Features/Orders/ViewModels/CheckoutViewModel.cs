using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyApp.Features.Orders.ViewModels
{
    public class CheckoutViewModel
    {
        OrderService orderService;

        public List<BasketItem> BasketItems { get; private set; }

        public string TotalPriceString { get; private set; }

        public CheckoutViewModel(OrderService userService)
        {
            orderService = userService;

            Dictionary<int, BasketEntry> itemsInBasket = userService.ActiveUser.Basket;
            BasketItems = new();

            foreach (KeyValuePair<int, BasketEntry> item in itemsInBasket)
            {
                Item currentItem = userService.ItemsRepository.GetItem(item.Key);

                float userDiscount;
                if (userService.ActiveUser.UserDiscounts.ContainsKey(currentItem.Id))
                    userDiscount = userService.ActiveUser.UserDiscounts[currentItem.Id];
                else
                    userDiscount = 0f;

                string alteredImagePath;
                if (currentItem.ImagePath.StartsWith("ms-appx://"))
                {
                    alteredImagePath = currentItem.ImagePath;
                }
                else
                {
                    int startingIndexOfImagePathSubstring = currentItem.ImagePath.IndexOf("\\Assets");
                    if (startingIndexOfImagePathSubstring != -1)
                    {
                        string backwardSlashedImagePath = currentItem.ImagePath.Substring(startingIndexOfImagePathSubstring);
                        alteredImagePath = "ms-appx://" + backwardSlashedImagePath.Replace("\\", "/");
                    }
                    else
                    {
                        alteredImagePath = "ms-appx:///Assets/logo.png";
                    }
                }

                BasketItem basketItem = new(
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
            }

            float totalPrice = 0f;

            foreach (BasketItem item in BasketItems)
            {
                totalPrice += item.FinalPriceAfterDiscount;
            }

            TotalPriceString = totalPrice.ToString("0.00") + " RON";
        }
    }
}
