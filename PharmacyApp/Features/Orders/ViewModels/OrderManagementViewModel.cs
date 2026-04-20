using Microsoft.UI.Xaml.Controls;
using PharmacyApp.Common.Commands;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PharmacyApp.Features.Orders.ViewModels
{

    public class OrderDetail
    {
        public int OrderID { get; set; }
        public string UserEmail { get; set; }
        public bool IsComplete { get; set; }
        public bool IsExpired { get; set; }
        public DateOnly PickUpDate { get; set; }
        // TODO MAGIC NUMBER DETECTED (why did it take me so long to realize)
        public DateOnly ExpirationDate { get { return PickUpDate.AddDays(Order.OrderExpirationDays); } }

        public string OrderString { get { return "Order#" + OrderID; } }
        public string PickUpDateString { get { return PickUpDate.ToString("yyyy.MM.dd"); } }
        public string ExpirationDateString { get { return ExpirationDate.ToString("yyyy.MM.dd"); } }


        public OrderDetail(Order orderDetails, string userEmail)
        {
            OrderID = orderDetails.Id;
            UserEmail = userEmail;
            IsComplete = orderDetails.IsCompleted;
            IsExpired = orderDetails.IsExpired;
            PickUpDate = orderDetails.PickUpDate;
        }
    }


    public class OrderManagementViewModel : INotifyPropertyChanged
    {
        private const int EmptyLength = 0;


        OrderService orderService;

        List<OrderDetail> baseOrderList;
        public ObservableCollection<OrderDetail> FilteredOrderList { get; set; }

        public ICommand RedirectToDetailPageCommand { get; set; }

        string orderIDInput;
        string userEmailInput;
        bool isIncompleteCheckbox;
        bool isExpiredCheckbox;

        // properties for the UI elements to modify
        public string OrderIDInput 
        {
            get { return orderIDInput; }
            set { 
                orderIDInput = value; 
                OnPropertyChanged();
                ReapplyFilters();
            }
        }
        public string UserEmailInput 
        { 
            get { return userEmailInput; }
            set { 
                userEmailInput = value; 
                OnPropertyChanged();
                ReapplyFilters();
            }
        }
        public bool IsIncompleteCheckbox 
        { 
            get { return isIncompleteCheckbox; }
            set { 
                isIncompleteCheckbox = value; 
                OnPropertyChanged();
                ReapplyFilters();
            }
        }
        public bool IsExpiredCheckbox 
        { 
            get { return isExpiredCheckbox; }
            set { 
                isExpiredCheckbox = value; 
                OnPropertyChanged();
                ReapplyFilters();
            }
        }


        public OrderManagementViewModel(OrderService newOrderServ)
        {
            orderService = newOrderServ;
            baseOrderList = new();
            FilteredOrderList = new();
            RedirectToDetailPageCommand = new RelayCommandWithOneParameter<OrderDetail>(OnClickDetailButton);

            foreach (Order currOrder in orderService.OrdersRepository.GetAllOrders())
            {
                int userID = orderService.OrdersRepository.GetOrder(currOrder.Id).ClientId;
                string currUserEmail = orderService.UsersRepository.GetUserById(userID).Email;

                OrderDetail currOrderDetail = new(currOrder, currUserEmail);

                baseOrderList.Add(currOrderDetail);
                FilteredOrderList.Add(currOrderDetail);
            }
        }

        // TODO refactor ASAP to use rather the static references
        public delegate void PageChanged(Tuple<OrderService, OrderDetail> args);

        public event PageChanged ClickDetailButton;

        public virtual void OnClickDetailButton(OrderDetail chosenOrder)
        {
            ClickDetailButton?.Invoke(new Tuple<OrderService, OrderDetail>(orderService, chosenOrder));
        }


        private void ReapplyFilters()
        {
            List<OrderDetail> intermediateFilteredOrderList = new();

            foreach (OrderDetail iterOrderDetail in baseOrderList)
                intermediateFilteredOrderList.Add(iterOrderDetail);

            // TODO maybe not set the filtered order list
            // after each individual filter
            // TODO how to do in-place filtering??

            try
            {
                int inputtedOrderID = int.Parse(orderIDInput);
                List<OrderDetail> result = intermediateFilteredOrderList
                    .Where<OrderDetail>(order => order.OrderID == inputtedOrderID)
                    .ToList<OrderDetail>();

                intermediateFilteredOrderList.Clear();
                foreach (OrderDetail resultOrder in result)
                    intermediateFilteredOrderList.Add(resultOrder);
            } 
            catch (Exception e) { }


            if (userEmailInput is not null)
            {
                if (userEmailInput.Length != EmptyLength)
                {
                    List<OrderDetail> result = intermediateFilteredOrderList
                        .Where<OrderDetail>(order => order.UserEmail == userEmailInput)
                        .ToList<OrderDetail>();

                    intermediateFilteredOrderList.Clear();
                    foreach (OrderDetail resultOrder in result)
                        intermediateFilteredOrderList.Add(resultOrder);
                }
            }

            if (isIncompleteCheckbox)
            {
                List<OrderDetail> result = intermediateFilteredOrderList
                    .Where<OrderDetail>(order => !order.IsComplete && !order.IsExpired)
                    .ToList<OrderDetail>();

                intermediateFilteredOrderList.Clear();
                foreach (OrderDetail resultOrder in result)
                    intermediateFilteredOrderList.Add(resultOrder);
            }

            if (isExpiredCheckbox)
            {
                List<OrderDetail> result = intermediateFilteredOrderList
                    .Where<OrderDetail>(order => order.IsExpired)
                    .ToList<OrderDetail>();

                intermediateFilteredOrderList.Clear();
                foreach (OrderDetail resultOrder in result)
                    intermediateFilteredOrderList.Add(resultOrder);
            }


            FilteredOrderList.Clear();
            foreach (OrderDetail resultOrder in intermediateFilteredOrderList)
                FilteredOrderList.Add(resultOrder);

        }


        // To handle the changing of the filtering properties automatically
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] String propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
