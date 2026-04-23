using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PharmacyApp.Models;
using Windows.UI.WebUI;
using Microsoft.Data.SqlClient;

namespace PharmacyApp.Common.Repositories
{
    public class SQLOrdersRepository : IOrdersRepository
    {
        private const int FirstElementIndex = 0;

        public SQLOrdersRepository()
        {
        }

        public void AddOrder(int clientId, DateOnly pickUpDate, bool isCompleted = false, bool isExpired = false)
        {
            string connString = SQLUtility.GetConnectionString();
            string pickUpDateString = $"{pickUpDate.Year}-{pickUpDate.Month}-{pickUpDate.Day}";
            string insertCommandString = "INSERT INTO Orders (clientId, isCompleted, isExpired, pickUpDate) " +
                                        $"VALUES ({clientId}, '{isCompleted}', '{isExpired}', '{pickUpDateString}')";

            using SqlConnection conn = new SqlConnection(connString);

            SqlCommand insertOrderCommand = new SqlCommand(insertCommandString, conn);
            conn.Open();
            insertOrderCommand.ExecuteNonQuery();
        }

        public void AddOrderWithItems(int clientId, DateOnly pickUpDate, Dictionary<int, Tuple<int, float>> items,
                                      bool isCompleted = false, bool isExpired = false)
        {
            List<Order> ordersBeforeAdd = GetOrdersOfClient(clientId);
            AddOrder(clientId, pickUpDate, isCompleted, isExpired);
            List<Order> ordersAfterAdd = GetOrdersOfClient(clientId);

            List<Order> result = new ();
            foreach (Order order in ordersAfterAdd)
            {
                if (!ordersBeforeAdd.Contains(order))
                {
                    result.Add(order);
                }
            }
            Order newOrder = result[FirstElementIndex];

            foreach (KeyValuePair<int, Tuple<int, float>> item in items)
            {
                int itemId = item.Key;
                int itemQuantity = item.Value.Item1;
                float finalPrice = item.Value.Item2;

                newOrder.AddItemToOrder(itemId, itemQuantity, finalPrice);
            }
            UpdateOrder(newOrder);
        }

        public void RemoveOrder(int orderIdToBeRemoved)
        {
            string connString = SQLUtility.GetConnectionString();
            string deleteItemsInOrderString = $"DELETE FROM OrderItems WHERE orderId = {orderIdToBeRemoved}";
            string deleteCommandString = $"DELETE FROM Orders WHERE orderId = {orderIdToBeRemoved}";

            using SqlConnection conn = new (connString);

            SqlCommand deleteItemsInOrderCommand = new (deleteItemsInOrderString, conn);
            SqlCommand deleteOrderCommand = new (deleteCommandString, conn);

            conn.Open();
            deleteItemsInOrderCommand.ExecuteNonQuery();
            deleteOrderCommand.ExecuteNonQuery();
        }

        public void UpdateOrder(Order newOrder)
        {
            string connString = SQLUtility.GetConnectionString();
            string pickUpDateString = $"{newOrder.PickUpDate.Year}-{newOrder.PickUpDate.Month}-{newOrder.PickUpDate.Day}";
            string updateCommandString = $"UPDATE Orders " +
                                        $"SET clientId = {newOrder.ClientId}, " +
                                        $"isCompleted = '{newOrder.IsCompleted}', " +
                                        $"isExpired = '{newOrder.IsExpired}', " +
                                        $"pickUpDate = '{pickUpDateString}' " +
                                        $"WHERE orderId = {newOrder.Id}";

            using SqlConnection conn = new SqlConnection(connString);

            SqlCommand updateOrderCommand = new SqlCommand(updateCommandString, conn);
            conn.Open();
            updateOrderCommand.ExecuteNonQuery();

            string deleteItemsInOrderCommandString = $"DELETE FROM OrderItems WHERE orderId = {newOrder.Id}";
            SqlCommand deleteItemsInOrderCommand = new SqlCommand(deleteItemsInOrderCommandString, conn);
            deleteItemsInOrderCommand.ExecuteNonQuery();

            foreach (KeyValuePair<int, Tuple<int, float>> itemInOrder in newOrder.ItemQuantitiesWithFinalPrice)
            {
                int itemId = itemInOrder.Key;
                int itemQuantity = itemInOrder.Value.Item1;
                float finalPrice = itemInOrder.Value.Item2;

                string insertItemsInOrderCommandString =
                    $"INSERT INTO OrderItems (orderId, itemId, orderQuantity, price) " +
                    $"VALUES ({newOrder.Id}, {itemId}, {itemQuantity}, {finalPrice})";
                SqlCommand insertItemsInOrderCommand = new SqlCommand(insertItemsInOrderCommandString, conn);
                insertItemsInOrderCommand.ExecuteNonQuery();
            }
        }

        public Order GetOrder(int orderId)
        {
            string connString = SQLUtility.GetConnectionString();
            string selectOrderCommandString = $"SELECT * FROM Orders WHERE orderId = {orderId}";
            string selectItemsInOrderCommandString = $"SELECT itemId, orderQuantity, price FROM OrderItems WHERE orderId = {orderId}";

            using SqlConnection conn = new SqlConnection(connString);

            SqlDataAdapter orderAdapter = new SqlDataAdapter(selectOrderCommandString, conn);
            SqlDataAdapter itemsInOrderAdapter = new SqlDataAdapter(selectItemsInOrderCommandString, conn);
            DataSet orderDataFromDb = new DataSet();

            conn.Open();
            orderAdapter.Fill(orderDataFromDb, "Orders");
            itemsInOrderAdapter.Fill(orderDataFromDb, "OrderItems");

            DataRow resultingRow = orderDataFromDb.Tables["Orders"].Rows[0];

            int resultingOrderId = (int)resultingRow["orderId"];
            int resultingClientId = (int)resultingRow["clientId"];
            bool resultingCompletedStatus = (bool)resultingRow["isCompleted"];
            bool resultingExpiredStatus = (bool)resultingRow["isExpired"];
            DateOnly resultingPickUpDate = DateOnly.FromDateTime((DateTime)resultingRow["pickUpDate"]);

            Order resultingOrder = new Order(resultingOrderId, resultingClientId, resultingPickUpDate, resultingCompletedStatus, resultingExpiredStatus);

            foreach (DataRow itemInOrderRow in orderDataFromDb.Tables["OrderItems"].Rows)
            {
                int itemId = (int)itemInOrderRow["itemId"];
                int itemQuantity = (int)itemInOrderRow["orderQuantity"];
                float finalPrice = (float)(decimal)itemInOrderRow["price"];
                resultingOrder.AddItemToOrder(itemId, itemQuantity, finalPrice);
            }

            return resultingOrder;
        }

        private List<Order> GetOrdersFromSelectCommand(string selectOrdersCommandString)
        {
            List<Order> orders = new ();
            List<int> orderIds = new ();

            string connString = SQLUtility.GetConnectionString();

            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlDataAdapter orderAdapter = new SqlDataAdapter(selectOrdersCommandString, conn);
                DataSet orderInfoFromDb = new DataSet();

                conn.Open();
                orderAdapter.Fill(orderInfoFromDb, "Orders");

                foreach (DataRow orderRow in orderInfoFromDb.Tables["Orders"].Rows)
                {
                    orderIds.Add((int)orderRow["orderId"]);
                }
            }

            foreach (int orderId in orderIds)
            {
                orders.Add(GetOrder(orderId));
            }
            return orders;
        }

        public List<Order> GetAllOrders()
        {
            string selectAllOrdersCommandString = $"SELECT * FROM Orders";
            return GetOrdersFromSelectCommand(selectAllOrdersCommandString);
        }

        public List<Order> GetOrdersOfClient(int clientId)
        {
            string selectOrdersCommandString = $"SELECT * FROM Orders WHERE clientId = {clientId}";
            return GetOrdersFromSelectCommand(selectOrdersCommandString);
        }

        public bool OrderExists(int orderId)
        {
            string connString = SQLUtility.GetConnectionString();
            string selectCommandString = $"SELECT * FROM Orders WHERE orderId = {orderId}";

            using SqlConnection conn = new SqlConnection(connString);

            SqlDataAdapter ordersAdapter = new SqlDataAdapter(selectCommandString, conn);
            DataSet orders = new DataSet();

            conn.Open();
            ordersAdapter.Fill(orders, "Orders");
            if (orders.Tables["Orders"].Rows.Count > 0)
            {
                return true;
            }

            return false;
        }
    }
}
