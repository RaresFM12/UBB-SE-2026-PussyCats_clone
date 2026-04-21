using PharmacyApp.Features.Orders.Logic;
using System.Collections.Generic;
using System;
using PharmacyApp.Features.Orders.Logic;

namespace PharmacyApp.Features.Orders.ViewModels
{
    public class CheckoutViewModel
    {
        IOrderService OrderService;

        public List<BasketItemViewModel> BasketItems { get; private set; }

        public string TotalPriceString { get; private set; }

        public CheckoutViewModel(IOrderService userService)
        {
            OrderService = userService;
            BasketItems = OrderService.GetBasketItems();

            Tuple<float, float> totals = OrderService.CalculateBasketTotalSum(BasketItems);
            TotalPriceString = totals.Item2.ToString("0.00") + " RON";
        }
    }
}
