using System;
using System.Collections.Generic;
using PharmacyApp.Models;

namespace PharmacyApp.Common.Services
{
    public interface IAdminService
    {
        List<Item> GetAllItems();
        List<Substance> GetAllSubstances();
        List<Item> SearchItemsByName(string query);
        Item GetItemById(int id);
        Substance GetSubstanceByName(string name);
        bool SubstanceExists(string name);

        void AddItem(Item newItem);
        void AddItemWithQuantity(Item newItem);
        void AddItemFromDetails(
            string name,
            string producer,
            string category,
            float price,
            int numberOfPills,
            string label,
            string description,
            string imagePath,
            float discount,
            Dictionary<string, float> activeSubstances,
            Dictionary<DateOnly, int> batches);
        void RemoveItemById(int id);
        void UpdateItemById(int id, Item updatedItem);
        void UpdateItemFromDetails(
            int id,
            string name,
            string producer,
            string category,
            float price,
            int numberOfPills,
            string label,
            string description,
            string imagePath,
            float discount,
            Dictionary<string, float> activeSubstances,
            Dictionary<DateOnly, int> batches);
        void AddSubstance(Substance newSubstance);
        void RemoveSubstanceByName(Substance substance);
        void UpdateSubstanceByName(string name, Substance substance);
        bool TryValidateActiveSubstance(
            string substanceName,
            float concentration,
            IReadOnlyDictionary<string, float> activeSubstances,
            out string errorMessage);
        bool TryValidateBatch(
            DateOnly expirationDate,
            int packs,
            DateOnly currentDate,
            out string errorMessage);
        void ValidateItemForAdd(Item item);
        List<Item> GetExpiredItems();
        Notification SendNewStockNotification(Item item);
        Notification SendAboutToExpireNotification();
        List<Notification> GetNotificationsForUser(User user);
        List<Tuple<int, string, int>> GetTop30Items();
        Dictionary<string, int> GetTop30Substances();
    }
}
