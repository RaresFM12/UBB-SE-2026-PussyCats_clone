using System;
using System.Collections.Generic;
using PharmacyApp.Common.Repositories;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Models;

namespace PharmacyApp.Features.Orders.Logic
{
    public class OrderService : IOrderService
    {
        private const float MaximumDiscountLimit = 1f;
        private const float MinimumDiscountLimit = 0f;
        private const float PercentageDivisor = 100f;
        private const int DefaultNumberOfRequiredPills = 40;
        private const string ValidPrescriptionIdentifier = "testPrescription";
        private const string DefaultPrescriptionItemName = "prescript1";

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

        private float NormalizeDiscount(float discountPercentage)
        {
            if (discountPercentage > MaximumDiscountLimit)
            {
                discountPercentage /= PercentageDivisor;
            }

            if (discountPercentage < MinimumDiscountLimit)
            {
                return MinimumDiscountLimit;
            }

            if (discountPercentage > MaximumDiscountLimit)
            {
                return MaximumDiscountLimit;
            }

            return discountPercentage;
        }

        public void AddToBasket(int itemId, int quantityToBuy)
        {
            AddToBasket(itemId, quantityToBuy, MinimumDiscountLimit);
        }

        public void AddToBasket(int itemId, int quantityToBuy, float extraDiscountPercentage = MinimumDiscountLimit)
        {
            if (ActiveUser.Basket.ContainsKey(itemId))
            {
                ActiveUser.Basket[itemId].Quantity += quantityToBuy;

                if (extraDiscountPercentage > ActiveUser.Basket[itemId].ExtraDiscountPercentage)
                {
                    ActiveUser.Basket[itemId].ExtraDiscountPercentage = extraDiscountPercentage;
                }

                return;
            }

            ActiveUser.AddItemToBasket(itemId, quantityToBuy, extraDiscountPercentage);
        }

        public void UpdateBasketItemQuantity(int itemId, int newQuantityToBuy)
        {
            ActiveUser.Basket[itemId].Quantity = newQuantityToBuy;

            if (ActiveUser.Basket[itemId].Quantity <= 0)
            {
                ActiveUser.RemoveItemFromBasket(itemId);
            }
        }

        public void RemoveFromBasket(int itemIdToRemove)
        {
            ActiveUser.RemoveItemFromBasket(itemIdToRemove);
        }

        public void CompleteOrder(int orderId, Dictionary<int, Tuple<int, float>> updatedQuantities)
        {
            Order orderToComplete = OrdersRepository.GetOrder(orderId);
            DateTime timeNow = DateTime.Now;
            DateOnly currentDate = new DateOnly(timeNow.Year, timeNow.Month, timeNow.Day);

            foreach (var itemQuantityEntry in updatedQuantities)
            {
                int currentItemId = itemQuantityEntry.Key;
                int preferredItemQuantity = itemQuantityEntry.Value.Item1;
                Item itemToVerify = ItemsRepository.GetItem(currentItemId);

                if (itemToVerify.QuantityAtSpecifiedDate(currentDate) < preferredItemQuantity)
                {
                    throw new ArgumentException("We don't have enough of " + itemToVerify.Name +
                        " - " + itemToVerify.Producer + "; " +
                        "delete the item from the order if you wish to complete it");
                }
            }

            orderToComplete.IsCompleted = true;

            foreach (var itemEntryInOrder in orderToComplete.ItemQuantitiesWithFinalPrice)
            {
                orderToComplete.RemoveItemFromOrder(itemEntryInOrder.Key);
            }

            foreach (var itemQuantityEntry in updatedQuantities)
            {
                orderToComplete.AddItemToOrder(itemQuantityEntry.Key,
                                                itemQuantityEntry.Value.Item1,
                                                itemQuantityEntry.Value.Item2);
            }

            OrdersRepository.UpdateOrder(orderToComplete);

            foreach (var itemQuantityEntry in updatedQuantities)
            {
                int currentItemId = itemQuantityEntry.Key;
                int itemQuantityToSubtract = itemQuantityEntry.Value.Item1;
                Item itemToUpdate = ItemsRepository.GetItem(currentItemId);

                itemToUpdate.RemoveQuantity(itemQuantityToSubtract, currentDate);
                ItemsRepository.UpdateItem(itemToUpdate);
            }
        }

        public void ModifyIncompleteOrder(int orderIdToModify,
            Dictionary<int, Tuple<int, float>> updatedQuantities,
            DateOnly updatedPickUpDate)
        {
            Order orderToModify = OrdersRepository.GetOrder(orderIdToModify);

            DateOnly todayDate = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            if (updatedPickUpDate <= todayDate)
            {
                throw new ArgumentException("The new pick-up date must be later than the current date");
            }

            foreach (var itemQuantityEntry in updatedQuantities)
            {
                int currentItemId = itemQuantityEntry.Key;
                int preferredItemQuantity = itemQuantityEntry.Value.Item1;
                Item itemToVerify = ItemsRepository.GetItem(currentItemId);

                if (itemToVerify.QuantityAtSpecifiedDate(updatedPickUpDate) < preferredItemQuantity)
                {
                    throw new ArgumentException("On " + updatedPickUpDate.ToString("yyyy.MM.dd") + ", " +
                        "we will have only " + itemToVerify.QuantityAtSpecifiedDate(updatedPickUpDate) +
                        " boxes of " + itemToVerify.Name + " - " + itemToVerify.Producer);
                }
            }

            orderToModify.PickUpDate = updatedPickUpDate;

            foreach (var itemEntryInOrder in orderToModify.ItemQuantitiesWithFinalPrice)
            {
                orderToModify.RemoveItemFromOrder(itemEntryInOrder.Key);
            }

            foreach (var itemQuantityEntry in updatedQuantities)
            {
                orderToModify.AddItemToOrder(itemQuantityEntry.Key,
                                             itemQuantityEntry.Value.Item1,
                                             itemQuantityEntry.Value.Item2);
            }

            OrdersRepository.UpdateOrder(orderToModify);
        }

        public void PlaceOrderFromBasket(DateOnly chosenPickUpDate)
        {
            Dictionary<int, Tuple<int, float>> itemInformationForOrder = new ();

            foreach (KeyValuePair<int, BasketEntry> basketItemEntry in ActiveUser.Basket)
            {
                Item currentItem = ItemsRepository.GetItem(basketItemEntry.Key);
                int currentItemQuantity = basketItemEntry.Value.Quantity;
                float extraDiscountAmount = NormalizeDiscount(basketItemEntry.Value.ExtraDiscountPercentage);

                int itemQuantityAtPickUpDate = currentItem.QuantityAtSpecifiedDate(chosenPickUpDate);

                if (currentItemQuantity > itemQuantityAtPickUpDate)
                {
                    throw new ArgumentException("On " + chosenPickUpDate.ToString("yyyy.MM.dd") + ", " +
                                                "we will have only " + itemQuantityAtPickUpDate + " boxes " +
                                                "of " + currentItem.Name + " by " + currentItem.Producer + " " +
                                                "instead of " + currentItemQuantity + ".");
                }

                float itemDiscountAmount = NormalizeDiscount(currentItem.DiscountPercentage);
                float userDiscountAmount = MinimumDiscountLimit;

                if (ActiveUser.UserDiscounts.ContainsKey(currentItem.Id))
                {
                    userDiscountAmount = NormalizeDiscount(ActiveUser.UserDiscounts[currentItem.Id]);
                }

                float finalPriceCalculation = currentItemQuantity * currentItem.Price;
                finalPriceCalculation *= (MaximumDiscountLimit - itemDiscountAmount);
                finalPriceCalculation *= (MaximumDiscountLimit - extraDiscountAmount);
                finalPriceCalculation *= (MaximumDiscountLimit - userDiscountAmount);

                itemInformationForOrder.Add(currentItem.Id, new Tuple<int, float>(currentItemQuantity, finalPriceCalculation));
            }

            OrdersRepository.AddOrderWithItems(ActiveUser.Id, chosenPickUpDate, itemInformationForOrder);
            ActiveUser.Basket.Clear();
        }

        public void ResubmitExpiredOrder(int orderIdToResubmit, DateOnly chosenPickUpDate)
        {
            Order expiredOrder = OrdersRepository.GetOrder(orderIdToResubmit);
            Dictionary<int, Tuple<int, float>> itemInformationForOrder = expiredOrder.ItemQuantitiesWithFinalPrice;

            foreach (KeyValuePair<int, Tuple<int, float>> orderItemEntry in itemInformationForOrder)
            {
                Item currentItem = ItemsRepository.GetItem(orderItemEntry.Key);
                int currentItemQuantity = orderItemEntry.Value.Item1;
                int itemQuantityAtPickUpDate = currentItem.QuantityAtSpecifiedDate(chosenPickUpDate);

                if (currentItemQuantity > itemQuantityAtPickUpDate)
                {
                    throw new ArgumentException("On " + chosenPickUpDate.ToString("yyyy.MM.dd") + ", " +
                                                "we will have only " + itemQuantityAtPickUpDate + " boxes " +
                                                "of " + currentItem.Name + " by " + currentItem.Producer + " " +
                                                "instead of " + currentItemQuantity + ".");
                }
            }

            OrdersRepository.AddOrderWithItems(ActiveUser.Id, chosenPickUpDate, itemInformationForOrder);
        }

        public void CancelOrder(int orderIdToCancel)
        {
            Order orderToCancel = OrdersRepository.GetOrder(orderIdToCancel);
            orderToCancel.IsExpired = true;
            OrdersRepository.UpdateOrder(orderToCancel);
        }

        public Dictionary<int, int> FillBasketFromPrescription(string prescriptionIdentifier)
        {
            if (!prescriptionIdentifier.Equals(ValidPrescriptionIdentifier))
            {
                throw new ArgumentException("Invalid prescription ID");
            }

            return ItemsRepository.GetCheapestPrescriptionItems(DefaultPrescriptionItemName, DefaultNumberOfRequiredPills);
        }
    }
}