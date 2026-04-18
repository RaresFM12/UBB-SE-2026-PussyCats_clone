using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
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

        public void AddToBasket(int itemId, int quantityToBuy)
        {
            AddToBasket(itemId, quantityToBuy, 0f);
        }

        public void AddToBasket(int itemId, int quantityToBuy, float extraDiscountPercentage = 0f)
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

        public Dictionary<int, int> FillBasketFromPrescription(string prescriptionId)
        {
            Dictionary<int, int> items = new();

            if (!prescriptionId.Equals("testPrescription"))
                throw new ArgumentException("Invalid prescription ID");

            string itemName = "prescript1";
            int nrOfRequiredPills = 40;

            Dictionary<string, float> searchedActiveSubstances = new();
            string connString = SQLUtility.GetConnectionString();
            string selectExactItemsCommandString =
                $"SELECT * FROM Items " +
                $"WHERE name = '{itemName}' " +
                $"AND numberOfPills = {nrOfRequiredPills} " +
                $"ORDER BY price";

            DataSet resultsAcrossQueries = new();

            using SqlConnection conn = new(connString);
            SqlDataAdapter exactFinderAdapter = new(selectExactItemsCommandString, conn);

            conn.Open();
            exactFinderAdapter.Fill(resultsAcrossQueries, "ExactNameAndPills");

            Item preferredItem = ItemsRepository.GetItemsByName(itemName)[0];
            int numberOfRequiredSubstances = preferredItem.ActiveSubstances.Count;

            if (resultsAcrossQueries.Tables["ExactNameAndPills"].Rows.Count != 0)
            {
                DataRow entryRow = resultsAcrossQueries.Tables["ExactNameAndPills"].Rows[0];
                if ((int)entryRow["quantity"] != 0)
                {
                    items.Add((int)entryRow["itemId"], 1);
                    return items;
                }
            }

            string selectExactSubstitutesCommandString =
                "SELECT * FROM Items I " +
                "WHERE I.itemId IN (" +
                    "SELECT DISTINCT ISub.itemId " +
                    "FROM ItemSubstances ISub " +
                    "WHERE NOT EXISTS ( " +
                        "(SELECT ISub1.name, ISub1.concentration FROM ItemSubstances ISub1 " +
                        "INNER JOIN Items I ON ISub1.itemId = I.itemId " +
                        $"WHERE I.name = '{itemName}') " +
                        "EXCEPT " +
                        "(SELECT ISub2.name, ISub2.concentration FROM ItemSubstances ISub2 " +
                        "WHERE ISub.itemId = ISub2.itemId)" +
                    ")" +
                $") AND I.numberOfPills = {nrOfRequiredPills} " +
                "ORDER BY I.price";

            SqlDataAdapter substituteFinderAdapter = new(selectExactSubstitutesCommandString, conn);
            substituteFinderAdapter.Fill(resultsAcrossQueries, "Substitutes");

            if (resultsAcrossQueries.Tables["Substitutes"].Rows.Count != 0)
            {
                int cheapestItemID = -1;
                float cheapestPrice = 99999999f;

                foreach (DataRow substituteCandidateEntry in resultsAcrossQueries.Tables["Substitutes"].Rows)
                {
                    int currItemID = (int)substituteCandidateEntry["itemId"];
                    Item currItem = ItemsRepository.GetItem(currItemID);

                    if (currItem.ActiveSubstances.Count == numberOfRequiredSubstances &&
                        currItem.Quantity != 0)
                    {
                        float initialPrice = currItem.Price;
                        float itemDiscount = NormalizeDiscount(currItem.DiscountPercentage);
                        float userDiscount = 0f;

                        if (ActiveUser.UserDiscounts.ContainsKey(currItem.Id))
                            userDiscount = NormalizeDiscount(ActiveUser.UserDiscounts[currItem.Id]);

                        float finalPrice = initialPrice * (1 - itemDiscount) * (1 - userDiscount);

                        if (finalPrice < cheapestPrice)
                        {
                            cheapestPrice = finalPrice;
                            cheapestItemID = currItem.Id;
                        }
                    }
                }

                if (cheapestItemID != -1)
                {
                    if (ItemsRepository.GetItem(cheapestItemID).Quantity != 0)
                    {
                        items.Add(cheapestItemID, 1);
                        return items;
                    }
                }
            }

            string selectMultipliedSubstitutesCommandString =
                "SELECT * FROM Items I " +
                "WHERE I.itemId IN (" +
                    "SELECT DISTINCT ISub.itemId " +
                    "FROM ItemSubstances ISub " +
                    "WHERE NOT EXISTS ( " +
                        "(SELECT ISub1.name, ISub1.concentration FROM ItemSubstances ISub1 " +
                        "INNER JOIN Items I ON ISub1.itemId = I.itemId " +
                        $"WHERE I.name = '{itemName}') " +
                        "EXCEPT " +
                        "(SELECT ISub2.name, ISub2.concentration FROM ItemSubstances ISub2 " +
                        "WHERE ISub.itemId = ISub2.itemId)" +
                    ")" +
                $") AND I.numberOfPills < {nrOfRequiredPills} " +
                "ORDER BY I.price";

            SqlDataAdapter multipliedSubstituteFinderAdapter = new(selectMultipliedSubstitutesCommandString, conn);
            multipliedSubstituteFinderAdapter.Fill(resultsAcrossQueries, "Multiplies");

            if (resultsAcrossQueries.Tables["Multiplies"].Rows.Count != 0)
            {
                int cheapestItemId = -1;
                int cheapestItemQuantity = -1;
                float cheapestPrice = 10000000f;

                foreach (DataRow substituteCandidateEntry in resultsAcrossQueries.Tables["Multiplies"].Rows)
                {
                    int currItemID = (int)substituteCandidateEntry["itemId"];
                    Item currItem = ItemsRepository.GetItem(currItemID);

                    if (currItem.ActiveSubstances.Count == numberOfRequiredSubstances &&
                        currItem.Quantity != 0)
                    {
                        int multiplier = (int)Math.Ceiling((double)nrOfRequiredPills / currItem.NumberOfPills);

                        if (currItem.Quantity < multiplier)
                            continue;

                        float itemDiscount = NormalizeDiscount(currItem.DiscountPercentage);
                        float userDiscount = 0f;

                        if (ActiveUser.UserDiscounts.ContainsKey(currItem.Id))
                            userDiscount = NormalizeDiscount(ActiveUser.UserDiscounts[currItem.Id]);

                        float finalPrice = currItem.Price * multiplier * (1 - itemDiscount) * (1 - userDiscount);

                        if (finalPrice < cheapestPrice)
                        {
                            cheapestPrice = finalPrice;
                            cheapestItemId = currItem.Id;
                            cheapestItemQuantity = multiplier;
                        }
                    }
                }

                if (cheapestItemId != -1 && cheapestItemQuantity != -1)
                {
                    items.Add(cheapestItemId, cheapestItemQuantity);
                    return items;
                }
            }

            throw new ArgumentException("Medicine couldn't be retrieved");
        }
    }
}