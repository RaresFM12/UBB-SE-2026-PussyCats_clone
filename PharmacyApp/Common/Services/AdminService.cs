using PharmacyApp.Common.Repositories;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PharmacyApp.Common.Services
{
    public class AdminService : IAdminService
    {
        private const int EmptyQuantity = 0;
        private const int MinPositiveValue = 1;

        private IItemsRepository itemRepository;
        private ISubstancesRepository substanceRepository;

        public AdminService()
        {
            this.itemRepository = new SQLItemsRepository();
            this.substanceRepository = new SQLSubstancesRepository();
        }
        public AdminService(IItemsRepository itemRepo, ISubstancesRepository substanceRepo)
        {
            this.itemRepository = itemRepo;
            this.substanceRepository = substanceRepo;
        }

        public void AddItem(Item newItem)
        {
            try
            {
                ValidateItemForAdd(newItem);
                itemRepository.AddItemWithQuantity(newItem.Name, newItem.Producer, newItem.Category,
                                       newItem.Price, newItem.NumberOfPills, newItem.Quantity, newItem.ActiveSubstances, newItem.Batches,
                                       newItem.Label, newItem.Description, newItem.ImagePath,
                                       newItem.DiscountPercentage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding item: {ex.Message}");
                return;

            }
        }

        public void AddItemWithQuantity(Item newItem)
        {
            try
            {
                ValidateItemForAdd(newItem);
                itemRepository.AddItemWithQuantity(newItem.Name, newItem.Producer, newItem.Category,
                                       newItem.Price, newItem.NumberOfPills,
                                       newItem.Quantity, newItem.ActiveSubstances, newItem.Batches,
                                       newItem.Label, newItem.Description, newItem.ImagePath,
                                       newItem.DiscountPercentage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding item: {ex.Message}");
                return;
            }
        }

        public void RemoveItem(int id)
        {
            itemRepository.RemoveItem(id);
        }

        public void UpdateItem(int id, Item updatedItem)
        {
            if (!itemRepository.ItemExists(id))
            {
                throw new ArgumentException("Item with the specified ID does not exist.");
            }

            Item previousItem = itemRepository.GetItem(id);
            if (previousItem.Quantity == EmptyQuantity && updatedItem.Quantity >= MinPositiveValue)
            {
                SendNewStockNotification(updatedItem);
            }
            updatedItem.Id = id;
            itemRepository.UpdateItem(updatedItem);
        }

        public void AddSubstance(Substance newSubstance)
        {
            substanceRepository.AddSubstance(newSubstance.Name, newSubstance.LethalDose, newSubstance.Description);
        }

        public void RemoveSubstance(Substance substance)
        {
            substanceRepository.RemoveSubstance(substance.Name);
        }

        public void UpdateSubstance(string name, Substance substance)
        {
            substanceRepository.UpdateSubstance(substance);
        }

        public Notification SendNewStockNotification(Item item)
        {
            string message = $"The item {item.Name} is back in stock with quantity {item.Quantity}," +
                $"number of pills {item.NumberOfPills!}," +
                $"producer {item.Producer}";
            Notification notification = new Notification("Stock Alert", "New item back in stock!");
            return notification;
        }

        public List<Item> GetExpiredItems()
        {
            List<Item> expiredItems = new List<Item>();
            List<Item> allItems = itemRepository.GetAllItems();
            DateOnly currentDate = DateOnly.FromDateTime(DateTime.Now);
            foreach (Item item in allItems)
            {
                foreach (var batch in item.Batches)
                {
                    if (batch.Key < currentDate)
                    {
                        expiredItems.Add(item);
                        break;
                    }
                }
            }
            SendAboutToExpireNotification();
            return expiredItems;
        }

        public Notification SendAboutToExpireNotification()
        {
            Notification notification = new Notification("Product Expired", "Some items have expired. Please check and remove them.");
            return notification;
        }

        public void ValidateItemForAdd(Item item)
        {
            if (item.Name == "" ||
                item.Producer == "" ||
                item.Price < MinPositiveValue ||
                item.NumberOfPills < MinPositiveValue ||
                item.Quantity < EmptyQuantity ||
                item.DiscountPercentage < EmptyQuantity ||
                item.ActiveSubstances.Count == EmptyQuantity)
            {
                throw new ArgumentException("Invalid item data. Please check the input and try again.");
            }
        }

        public List<Notification> GetNotificationsForUser(User user)
        {
            List<Notification> notifications = new List<Notification>();

            if (user.IsAdmin)
            {
                List<Item> items = itemRepository.GetAllItems();
                foreach (Item item in items)
                {
                    foreach (KeyValuePair<DateOnly, int> batch in item.Batches)
                    {
                        if (new DateTime(batch.Key.Year, batch.Key.Month, batch.Key.Day) <= DateTime.Today)
                        {
                            notifications.Add(new Notification("Product Expired", $"Product: {item.Id} expired. Please remove it", "Go to Products"));
                            break;
                        }
                    }
                }
            }

            foreach (int itemId in user.StockAlerts)
            {
                Item item = itemRepository.GetItem(itemId);
                if (item.Quantity >= MinPositiveValue)
                {
                    string concentrations = item.ActiveSubstances != null && item.ActiveSubstances.Any()
                        ? string.Join(", ", item.ActiveSubstances.Select(substance => $"{substance.Key} ({substance.Value})"))
                        : "None";
                    string body = $"{item.Name}, {item.NumberOfPills} pills, {concentrations}, {item.Producer}";
                    notifications.Add(new Notification("Stock Alert", body, "Go to products"));
                }
            }

            return notifications;
        }

        public List<Tuple<int, string, int>> GetTop30Items()
        {
            return itemRepository.GetTop30Items();
        }

        public Dictionary<string, int> GetTop20Substances()
        {
            return substanceRepository.GetTop20Substances();

        }
    }
}
