using Moq;
using NUnit.Framework;
using PharmacyApp.Features.Orders.ViewModels;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;

namespace PharmacyApp.Tests.Unit.Features.Orders.ViewModels
{
    [TestFixture]
    public class ResubmitOrderViewModelTests
    {
        private Mock<IOrderService> _orderBusinessLogicServiceMock;
        private const int _testOrderIdentifier = 55;

        [SetUp]
        public void Setup()
        {
            _orderBusinessLogicServiceMock = new Mock<IOrderService>();
        }

        [Test]
        public void Constructor_WhenCalled_CalculatesTotalAccumulatedPriceCorrectly()
        {
            // Arrange
            // 1. Cream comanda expirata cu date de test
            var expiredOrderInformation = new Order(_testOrderIdentifier, 101, DateOnly.FromDateTime(DateTime.Now.AddDays(-10)));

            // Adaugam produse (ID, Cantitate, Pret)
            expiredOrderInformation.AddItemToOrder(1, 2, 20.00f);
            expiredOrderInformation.AddItemToOrder(2, 1, 15.50f);

            _orderBusinessLogicServiceMock.Setup(service => service.GetOrderById(_testOrderIdentifier))
                                          .Returns(expiredOrderInformation);

            // 2. Cream obiectul Item folosind fix cei 10 parametri ceruti de constructorul vostru
            // Ordinea: ID, Nume, Producator, Categorie, Pret, NrPastile, Imagine, Label, Descriere, Discount
            var pharmacyItemUnderTesting = new Item(
                1,
                "Paracetamol",
                "Zentiva",
                "Analgezic",
                10.0f,
                20,
                "image.png",
                "Label",
                "Description",
                0.0f
            );

            _orderBusinessLogicServiceMock.Setup(service => service.GetItemById(It.IsAny<int>()))
                                          .Returns(pharmacyItemUnderTesting);

            // Act
            var resubmitOrderViewModel = new ResubmitOrderViewModel(_orderBusinessLogicServiceMock.Object, _testOrderIdentifier);

            // Assert
            // Calcul: (2 * 20.00) + (1 * 15.50) = 55.50
            Assert.That(resubmitOrderViewModel.TotalAccumulatedPriceString, Is.EqualTo("55.50 RON"));
        }

        [Test]
        public void Constructor_OrderHasItems_PopulatesOrderItemsDetailList()
        {
            // Arrange
            var expiredOrderInformation = new Order(_testOrderIdentifier, 101, DateOnly.FromDateTime(DateTime.Now));
            expiredOrderInformation.AddItemToOrder(1, 1, 10.00f);

            _orderBusinessLogicServiceMock.Setup(service => service.GetOrderById(_testOrderIdentifier))
                                          .Returns(expiredOrderInformation);

            var pharmacyItemUnderTesting = new Item(
                1, "Aspirin", "Bayer", "Analgezic", 5.0f, 10, "img.png", "Lab", "Desc", 0.0f
            );

            _orderBusinessLogicServiceMock.Setup(service => service.GetItemById(1))
                                          .Returns(pharmacyItemUnderTesting);

            // Act
            var resubmitOrderViewModel = new ResubmitOrderViewModel(_orderBusinessLogicServiceMock.Object, _testOrderIdentifier);

            // Assert
            Assert.That(resubmitOrderViewModel.OrderItemsDetailList.Count, Is.EqualTo(1));
        }
    }
}