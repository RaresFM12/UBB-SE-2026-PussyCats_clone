using Moq;
using NUnit.Framework;
using PharmacyApp.Common.Services;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Orders.ViewModels;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;

namespace PharmacyApp.Tests.Unit.Features.Orders.ViewModels
{
    [TestFixture]
    public class CheckoutViewModelTests
    {
        private const int DefaultUserIdentifier = 1;
        private const int DefaultItemIdentifier = 1;
        private const int DefaultItemQuantity = 1;
        private const int DaysToAddToCurrentDate = 2;
        private const int ZeroLoyaltyPoints = 0;
        private const float ZeroDiscountAmount = 0f;
        private const float DefaultTotalPriceBeforeDiscount = 100f;
        private const float DefaultTotalPriceAfterDiscount = 80f;

        private Mock<IOrderService> mockOrderService;
        private Mock<INavigationService> mockNavigationService;
        private User standardTestUser;
        private CheckoutViewModel checkoutViewModel;

        private static User CreateStandardUser()
        {
            return new User(DefaultUserIdentifier, "test@test.com", "1234567890", "hashedPassword", false, false, "TestUser", false, ZeroLoyaltyPoints);
        }

        [SetUp]
        public void Setup()
        {
            mockOrderService = new Mock<IOrderService>();
            mockNavigationService = new Mock<INavigationService>();
            standardTestUser = CreateStandardUser();

            checkoutViewModel = new CheckoutViewModel(
                mockOrderService.Object,
                mockNavigationService.Object,
                standardTestUser);
        }

        [Test]
        public void PlaceOrderCommand_WhenBasketIsEmpty_DoesNotCallPlaceOrderFromBasket()
        {
            standardTestUser.Basket.Clear();

            checkoutViewModel.PlaceOrderCommand.Execute(null);

            mockOrderService.Verify(service => service.PlaceOrderFromBasket(It.IsAny<DateOnly>()), Times.Never);
        }

        [Test]
        public void PlaceOrderCommand_WhenBasketHasItems_CallsPlaceOrderFromBasketWithSelectedDate()
        {
            standardTestUser.AddItemToBasket(DefaultItemIdentifier, DefaultItemQuantity, ZeroDiscountAmount);
            DateTime selectedPickUpDate = DateTime.Now.AddDays(DaysToAddToCurrentDate);
            checkoutViewModel.SelectedDate = selectedPickUpDate;

            checkoutViewModel.PlaceOrderCommand.Execute(null);

            mockOrderService.Verify(service => service.PlaceOrderFromBasket(DateOnly.FromDateTime(selectedPickUpDate)), Times.Once);
        }

        [Test]
        public void PlaceOrderCommand_AfterSuccessfulOrder_NavigatesToOrderSuccessPage()
        {
            standardTestUser.AddItemToBasket(DefaultItemIdentifier, DefaultItemQuantity, ZeroDiscountAmount);

            checkoutViewModel.PlaceOrderCommand.Execute(null);

            mockNavigationService.Verify(navigation => navigation.NavigateTo("OrderSuccessPage"), Times.Once);
        }

        [Test]
        public void TotalPrice_WhenItemsInBasket_CalculatesCorrectSum()
        {
            Tuple<float, float> calculatedTotals = new Tuple<float, float>(DefaultTotalPriceBeforeDiscount, DefaultTotalPriceAfterDiscount);

            mockOrderService.Setup(service => service.CalculateBasketTotalSum(It.IsAny<List<BasketItemViewModel>>()))
                            .Returns(calculatedTotals);

            Assert.AreEqual(DefaultTotalPriceAfterDiscount, checkoutViewModel.TotalPriceAfterDiscount);
        }
    }
}