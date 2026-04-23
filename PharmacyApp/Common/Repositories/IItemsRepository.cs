using System;
using System.Collections.Generic;
using PharmacyApp.Models;

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

        void RemoveItemById(int idToBeRemoved);
        Item GetItemById(int id);
        List<Item> GetAllItems();
        List<Item> GetItemsByName(string name);
        void UpdateItemById(Item newItem);
        bool ItemExists(int id);
        List<Tuple<int, string, int>> GetTop30Items();
        Dictionary<int, int> GetItemsFromPrescription(string prescriptionId, Dictionary<int, float> userDiscounts);
        Dictionary<int, int> GetCheapestPrescriptionItems(string prescriptionName, int requiredPills);
    }
}