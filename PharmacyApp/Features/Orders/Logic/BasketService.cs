using System;
using System.Collections.Generic;
using System.Linq;
using PharmacyApp.Common.Repositories;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Features.Orders.ViewModels;
using PharmacyApp.Models;

namespace PharmacyApp.Features.Orders.Logic
{
    public class BasketService : IBasketService
    {
        public ISubstancesRepository SubstancesRepository { get; private set; }
        public IItemsRepository ItemsRepository { get; private set; }
        public IUsersRepository UsersRepository { get; private set; }
        public IOrdersRepository OrdersRepository { get; private set; }

        private User injectedActiveUser;
        public User ActiveUser
        {
            get { return injectedActiveUser ?? ServiceWrapper.UserAccountService.CurrentUser; }
        }

        public BasketService()
        {
            SubstancesRepository = new SQLSubstancesRepository();
            ItemsRepository = new SQLItemsRepository();
            UsersRepository = new SQLUsersRepository();
            OrdersRepository = new SQLOrdersRepository();
        }

        public BasketService(ISubstancesRepository substancesRepository, IItemsRepository itemsRepository,
                            IUsersRepository usersRepository, IOrdersRepository ordersRepository, User activeUser)
        {
            SubstancesRepository = substancesRepository;
            ItemsRepository = itemsRepository;
            UsersRepository = usersRepository;
            OrdersRepository = ordersRepository;
            injectedActiveUser = activeUser;
        }

        private float NormalizeDiscount(float discount)
        {
            if (discount > 1f)
                discount /= 100f;

            if (discount < 0f)
                return 0f;

            if (discount > 1f)
                return 1f;

            return discount;
        }

        private static float RoundDownTo2Decimals(float value)
        {
            decimal temp = Math.Truncate((decimal)value * 100m) / 100m;
            return (float)temp;
        }

        private static string BuildImagePath(string originalPath)
        {
            if (string.IsNullOrWhiteSpace(originalPath))
                return "ms-appx:///Assets/logo.png";

            if (originalPath.StartsWith("ms-appx://", StringComparison.OrdinalIgnoreCase))
                return originalPath;

            int assetsIndex = originalPath.IndexOf("\\Assets", StringComparison.OrdinalIgnoreCase);
            if (assetsIndex != -1)
            {
                string backwardSlashedPath = originalPath.Substring(assetsIndex);
                return "ms-appx://" + backwardSlashedPath.Replace("\\", "/");
            }

            return "ms-appx:///Assets/logo.png";
        }

        private BasketItemViewModel BuildBasketItemViewModel(int itemId, BasketEntry basketEntry)
        {
            Item currentItem = ItemsRepository.GetItem(itemId);

            float baseItemDiscount = NormalizeDiscount(currentItem.DiscountPercentage);
            float extraItemDiscount = NormalizeDiscount(basketEntry.ExtraDiscountPercentage);
            float userDiscount = 0f;

            if (ActiveUser.UserDiscounts.ContainsKey(currentItem.Id))
                userDiscount = NormalizeDiscount(ActiveUser.UserDiscounts[currentItem.Id]);

            BasketItemViewModel basketItem = new BasketItemViewModel(
                currentItem.Id,
                BuildImagePath(currentItem.ImagePath),
                currentItem.Name,
                currentItem.Producer,
                basketEntry.Quantity,
                baseItemDiscount,
                extraItemDiscount,
                userDiscount,
                currentItem.Price);

            RecalculateBasketItemPrices(basketItem);

            return basketItem;
        }

        public void AddToBasket(int itemId, int quantityToBuy)
        {
            AddItemToBasket(itemId, quantityToBuy, 0f);
        }

        public void AddItemToBasket(int itemId, int quantityToBuy, float extraDiscountPercentage = 0f)
        {
            if (ActiveUser.Basket.ContainsKey(itemId))
            {
                ActiveUser.Basket[itemId].Quantity += quantityToBuy;

                if (extraDiscountPercentage > ActiveUser.Basket[itemId].ExtraDiscountPercentage)
                    ActiveUser.Basket[itemId].ExtraDiscountPercentage = extraDiscountPercentage;

                return;
            }

            ActiveUser.AddItemToBasket(itemId, quantityToBuy, extraDiscountPercentage);
        }

        public void UpdateBasketItemQuantity(int itemId, int newQuantityToBuy)
        {
            ActiveUser.Basket[itemId].Quantity = newQuantityToBuy;

            if (ActiveUser.Basket[itemId].Quantity <= 0)
                ActiveUser.RemoveItemFromBasket(itemId);
        }

        public void RemoveFromBasket(int itemIdToRemove)
        {
            ActiveUser.RemoveItemFromBasket(itemIdToRemove);
        }

        public List<BasketItemViewModel> GetBasketItems()
        {
            List<BasketItemViewModel> basketItems = new();
            List<int> invalidItemIds = new();

            if (ActiveUser == null)
                return basketItems;

            foreach (KeyValuePair<int, BasketEntry> item in ActiveUser.Basket)
            {
                try
                {
                    basketItems.Add(BuildBasketItemViewModel(item.Key, item.Value));
                }
                catch
                {
                    invalidItemIds.Add(item.Key);
                }
            }

            foreach (int invalidItemId in invalidItemIds)
                ActiveUser.RemoveItemFromBasket(invalidItemId);

            return basketItems;
        }

        public void ApplyPrescriptionToBasket(string prescriptionId)
        {
            Dictionary<int, int> prescriptionItems = FillBasketFromPrescription(prescriptionId);

            if (prescriptionItems.Count == 0)
                throw new ArgumentException("Medicine couldn't be retrieved");

            foreach (KeyValuePair<int, int> itemEntry in prescriptionItems)
                AddItemToBasket(itemEntry.Key, itemEntry.Value, 0f);
        }

        public void RecalculateBasketItemPrices(BasketItemViewModel basketItem)
        {
            float finalPriceBeforeDiscount = RoundDownTo2Decimals(
                basketItem.InitialPricePerBox * basketItem.ItemQuantityInBasket);

            float discountedPrice = finalPriceBeforeDiscount;
            discountedPrice *= (1 - basketItem.BaseItemDiscount);
            discountedPrice *= (1 - basketItem.ExtraItemDiscount);
            discountedPrice *= (1 - basketItem.ItemActiveUserDiscount);

            basketItem.SetFinalPrices(
                finalPriceBeforeDiscount,
                RoundDownTo2Decimals(Math.Max(0f, discountedPrice)));
        }

        public Tuple<float, float> CalculateBasketTotalSum(IEnumerable<BasketItemViewModel> basketItems)
        {
            float totalBefore = basketItems.Sum(item => item.FinalPriceBeforeDiscount);
            float totalAfter = basketItems.Sum(item => item.FinalPriceAfterDiscount);
            return new Tuple<float, float>(totalBefore, totalAfter);
        }

        public Dictionary<int, int> FillBasketFromPrescription(string prescriptionId)
        {
            return ItemsRepository.GetItemsFromPrescription(prescriptionId, ActiveUser.UserDiscounts);
        }
    }
}
