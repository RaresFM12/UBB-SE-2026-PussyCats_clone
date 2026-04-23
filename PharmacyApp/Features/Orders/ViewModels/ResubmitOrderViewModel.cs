using PharmacyApp.Common.Services;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PharmacyApp.Features.Orders.ViewModels
{
    public class ResubmitOrderViewModel
    {
        private readonly IOrderService _orderBusinessLogicService;
        public int ShownOrderIdentificationNumber { get; private set; }

        public List<ItemDetail> OrderItemsDetailList { get; private set; }
        public string TotalAccumulatedPriceString { get; private set; }

        public ResubmitOrderViewModel(IOrderService orderBusinessLogicService, int currentOrderIdentificationNumber)
        {
            _orderBusinessLogicService = orderBusinessLogicService;
            ShownOrderIdentificationNumber = currentOrderIdentificationNumber;

            Order currentOrderInformation = _orderBusinessLogicService.GetOrderById(currentOrderIdentificationNumber);

            Dictionary<int, OrderItem> orderedItemsDictionary = currentOrderInformation.OrderedItems;
            OrderItemsDetailList = new List<ItemDetail>();

            foreach (KeyValuePair<int, OrderItem> orderItemInformationEntry in orderedItemsDictionary)
            {
                Item currentPharmacyItem = _orderBusinessLogicService.GetItemById(orderItemInformationEntry.Key);

                string modifiedImagePath = currentPharmacyItem.ImagePath;

                ItemDetail itemDetailRepresentation = new ItemDetail(
                        currentPharmacyItem.Id,
                        modifiedImagePath,
                        currentPharmacyItem.Name + " - " + currentPharmacyItem.Producer,
                        orderItemInformationEntry.Value.Quantity,
                        orderItemInformationEntry.Value.FinalPrice
                    );

                OrderItemsDetailList.Add(itemDetailRepresentation);
            }

            // Calculam pretul total folosind denumiri complete
            float totalCalculatedPrice = 0f;

            foreach (ItemDetail individualItemDetail in OrderItemsDetailList)
            {
                totalCalculatedPrice += individualItemDetail.ItemFinalPrice;
            }

            const string PriceFormattingPattern = "0.00";
            const string CurrencySuffix = " RON";
            TotalAccumulatedPriceString = totalCalculatedPrice.ToString(PriceFormattingPattern) + CurrencySuffix;
        }
    }
}