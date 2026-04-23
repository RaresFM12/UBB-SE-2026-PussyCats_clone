using System;
using System.Collections.Generic;

namespace PharmacyApp.Models
{
    public class Order : IEquatable<Order>
    {
        public const int OrderExpirationDays = 7;

        public int Id { get; private set; }
        public string IdString => "Order#" + Id;
        public int ClientId { get; set; }
        public DateOnly PickUpDate { get; set; }
        public string PickUpDateString => PickUpDate.ToString("yyyy.MM.dd");
        public string ExpirationDateString => PickUpDate.AddDays(OrderExpirationDays).ToString("yyyy.MM.dd");
        public bool IsCompleted { get; set; }
        public bool IsExpired { get; set; }

        // Clean Architecture: Replaced Tuple with semantic OrderItem
        public Dictionary<int, OrderItem> OrderedItems { get; private set; }

        public Order(int id, int clientId, DateOnly pickUpDate, bool isCompleted = false, bool isExpired = false)
        {
            Id = id;
            ClientId = clientId;
            PickUpDate = pickUpDate;
            IsCompleted = isCompleted;
            IsExpired = isExpired;
            OrderedItems = new Dictionary<int, OrderItem>();
        }

        public bool Equals(Order other)
        {
            if (other is null) return false;
            return Id == other.Id;
        }

        public void AddItemToOrder(int newItemId, int itemQuantity, float finalPrice)
        {
            if (OrderedItems.ContainsKey(newItemId))
                throw new ArgumentException("Item #" + newItemId + " already exists in order.");

            OrderedItems[newItemId] = new OrderItem(newItemId, itemQuantity, finalPrice);
        }

        public void ChangeItemInfoInOrder(int itemId, int newItemQuantity, float newFinalPrice)
        {
            if (!OrderedItems.ContainsKey(itemId))
                throw new ArgumentException("Item #" + itemId + " doesn't exist in this order.");

            OrderedItems[itemId].Quantity = newItemQuantity;
            OrderedItems[itemId].FinalPrice = newFinalPrice;
        }

        public void RemoveItemFromOrder(int itemId)
        {
            if (!OrderedItems.ContainsKey(itemId))
                throw new ArgumentException("Item #" + itemId + " doesn't exist in this order.");

            OrderedItems.Remove(itemId);
        }
    }
}