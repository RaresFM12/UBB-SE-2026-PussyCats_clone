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

        public void AddOrder(int clientIdentifier, DateOnly pickUpDate, bool isCompleted = false, bool isExpired = false)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            string insertCommandString = "INSERT INTO Orders (clientId, isCompleted, isExpired, pickUpDate) VALUES (@clientIdentifier, @isCompleted, @isExpired, @pickUpDate)";

            using SqlCommand command = new SqlCommand(insertCommandString, connection);
            command.Parameters.AddWithValue("@clientIdentifier", clientIdentifier);
            command.Parameters.AddWithValue("@isCompleted", isCompleted);
            command.Parameters.AddWithValue("@isExpired", isExpired);
            command.Parameters.AddWithValue("@pickUpDate", pickUpDate.ToDateTime(TimeOnly.MinValue));

            connection.Open();
            command.ExecuteNonQuery();
        }

        public void AddOrderWithItems(int clientIdentifier, DateOnly pickUpDate, Dictionary<int, OrderItem> items, bool isCompleted = false, bool isExpired = false)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string insertOrderQuery = @"
                INSERT INTO Orders (clientId, isCompleted, isExpired, pickUpDate) 
                VALUES (@clientIdentifier, @isCompleted, @isExpired, @pickUpDate);
                SELECT SCOPE_IDENTITY();";

            using SqlCommand insertCommand = new SqlCommand(insertOrderQuery, connection);
            insertCommand.Parameters.AddWithValue("@clientIdentifier", clientIdentifier);
            insertCommand.Parameters.AddWithValue("@isCompleted", isCompleted);
            insertCommand.Parameters.AddWithValue("@isExpired", isExpired);
            insertCommand.Parameters.AddWithValue("@pickUpDate", pickUpDate.ToDateTime(TimeOnly.MinValue));

            int newOrderIdentifier = Convert.ToInt32(insertCommand.ExecuteScalar());

            foreach (var item in items.Values)
            {
                string insertItemQuery = "INSERT INTO OrderItems (orderId, itemId, orderQuantity, price) VALUES (@orderIdentifier, @itemIdentifier, @quantity, @price)";
                using SqlCommand itemCommand = new SqlCommand(insertItemQuery, connection);
                itemCommand.Parameters.AddWithValue("@orderIdentifier", newOrderIdentifier);
                itemCommand.Parameters.AddWithValue("@itemIdentifier", item.ItemId);
                itemCommand.Parameters.AddWithValue("@quantity", item.Quantity);
                itemCommand.Parameters.AddWithValue("@price", item.FinalPrice);
                itemCommand.ExecuteNonQuery();
            }
        }

        public void RemoveOrder(int orderIdentifierToBeRemoved)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string deleteItemsQuery = "DELETE FROM OrderItems WHERE orderId = @orderIdentifier";
            using SqlCommand deleteItemsCommand = new SqlCommand(deleteItemsQuery, connection);
            deleteItemsCommand.Parameters.AddWithValue("@orderIdentifier", orderIdentifierToBeRemoved);
            deleteItemsCommand.ExecuteNonQuery();

            string deleteOrderQuery = "DELETE FROM Orders WHERE orderId = @orderIdentifier";
            using SqlCommand deleteOrderCommand = new SqlCommand(deleteOrderQuery, connection);
            deleteOrderCommand.Parameters.AddWithValue("@orderIdentifier", orderIdentifierToBeRemoved);
            deleteOrderCommand.ExecuteNonQuery();
        }

        public void UpdateOrder(Order newOrder)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            string updateQuery = @"
                UPDATE Orders 
                SET clientId = @clientIdentifier, isCompleted = @isCompleted, isExpired = @isExpired, pickUpDate = @pickUpDate 
                WHERE orderId = @orderIdentifier";

            using SqlCommand updateCommand = new SqlCommand(updateQuery, connection);
            updateCommand.Parameters.AddWithValue("@clientIdentifier", newOrder.ClientId);
            updateCommand.Parameters.AddWithValue("@isCompleted", newOrder.IsCompleted);
            updateCommand.Parameters.AddWithValue("@isExpired", newOrder.IsExpired);
            updateCommand.Parameters.AddWithValue("@pickUpDate", newOrder.PickUpDate.ToDateTime(TimeOnly.MinValue));
            updateCommand.Parameters.AddWithValue("@orderIdentifier", newOrder.Id);
            updateCommand.ExecuteNonQuery();

            string deleteItemsQuery = "DELETE FROM OrderItems WHERE orderId = @orderIdentifier";
            using SqlCommand deleteItemsCommand = new SqlCommand(deleteItemsQuery, connection);
            deleteItemsCommand.Parameters.AddWithValue("@orderIdentifier", newOrder.Id);
            deleteItemsCommand.ExecuteNonQuery();

            foreach (var item in newOrder.OrderedItems.Values)
            {
                string insertItemQuery = "INSERT INTO OrderItems (orderId, itemId, orderQuantity, price) VALUES (@orderIdentifier, @itemIdentifier, @quantity, @price)";
                using SqlCommand itemCommand = new SqlCommand(insertItemQuery, connection);
                itemCommand.Parameters.AddWithValue("@orderIdentifier", newOrder.Id);
                itemCommand.Parameters.AddWithValue("@itemIdentifier", item.ItemId);
                itemCommand.Parameters.AddWithValue("@quantity", item.Quantity);
                itemCommand.Parameters.AddWithValue("@price", item.FinalPrice);
                itemCommand.ExecuteNonQuery();
            }
        }

        public Order GetOrder(int orderIdentifier)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            string query = "SELECT * FROM Orders WHERE orderId = @orderIdentifier";
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@orderIdentifier", orderIdentifier);

            connection.Open();
            var orders = ExtractOrdersFromCommand(command, connection);
            return orders.Count > 0 ? orders[0] : null;
        }

        public List<Order> GetAllOrders()
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            string query = "SELECT * FROM Orders";
            using SqlCommand command = new SqlCommand(query, connection);

            connection.Open();
            return ExtractOrdersFromCommand(command, connection);
        }

        public List<Order> GetOrdersOfClient(int clientIdentifier)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            string query = "SELECT * FROM Orders WHERE clientId = @clientIdentifier";
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@clientIdentifier", clientIdentifier);

            connection.Open();
            return ExtractOrdersFromCommand(command, connection);
        }

        public bool OrderExists(int orderIdentifier)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            string query = "SELECT COUNT(1) FROM Orders WHERE orderId = @orderIdentifier";
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@orderIdentifier", orderIdentifier);

            connection.Open();
            int count = Convert.ToInt32(command.ExecuteScalar());
            return count > 0;
        }

        private List<Order> ExtractOrdersFromCommand(SqlCommand command, SqlConnection connection)
        {
            List<Order> orders = new List<Order>();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int orderIdentifier = reader.GetInt32(reader.GetOrdinal("orderId"));
                    int clientIdentifier = reader.GetInt32(reader.GetOrdinal("clientId"));
                    bool isCompleted = reader.GetBoolean(reader.GetOrdinal("isCompleted"));
                    bool isExpired = reader.GetBoolean(reader.GetOrdinal("isExpired"));
                    DateOnly pickUpDate = DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("pickUpDate")));

                    orders.Add(new Order(orderIdentifier, clientIdentifier, pickUpDate, isCompleted, isExpired));
                }
            }

            foreach (var order in orders)
            {
                string itemsQuery = "SELECT itemId, orderQuantity, price FROM OrderItems WHERE orderId = @orderIdentifier";
                using SqlCommand itemsCommand = new SqlCommand(itemsQuery, connection);
                itemsCommand.Parameters.AddWithValue("@orderIdentifier", order.Id);

                using SqlDataReader itemsReader = itemsCommand.ExecuteReader();
                while (itemsReader.Read())
                {
                    int itemIdentifier = itemsReader.GetInt32(itemsReader.GetOrdinal("itemId"));
                    int quantity = itemsReader.GetInt32(itemsReader.GetOrdinal("orderQuantity"));
                    float price = (float)itemsReader.GetDecimal(itemsReader.GetOrdinal("price"));

                    order.AddItemToOrder(itemIdentifier, quantity, price);
                }
            }

            return orders;
        }
    }
}