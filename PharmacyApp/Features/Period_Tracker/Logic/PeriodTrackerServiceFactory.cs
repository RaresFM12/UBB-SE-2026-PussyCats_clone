using PharmacyApp.Common.Repositories;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Features.Orders.Logic;

namespace PharmacyApp.Features.Period_Tracker.Logic
{
    public class PeriodTrackerServiceFactory : IPeriodTrackerServiceFactory
    {
        private readonly IUsersRepository usersRepository;
        private readonly IItemsRepository itemsRepository;
        private readonly ICurrentUserService currentUserService;
        private readonly IOrderService orderService;

        public PeriodTrackerServiceFactory(
            IUsersRepository usersRepository,
            IItemsRepository itemsRepository,
            ICurrentUserService currentUserService,
            IOrderService orderService)
        {
            this.usersRepository = usersRepository;
            this.itemsRepository = itemsRepository;
            this.currentUserService = currentUserService;
            this.orderService = orderService;
        }

        public IPeriodTrackerService CreatePeriodTrackerService()
        {
            return new PeriodTrackerService(usersRepository, currentUserService);
        }

        public IWellnessItemsService CreateWellnessItemsService()
        {
            return new WellnessItemsService(itemsRepository);
        }

        public IBasketService CreateBasketService()
        {
            return new BasketService(orderService);
        }
    }
}