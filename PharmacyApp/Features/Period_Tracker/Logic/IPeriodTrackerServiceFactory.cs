namespace PharmacyApp.Features.Period_Tracker.Logic
{
    public interface IPeriodTrackerServiceFactory
    {
        IPeriodTrackerService CreatePeriodTrackerService();
        IWellnessItemsService CreateWellnessItemsService();
        IBasketService CreateBasketService();
    }
}
