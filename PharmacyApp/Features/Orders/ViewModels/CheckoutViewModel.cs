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

            // get the info for every item (from Items, Users) inside a wrapper class
            Dictionary<int, int> itemsInBasket = userService.ActiveUser.Basket;
            BasketItems = new();

            foreach (KeyValuePair<int, int> item in itemsInBasket)
            {
                Item currentItem = userService.ItemsRepository.GetItem(item.Key);

                float userDiscount;
                if (userService.ActiveUser.UserDiscounts.ContainsKey(currentItem.Id))
                    userDiscount = userService.ActiveUser.UserDiscounts[currentItem.Id];
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

                BasketItems.Add(basketItem);
            }

            // to set the final price for the UI (we don't have to update
            // anything in the list view)
            float totalPrice = 0f;

            foreach (BasketItem item in BasketItems)
            {
                totalPrice += item.FinalPriceAfterDiscount;
            }

            TotalPriceString = totalPrice.ToString("0.00") + " RON";
        }
    }
}
