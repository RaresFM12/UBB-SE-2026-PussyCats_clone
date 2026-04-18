using PharmacyApp.Common.Services;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyApp.Features.Orders.ViewModels
{

    public class ItemDetail
    {
        public int ItemID { get; private set; }
        public string ItemThumbnailImagePath { get; private set; }
        public string ItemDescription { get; private set; }
        public string ItemQuantityString { get { return "Quantity: " + ItemQuantity; } }
        public string ItemFinalPriceString { get { return ItemFinalPrice.ToString("0.00") + " RON"; } }
        public int ItemQuantity { get; private set; }
        public float ItemFinalPrice { get; private set; } 

        public ItemDetail(int itemID, string imagePath, string description, int quantity, float finalPrice)
        {
            ItemID = itemID;
            ItemThumbnailImagePath = imagePath;
            ItemDescription = description;
            ItemQuantity = quantity;
            ItemFinalPrice = finalPrice;
        }
    }


    internal class NonEditDetailViewModel
    {

        OrderService orderService;

        public List<ItemDetail> OrderItems { get; private set; }
        public string TotalPriceString { get; private set; }
        public string StatusString { get; private set; }
        public DateOnly PickUpDate { get; private set; }
        public string PickUpDateString { get { return PickUpDate.ToString("yyyy.MM.dd"); } }

        public NonEditDetailViewModel(OrderService orderServ, int orderID)
        {
            orderService = orderServ;
            OrderItems = new();

            Order shownOrder = orderService.OrdersRepository.GetOrder(orderID);
            float totalPrice = 0f;

            foreach (var currentOrderEntry in shownOrder.ItemQuantitiesWithFinalPrice)
            {
                int itemID = currentOrderEntry.Key;
                int itemQuantity = currentOrderEntry.Value.Item1;
                float itemTotalPrice = currentOrderEntry.Value.Item2;

                Item currentItem = orderServ.ItemsRepository.GetItem(itemID);

                // TODO figure out, why does the image in XAML take FORWARD slashes
                // instead of BACKWARD slashes, like everything else in Windows
                string alteredImagePath = currentItem.ImagePath;

                string itemDescription = currentItem.Name + " - " + currentItem.Producer;

                OrderItems.Add(
                    new ItemDetail(itemID, alteredImagePath, itemDescription,
                                    itemQuantity, itemTotalPrice)
                );

                totalPrice += itemTotalPrice;
            }

            TotalPriceString = totalPrice.ToString("0.00") + " RON";

            if (!shownOrder.IsExpired && !shownOrder.IsCompleted)
                StatusString = "Incomplete";
            else if (shownOrder.IsExpired)
                StatusString = "Expired";
            else
                StatusString = "Complete";

            PickUpDate = shownOrder.PickUpDate;
        }
    }
}
