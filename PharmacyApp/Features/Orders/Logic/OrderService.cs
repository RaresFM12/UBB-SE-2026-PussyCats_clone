using PharmacyApp.Common.Repositories;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Features.Orders.ViewModels;
using PharmacyApp.Models;
using System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PharmacyApp.Features.Orders.Logic
{

    public class OrderService : IOrderService
    {
        private const float MinDiscount = 0f;
        private const float MaxDiscount = 1f;
        private const float PercentageDivisor = 100f;
        private const decimal PriceRoundingFactor = 100m;
        private const int NotFoundIndex = -1;
        private const float NoExtraDiscount = 0f;
        private const int EmptyQuantity = 0;

        public ISubstancesRepository SubstancesRepository { get; private set; }
        public IItemsRepository ItemsRepository { get; private set; }
        public IUsersRepository UsersRepository { get; private set; }
        public IOrdersRepository OrdersRepository { get; private set; }

        private User injectedActiveUser;
        public User ActiveUser
        {
            get { return injectedActiveUser ?? ServiceWrapper.UserAccountService.CurrentUser; }
        }

        public OrderService()
        {
            SubstancesRepository = new SQLSubstancesRepository();
            ItemsRepository = new SQLItemsRepository();
            UsersRepository = new SQLUsersRepository();
            OrdersRepository = new SQLOrdersRepository();
        }

        public OrderService(ISubstancesRepository substancesRepository, IItemsRepository itemsRepository,
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
            if (discount > MaxDiscount)
                discount /= PercentageDivisor;

            if (discount < MinDiscount)
                return MinDiscount;

            if (discount > MaxDiscount)
                return MaxDiscount;

            return discount;
        }

        private static float RoundDownTo2Decimals(float value)
        {
            decimal temp = Math.Truncate((decimal)value * PriceRoundingFactor) / PriceRoundingFactor;
            return (float)temp;
        }

        private static string BuildImagePath(string originalPath)
        {
            if (string.IsNullOrWhiteSpace(originalPath))
                return "ms-appx:///Assets/logo.png";

            if (originalPath.StartsWith("ms-appx://", StringComparison.OrdinalIgnoreCase))
                return originalPath;

            int assetsIndex = originalPath.IndexOf("\\Assets", StringComparison.OrdinalIgnoreCase);
            if (assetsIndex != NotFoundIndex)
            {
                string backwardSlashedPath = originalPath.Substring(assetsIndex);
                return "ms-appx://" + backwardSlashedPath.Replace("\\", "/");
            }

            return "ms-appx:///Assets/logo.png";
        }

        public void CompleteOrder(int orderID, Dictionary<int, Tuple<int, float>> updatedQuantities)
        {
            Order orderToComplete = OrdersRepository.GetOrder(orderID);
            DateTime timeNow = DateTime.Now;
            DateOnly currentDate = new DateOnly(timeNow.Year, timeNow.Month, timeNow.Day);

            foreach (var itemQuantityEntry in updatedQuantities)
            {
                int itemID = itemQuantityEntry.Key;
                int preferredItemQuantity = itemQuantityEntry.Value.Item1;
                Item itemToVerify = ItemsRepository.GetItem(itemID);

                if (itemToVerify.QuantityAtSpecifiedDate(currentDate) < preferredItemQuantity)
                    throw new ArgumentException("We don't have enough of " + itemToVerify.Name +
                        " - " + itemToVerify.Producer + "; " +
                        "delete the item from the order if you wish to complete it");
            }

            orderToComplete.IsCompleted = true;

            foreach (var itemEntryInOrder in orderToComplete.ItemQuantitiesWithFinalPrice)
                orderToComplete.RemoveItemFromOrder(itemEntryInOrder.Key);

            foreach (var itemQuantityEntry in updatedQuantities)
                orderToComplete.AddItemToOrder(itemQuantityEntry.Key,
                                                itemQuantityEntry.Value.Item1,
                                                itemQuantityEntry.Value.Item2);

            OrdersRepository.UpdateOrder(orderToComplete);

            foreach (var itemQuantityEntry in updatedQuantities)
            {
                int itemID = itemQuantityEntry.Key;
                int itemQuantityToSubtract = itemQuantityEntry.Value.Item1;
                Item itemToUpdate = ItemsRepository.GetItem(itemID);

                itemToUpdate.RemoveQuantity(itemQuantityToSubtract, currentDate);
                ItemsRepository.UpdateItem(itemToUpdate);
            }
        }

        public void ModifyIncompleteOrder(int orderIDToModify,
            Dictionary<int, Tuple<int, float>> updatedQuantities,
            DateOnly updatedPickUpDate)
        {
            Order orderToModify = OrdersRepository.GetOrder(orderIDToModify);

            DateOnly today = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            if (updatedPickUpDate <= today)
                throw new ArgumentException("The new pick-up date must be later than the current date");

            foreach (var itemQuantityEntry in updatedQuantities)
            {
                int itemID = itemQuantityEntry.Key;
                int preferredItemQuantity = itemQuantityEntry.Value.Item1;
                Item itemToVerify = ItemsRepository.GetItem(itemID);

                if (itemToVerify.QuantityAtSpecifiedDate(updatedPickUpDate) < preferredItemQuantity)
                    throw new ArgumentException("On " + updatedPickUpDate.ToString("yyyy.MM.dd") + ", " +
                        "we will have only " + itemToVerify.QuantityAtSpecifiedDate(updatedPickUpDate) +
                        " boxes of " + itemToVerify.Name + " - " + itemToVerify.Producer);
            }

            orderToModify.PickUpDate = updatedPickUpDate;

            foreach (var itemEntryInOrder in orderToModify.ItemQuantitiesWithFinalPrice)
                orderToModify.RemoveItemFromOrder(itemEntryInOrder.Key);

            foreach (var itemQuantityEntry in updatedQuantities)
                orderToModify.AddItemToOrder(itemQuantityEntry.Key,
                                             itemQuantityEntry.Value.Item1,
                                             itemQuantityEntry.Value.Item2);

            OrdersRepository.UpdateOrder(orderToModify);
        }

        public void PlaceOrderFromBasket(DateOnly chosenPickUpDate)
        {
            Dictionary<int, Tuple<int, float>> itemInfoForOrder = new();

            foreach (KeyValuePair<int, BasketEntry> basketItemEntry in ActiveUser.Basket)
            {
                Item currentItem = ItemsRepository.GetItem(basketItemEntry.Key);
                int currentItemQuantity = basketItemEntry.Value.Quantity;
                float extraDiscount = NormalizeDiscount(basketItemEntry.Value.ExtraDiscountPercentage);

                int itemQuantityAtPickUpDate = currentItem.QuantityAtSpecifiedDate(chosenPickUpDate);

                if (currentItemQuantity > itemQuantityAtPickUpDate)
                    throw new ArgumentException("On " + chosenPickUpDate.ToString("yyyy.MM.dd") + ", " +
                                                "we will have only " + itemQuantityAtPickUpDate + " boxes " +
                                                "of " + currentItem.Name + " by " + currentItem.Producer + " " +
                                                "instead of " + currentItemQuantity + ".");

                float itemDiscount = NormalizeDiscount(currentItem.DiscountPercentage);
                float userDiscount = MinDiscount;

                if (ActiveUser.UserDiscounts.ContainsKey(currentItem.Id))
                    userDiscount = NormalizeDiscount(ActiveUser.UserDiscounts[currentItem.Id]);

                float finalPrice = currentItemQuantity * currentItem.Price;
                finalPrice *= (1 - itemDiscount);
                finalPrice *= (1 - extraDiscount);
                finalPrice *= (1 - userDiscount);

                itemInfoForOrder.Add(currentItem.Id, new Tuple<int, float>(currentItemQuantity, finalPrice));
            }

            OrdersRepository.AddOrderWithItems(ActiveUser.Id, chosenPickUpDate, itemInfoForOrder);
            ActiveUser.Basket.Clear();
        }

        public void ResubmitExpiredOrder(int orderIDToResubmit, DateOnly chosenPickUpDate)
        {
            Order expiredOrder = OrdersRepository.GetOrder(orderIDToResubmit);
            Dictionary<int, Tuple<int, float>> itemInfoForOrder = expiredOrder.ItemQuantitiesWithFinalPrice;

            foreach (KeyValuePair<int, Tuple<int, float>> orderItemEntry in itemInfoForOrder)
            {
                Item currentItem = ItemsRepository.GetItem(orderItemEntry.Key);
                int currentItemQuantity = orderItemEntry.Value.Item1;
                int itemQuantityAtPickUpDate = currentItem.QuantityAtSpecifiedDate(chosenPickUpDate);

                if (currentItemQuantity > itemQuantityAtPickUpDate)
                    throw new ArgumentException("On " + chosenPickUpDate.ToString("yyyy.MM.dd") + ", " +
                                                "we will have only " + itemQuantityAtPickUpDate + " boxes " +
                                                "of " + currentItem.Name + " by " + currentItem.Producer + " " +
                                                "instead of " + currentItemQuantity + ".");
            }

            OrdersRepository.AddOrderWithItems(ActiveUser.Id, chosenPickUpDate, itemInfoForOrder);
        }

        public void CancelOrder(int orderId)
        {
            Order orderToCancel = OrdersRepository.GetOrder(orderId);
            orderToCancel.IsExpired = true;
            OrdersRepository.UpdateOrder(orderToCancel);
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
            AddItemToBasket(itemId, quantityToBuy, NoExtraDiscount);
        }

        public void AddItemToBasket(int itemId, int quantityToBuy, float extraDiscountPercentage = NoExtraDiscount)
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

            if (ActiveUser.Basket[itemId].Quantity <= EmptyQuantity)
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
                AddItemToBasket(itemEntry.Key, itemEntry.Value, NoExtraDiscount);
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
                RoundDownTo2Decimals(Math.Max(MinDiscount, discountedPrice)));
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