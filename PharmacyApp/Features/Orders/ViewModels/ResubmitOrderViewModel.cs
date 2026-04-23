using System;
using System.Collections.Generic;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Models;

namespace PharmacyApp.Features.Orders.ViewModels
{
    public class ResubmitOrderViewModel : IResubmitOrderViewModel
    {
        private readonly IOrderService orderService;

        public int ShownOrderID { get; set; }

        public List<ItemDetail> OrderItems { get; private set; }

        public string TotalPriceString { get; private set; }

        public ResubmitOrderViewModel(IOrderService injectedOrderService, int currOrderID)
        {
            orderService = injectedOrderService;
            ShownOrderID = currOrderID;

            Order currOrder = orderService.OrdersRepository.GetOrder(currOrderID);
            Dictionary<int, Tuple<int, float>> itemsInOrder = currOrder.ItemQuantitiesWithFinalPrice;
            OrderItems = new ();

            foreach (KeyValuePair<int, Tuple<int, float>> orderItemEntry in itemsInOrder)
            {
                Item currentItem = orderService.ItemsRepository.GetItemById(orderItemEntry.Key);

                string alteredImagePath = currentItem.ImagePath;

                ItemDetail itemRepresentation = new ItemDetail(
                        currentItem.Id,
                        alteredImagePath,
                        currentItem.Name + " - " + currentItem.Producer,
                        orderItemEntry.Value.Item1,
                        orderItemEntry.Value.Item2);

                OrderItems.Add(itemRepresentation);
            }

            float totalPrice = 0f;

            foreach (ItemDetail item in OrderItems)
            {
                totalPrice += item.ItemFinalPrice;
            }

            TotalPriceString = totalPrice.ToString("0.00") + " RON";
        }
    }
}