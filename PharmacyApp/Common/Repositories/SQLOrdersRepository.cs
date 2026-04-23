using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using PharmacyApp.Models;

namespace PharmacyApp.Common.Repositories
{
    public class SQLOrdersRepository : IOrdersRepository
    {
        private readonly string _connectionString;

        public SQLOrdersRepository()
        {
            _connectionString = SQLUtility.GetConnectionString();
        }

        public void AddOrder(int clientId, DateOnly pickUpDate, bool isCompleted = false, bool isExpired = false)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            string insertCommandString = "INSERT INTO Orders (clientId, isCompleted, isExpired, pickUpDate) VALUES (@clientId, @isCompleted, @isExpired, @pickUpDate)";

            using SqlCommand cmd = new SqlCommand(insertCommandString, conn);
            cmd.Parameters.AddWithValue("@clientId", clientId);
            cmd.Parameters.AddWithValue("@isCompleted", isCompleted);
            cmd.Parameters.AddWithValue("@isExpired", isExpired);
            cmd.Parameters.AddWithValue("@pickUpDate", pickUpDate.ToDateTime(TimeOnly.MinValue));

            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void AddOrderWithItems(int clientId, DateOnly pickUpDate, Dictionary<int, OrderItem> items, bool isCompleted = false, bool isExpired = false)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            string insertOrderQuery = @"
                INSERT INTO Orders (clientId, isCompleted, isExpired, pickUpDate) 
                VALUES (@clientId, @isCompleted, @isExpired, @pickUpDate);
                SELECT SCOPE_IDENTITY();";

            using SqlCommand insertCmd = new SqlCommand(insertOrderQuery, conn);
            insertCmd.Parameters.AddWithValue("@clientId", clientId);
            insertCmd.Parameters.AddWithValue("@isCompleted", isCompleted);
            insertCmd.Parameters.AddWithValue("@isExpired", isExpired);
            insertCmd.Parameters.AddWithValue("@pickUpDate", pickUpDate.ToDateTime(TimeOnly.MinValue));

            int newOrderId = Convert.ToInt32(insertCmd.ExecuteScalar());

            foreach (var item in items.Values)
            {
                string insertItemQuery = "INSERT INTO OrderItems (orderId, itemId, orderQuantity, price) VALUES (@orderId, @itemId, @quantity, @price)";
                using SqlCommand itemCmd = new SqlCommand(insertItemQuery, conn);
                itemCmd.Parameters.AddWithValue("@orderId", newOrderId);
                itemCmd.Parameters.AddWithValue("@itemId", item.ItemId);
                itemCmd.Parameters.AddWithValue("@quantity", item.Quantity);
                itemCmd.Parameters.AddWithValue("@price", item.FinalPrice);
                itemCmd.ExecuteNonQuery();
            }
        }

        public void RemoveOrder(int orderIdToBeRemoved)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            string deleteItemsQuery = "DELETE FROM OrderItems WHERE orderId = @orderId";
            using SqlCommand deleteItemsCmd = new SqlCommand(deleteItemsQuery, conn);
            deleteItemsCmd.Parameters.AddWithValue("@orderId", orderIdToBeRemoved);
            deleteItemsCmd.ExecuteNonQuery();

            string deleteOrderQuery = "DELETE FROM Orders WHERE orderId = @orderId";
            using SqlCommand deleteOrderCmd = new SqlCommand(deleteOrderQuery, conn);
            deleteOrderCmd.Parameters.AddWithValue("@orderId", orderIdToBeRemoved);
            deleteOrderCmd.ExecuteNonQuery();
        }

        public void UpdateOrder(Order newOrder)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            string updateQuery = @"
                UPDATE Orders 
                SET clientId = @clientId, isCompleted = @isCompleted, isExpired = @isExpired, pickUpDate = @pickUpDate 
                WHERE orderId = @orderId";

            using SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
            updateCmd.Parameters.AddWithValue("@clientId", newOrder.ClientId);
            updateCmd.Parameters.AddWithValue("@isCompleted", newOrder.IsCompleted);
            updateCmd.Parameters.AddWithValue("@isExpired", newOrder.IsExpired);
            updateCmd.Parameters.AddWithValue("@pickUpDate", newOrder.PickUpDate.ToDateTime(TimeOnly.MinValue));
            updateCmd.Parameters.AddWithValue("@orderId", newOrder.Id);
            updateCmd.ExecuteNonQuery();

            string deleteItemsQuery = "DELETE FROM OrderItems WHERE orderId = @orderId";
            using SqlCommand deleteItemsCmd = new SqlCommand(deleteItemsQuery, conn);
            deleteItemsCmd.Parameters.AddWithValue("@orderId", newOrder.Id);
            deleteItemsCmd.ExecuteNonQuery();

            foreach (var item in newOrder.OrderedItems.Values)
            {
                string insertItemQuery = "INSERT INTO OrderItems (orderId, itemId, orderQuantity, price) VALUES (@orderId, @itemId, @quantity, @price)";
                using SqlCommand itemCmd = new SqlCommand(insertItemQuery, conn);
                itemCmd.Parameters.AddWithValue("@orderId", newOrder.Id);
                itemCmd.Parameters.AddWithValue("@itemId", item.ItemId);
                itemCmd.Parameters.AddWithValue("@quantity", item.Quantity);
                itemCmd.Parameters.AddWithValue("@price", item.FinalPrice);
                itemCmd.ExecuteNonQuery();
            }
        }

        public Order GetOrder(int orderId)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            string query = "SELECT * FROM Orders WHERE orderId = @orderId";
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@orderId", orderId);

            conn.Open();
            var orders = ExtractOrdersFromCommand(cmd, conn);
            return orders.Count > 0 ? orders[0] : null;
        }

        public List<Order> GetAllOrders()
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            string query = "SELECT * FROM Orders";
            using SqlCommand cmd = new SqlCommand(query, conn);

            conn.Open();
            return ExtractOrdersFromCommand(cmd, conn);
        }

        public List<Order> GetOrdersOfClient(int clientId)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            string query = "SELECT * FROM Orders WHERE clientId = @clientId";
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@clientId", clientId);

            conn.Open();
            return ExtractOrdersFromCommand(cmd, conn);
        }

        public bool OrderExists(int orderId)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            string query = "SELECT COUNT(1) FROM Orders WHERE orderId = @orderId";
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@orderId", orderId);

            conn.Open();
            int count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }

        private List<Order> ExtractOrdersFromCommand(SqlCommand command, SqlConnection conn)
        {
            List<Order> orders = new List<Order>();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int orderId = reader.GetInt32(reader.GetOrdinal("orderId"));
                    int clientId = reader.GetInt32(reader.GetOrdinal("clientId"));
                    bool isCompleted = reader.GetBoolean(reader.GetOrdinal("isCompleted"));
                    bool isExpired = reader.GetBoolean(reader.GetOrdinal("isExpired"));
                    DateOnly pickUpDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("pickUpDate")));

                    orders.Add(new Order(orderId, clientId, pickUpDate, isCompleted, isExpired));
                }
            }

            foreach (var order in orders)
            {
                string itemsQuery = "SELECT itemId, orderQuantity, price FROM OrderItems WHERE orderId = @orderId";
                using SqlCommand itemsCmd = new SqlCommand(itemsQuery, conn);
                itemsCmd.Parameters.AddWithValue("@orderId", order.Id);

                using SqlDataReader itemsReader = itemsCmd.ExecuteReader();
                while (itemsReader.Read())
                {
                    int itemId = itemsReader.GetInt32(itemsReader.GetOrdinal("itemId"));
                    int quantity = itemsReader.GetInt32(itemsReader.GetOrdinal("orderQuantity"));
                    float price = (float)itemsReader.GetDecimal(itemsReader.GetOrdinal("price"));

                    order.AddItemToOrder(itemId, quantity, price);
                }
            }

            return orders;
        }
    }
}