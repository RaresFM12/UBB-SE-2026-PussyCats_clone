using System;
using System.Collections.Generic;
using System.Linq;

namespace PharmacyApp.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Producer { get; set; }
        public float Price { get; set; }
        public string Category { get; set; }
        // should be a relative path to the Assets folder
        public string ImagePath { get; set; }
        public int NumberOfPills { get; set; }
        // Quantity's setter is private, because we aren't supposed to modify it directly
        public int Quantity { get; private set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public float DiscountPercentage { get; set; }
        public Dictionary<string, float> ActiveSubstances { get; set; }
        public Dictionary<DateOnly, int> Batches { get; set; }

        // I changed the constructor, so that we don't have a way to
        // add the quantity directly, we have to wait for the batches to modify it
        // so DON'T FORGET TO CALL AddNewBatchToItem() to get the correct result
        public Item(int id, string name, string producer, string category,
                    float price, int numberOfPills,
                    string label = "", string description = "", string imagePath = "..\\..\\Assets\\placeholder.png",
                    float discount = 0f)
        {
            Id = id;
            Name = name;
            Producer = producer;
            Price = price;
            NumberOfPills = numberOfPills;
            Category = category;
            ImagePath = imagePath;
            Quantity = 0;
            Label = label;
            Description = description;
            DiscountPercentage = discount;
            ActiveSubstances = new Dictionary<string, float>();
            Batches = new Dictionary<DateOnly, int>();
        }
        public Item(int id, string name, string producer, string category,
                    float price, int numberOfPills,
                    string label = "", string description = "", string imagePath = "..\\..\\Assets\\placeholder.png",
                    float discount = 0f, int quantity = 0)
        {
            Id = id;
            Name = name;
            Producer = producer;
            Price = price;
            NumberOfPills = numberOfPills;
            Category = category;
            ImagePath = imagePath;
            Quantity = quantity;
            Label = label;
            Description = description;
            DiscountPercentage = discount;
            ActiveSubstances = new Dictionary<string, float>();
            Batches = new Dictionary<DateOnly, int>();
        }

        public Item(string name, string producer, string category,
            float price, int numberOfPills,
            int quantity = 0,
            string label = "", string description = "", string imagePath = "..\\..\\Assets\\placeholder.png",
            float discount = 0f)
        {
            Name = name;
            Producer = producer;
            Price = price;
            NumberOfPills = numberOfPills;
            Category = category;
            ImagePath = imagePath;
            Quantity = quantity;
            Label = label;
            Description = description;
            DiscountPercentage = discount;
            ActiveSubstances = new Dictionary<string, float>();
            Batches = new Dictionary<DateOnly, int>();
        }

        public Item(string name, string producer, string category,
                    float price, int numberOfPills,
                    Dictionary<string, float> activeSubstances, Dictionary<DateOnly, int> batches,
                    int quantity = 0,
                    string label = "", string description = "", string imagePath = "..\\..\\Assets\\placeholder.png",
                    float discount = 0f)
        {
            Name = name;
            Producer = producer;
            Price = price;
            NumberOfPills = numberOfPills;
            Category = category;
            ImagePath = imagePath;
            Quantity = quantity;
            Label = label;
            Description = description;
            DiscountPercentage = discount;
            ActiveSubstances = activeSubstances;
            Batches = batches;
        }

        public void AddActiveSubstanceToItem(string newSubstanceName, float concentration)
        {
            if (ActiveSubstances.ContainsKey(newSubstanceName))
            {
                throw new ArgumentException(newSubstanceName + "is already inside the medication");
            }

            ActiveSubstances[newSubstanceName] = concentration;
        }

        public void ChangeActiveSubstanceConcentration(string newSubstanceName, float newConcentration)
        {
            if (!ActiveSubstances.ContainsKey(newSubstanceName))
            {
                throw new ArgumentException(newSubstanceName + "is not inside the medication");
            }

            ActiveSubstances[newSubstanceName] = newConcentration;
        }

        public void RemoveActiveSubstanceFromItem(string substanceName)
        {
            if (!ActiveSubstances.ContainsKey(substanceName))
            {
                throw new ArgumentException(substanceName + "is not inside the medication");
            }

            ActiveSubstances.Remove(substanceName);
        }

        public void AddNewBatchToItem(DateOnly newExpirationDate, int nrOfPacks)
        {
            if (Batches.ContainsKey(newExpirationDate))
            {
                Batches[newExpirationDate] += nrOfPacks;
                Quantity += nrOfPacks;
                return;
            }

            Batches[newExpirationDate] = nrOfPacks;

            // because we aren't supposed to modify Quantity directly
            // and let the batches automatically modify the Quantity property
            this.Quantity += nrOfPacks;
        }

        public void ChangeNumberOfPacksForBatch(DateOnly expirationDate, int newNrOfPacks)
        {
            // we need to know the difference between the old and new number of packs
            // to update the quantity accordingly
            int oldNrOfPacks = Batches[expirationDate];

            if (!Batches.ContainsKey(expirationDate))
            {
                throw new ArgumentException("A batch with expiration date " + expirationDate.ToString() + " doesn't exist");
            }

            Batches[expirationDate] = newNrOfPacks;
            Quantity += (newNrOfPacks - oldNrOfPacks);
        }

        public void RemoveBatchFromItem(DateOnly expirationDate)
        {
            if (!Batches.ContainsKey(expirationDate))
            {
                throw new ArgumentException("A batch with expiration date " + expirationDate.ToString() + " doesn't exist");
            }

            Quantity -= Batches[expirationDate];
            Batches.Remove(expirationDate);
        }

        // no error checking here cuz this is called only when we
        // already know that we can remove this amount
        public void RemoveQuantityFromItem(int quantityToRemove, DateOnly dateAfter)
        {
            // get dates that have batches associated with it
            List<DateOnly> sortedExpirationDates = Batches.Keys.ToList<DateOnly>();
            sortedExpirationDates.Sort();

            int indexForDate = 0;
            int remainingQuantity = quantityToRemove;
            while (remainingQuantity > 0)
            {
                // we skip the batches that are already expired
                if (sortedExpirationDates[indexForDate] < dateAfter)
                {
                    indexForDate++;
                    continue;
                }

                // if the batch has less than the remaining quantity
                // to remove then the whole batch is removed
                if (remainingQuantity > Batches[sortedExpirationDates[indexForDate]])
                {
                    remainingQuantity -= Batches[sortedExpirationDates[indexForDate]];
                    RemoveBatchFromItem(sortedExpirationDates[indexForDate]);
                    indexForDate++;
                    continue;
                }

                // we only remove part of the batch (as the remaining
                // quantity to be removed goes to zero)
                int newBatchQuantity = Batches[sortedExpirationDates[indexForDate]] - remainingQuantity;
                ChangeNumberOfPacksForBatch(sortedExpirationDates[indexForDate], newBatchQuantity);
                remainingQuantity = 0;
                indexForDate++;
            }
        }

        public int GetQuantityAtSpecifiedDate(DateOnly date)
        {
            int validatedQuantity = 0;

            foreach (KeyValuePair<DateOnly, int> batchEntry in Batches)
            {
                DateOnly currentBatchExpirationDate = batchEntry.Key;
                int currentBatchQuantity = batchEntry.Value;

                if (date < currentBatchExpirationDate)
                {
                    validatedQuantity += currentBatchQuantity;
                }
            }

            return validatedQuantity;
        }
    }
}
