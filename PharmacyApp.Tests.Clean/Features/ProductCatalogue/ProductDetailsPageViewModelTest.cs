using System.Globalization;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Products_Catalogue.Service;
using PharmacyApp.Features.Products_Catalogue.ViewModels;
using PharmacyApp.Models;

namespace PharmacyApp.Tests.UnitTests
{
    [TestFixture]
    public class ProductDetailsPageViewModelTest
    {
        private Mock<IOrderService> mockOrderService;
        private User validUser;
        private Item validItem;
        private ProductDetailsPageViewModel viewModel;

        [SetUp]
        public void Setup()
        {
            var culture = new CultureInfo("ro-RO");
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            mockOrderService = new Mock<IOrderService>();

            validUser = new User(
                id: 1,
                email: "test@pharmacy.com",
                phoneNumber: "0700000000",
                passwordHash: "dummyHash123!",
                isAdmin: false,
                isDisabled: false,
                userName: "testuser",
                discountNotifications: false,
                loyaltyPoints: 0);

            validItem = new Item(
                id: 100,
                name: "Test Medicine",
                producer: "Test Producer",
                category: "Medicine",
                price: 50.0f,
                numberOfPills: 30,
                quantity: 20);

            viewModel = new ProductDetailsPageViewModel();
            viewModel.Initialize(validItem, validUser, mockOrderService.Object);
        }

        private Item CreateItemWithQuantity(int quantity, float discount = 0f)
        {
            return new Item(
                id: 100,
                name: "Test Med",
                producer: "Prod",
                category: "Cat",
                price: 50.0f,
                numberOfPills: 30,
                discount: discount,
                quantity: quantity);
        }

        [Test]
        public void TryAddToBasket_UserIsNull_ReturnsSuccessFalse()
        {
            viewModel.Initialize(validItem, null, mockOrderService.Object);
            var result = viewModel.TryAddToBasket("5");

            Assert.That(result.success, Is.False);
        }

        [Test]
        public void TryAddToBasket_UserIsNull_ReturnsNavigateToLoginTrue()
        {
            viewModel.Initialize(validItem, null, mockOrderService.Object);
            var result = viewModel.TryAddToBasket("5");

            Assert.That(result.navigateToLogin, Is.True);
        }

        [Test]
        public void TryAddToBasket_QuantityIsNotANumber_ReturnsSuccessFalse()
        {
            var result = viewModel.TryAddToBasket("abc");

            Assert.That(result.success, Is.False);
        }

        [Test]
        public void TryAddToBasket_QuantityIsNotANumber_SetsInvalidQuantityError()
        {
            viewModel.TryAddToBasket("abc");

            Assert.That(viewModel.ErrorText, Is.EqualTo("Invalid quantity selected"));
        }

        [Test]
        public void TryAddToBasket_QuantityIsZero_ReturnsSuccessFalse()
        {
            var result = viewModel.TryAddToBasket("0");

            Assert.That(result.success, Is.False);
        }

        [Test]
        public void TryAddToBasket_QuantityExceedsFifty_ReturnsSuccessFalse()
        {
            var result = viewModel.TryAddToBasket("51");

            Assert.That(result.success, Is.False);
        }

        [Test]
        public void TryAddToBasket_QuantityExceedsStock_ReturnsSuccessFalse()
        {
            var result = viewModel.TryAddToBasket("21");

            Assert.That(result.success, Is.False);
        }

        [Test]
        public void TryAddToBasket_ValidQuantity_ReturnsSuccessTrue()
        {
            var result = viewModel.TryAddToBasket("5");

            Assert.That(result.success, Is.True);
        }

        [Test]
        public void TryAddToBasket_ValidQuantity_CallsOrderServiceOnce()
        {
            viewModel.TryAddToBasket("5");

            mockOrderService.Verify(service => service.AddToBasket(100, 5), Times.Once());
        }

        [Test]
        public void TryAddToBasket_ItemAlreadyInBasket_SetsAlreadyInBasketError()
        {
            mockOrderService
                .Setup(service => service.AddToBasket(It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new ArgumentException());

            viewModel.TryAddToBasket("5");

            Assert.That(viewModel.ErrorText, Is.EqualTo("Item already in basket"));
        }

        [Test]
        public void FinalPriceDisplay_ItemIsNull_ReturnsEmptyString()
        {
            viewModel.Initialize(null, validUser, mockOrderService.Object);

            Assert.That(viewModel.FinalPriceDisplay, Is.EqualTo(string.Empty));
        }

        [Test]
        public void FinalPriceDisplay_ItemHasDiscount_ReturnsDiscountedPriceString()
        {
            var item = CreateItemWithQuantity(10, discount: 20f);
            viewModel.Initialize(item, validUser, mockOrderService.Object);

            Assert.That(viewModel.FinalPriceDisplay, Is.EqualTo("40,00 lei"));
        }

        [Test]
        public void OldPriceDisplay_HasDiscount_ReturnsOriginalPriceString()
        {
            var item = CreateItemWithQuantity(10, discount: 20f);
            viewModel.Initialize(item, validUser, mockOrderService.Object);

            Assert.That(viewModel.OldPriceDisplay, Is.EqualTo("50,00 lei"));
        }

        [Test]
        public void OldPriceDisplay_NoDiscount_ReturnsEmptyString()
        {
            var item = CreateItemWithQuantity(10, discount: 0f);
            viewModel.Initialize(item, validUser, mockOrderService.Object);

            Assert.That(viewModel.OldPriceDisplay, Is.EqualTo(string.Empty));
        }

        [Test]
        public void DiscountDisplay_HasDiscount_ReturnsPercentageOffString()
        {
            var item = CreateItemWithQuantity(10, discount: 20f);
            viewModel.Initialize(item, validUser, mockOrderService.Object);

            Assert.That(viewModel.DiscountDisplay, Is.EqualTo("20% off"));
        }

        [Test]
        public void DiscountDisplay_NoDiscount_ReturnsEmptyString()
        {
            var item = CreateItemWithQuantity(10, discount: 0f);
            viewModel.Initialize(item, validUser, mockOrderService.Object);

            Assert.That(viewModel.DiscountDisplay, Is.EqualTo(string.Empty));
        }

        [Test]
        public void HasDiscount_DiscountGreaterThanZero_ReturnsTrue()
        {
            var item = CreateItemWithQuantity(10, discount: 5f);
            viewModel.Initialize(item, validUser, mockOrderService.Object);

            Assert.That(viewModel.HasDiscount, Is.True);
        }

        [Test]
        public void HasDiscount_DiscountIsZero_ReturnsFalse()
        {
            var item = CreateItemWithQuantity(10, discount: 0f);
            viewModel.Initialize(item, validUser, mockOrderService.Object);

            Assert.That(viewModel.HasDiscount, Is.False);
        }

        [Test]
        public void SubstancesText_SubstancesDictIsEmpty_ReturnsNone()
        {
            var item = CreateItemWithQuantity(10);
            item.ActiveSubstances.Clear(); // Ensure it's empty
            viewModel.Initialize(item, validUser, mockOrderService.Object);

            Assert.That(viewModel.SubstancesText, Is.EqualTo("None"));
        }

        [Test]
        public void SubstancesText_HasSubstances_ReturnsFormattedString()
        {
            var item = CreateItemWithQuantity(10);
            item.ActiveSubstances.Clear();
            item.ActiveSubstances.Add("Aspirin", 500f);
            item.ActiveSubstances.Add("Caffeine", 50f);
            viewModel.Initialize(item, validUser, mockOrderService.Object);

            Assert.That(viewModel.SubstancesText, Is.EqualTo("Aspirin (500), Caffeine (50)"));
        }

        [Test]
        public void ProductName_ItemIsNull_ReturnsEmptyString()
        {
            viewModel.Initialize(null, validUser, mockOrderService.Object);
            Assert.That(viewModel.ProductName, Is.EqualTo(string.Empty));
        }

        [Test]
        public void DescriptionText_ItemIsNull_ReturnsEmptyString()
        {
            viewModel.Initialize(null, validUser, mockOrderService.Object);
            Assert.That(viewModel.DescriptionText, Is.EqualTo(string.Empty));
        }

        [Test]
        public void HasDiscount_ItemIsNull_ReturnsFalse()
        {
            viewModel.Initialize(null, validUser, mockOrderService.Object);
            Assert.That(viewModel.HasDiscount, Is.False);
        }

        [Test]
        public void IsAddToCartEnabled_ItemIsNull_ReturnsFalse()
        {
            viewModel.Initialize(null, validUser, mockOrderService.Object);
            Assert.That(viewModel.IsAddToCartEnabled, Is.False);
        }

        [Test]
        public void IsAddToCartEnabled_StockIsZero_ReturnsFalse()
        {
            var item = CreateItemWithQuantity(0);
            viewModel.Initialize(item, validUser, mockOrderService.Object);
            Assert.That(viewModel.IsAddToCartEnabled, Is.False);
        }

        [Test]
        public void IsAddToCartEnabled_StockIsAboveZero_ReturnsTrue()
        {
            var item = CreateItemWithQuantity(10);
            viewModel.Initialize(item, validUser, mockOrderService.Object);
            Assert.That(viewModel.IsAddToCartEnabled, Is.True);
        }

        [Test]
        public void IsQuantityBoxEnabled_StockIsZero_ReturnsFalse()
        {
            var item = CreateItemWithQuantity(0);
            viewModel.Initialize(item, validUser, mockOrderService.Object);
            Assert.That(viewModel.IsQuantityBoxEnabled, Is.False);
        }

        [Test]
        public void IsQuantityBoxEnabled_ItemIsNull_ReturnsFalse()
        {
            viewModel.Initialize(null, validUser, mockOrderService.Object);
            Assert.That(viewModel.IsQuantityBoxEnabled, Is.False);
        }

        [Test]
        public void IsQuantityBoxEnabled_StockIsAboveZero_ReturnsTrue()
        {
            var item = CreateItemWithQuantity(10);
            viewModel.Initialize(item, validUser, mockOrderService.Object);
            Assert.That(viewModel.IsQuantityBoxEnabled, Is.True);
        }

        [Test]
        public void ProductName_ItemIsNotNull_ReturnsName()
        {
            var item = CreateItemWithQuantity(10);
            viewModel.Initialize(item, validUser, mockOrderService.Object);
            Assert.That(viewModel.ProductName, Is.EqualTo("Test Med"));
        }

        [Test]
        public void DescriptionText_ItemIsNotNull_ReturnsDescription()
        {
            var item = CreateItemWithQuantity(10);
            item.Description = "A great medicine";
            viewModel.Initialize(item, validUser, mockOrderService.Object);
            Assert.That(viewModel.DescriptionText, Is.EqualTo("A great medicine"));
        }

        [Test]
        public void LabelText_ItemIsNull_ReturnsEmptyString()
        {
            viewModel.Initialize(null, validUser, mockOrderService.Object);
            Assert.That(viewModel.LabelText, Is.EqualTo(string.Empty));
        }

        [Test]
        public void LabelText_ItemIsNotNull_ReturnsLabel()
        {
            var item = CreateItemWithQuantity(10);
            item.Label = "Rx Only";
            viewModel.Initialize(item, validUser, mockOrderService.Object);
            Assert.That(viewModel.LabelText, Is.EqualTo("Rx Only"));
        }

        [Test]
        public void ProducerText_ItemIsNull_ReturnsEmptyString()
        {
            viewModel.Initialize(null, validUser, mockOrderService.Object);
            Assert.That(viewModel.ProducerText, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ProducerText_ItemIsNotNull_ReturnsProducer()
        {
            var item = CreateItemWithQuantity(10);
            viewModel.Initialize(item, validUser, mockOrderService.Object);
            Assert.That(viewModel.ProducerText, Is.EqualTo("Prod")); // "Prod" is set in the helper method
        }

        [Test]
        public void CategoryText_ItemIsNull_ReturnsEmptyString()
        {
            viewModel.Initialize(null, validUser, mockOrderService.Object);
            Assert.That(viewModel.CategoryText, Is.EqualTo(string.Empty));
        }

        [Test]
        public void CategoryText_ItemIsNotNull_ReturnsCategory()
        {
            var item = CreateItemWithQuantity(10);
            viewModel.Initialize(item, validUser, mockOrderService.Object);
            Assert.That(viewModel.CategoryText, Is.EqualTo("Cat")); // "Cat" is set in the helper method
        }

        [Test]
        public void PillsText_ItemIsNull_ReturnsEmptyString()
        {
            viewModel.Initialize(null, validUser, mockOrderService.Object);
            Assert.That(viewModel.PillsText, Is.EqualTo(string.Empty));
        }

        [Test]
        public void PillsText_ItemIsNotNull_ReturnsNumberOfPills()
        {
            var item = CreateItemWithQuantity(10);
            viewModel.Initialize(item, validUser, mockOrderService.Object);
            Assert.That(viewModel.PillsText, Is.EqualTo("30")); // 30 is set in the helper method
        }

        [Test]
        public void ImagePath_ItemIsNull_ReturnsEmptyString()
        {
            viewModel.Initialize(null, validUser, mockOrderService.Object);
            Assert.That(viewModel.ImagePath, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ImagePath_ItemIsNotNull_ReturnsImagePath()
        {
            var item = CreateItemWithQuantity(10);
            item.ImagePath = "Images/pill.png";
            viewModel.Initialize(item, validUser, mockOrderService.Object);
            Assert.That(viewModel.ImagePath, Is.EqualTo("Images/pill.png"));
        }

        [Test]
        public void StockText_ItemIsNull_ReturnsEmptyString()
        {
            viewModel.Initialize(null, validUser, mockOrderService.Object);
            Assert.That(viewModel.StockText, Is.EqualTo(string.Empty));
        }

        [Test]
        public void CurrentStockLevel_ItemIsNull_ReturnsUnknown()
        {
            viewModel.Initialize(null, validUser, mockOrderService.Object);
            Assert.That(viewModel.CurrentStockLevel, Is.EqualTo(StockLevel.Unknown));
        }

        [Test]
        public void CurrentStockLevel_StockIsZero_ReturnsOutOfStock()
        {
            var item = CreateItemWithQuantity(0);
            viewModel.Initialize(item, validUser, mockOrderService.Object);
            Assert.That(viewModel.CurrentStockLevel, Is.EqualTo(StockLevel.OutOfStock));
        }

        [Test]
        public void CurrentStockLevel_StockIsBelowThreshold_ReturnsLowStock()
        {
            var item = CreateItemWithQuantity(5);
            viewModel.Initialize(item, validUser, mockOrderService.Object);
            Assert.That(viewModel.CurrentStockLevel, Is.EqualTo(StockLevel.LowStock));
        }

        [Test]
        public void CurrentStockLevel_StockIsHigh_ReturnsInStock()
        {
            var item = CreateItemWithQuantity(50);
            viewModel.Initialize(item, validUser, mockOrderService.Object);
            Assert.That(viewModel.CurrentStockLevel, Is.EqualTo(StockLevel.InStock));
        }

        [Test]
        public void OnPropertyChanged_WithSubscriber_InvokesEvent()
        {
            bool eventFired = false;
            viewModel.PropertyChanged += (sender, args) => eventFired = true;

            viewModel.Initialize(CreateItemWithQuantity(10), validUser, mockOrderService.Object);

            Assert.That(eventFired, Is.True);
        }

        [Test]
        public void StockText_StockIsZero_ReturnsOutOfStock()
        {
            var item = CreateItemWithQuantity(0);
            viewModel.Initialize(item, validUser, mockOrderService.Object);
            Assert.That(viewModel.StockText, Is.EqualTo("Out of stock"));
        }

        [Test]
        public void StockText_StockIsBelowThreshold_ReturnsLowStockText()
        {
            var item = CreateItemWithQuantity(5);
            viewModel.Initialize(item, validUser, mockOrderService.Object);
            Assert.That(viewModel.StockText, Is.EqualTo("Only 5 in stock"));
        }

        [Test]
        public void StockText_StockIsHigh_ReturnsInStock()
        {
            var item = CreateItemWithQuantity(50);
            viewModel.Initialize(item, validUser, mockOrderService.Object);
            Assert.That(viewModel.StockText, Is.EqualTo("In stock"));
        }

        [Test]
        public void OldPriceDisplay_ItemIsNull_ReturnsEmptyString()
        {
            viewModel.Initialize(null, validUser, mockOrderService.Object);
            Assert.That(viewModel.OldPriceDisplay, Is.EqualTo(string.Empty));
        }

        [Test]
        public void DiscountDisplay_ItemIsNull_ReturnsEmptyString()
        {
            viewModel.Initialize(null, validUser, mockOrderService.Object);
            Assert.That(viewModel.DiscountDisplay, Is.EqualTo(string.Empty));
        }

        [Test]
        public void HasDiscount_DiscountIsNegative_ReturnsFalse()
        {
            var item = CreateItemWithQuantity(10, discount: -10f);
            viewModel.Initialize(item, validUser, mockOrderService.Object);
            Assert.That(viewModel.HasDiscount, Is.False);
        }

        [Test]
        public void StockText_StockExactlyAtThreshold_ReturnsInStock()
        {
            var item = CreateItemWithQuantity(ProductCatalogueService.LowStockThreshold);
            viewModel.Initialize(item, validUser, mockOrderService.Object);
            Assert.That(viewModel.StockText, Is.EqualTo("In stock"));
        }

        [Test]
        public void CurrentStockLevel_StockExactlyAtThreshold_ReturnsInStock()
        {
            var item = CreateItemWithQuantity(ProductCatalogueService.LowStockThreshold);
            viewModel.Initialize(item, validUser, mockOrderService.Object);
            Assert.That(viewModel.CurrentStockLevel, Is.EqualTo(StockLevel.InStock));
        }
    }
}