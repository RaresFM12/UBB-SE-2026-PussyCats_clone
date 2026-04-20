using Microsoft.Data.SqlClient;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PharmacyApp.Common.Repositories
{
    public class SQLItemsRepository : IItemsRepository
    {
        public SQLItemsRepository()
        {
        }

        public void AddItem(string name, string producer, string category,
            float price, int nrOfPills,
            string label = "", string description = "", string imagePath = "..\\..\\Assets\\placeholder.png",
            float discount = 0f)
        {
            string connString = SQLUtility.GetConnectionString();
            System.Diagnostics.Debug.WriteLine($"Connection string in SQLItemsRepository.AddItem: {connString}");
            string insertNewItemString =
                "INSERT INTO Items (name, price, category, numberOfPills, producer, imagePath, quantity, label, description, discountPercentage) " +
                $"VALUES ('{name}', {price}, '{category}', {nrOfPills}, '{producer}', '{imagePath}', 0, '{label}', '{description}', {discount})";

            using SqlConnection conn = new SqlConnection(connString);

            SqlCommand insertNewItemCommand = new SqlCommand(insertNewItemString, conn);

            conn.Open();
            insertNewItemCommand.ExecuteNonQuery();
            // from what I saw in the docs, the program leaving
            // the using block automatically closes the open connection
            // and disposes of it
        }

        // only used when adding batches from the ui, so we can set the initial quantity of the item in the store
        
        // actually modifies all tables not just the item tables
        public void AddItemWithQuantity(string name, string producer, string category,
            float price, int nrOfPills,
            int quantity, Dictionary<string, float> activeSubstances, Dictionary<DateOnly, int> batches,
            string label = "", string description = "", string imagePath = "..\\..\\Assets\\placeholder.png",
            float discount = 0f)
        {
            string connString = SQLUtility.GetConnectionString();
            System.Diagnostics.Debug.WriteLine($"Connection string in SQLItemsRepository.AddItemWithQuantity: {connString}");
            string insertNewItemString =
                "INSERT INTO Items (name, price, category, numberOfPills, producer, imagePath, quantity, label, description, discountPercentage) " +
                $"VALUES ('{name}', {price}, '{category}', {nrOfPills}, '{producer}', '{imagePath}', {quantity}, '{label}', '{description}', {discount})";

            using SqlConnection conn = new SqlConnection(connString);
            SqlCommand insertNewItemCommand = new SqlCommand(insertNewItemString, conn);
            conn.Open();
            insertNewItemCommand.ExecuteNonQuery();

            string insertActiveSubstancesString = $"INSERT INTO ItemSubstances (itemId, name, concentration) VALUES ";
            for (int i = 0; i < activeSubstances.Count; i++)
            {
                if(i== activeSubstances.Count - 1)
                    insertActiveSubstancesString +=
                        $"((SELECT MAX(itemId) FROM Items),'{activeSubstances.ElementAt(i).Key}', {activeSubstances.ElementAt(i).Value});";
                 else
                     insertActiveSubstancesString +=
                         $"((SELECT MAX(itemId) FROM Items),'{activeSubstances.ElementAt(i).Key}', {activeSubstances.ElementAt(i).Value}), ";
            }

            SqlCommand insertActiveSubstancesCommand = new SqlCommand(insertActiveSubstancesString, conn);
            insertActiveSubstancesCommand.ExecuteNonQuery();

            string insertBatchesString = $"INSERT INTO ItemExpirationDates (itemId, expirationDate, numberOfPacks) VALUES ";
            for(int i = 0;i < batches.Count; i++) {
                //string insertBatchExpirationDate = $"{batches.ElementAt(i).Key.Year}-{batches.ElementAt(i).Key.Month}-{batches.ElementAt(i).Key.Day}";
                if (i == batches.Count - 1)
                    insertBatchesString +=
                        $"((SELECT MAX(itemId) FROM Items), '{batches.ElementAt(i).Key}', {batches.ElementAt(i).Value});";
                else
                    insertBatchesString +=
                        $"((SELECT MAX(itemId) FROM Items), '{batches.ElementAt(i).Key}', {batches.ElementAt(i).Value}), ";
            }
            SqlCommand insertBatchesCommand = new SqlCommand(insertBatchesString, conn);
            insertBatchesCommand.ExecuteNonQuery();


        }

        public void RemoveItem(int idToBeRemoved)
        {
            string connString = SQLUtility.GetConnectionString();
            string deleteItemString = $"DELETE FROM Items WHERE itemId={idToBeRemoved}";
            string deleteActiveSubstancesCommandString = $"DELETE FROM ItemSubstances WHERE itemId = {idToBeRemoved}";
            string deleteBatchesCommandString = $"DELETE FROM ItemExpirationDates WHERE itemId = {idToBeRemoved}";
            string deleteItemsFromOrdersCommandString = $"DELETE FROM OrderItems WHERE itemId = {idToBeRemoved}";
            string deleteUserNotificationsCommandString = $"DELETE FROM UserNotifications WHERE itemId = {idToBeRemoved}";
            string deleteUserDiscountsCommandString = $"DELETE FROM UserDiscounts WHERE itemId = {idToBeRemoved}";

            using SqlConnection conn = new SqlConnection(connString);

            conn.Open();

            // we have to delete
            // 
            // the active substances from ItemSubstances, 
            // the batches of the item from ItemExpirationDates
            // and the references to the item from OrderItems, UserNotifications and UserDiscounts
            // 
            // before removing the item itself (because of foreign key constraints)

            SqlCommand deleteActiveSubstancesCommand = new SqlCommand(deleteActiveSubstancesCommandString, conn);
            deleteActiveSubstancesCommand.ExecuteNonQuery();

            SqlCommand deleteBatchesCommand = new SqlCommand(deleteBatchesCommandString, conn);
            deleteBatchesCommand.ExecuteNonQuery();

            SqlCommand deleteItemsFromOrdersCommand = new SqlCommand(deleteItemsFromOrdersCommandString, conn);
            deleteItemsFromOrdersCommand.ExecuteNonQuery();

            SqlCommand deleteUserNotificationsCommand = new SqlCommand(deleteUserNotificationsCommandString, conn);
            deleteUserNotificationsCommand.ExecuteNonQuery();

            SqlCommand deleteUserDiscountsCommand = new SqlCommand(deleteUserDiscountsCommandString, conn);
            deleteUserDiscountsCommand.ExecuteNonQuery();

            SqlCommand deleteItemCommand = new SqlCommand(deleteItemString, conn);
            deleteItemCommand.ExecuteNonQuery();
        }

        public Item GetItem(int id)
        {
            // we have to get the active substances and the batches on the particular
            // item as well to create the item fully

            string connString = SQLUtility.GetConnectionString();
            string selectItemString = $"SELECT * FROM Items WHERE itemId={id}";
            string selectActiveSubstances = $"SELECT name, concentration FROM ItemSubstances WHERE itemId={id}";
            string selectBatches = $"SELECT expirationDate, numberOfPacks FROM ItemExpirationDates WHERE itemId={id}";

            using SqlConnection conn = new SqlConnection(connString);
            
            SqlDataAdapter itemAdapter = new SqlDataAdapter(selectItemString, conn);
            SqlDataAdapter activeSubstancesAdapter = new SqlDataAdapter(selectActiveSubstances, conn);
            SqlDataAdapter batchesAdapter = new SqlDataAdapter(selectBatches, conn);
            DataSet itemDataFromDb = new DataSet();

            conn.Open();
            itemAdapter.Fill(itemDataFromDb, "Items");
            activeSubstancesAdapter.Fill(itemDataFromDb, "ActiveSubstances");
            batchesAdapter.Fill(itemDataFromDb, "Batches");

            // we should expect only one row as result (cuz id is unique for all)

            DataRow resultRow = itemDataFromDb.Tables["Items"].Rows[0];


            // that conversion from object{decimal} to decimal then to float isn't pretty (for price and discount)

            Item resultItem = new Item(
                (int)               resultRow["itemId"], 
                (string)            resultRow["name"],
                (string)            resultRow["producer"],
                (string)            resultRow["category"], 
                (float)(decimal)    resultRow["price"], 
                (int)               resultRow["numberOfPills"],
                (string)            resultRow["label"], 
                (string)            resultRow["description"],
                (string)            resultRow["imagePath"], 
                (float)(decimal)    resultRow["discountPercentage"]);


            // inserting the active substances and batches for the particular item one by one

            foreach (DataRow substanceRow in itemDataFromDb.Tables["ActiveSubstances"].Rows)
            {
                resultItem.addActiveSubstance((string)substanceRow["name"],
                    (float)(decimal)substanceRow["concentration"]);
            }

            foreach (DataRow batchRow in itemDataFromDb.Tables["Batches"].Rows)
            {
                DateOnly extractedExpirationDate = DateOnly.FromDateTime((DateTime)batchRow["expirationDate"]);
                resultItem.addNewBatch(extractedExpirationDate, (int)batchRow["numberOfPacks"]);
            }

            return resultItem;
            
        }


        public List<Item> GetAllItems()
        {
            string connString = SQLUtility.GetConnectionString();
            List<Item> resultItems = new List<Item>();

            string selectItemString = $"SELECT * FROM Items";

            using SqlConnection conn = new SqlConnection(connString);
            SqlDataAdapter itemAdapter = new SqlDataAdapter(selectItemString, conn);
            DataSet itemDataFromDb = new DataSet();

            conn.Open();
            itemAdapter.Fill(itemDataFromDb, "Items");

            // for each item that we get by name, we have repeat basically what happens in getItem(int id)

            foreach (DataRow itemRow in itemDataFromDb.Tables["Items"].Rows)
            {
                // that conversion from object{decimal} to decimal then to float isn't pretty (for price and discount)

                Item individualItem = new Item(
                    (int)itemRow["itemId"],
                    (string)itemRow["name"],
                    (string)itemRow["producer"],
                    (string)itemRow["category"],
                    (float)(decimal)itemRow["price"],
                    (int)itemRow["numberOfPills"],
                    (string)itemRow["label"],
                    (string)itemRow["description"],
                    (string)itemRow["imagePath"],
                    (float)(decimal)itemRow["discountPercentage"],
                    (int)itemRow["quantity"]);
                    


                // for every item we need to get its substances and batches

                string selectActiveSubstances =
                    $"SELECT name, concentration FROM ItemSubstances WHERE itemId={individualItem.Id}";
                string selectBatches =
                    $"SELECT expirationDate, numberOfPacks FROM ItemExpirationDates WHERE itemId={individualItem.Id}";
                SqlDataAdapter activeSubstancesAdapter = new SqlDataAdapter(selectActiveSubstances, conn);
                SqlDataAdapter batchesAdapter = new SqlDataAdapter(selectBatches, conn);

                DataSet individualItemDataFromDb = new DataSet();
                activeSubstancesAdapter.Fill(individualItemDataFromDb, "ActiveSubstances");
                batchesAdapter.Fill(individualItemDataFromDb, "Batches");

                foreach (DataRow substanceRow in individualItemDataFromDb.Tables["ActiveSubstances"].Rows)
                {
                    individualItem.addActiveSubstance((string)substanceRow["name"],
                        (float)(decimal)substanceRow["concentration"]);
                }

                foreach (DataRow batchRow in individualItemDataFromDb.Tables["Batches"].Rows)
                {
                    DateOnly extractedExpirationDate = DateOnly.FromDateTime((DateTime)batchRow["expirationDate"]);
                    individualItem.addNewBatch(extractedExpirationDate, (int)batchRow["numberOfPacks"]);
                }


                resultItems.Add(individualItem);
            }

            return resultItems;
        }


        public List<Item> GetItemsByName(string name)
        {
            string connString = SQLUtility.GetConnectionString();
            List<Item> resultItems = new List<Item>();

            string selectItemString = $"SELECT * FROM Items WHERE name='{name}'";

            using SqlConnection conn = new SqlConnection(connString);
            SqlDataAdapter itemAdapter = new SqlDataAdapter(selectItemString, conn);
            DataSet itemDataFromDb = new DataSet();

            conn.Open();
            itemAdapter.Fill(itemDataFromDb, "Items");

            // for each item that we get by name, we have repeat basically what happens in getItem(int id)

            foreach (DataRow itemRow in itemDataFromDb.Tables["Items"].Rows)
            {
                // that conversion from object{decimal} to decimal then to float isn't pretty (for price and discount)

                Item individualItem = new Item(
                    (int)               itemRow["itemId"], 
                    (string)            itemRow["name"],
                    (string)            itemRow["producer"],
                    (string)            itemRow["category"], 
                    (float)(decimal)    itemRow["price"], 
                    (int)               itemRow["numberOfPills"],
                    (string)            itemRow["label"], 
                    (string)            itemRow["description"],
                    (string)            itemRow["imagePath"], 
                    (float)(decimal)    itemRow["discountPercentage"]);


                // for every item we need to get its substances and batches

                string selectActiveSubstances =
                    $"SELECT name, concentration FROM ItemSubstances WHERE itemId={individualItem.Id}";
                string selectBatches =
                    $"SELECT expirationDate, numberOfPacks FROM ItemExpirationDates WHERE itemId={individualItem.Id}";
                SqlDataAdapter activeSubstancesAdapter = new SqlDataAdapter(selectActiveSubstances, conn);
                SqlDataAdapter batchesAdapter = new SqlDataAdapter(selectBatches, conn);

                DataSet individualItemDataFromDb = new DataSet();
                activeSubstancesAdapter.Fill(individualItemDataFromDb, "ActiveSubstances");
                batchesAdapter.Fill(individualItemDataFromDb, "Batches");

                foreach (DataRow substanceRow in individualItemDataFromDb.Tables["ActiveSubstances"].Rows)
                {
                    individualItem.addActiveSubstance((string)substanceRow["name"],
                        (float)(decimal)substanceRow["concentration"]);
                }

                foreach (DataRow batchRow in individualItemDataFromDb.Tables["Batches"].Rows)
                {
                    DateOnly extractedExpirationDate = DateOnly.FromDateTime((DateTime)batchRow["expirationDate"]);
                    individualItem.addNewBatch(extractedExpirationDate, (int)batchRow["numberOfPacks"]);
                }


                resultItems.Add(individualItem);
            }

            return resultItems;
        }

        public void UpdateItem(Item newItem)
        {
            string connString = SQLUtility.GetConnectionString();
            string updateItemString = $"UPDATE Items " +
                                      $"SET name = '{newItem.Name}', " +
                                      $"price = {newItem.Price}, " +
                                      $"category = '{newItem.Category}', " +
                                      $"numberOfPills = {newItem.NumberOfPills}, " +
                                      $"producer = '{newItem.Producer}', " +
                                      $"imagePath = '{newItem.ImagePath}', " +
                                      $"quantity = {newItem.Quantity}, " +
                                      $"label = '{newItem.Label}', " +
                                      $"description = '{newItem.Description}', " +
                                      $"discountPercentage = {newItem.DiscountPercentage} " +
                                      $"WHERE itemId = {newItem.Id}";

            using SqlConnection conn = new SqlConnection(connString);

            // this command updates ONLY the Items table

            conn.Open();
            SqlCommand updateItemCommand = new SqlCommand(updateItemString, conn);
            updateItemCommand.ExecuteNonQuery();


            // we still have to update the ItemSubstances and ItemExpirationDates
            // I thought about just deleting the old entries in both tables associated
            // with the given item id, and putting the updated entries into the tables
            // looks a bit janky ngl, might be changed later

            // updating ItemSubstances for the specific item

            string deleteActiveSubstancesCommandString = $"DELETE FROM ItemSubstances WHERE itemId = {newItem.Id}";
            SqlCommand deleteActiveSubstancesCommand = new SqlCommand(deleteActiveSubstancesCommandString, conn);
            deleteActiveSubstancesCommand.ExecuteNonQuery();

            foreach (KeyValuePair<string, float> activeSubstance in newItem.ActiveSubstances)
            {
                string insertActiveSubstanceCommandString =
                    $"INSERT INTO ItemSubstances (itemId, name, concentration) " +
                    $"VALUES ({newItem.Id}, '{activeSubstance.Key}', {activeSubstance.Value})";
                SqlCommand insertActiveSubstanceCommand = new SqlCommand(insertActiveSubstanceCommandString, conn);
                insertActiveSubstanceCommand.ExecuteNonQuery();
            }


            // updating ItemExpirationDates for the specific item

            string deleteBatchesCommandString = $"DELETE FROM ItemExpirationDates WHERE itemId = {newItem.Id}";
            SqlCommand deleteBatchesCommand = new SqlCommand(deleteBatchesCommandString, conn);
            deleteBatchesCommand.ExecuteNonQuery();

            foreach (KeyValuePair<DateOnly, int> batch in newItem.Batches)
            {
                string insertBatchExpirationDate = $"{batch.Key.Year}-{batch.Key.Month}-{batch.Key.Day}";
                string insertBatchCommandString =
                    $"INSERT INTO ItemExpirationDates (itemId, expirationDate, numberOfPacks) " +
                    $"VALUES ({newItem.Id}, '{insertBatchExpirationDate}', {batch.Value})";
                SqlCommand insertBatchCommand = new SqlCommand(insertBatchCommandString, conn);
                insertBatchCommand.ExecuteNonQuery();
            }
        }

        public bool ItemExists(int id)
        {
            string connString = SQLUtility.GetConnectionString();
            string selectQueryString = $"SELECT * FROM Items WHERE itemId={id}";

            using SqlConnection conn = new SqlConnection(connString);

            SqlDataAdapter itemsAdapter = new SqlDataAdapter(selectQueryString, conn);
            DataSet items = new DataSet();

            conn.Open();
            itemsAdapter.Fill(items, "Items");

            if (items.Tables["Items"].Rows.Count > 0)
                return true;

            return false;
        }

        public List<Tuple<int, string, int>> GetTop30Items()
        {
            string connString = SQLUtility.GetConnectionString();
            List<Tuple<int, string, int>> resultItems = new List<Tuple<int, string, int>>();
            string selectItemString = $"SELECT TOP 30 i.itemId, i.name, COUNT(orderId) as nbOrders FROM Items i INNER JOIN OrderItems oi ON i.itemId=oi.itemId GROUP BY i.itemId, i.name ORDER BY COUNT(orderId) DESC";
            using SqlConnection conn = new SqlConnection(connString);
            SqlDataAdapter itemAdapter = new SqlDataAdapter(selectItemString, conn);
            DataSet itemDataFromDb = new DataSet();
            conn.Open();
            itemAdapter.Fill(itemDataFromDb, "Items");
            foreach (DataRow itemRow in itemDataFromDb.Tables["Items"].Rows)
            {
                int itemId = (int)itemRow["itemId"];
                string name = (string)itemRow["name"];
                int nbOrders = (int)itemRow["nbOrders"];

                resultItems.Add(new Tuple<int, string, int>(itemId, name, nbOrders));
            }
            return resultItems;
        }

        private static float NormalizeDiscount(float discount)
        {
            if (discount > 1f)
                discount /= 100f;

            if (discount < 0f)
                return 0f;

            if (discount > 1f)
                return 1f;

            return discount;
        }

        public Dictionary<int, int> GetItemsFromPrescription(string prescriptionId, Dictionary<int, float> userDiscounts)
        {
            Dictionary<int, int> items = new();

            if (string.IsNullOrWhiteSpace(prescriptionId) || !prescriptionId.Equals("testPrescription"))
                throw new ArgumentException("Invalid prescription ID");

            string itemName = "Nurofen Express";
            int nrOfRequiredPills = 40;
            userDiscounts ??= new Dictionary<int, float>();

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

            List<Item> preferredItems = GetItemsByName(itemName);

            if (preferredItems.Count == 0)
                throw new ArgumentException("Medicine couldn't be retrieved");

            Item preferredItem = preferredItems[0];
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
                    Item currItem = GetItem(currItemID);

                    if (currItem.ActiveSubstances.Count == numberOfRequiredSubstances &&
                        currItem.Quantity != 0)
                    {
                        float initialPrice = currItem.Price;
                        float itemDiscount = NormalizeDiscount(currItem.DiscountPercentage);
                        float userDiscount = 0f;

                        if (userDiscounts.ContainsKey(currItem.Id))
                            userDiscount = NormalizeDiscount(userDiscounts[currItem.Id]);

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
                    if (GetItem(cheapestItemID).Quantity != 0)
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
                    Item currItem = GetItem(currItemID);

                    if (currItem.ActiveSubstances.Count == numberOfRequiredSubstances &&
                        currItem.Quantity != 0)
                    {
                        int multiplier = (int)Math.Ceiling((double)nrOfRequiredPills / currItem.NumberOfPills);

                        if (currItem.Quantity < multiplier)
                            continue;

                        float itemDiscount = NormalizeDiscount(currItem.DiscountPercentage);
                        float userDiscount = 0f;

                        if (userDiscounts.ContainsKey(currItem.Id))
                            userDiscount = NormalizeDiscount(userDiscounts[currItem.Id]);

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