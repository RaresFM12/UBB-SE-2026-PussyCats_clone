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
    public class ResubmitOrderViewModel
    {

        OrderService orderServ;
        public int shownOrderID;

        // on the resubmit page we can't modify the order, just put a new one based on another expired one
        public List<ItemDetail> OrderItems { get; private set; }

        public string TotalPriceString { get; private set; }


        // repeat functionality from checkout page...........this feels bad
        public ResubmitOrderViewModel(OrderService orderService, int currOrderID)
        {
            orderServ = orderService;
            shownOrderID = currOrderID;

            Order currOrder = orderServ.OrdersRepository.GetOrder(currOrderID);
            Dictionary<int, Tuple<int, float>> itemsInOrder = currOrder.ItemQuantitiesWithFinalPrice;
            OrderItems = new();

            foreach (KeyValuePair<int, Tuple<int, float>> orderItemEntry in itemsInOrder)
            {
                Item currentItem = orderServ.ItemsRepository.GetItem(orderItemEntry.Key);

                // TODO figure out, why does the image in XAML take FORWARD slashes
                // instead of BACKWARD slashes, like everything else in Windows
                string alteredImagePath = currentItem.ImagePath;

                ItemDetail itemRepresentation = new ItemDetail(
                        currentItem.Id,
                        alteredImagePath,
                        currentItem.Name + " - " + currentItem.Producer,
                        orderItemEntry.Value.Item1,
                        orderItemEntry.Value.Item2
                    );

                OrderItems.Add(itemRepresentation);
            }

            // to set the final price for the UI (we don't have to update
            // anything in the list view)
            float totalPrice = 0f;

            foreach (ItemDetail item in OrderItems)
            {
                totalPrice += item.ItemFinalPrice;
            }

            TotalPriceString = totalPrice.ToString("0.00") + " RON";
        }

    }
}
