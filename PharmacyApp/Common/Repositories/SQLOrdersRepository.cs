using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Linq;
using Windows.UI.WebUI;

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
            // because we don't know the orderId for the order that's about to be inserted,
            // we have to keep track of what orders are associated with the user
            // before and after the insertion, the difference is the freshly inserted order
            // which we'll modify by adding the items, then update it

            // TODO maybe we can ditch this and just get the last order of ordersAfterAdd?
            // (if the newest order is always at the end, just like in the DBMS, it should
            // make sense, cuz of auto-incremented ids...I'll test later)

            List<Order> ordersBeforeAdd = GetOrdersOfClient(clientId);
            AddOrder(clientId, pickUpDate, isCompleted, isExpired);
            List<Order> ordersAfterAdd = GetOrdersOfClient(clientId);



            // WTF WHY WOULDN'T YOU WORK
            //IEnumerable<Order> difference = ordersAfterAdd.Except<Order>(ordersBeforeAdd);
            //Order newOrder = difference.First();

            // since apparently the code up top wouldn't calculate the SET DIFFERENCE
            // I'm gonna do it manually for now

            // it's fine? cuz every order is unique by its ID, by which we compare

            List<Order> result = new();
            foreach (Order order in ordersAfterAdd)
            {
                if (!ordersBeforeAdd.Contains(order))
                    result.Add(order);
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

            using SqlConnection conn = new SqlConnection(connString);

            SqlCommand deleteItemsInOrderCommand = new SqlCommand(deleteItemsInOrderString, conn);
            SqlCommand deleteOrderCommand = new SqlCommand(deleteCommandString, conn);

            // first we have to delete the entries in OrderItems for this orderId
            // i.e. the items that are included in this order
            // because of foreign key constraints

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


            // we have to also update the items, their quantities and final prices
            // that are inside this order for the table OrderItems

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
            // we have to get the items of the particular order from OrderItems
            // in order to put those into the Order object's dictionary for items
            // therefore create the Order object fully

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


            // we expect only one order as a result (cuz orderId is unique for every entry)

            DataRow resultingRow = orderDataFromDb.Tables["Orders"].Rows[0];

            int resultingOrderId = (int)resultingRow["orderId"];
            int resultingClientId = (int)resultingRow["clientId"];
            bool resultingCompletedStatus = (bool)resultingRow["isCompleted"];
            bool resultingExpiredStatus = (bool)resultingRow["isExpired"];
            DateOnly resultingPickUpDate = DateOnly.FromDateTime((DateTime)resultingRow["pickUpDate"]);

            Order resultingOrder = new Order(resultingOrderId, resultingClientId, resultingPickUpDate, resultingCompletedStatus, resultingExpiredStatus);


            // inserting the items that were already inside this order at the time of querying

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
            List<Order> orders = new List<Order>();
            List<int> orderIds = new List<int>();

            string connString = SQLUtility.GetConnectionString();

            using (SqlConnection conn = new SqlConnection(connString))
            {
                SqlDataAdapter orderAdapter = new SqlDataAdapter(selectOrdersCommandString, conn);
                DataSet orderInfoFromDb = new DataSet();

                conn.Open();
                orderAdapter.Fill(orderInfoFromDb, "Orders");

                foreach (DataRow orderRow in orderInfoFromDb.Tables["Orders"].Rows)
                    orderIds.Add((int)orderRow["orderId"]);
            }

            foreach (int orderId in orderIds)
                orders.Add(GetOrder(orderId));

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
                return true;

            return false;
        }
    }
}
