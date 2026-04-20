namespace PharmacyApp.Features.Period_Tracker.Logic
{
    public interface IBasketService
    {
        void AddToBasket(int itemId, int quantity, float extraDiscountPercentage = 0f);
    }
}