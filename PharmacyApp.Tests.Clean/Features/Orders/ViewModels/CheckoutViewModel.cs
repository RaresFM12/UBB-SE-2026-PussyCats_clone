using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Orders.ViewModels;

namespace PharmacyApp.Tests.Unit.Features.Orders.ViewModels
{
    [TestFixture]
    public class CheckoutViewModelTests
    {
        private Mock<IOrderService> mockOrderService;

        [SetUp]
        public void Setup()
        {
            mockOrderService = new Mock<IOrderService>();
        }

        private static BasketItemViewModel MakeBasketItem(int id, float priceBefore, float priceAfter)
        {
            BasketItemViewModel item = new BasketItemViewModel(id, "img", "Name", "Prod", 1, 0f, 0f, 0f, priceBefore);
            item.SetFinalPrices(priceBefore, priceAfter);
            return item;
        }

        [Test]
        public void Constructor_SetsBasketItemsFromService()
        {
            List<BasketItemViewModel> items = new List<BasketItemViewModel> { MakeBasketItem(1, 10f, 8f) };

            mockOrderService.Setup(service => service.GetBasketItems()).Returns(items);
            mockOrderService
                .Setup(service => service.CalculateBasketTotalSum(It.IsAny<IEnumerable<BasketItemViewModel>>()))
                .Returns(new Tuple<float, float>(10f, 8f));

            CheckoutViewModel viewModel = new CheckoutViewModel(mockOrderService.Object);

            Assert.That(viewModel.BasketItems.Count, Is.EqualTo(1));
        }

        [Test]
        public void Constructor_SetsTotalPriceStringToDiscountedTotal()
        {
            mockOrderService.Setup(service => service.GetBasketItems()).Returns(new List<BasketItemViewModel>());
            mockOrderService
                .Setup(service => service.CalculateBasketTotalSum(It.IsAny<IEnumerable<BasketItemViewModel>>()))
                .Returns(new Tuple<float, float>(50f, 37.5f));

            CheckoutViewModel viewModel = new CheckoutViewModel(mockOrderService.Object);

            Assert.That(viewModel.TotalPriceString, Is.EqualTo("37.50 RON"));
        }

        [Test]
        public void Constructor_EmptyBasket_SetsTotalPriceStringToZero()
        {
            mockOrderService.Setup(service => service.GetBasketItems()).Returns(new List<BasketItemViewModel>());
            mockOrderService
                .Setup(service => service.CalculateBasketTotalSum(It.IsAny<IEnumerable<BasketItemViewModel>>()))
                .Returns(new Tuple<float, float>(0f, 0f));

            CheckoutViewModel viewModel = new CheckoutViewModel(mockOrderService.Object);

            Assert.That(viewModel.TotalPriceString, Is.EqualTo("0.00 RON"));
        }
    }
}