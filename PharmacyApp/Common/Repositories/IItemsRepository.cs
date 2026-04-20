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

        // couldnt update quantity otherwise
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
        Dictionary<int, int> GetItemsFromPrescription(string prescriptionId, Dictionary<int, float> userDiscounts);

        public List<Tuple<int, string, int>> GetTop30Items();
    }
}
