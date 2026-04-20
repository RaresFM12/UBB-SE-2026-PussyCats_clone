using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;
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

            foreach (KeyValuePair<int, BasketEntry> item in ActiveUser.Basket)
                basketItems.Add(BuildBasketItemViewModel(item.Key, item.Value));

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
            Dictionary<int, int> items = new();

            if (!prescriptionId.Equals("testPrescription"))
                throw new ArgumentException("Invalid prescription ID");

            string itemName = "prescript1";
            int nrOfRequiredPills = 40;

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
