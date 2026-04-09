using PharmacyApp.Common.Commands;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using static PharmacyApp.Features.Orders.ViewModels.OrderManagementViewModel;

namespace PharmacyApp.Features.Orders.ViewModels
{
    class OrderHistoryViewModel
    {
        OrderService orderService;
        List<Order> baseOrderList;

        public ICommand CancelCommand { get; private set; }
        public ICommand ResubmitCommand { get; private set; }
        public ICommand GoToDetailPageCommand { get; private set; }
        public ObservableCollection<Order> OrderHistory;

        bool isExpiredCheckbox;
        public bool IsExpiredCheckbox 
        { 
            get { return isExpiredCheckbox; } 
            set { isExpiredCheckbox = value; ReapplyFilters(); }
        }

        public OrderHistoryViewModel(OrderService userService)
        {
            orderService = userService;
            CancelCommand = new RelayCommandWithOneParameter<Order>(CancelOrderCommand);
            ResubmitCommand = new RelayCommandWithOneParameter<Order>(ResubmitExpiredOrderCommand);
            GoToDetailPageCommand = new RelayCommandWithOneParameter<Order>(DisplayOrderDetailCommand);
            OrderHistory = new();
            baseOrderList = new();

            int clientId = orderService.ActiveUser.Id;
            List<Order> userOrders = orderService.OrdersRepository.GetOrdersOfClient(clientId);
            foreach (Order currentOrder in userOrders)
            {
                OrderHistory.Add(currentOrder);
                baseOrderList.Add(currentOrder);
            }
        }

        private void CancelOrderCommand(Order orderToCancel)
        {
            OnClickCancelButton(orderToCancel);
        }

        private void ResubmitExpiredOrderCommand(Order orderToResubmit)
        {
            OnClickResubmitButton(orderToResubmit);
        }

        private void DisplayOrderDetailCommand(Order orderToModify)
        {
            OnClickDetailButton(orderToModify);
        }

        private void ReapplyFilters()
        {
            List<Order> intermediateFilteredOrderList = new();

            foreach (Order order in baseOrderList)
                intermediateFilteredOrderList.Add(order);

            if (isExpiredCheckbox)
            {
                List<Order> result = intermediateFilteredOrderList
                    .Where<Order>(order => order.IsExpired)
                    .ToList<Order>();

                intermediateFilteredOrderList.Clear();
                foreach (Order resultOrder in result)
                    intermediateFilteredOrderList.Add(resultOrder);
            }

            OrderHistory.Clear();
            foreach (Order resultOrder in intermediateFilteredOrderList)
                OrderHistory.Add(resultOrder);
        }

        public delegate void SelectedOrder(Tuple<OrderService, Order> args);

        public event SelectedOrder ClickDetailButton;

        public virtual void OnClickDetailButton(Order chosenOrder)
        {
            ClickDetailButton?.Invoke(new Tuple<OrderService, Order>(orderService, chosenOrder));
        }

        public event SelectedOrder ClickCancelButton;

        public virtual void OnClickCancelButton(Order orderToCancel)
        {
            ClickCancelButton?.Invoke(new Tuple<OrderService, Order>(orderService, orderToCancel));
        }

        public void CancelOrder(Order orderToCancel)
        {
            orderService.CancelOrder(orderToCancel.Id);

            orderToCancel.IsExpired = true;
            foreach (Order currOrder in baseOrderList)
            {
                if (currOrder.Id == orderToCancel.Id)
                {
                    currOrder.IsExpired = true;
                }
            }

            OrderHistory.Clear();
            foreach (Order resultOrder in baseOrderList)
                OrderHistory.Add(resultOrder);
        }


        public event SelectedOrder ClickResubmitButton;

        public virtual void OnClickResubmitButton(Order orderToResubmit)
        {
            ClickResubmitButton?.Invoke(new Tuple<OrderService, Order>(orderService, orderToResubmit));
        }
    }
}
