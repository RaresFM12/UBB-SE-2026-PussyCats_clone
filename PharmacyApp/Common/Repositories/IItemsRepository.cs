using PharmacyApp.Models;
using System;
using System.Collections.Generic;

namespace PharmacyApp.Common.Repositories
{
    public interface IItemsRepository
    {
        void AddItem(string name, string producer, string category,
                    float price, int nrOfPills,
                    string label = "", string description = "", string imagePath = "..\\..\\Assets\\placeholder.png",
                    float discount = 0f);

        void AddItemWithQuantity(string name, string producer, string category,
                    float price, int nrOfPills,
                    int quantity, Dictionary<string, float> activeSubstances, Dictionary<DateOnly, int> batches,
                    string label = "", string description = "", string imagePath = "..\\..\\Assets\\placeholder.png",
                    float discount = 0f);

        void RemoveItem(int idToBeRemoved);
        Item GetItem(int id);
        List<Item> GetAllItems();
        List<Item> GetItemsByName(string name);
        void UpdateItem(Item newItem);
        bool ItemExists(int id);
        List<Tuple<int, string, int>> GetTop30Items();

        Dictionary<int, int> GetCheapestPrescriptionItems(string prescriptionName, int requiredPills);
    }
}