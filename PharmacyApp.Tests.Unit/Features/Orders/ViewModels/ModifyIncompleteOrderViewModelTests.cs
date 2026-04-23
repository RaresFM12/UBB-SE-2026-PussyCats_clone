using Moq;
using NUnit.Framework;
using PharmacyApp.Features.Orders.ViewModels;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PharmacyApp.Tests.Unit.Features.Orders
{
    [TestFixture]
    public class ModifyIncompleteOrderViewModelTests
    {
        private Mock<IOrderService> _orderServiceMock;
        private const int _testOrderIdentifier = 10;
        private const int _testClientIdentifier = 100;

        [SetUp]
        public void Setup()
        {
            _orderServiceMock = new Mock<IOrderService>();
        }

        private Order CreateTestOrder(int orderIdentifier, DateOnly pickUpDate)
        {
            return new Order(orderIdentifier, _testClientIdentifier, pickUpDate);
        }

        private ItemDetail CreateTestItemDetail(int itemIdentifier, float finalPrice)
        {
            return new ItemDetail(itemIdentifier, "image.png", "Description", 1, finalPrice);
        }

        [Test]
        public void Constructor_ValidOrderIdentifier_LoadsOrderItemsFromService()
        {
            // Arrange
            var orderUnderTest = CreateTestOrder(_testOrderIdentifier, DateOnly.FromDateTime(DateTime.Now));
            orderUnderTest.AddItemToOrder(1, 1, 50.00f);

            _orderServiceMock.Setup(service => service.GetOrderById(_testOrderIdentifier)).Returns(orderUnderTest);

            // REPARARE AMBIGUITATE CONSTRUCTOR (10 parametri conform erorii tale):
            // 1. int (Identifier), 2. string (Name), 3. string (Producer), 4. string (Category), 
            // 5. float (Price), 6. int (NumberOfPills), 7. string (Image), 8. string (Label), 
            // 9. string (Description), 10. float (Discount)
            var pharmacyItemUnderTest = new Item(
                1,
                "Aspirin",
                "Bayer",
                "Analgesic",
                10.0f,
                30,
                "path/to/image.png",
                "LabelText",
                "ItemDescription",
                0.0f
            );

            _orderServiceMock.Setup(service => service.GetItemById(1)).Returns(pharmacyItemUnderTest);

            var modifyIncompleteOrderViewModel = new ModifyIncompleteOrderViewModel(_orderServiceMock.Object, _testOrderIdentifier);

            Assert.That(modifyIncompleteOrderViewModel.OrderItems.Count, Is.EqualTo(1));
        }

        [Test]
        public void Constructor_ValidOrder_SetsCorrectTotalPriceString()
        {
            // Arrange
            var orderUnderTest = CreateTestOrder(_testOrderIdentifier, DateOnly.FromDateTime(DateTime.Now));
            orderUnderTest.AddItemToOrder(1, 1, 25.50f);
            orderUnderTest.AddItemToOrder(2, 1, 10.00f);

            _orderServiceMock.Setup(service => service.GetOrderById(_testOrderIdentifier)).Returns(orderUnderTest);

            var pharmacyItemUnderTest = new Item(1, "Med", "Prod", "Cat", 10.0f, 30, "img", "lab", "desc", 0.0f);
            _orderServiceMock.Setup(service => service.GetItemById(It.IsAny<int>())).Returns(pharmacyItemUnderTest);

            // Act
            var modifyIncompleteOrderViewModel = new ModifyIncompleteOrderViewModel(_orderServiceMock.Object, _testOrderIdentifier);

            // Assert - Pastram "RON" conform codului tau original din ViewModel
            Assert.That(modifyIncompleteOrderViewModel.TotalPriceString, Is.EqualTo("35.50 RON"));
        }

        [Test]
        public void RemoveItemCommand_ValidItem_RemovesFromOrderItemsCollection()
        {
            // Arrange
            var orderUnderTest = CreateTestOrder(_testOrderIdentifier, DateOnly.FromDateTime(DateTime.Now));
            _orderServiceMock.Setup(service => service.GetOrderById(_testOrderIdentifier)).Returns(orderUnderTest);

            var modifyIncompleteOrderViewModel = new ModifyIncompleteOrderViewModel(_orderServiceMock.Object, _testOrderIdentifier);
            var itemDetailToBeRemoved = CreateTestItemDetail(1, 10.00f);
            modifyIncompleteOrderViewModel.OrderItems.Add(itemDetailToBeRemoved);

            // Act
            modifyIncompleteOrderViewModel.RemoveItemCommand.Execute(itemDetailToBeRemoved);

            // Assert
            Assert.That(modifyIncompleteOrderViewModel.OrderItems.Count, Is.EqualTo(0));
        }

        [Test]
        public void RemoveItemCommand_ValidItem_UpdatesTotalPriceString()
        {
            // Arrange
            var orderUnderTest = CreateTestOrder(_testOrderIdentifier, DateOnly.FromDateTime(DateTime.Now));
            _orderServiceMock.Setup(service => service.GetOrderById(_testOrderIdentifier)).Returns(orderUnderTest);

            var modifyIncompleteOrderViewModel = new ModifyIncompleteOrderViewModel(_orderServiceMock.Object, _testOrderIdentifier);
            var firstItemDetail = CreateTestItemDetail(1, 10.00f);
            var secondItemDetail = CreateTestItemDetail(2, 20.00f);
            modifyIncompleteOrderViewModel.OrderItems.Add(firstItemDetail);
            modifyIncompleteOrderViewModel.OrderItems.Add(secondItemDetail);

            // Act
            modifyIncompleteOrderViewModel.RemoveItemCommand.Execute(firstItemDetail);

            // Assert
            Assert.That(modifyIncompleteOrderViewModel.TotalPriceString, Is.EqualTo("20.00 RON"));
        }

        [Test]
        public void TotalPriceString_SetNewValue_RaisesPropertyChanged()
        {
            // Arrange
            var orderUnderTest = CreateTestOrder(_testOrderIdentifier, DateOnly.FromDateTime(DateTime.Now));
            _orderServiceMock.Setup(service => service.GetOrderById(_testOrderIdentifier)).Returns(orderUnderTest);
            var modifyIncompleteOrderViewModel = new ModifyIncompleteOrderViewModel(_orderServiceMock.Object, _testOrderIdentifier);

            bool propertyChangedWasRaised = false;
            modifyIncompleteOrderViewModel.PropertyChanged += (sender, propertyChangedEventArgs) =>
            {
                if (propertyChangedEventArgs.PropertyName == nameof(ModifyIncompleteOrderViewModel.TotalPriceString))
                {
                    propertyChangedWasRaised = true;
                }
            };

            // Act
            modifyIncompleteOrderViewModel.TotalPriceString = "100.00 RON";

            // Assert
            Assert.That(propertyChangedWasRaised, Is.True);
        }
    }
}