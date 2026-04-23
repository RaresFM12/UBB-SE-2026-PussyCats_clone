using Moq;
using NUnit.Framework;
using PharmacyApp.Features.Orders.ViewModels;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PharmacyApp.Tests.Unit.Features.Orders.ViewModels
{
    [TestFixture]
    public class OrderManagementViewModelTests
    {
        private Mock<IOrderService> _orderBusinessLogicServiceMock;
        private List<Order> _testOrdersCollection;

        [SetUp]
        public void Setup()
        {
            _orderBusinessLogicServiceMock = new Mock<IOrderService>();

            // Date de test
            var activeOrder = new Order(10, 1, DateOnly.FromDateTime(DateTime.Now.AddDays(1)));
            var expiredOrder = new Order(20, 2, DateOnly.FromDateTime(DateTime.Now.AddDays(-10)));
            _testOrdersCollection = new List<Order> { activeOrder, expiredOrder };

            // Setup Mock pentru Repository access
            _orderBusinessLogicServiceMock.Setup(service => service.OrdersRepository.GetAllOrders())
                                          .Returns(_testOrdersCollection);

            // Useri de test (9 parametri: id, email, phone, pass, admin, disabled, user, discount, loyalty)
            var firstUser = new User(1, "vlad@test.com", "0700", "hash", false, false, "VladUser", false, 0);
            var secondUser = new User(2, "admin@test.com", "0800", "hash", false, false, "AdminUser", false, 0);

            _orderBusinessLogicServiceMock.Setup(service => service.UsersRepository.GetUserById(1)).Returns(firstUser);
            _orderBusinessLogicServiceMock.Setup(service => service.UsersRepository.GetUserById(2)).Returns(secondUser);

            _orderBusinessLogicServiceMock.Setup(service => service.OrdersRepository.GetOrder(10)).Returns(activeOrder);
            _orderBusinessLogicServiceMock.Setup(service => service.OrdersRepository.GetOrder(20)).Returns(expiredOrder);
        }

        [Test]
        public void Constructor_WhenCalled_PopulatesFilteredOrderListWithAllAvailableOrders()
        {
            // Act
            var orderManagementViewModel = new OrderManagementViewModel(_orderBusinessLogicServiceMock.Object);

            // Assert
            Assert.That(orderManagementViewModel.FilteredOrderList.Count, Is.EqualTo(2));
        }

        [Test]
        public void OrderIDInput_ValidIdentifierProvided_FiltersCollectionToSingleMatchingOrder()
        {
            // Arrange
            var orderManagementViewModel = new OrderManagementViewModel(_orderBusinessLogicServiceMock.Object);

            // Act
            orderManagementViewModel.OrderIDInput = "10";

            // Assert
            Assert.That(orderManagementViewModel.FilteredOrderList.Count, Is.EqualTo(1));
            Assert.That(orderManagementViewModel.FilteredOrderList.First().OrderIdentifier, Is.EqualTo(10));
        }

        [Test]
        public void UserEmailInput_MatchingEmailProvided_FiltersCollectionCorrectly()
        {
            // Arrange
            var orderManagementViewModel = new OrderManagementViewModel(_orderBusinessLogicServiceMock.Object);

            // Act
            orderManagementViewModel.UserEmailInput = "admin@test.com";

            // Assert
            Assert.That(orderManagementViewModel.FilteredOrderList.All(order => order.UserEmailAddress == "admin@test.com"), Is.True);
        }

        [Test]
        public void IsExpiredCheckbox_SetToTrue_ShowsOnlyOrdersThatAreMarkedAsExpired()
        {
            // Arrange
            var orderManagementViewModel = new OrderManagementViewModel(_orderBusinessLogicServiceMock.Object);

            // Act
            orderManagementViewModel.IsExpiredCheckbox = true;

            // Assert
            Assert.That(orderManagementViewModel.FilteredOrderList.All(order => order.IsExpired), Is.True);
            Assert.That(orderManagementViewModel.FilteredOrderList.Count, Is.EqualTo(1));
        }
    }
}