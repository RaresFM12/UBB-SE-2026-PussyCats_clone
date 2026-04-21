using NUnit.Framework;
using Moq;
using PharmacyApp.Features.Products_Catalogue.ViewModels;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Models;
using System;

namespace PharmacyApp.Tests.UnitTests
{
    [TestFixture]
    public class ProductDetailsPageViewModelTest
    {
        private Mock<IOrderService> _mockOrderService;
        private User _validUser;
        private Item _validItem;
        private ProductDetailsPageViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            _mockOrderService = new Mock<IOrderService>();

            // 1. Create a valid dummy user using the explicit constructor
            _validUser = new User(
                id: 1,
                email: "test@pharmacy.com",
                phoneNumber: "0700000000",
                passwordHash: "dummyHash123!",
                isAdmin: false,
                isDisabled: false,
                userName: "testuser",
                discountNotifications: false,
                loyaltyPoints: 0
            );

            // 2. Create a valid dummy item using the explicit constructor
            // We use named arguments here to skip the optional string parameters (label, description, image)
            // and jump straight to setting the quantity to 20.
            _validItem = new Item(
                id: 100,
                name: "Test Medicine",
                producer: "Test Producer",
                category: "Medicine",
                price: 50.0f,
                nrOfPills: 30,
                quantity: 20
            );

            _viewModel = new ProductDetailsPageViewModel();
            _viewModel.Initialize(_validItem, _validUser, _mockOrderService.Object);
        }

        // ── F4.5 Validation: Null User ──────────────────────────────────────────

        [Test]
        public void TryAddToBasket_UserIsNull_ReturnsSuccessFalse()
        {
            _viewModel.Initialize(_validItem, null, _mockOrderService.Object);
            var result = _viewModel.TryAddToBasket("5");
            Assert.IsFalse(result.success);
        }

        [Test]
        public void TryAddToBasket_UserIsNull_ReturnsNavigateToLoginTrue()
        {
            _viewModel.Initialize(_validItem, null, _mockOrderService.Object);
            var result = _viewModel.TryAddToBasket("5");
            Assert.IsTrue(result.navigateToLogin);
        }

        // ── F4.5 Validation: Invalid Quantity Input ─────────────────────────────

        [Test]
        public void TryAddToBasket_QuantityIsNotANumber_ReturnsSuccessFalse()
        {
            var result = _viewModel.TryAddToBasket("abc");
            Assert.IsFalse(result.success);
        }

        [Test]
        public void TryAddToBasket_QuantityIsNotANumber_SetsInvalidQuantityError()
        {
            _viewModel.TryAddToBasket("abc");
            Assert.AreEqual("Invalid quantity selected", _viewModel.ErrorText);
        }

        [Test]
        public void TryAddToBasket_QuantityIsZero_ReturnsSuccessFalse()
        {
            var result = _viewModel.TryAddToBasket("0");
            Assert.IsFalse(result.success);
        }

        // ── F4.5 Validation: Quantity Limits ────────────────────────────────────

        [Test]
        public void TryAddToBasket_QuantityExceedsFifty_ReturnsSuccessFalse()
        {
            // Requirement F4.5: Quantity cannot be bigger than 50
            var result = _viewModel.TryAddToBasket("51");
            Assert.IsFalse(result.success);
        }

        [Test]
        public void TryAddToBasket_QuantityExceedsStock_ReturnsSuccessFalse()
        {
            // Item has 20 stock. Try to add 21.
            var result = _viewModel.TryAddToBasket("21");
            Assert.IsFalse(result.success);
        }

        // ── F4.5 Validation: Success ────────────────────────────────────────────

        [Test]
        public void TryAddToBasket_ValidQuantity_ReturnsSuccessTrue()
        {
            var result = _viewModel.TryAddToBasket("5");
            Assert.IsTrue(result.success);
        }

        [Test]
        public void TryAddToBasket_ValidQuantity_CallsOrderServiceOnce()
        {
            _viewModel.TryAddToBasket("5");

            // Verifies the mock was called exactly once with Item ID 100 and Qty 5
            _mockOrderService.Verify(s => s.AddToBasket(100, 5), Times.Once());
        }

        [Test]
        public void TryAddToBasket_ItemAlreadyInBasket_SetsAlreadyInBasketError()
        {
            _mockOrderService.Setup(s => s.AddToBasket(It.IsAny<int>(), It.IsAny<int>()))
                             .Throws(new ArgumentException());

            _viewModel.TryAddToBasket("5");

            Assert.AreEqual("Item already in basket", _viewModel.ErrorText);
        }
    }
}