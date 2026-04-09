using System;
using System.Collections.Generic;
using PharmacyApp.Models;

namespace PharmacyApp.Common.Services
{
    public interface IAdminService
    {
        void AddItem(Item newItem);
        void AddItemWithQuantity(Item newItem);
        void RemoveItem(int id);
        void UpdateItem(int id, Item updatedItem);
        void AddSubstance(Substance newSubstance);
        void RemoveSubstance(Substance substance);
        void UpdateSubstance(string name, Substance substance);
        void ValidateItemForAdd(Item item);
        List<Item> GetExpiredItems();
        Notification SendNewStockNotification(Item item);
        Notification SendAboutToExpireNotification();
        List<Notification> GetNotificationsForUser(User user);
        List<Tuple<int, string, int>> GetTop30Items();
        Dictionary<string, int> GetTop20Substances();
    }
}
