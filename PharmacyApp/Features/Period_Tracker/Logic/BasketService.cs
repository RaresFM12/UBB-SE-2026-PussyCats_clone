using PharmacyApp.Features.Orders.Logic;

namespace PharmacyApp.Features.Period_Tracker.Logic
{
    public class BasketService : IBasketService
    {
        private readonly OrderService orderService;

        public BasketService()
            : this(new OrderService())
        {
        }

        public BasketService(OrderService orderService)
        {
            this.orderService = orderService;
        }

        public void AddToBasket(int itemId, int quantity, float extraDiscountPercentage = 0f)
        {
            orderService.AddToBasket(itemId, quantity, extraDiscountPercentage);
        }
    }
}