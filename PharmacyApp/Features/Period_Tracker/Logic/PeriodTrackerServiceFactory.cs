namespace PharmacyApp.Features.Period_Tracker.Logic
{
    public class PeriodTrackerServiceFactory : IPeriodTrackerServiceFactory
    {
        public IPeriodTrackerService CreatePeriodTrackerService()
        {
            return new PeriodTrackerService();
        }

        public IWellnessItemsService CreateWellnessItemsService()
        {
            return new WellnessItemsService();
        }

        public IBasketService CreateBasketService()
        {
            return new BasketService();
        }
    }
}
