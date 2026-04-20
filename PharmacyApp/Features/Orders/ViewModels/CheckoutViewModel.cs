using PharmacyApp.Features.Orders.Logic;
using System.Collections.Generic;
using System;
using PharmacyApp.Features.Orders.Logic;

namespace PharmacyApp.Features.Orders.ViewModels
{
    public class CheckoutViewModel
    {
        IBasketService basketService;

        public List<BasketItemViewModel> BasketItems { get; private set; }

        public string TotalPriceString { get; private set; }

        public CheckoutViewModel(IBasketService userService)
        {
            basketService = userService;
            BasketItems = basketService.GetBasketItems();

            Tuple<float, float> totals = basketService.CalculateBasketTotalSum(BasketItems);
            TotalPriceString = totals.Item2.ToString("0.00") + " RON";
        }
    }
}
