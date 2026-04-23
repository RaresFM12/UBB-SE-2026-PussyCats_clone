using PharmacyApp.Common.Commands;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace PharmacyApp.Features.Orders.ViewModels
{
    public class OrderHistoryViewModel
    {
        private readonly IOrderService _orderBusinessLogicService;
        private List<Order> _baseUserOrderList;

        public ICommand CancelOrderCommand { get; private set; }
        public ICommand ResubmitExpiredOrderCommand { get; private set; }
        public ICommand NavigateToOrderDetailsPageCommand { get; private set; }

        public ObservableCollection<Order> UserOrderHistoryCollection { get; private set; }

        private bool _isExpiredOrdersFilterActive;
        public bool IsExpiredOrdersFilterActive
        {
            get { return _isExpiredOrdersFilterActive; }
            set
            {
                _isExpiredOrdersFilterActive = value;
                ReapplyOrderHistoryFilters();
            }
        }

        public delegate void SelectedOrderDelegate(Tuple<IOrderService, Order> serviceAndOrderParameters);
        public event SelectedOrderDelegate ClickDetailButtonEvent;
        public event SelectedOrderDelegate ClickResubmitButtonEvent;

        public OrderHistoryViewModel(IOrderService orderBusinessLogicService)
        {
            _orderBusinessLogicService = orderBusinessLogicService;
            _baseUserOrderList = new List<Order>();
            UserOrderHistoryCollection = new ObservableCollection<Order>();

            CancelOrderCommand = new RelayCommandWithOneParameter<Order>(ExecuteCancelOrderAction);
            ResubmitExpiredOrderCommand = new RelayCommandWithOneParameter<Order>(ExecuteNavigateToResubmitPageAction);
            NavigateToOrderDetailsPageCommand = new RelayCommandWithOneParameter<Order>(ExecuteNavigateToModifyPageAction);

            LoadUserOrdersFromDatabase();
        }

        private void LoadUserOrdersFromDatabase()
        {
            UserOrderHistoryCollection.Clear();
            _baseUserOrderList.Clear();

            int clientIdentificationNumber = _orderBusinessLogicService.ActiveUser.Id;

            List<Order> retrievedUserOrders = _orderBusinessLogicService.GetClientOrders(clientIdentificationNumber);

            foreach (Order currentOrderInList in retrievedUserOrders)
            {
                _baseUserOrderList.Add(currentOrderInList);
            }

            ReapplyOrderHistoryFilters();
        }

        private void ReapplyOrderHistoryFilters()
        {
            UserOrderHistoryCollection.Clear();

            IEnumerable<Order> filteredUserOrderList = _baseUserOrderList;

            if (_isExpiredOrdersFilterActive)
            {
                filteredUserOrderList = _baseUserOrderList.Where(currentOrderInFilter => currentOrderInFilter.IsExpired);
            }

            foreach (Order order in filteredUserOrderList)
            {
                UserOrderHistoryCollection.Add(order);
            }
        }

        private void ExecuteCancelOrderAction(Order orderToBeCancelled)
        {
            try
            {
                _orderBusinessLogicService.CancelOrder(orderToBeCancelled.Id);

                LoadUserOrdersFromDatabase();
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to cancel order: {exception.Message}");
            }
        }

        private void ExecuteNavigateToResubmitPageAction(Order orderToBeResubmitted)
        {
            ClickResubmitButtonEvent?.Invoke(new Tuple<IOrderService, Order>(_orderBusinessLogicService, orderToBeResubmitted));
        }

        private void ExecuteNavigateToModifyPageAction(Order orderToBeModified)
        {
            ClickDetailButtonEvent?.Invoke(new Tuple<IOrderService, Order>(_orderBusinessLogicService, orderToBeModified));
        }
    }
}