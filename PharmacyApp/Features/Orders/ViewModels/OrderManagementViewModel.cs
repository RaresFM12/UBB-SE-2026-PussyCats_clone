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
using System.Windows.Input;

namespace PharmacyApp.Features.Orders.ViewModels
{
    public class OrderDetail
    {
        public int OrderIdentifier { get; set; }
        public string UserEmailAddress { get; set; }
        public bool IsComplete { get; set; }
        public bool IsExpired { get; set; }
        public DateOnly PickUpDate { get; set; }
        public DateOnly ExpirationDate { get { return PickUpDate.AddDays(Order.OrderExpirationDays); } }

        public string OrderString { get { return "Order#" + OrderIdentifier; } }
        public string PickUpDateString { get { return PickUpDate.ToString("yyyy.MM.dd"); } }
        public string ExpirationDateString { get { return ExpirationDate.ToString("yyyy.MM.dd"); } }

        public OrderDetail(Order orderDetails, string userEmailAddress)
        {
            OrderIdentifier = orderDetails.Id;
            UserEmailAddress = userEmailAddress;
            IsComplete = orderDetails.IsCompleted;
            IsExpired = orderDetails.IsExpired;
            PickUpDate = orderDetails.PickUpDate;
        }
    }

    public class OrderManagementViewModel : INotifyPropertyChanged
    {
        private const int EmptyLength = 0;
        private readonly IOrderService _orderBusinessLogicService;

        private List<OrderDetail> _baseOrderList;
        public ObservableCollection<OrderDetail> FilteredOrderList { get; set; }
        public ICommand RedirectToDetailPageCommand { get; set; }

        private string _orderIdentifierInput;
        private string _userEmailInput;
        private bool _isIncompleteCheckbox;
        private bool _isExpiredCheckbox;

        public string OrderIDInput
        {
            get => _orderIdentifierInput;
            set
            {
                _orderIdentifierInput = value;
                OnPropertyChanged();
                ReapplyFilters();
            }
        }

        public string UserEmailInput
        {
            get => _userEmailInput;
            set
            {
                _userEmailInput = value;
                OnPropertyChanged();
                ReapplyFilters();
            }
        }

        public bool IsIncompleteCheckbox
        {
            get => _isIncompleteCheckbox;
            set
            {
                _isIncompleteCheckbox = value;
                OnPropertyChanged();
                ReapplyFilters();
            }
        }

        public bool IsExpiredCheckbox
        {
            get => _isExpiredCheckbox;
            set
            {
                _isExpiredCheckbox = value;
                OnPropertyChanged();
                ReapplyFilters();
            }
        }

        public OrderManagementViewModel(IOrderService orderBusinessLogicService)
        {
            _orderBusinessLogicService = orderBusinessLogicService;
            _baseOrderList = new List<OrderDetail>();
            FilteredOrderList = new ObservableCollection<OrderDetail>();
            RedirectToDetailPageCommand = new RelayCommandWithOneParameter<OrderDetail>(OnClickDetailButton);

            LoadInitialData();
        }

        private void LoadInitialData()
        {
            foreach (Order currentOrder in _orderBusinessLogicService.OrdersRepository.GetAllOrders())
            {
                int userIdentifier = _orderBusinessLogicService.OrdersRepository.GetOrder(currentOrder.Id).ClientId;
                string currentUserEmail = _orderBusinessLogicService.UsersRepository.GetUserById(userIdentifier).Email;

                OrderDetail currentOrderDetail = new OrderDetail(currentOrder, currentUserEmail);
                _baseOrderList.Add(currentOrderDetail);
                FilteredOrderList.Add(currentOrderDetail);
            }
        }

        public delegate void PageChangedEventHandler(Tuple<IOrderService, OrderDetail> navigationParameters);
        public event PageChangedEventHandler ClickDetailButton;

        public virtual void OnClickDetailButton(OrderDetail chosenOrder)
        {
            ClickDetailButton?.Invoke(new Tuple<IOrderService, OrderDetail>(_orderBusinessLogicService, chosenOrder));
        }

        private void ReapplyFilters()
        {
            IEnumerable<OrderDetail> results = _baseOrderList;

            if (int.TryParse(_orderIdentifierInput, out int inputtedIdentifier))
            {
                results = results.Where(order => order.OrderIdentifier == inputtedIdentifier);
            }

            if (!string.IsNullOrEmpty(_userEmailInput))
            {
                results = results.Where(order => order.UserEmailAddress.Contains(_userEmailInput, StringComparison.OrdinalIgnoreCase));
            }

            if (_isIncompleteCheckbox)
            {
                results = results.Where(order => !order.IsComplete && !order.IsExpired);
            }

            if (_isExpiredCheckbox)
            {
                results = results.Where(order => order.IsExpired);
            }

            FilteredOrderList.Clear();
            foreach (OrderDetail matchedOrder in results)
            {
                FilteredOrderList.Add(matchedOrder);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}