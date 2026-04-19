using PharmacyApp.Features.Orders.Logic;

namespace PharmacyApp.Features.Period_Tracker.Logic
{
    public class BasketService : IBasketService
    {
        private readonly IOrderService orderService;

        public BasketService()
            : this(new OrderService())
        {
        }

        public BasketService(IOrderService orderService)
        {
            this.orderService = orderService;
        }

        public void AddToBasket(int itemId, int quantity, float extraDiscountPercentage = 0f)
        {
            orderService.AddToBasket(itemId, quantity, extraDiscountPercentage);
        }
    }
}