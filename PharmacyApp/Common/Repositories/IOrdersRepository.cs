using System;
using System.Collections.Generic;
using PharmacyApp.Models;

namespace PharmacyApp.Common.Repositories
{
    public interface IOrdersRepository
    {
        void AddOrder(int clientId, DateOnly pickUpDate, bool isCompleted = false, bool isExpired = false);
        void AddOrderWithItems(int clientId, DateOnly pickUpDate, Dictionary<int, OrderItem> items, bool isCompleted = false, bool isExpired = false);
        void RemoveOrder(int orderIdToBeRemoved);
        Order GetOrder(int orderId);
        List<Order> GetAllOrders();
        List<Order> GetOrdersOfClient(int clientId);
        void UpdateOrder(Order newOrder);
        bool OrderExists(int orderId);
    }
}