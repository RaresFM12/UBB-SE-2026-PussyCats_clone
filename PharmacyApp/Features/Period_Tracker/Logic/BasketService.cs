using PharmacyApp.Features.Orders.Logic;

namespace PharmacyApp.Features.Period_Tracker.Logic
{
    public class BasketService : IBasketService
    {
        private readonly OrderService orderManagementService;

        public BasketService()
            : this(new OrderService())
        {
        }

        public BasketService(OrderService orderManagementService)
        {
            this.orderManagementService = orderManagementService;
        }

        public void AddToBasket(int itemId, int quantity, float extraDiscountPercentage = 0f)
        {
            orderManagementService.AddToBasket(itemId, quantity, extraDiscountPercentage);
        }
    }
}