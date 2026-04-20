using System;
using System;
using System.Collections.Generic;
using PharmacyApp.Common.Repositories;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Models;

namespace PharmacyApp.Features.Orders.Logic
{
    public class OrderService : IOrderService
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
            if (discount > 1f)
                discount /= 100f;

            if (discount < 0f)
                return 0f;

            if (discount > 1f)
                return 1f;

            return discount;
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
                float userDiscount = 0f;

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

    }
}