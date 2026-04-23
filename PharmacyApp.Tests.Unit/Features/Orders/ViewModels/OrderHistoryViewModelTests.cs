using Moq;
using NUnit.Framework;
using PharmacyApp.Features.Orders.ViewModels;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PharmacyApp.Tests.Unit.Features.Orders.ViewModels
{
    [TestFixture]
    public class OrderHistoryViewModelTests
    {
        private Mock<IOrderService> _orderBusinessLogicServiceMock;
        private User _testActiveUser;

        [SetUp]
        public void Setup()
        {
            _orderBusinessLogicServiceMock = new Mock<IOrderService>();

            // FIX FINAL: Am adaugat al 9-lea parametru (0) pentru 'loyaltyPoints'
            // Ordinea: id, email, telefon, parola, admin, disabled, username, discount, loyaltyPoints
            _testActiveUser = new User(
                1,
                "vlad@test.com",
                "0700000000",
                "parola_hash",
                false,
                false,
                "VladUser",
                false,
                0
            );

            _orderBusinessLogicServiceMock.Setup(service => service.ActiveUser).Returns(_testActiveUser);
        }

        private Order CreateSampleOrder(int orderIdentifier, bool isExpired)
        {
            var pickUpDate = isExpired
                ? DateOnly.FromDateTime(DateTime.Now.AddDays(-10))
                : DateOnly.FromDateTime(DateTime.Now.AddDays(2));

            return new Order(orderIdentifier, _testActiveUser.Id, pickUpDate);
        }

        [Test]
        public void Constructor_WhenInitialized_LoadsOrdersForActiveUser()
        {
            // Arrange
            var retrievedOrdersList = new List<Order>
            {
                CreateSampleOrder(10, false),
                CreateSampleOrder(11, false)
            };

            _orderBusinessLogicServiceMock.Setup(service => service.GetClientOrders(_testActiveUser.Id))
                                          .Returns(retrievedOrdersList);

            // Act - Redenumit pentru a evita abrevierea 'vm'
            var orderHistoryViewModel = new OrderHistoryViewModel(_orderBusinessLogicServiceMock.Object);

            // Assert - Sintaxa NUnit 4
            Assert.That(orderHistoryViewModel.UserOrderHistoryCollection.Count, Is.EqualTo(2));
        }

        [Test]
        public void IsExpiredOrdersFilterActive_SetToTrue_FiltersOnlyExpiredOrders()
        {
            // Arrange
            var mixedOrdersList = new List<Order>
            {
                CreateSampleOrder(1, true),
                CreateSampleOrder(2, false)
            };

            _orderBusinessLogicServiceMock.Setup(service => service.GetClientOrders(_testActiveUser.Id))
                                          .Returns(mixedOrdersList);

            var orderHistoryViewModel = new OrderHistoryViewModel(_orderBusinessLogicServiceMock.Object);

            // Act
            orderHistoryViewModel.IsExpiredOrdersFilterActive = true;

            // Assert
            Assert.That(orderHistoryViewModel.UserOrderHistoryCollection.All(order => order.IsExpired), Is.True);
            Assert.That(orderHistoryViewModel.UserOrderHistoryCollection.Count, Is.EqualTo(1));
        }

        [Test]
        public void CancelOrderCommand_ValidOrder_CallsServiceToCancelOrder()
        {
            // Arrange
            var orderToBeCancelled = CreateSampleOrder(100, false);
            _orderBusinessLogicServiceMock.Setup(service => service.GetClientOrders(_testActiveUser.Id))
                                          .Returns(new List<Order> { orderToBeCancelled });

            var orderHistoryViewModel = new OrderHistoryViewModel(_orderBusinessLogicServiceMock.Object);

            // Act
            orderHistoryViewModel.CancelOrderCommand.Execute(orderToBeCancelled);

            // Assert
            _orderBusinessLogicServiceMock.Verify(service => service.CancelOrder(100), Times.Once);
        }
    }
}