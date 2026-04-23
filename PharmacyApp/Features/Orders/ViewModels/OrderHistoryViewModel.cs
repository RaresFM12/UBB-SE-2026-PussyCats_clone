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
    class OrderHistoryViewModel
    {
        private readonly IOrderService _orderService;
        private List<Order> _baseOrderList;

        public ICommand CancelCommand { get; private set; }
        public ICommand ResubmitCommand { get; private set; }
        public ICommand GoToDetailPageCommand { get; private set; }

        public ObservableCollection<Order> OrderHistory { get; private set; }

        private bool _isExpiredCheckbox;
        public bool IsExpiredCheckbox
        {
            get { return _isExpiredCheckbox; }
            set
            {
                _isExpiredCheckbox = value;
                ReapplyFilters();
            }
        }

        public delegate void SelectedOrder(Tuple<IOrderService, Order> args);
        public event SelectedOrder ClickDetailButton;
        public event SelectedOrder ClickResubmitButton;

        public OrderHistoryViewModel(IOrderService orderService)
        {
            _orderService = orderService;
            _baseOrderList = new List<Order>();
            OrderHistory = new ObservableCollection<Order>();

            // The UI binds directly to these commands
            CancelCommand = new RelayCommandWithOneParameter<Order>(CancelOrder);
            ResubmitCommand = new RelayCommandWithOneParameter<Order>(NavigateToResubmitPage);
            GoToDetailPageCommand = new RelayCommandWithOneParameter<Order>(NavigateToModifyPage);

            LoadOrders();
        }

        // --- Core Data Loading ---
        private void LoadOrders()
        {
            OrderHistory.Clear();
            _baseOrderList.Clear();

            int clientId = _orderService.ActiveUser.Id;

            // This calls your backend service which auto-updates expiration statuses!
            List<Order> userOrders = _orderService.GetClientOrders(clientId);

            foreach (Order currentOrder in userOrders)
            {
                _baseOrderList.Add(currentOrder);
            }

            ReapplyFilters();
        }

        private void ReapplyFilters()
        {
            OrderHistory.Clear();

            IEnumerable<Order> filteredList = _baseOrderList;

            if (_isExpiredCheckbox)
            {
                filteredList = _baseOrderList.Where(order => order.IsExpired);
            }

            foreach (Order order in filteredList)
            {
                OrderHistory.Add(order);
            }
        }

        // --- Button Actions ---

        private void CancelOrder(Order orderToCancel)
        {
            try
            {
                // 1. Call your business logic to cancel in the database
                _orderService.CancelOrder(orderToCancel.Id);

                // 2. Refresh the UI list straight from the database to ensure sync
                LoadOrders();
            }
            catch (Exception ex)
            {
                // In a real app, you'd show a UI dialog here. 
                // For now, we catch it so the app doesn't crash if the rule fails.
                System.Diagnostics.Debug.WriteLine($"Failed to cancel: {ex.Message}");
            }
        }

        private void NavigateToResubmitPage(Order orderToResubmit)
        {
            // Fires the event to tell the View to change pages
            ClickResubmitButton?.Invoke(new Tuple<IOrderService, Order>(_orderService, orderToResubmit));
        }

        private void NavigateToModifyPage(Order orderToModify)
        {
            // Fires the event to tell the View to change pages
            ClickDetailButton?.Invoke(new Tuple<IOrderService, Order>(_orderService, orderToModify));
        }
    }
}