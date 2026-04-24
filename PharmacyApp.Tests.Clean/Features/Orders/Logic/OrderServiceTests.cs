using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PharmacyApp.Common.Repositories;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Orders.ViewModels;
using PharmacyApp.Models;

namespace PharmacyApp.Tests.Unit.Features.Orders.Logic
{
    [TestFixture]
    public class OrderServiceTests
    {
        private Mock<ISubstancesRepository> mockSubstancesRepository;
        private Mock<IItemsRepository> mockItemsRepository;
        private Mock<IUsersRepository> mockUsersRepository;
        private Mock<IOrdersRepository> mockOrdersRepository;
        private User activeUser;
        private OrderService orderService;

        private static User CreateUser(int id = 1, bool isAdmin = false)
        {
            return new User(id, "test@test.com", "1234567890", "hash", isAdmin, false,
                "TestUser", false, 0);
        }

        private static Item CreateItem(
            int id,
            string name = "ItemName",
            string producer = "Producer",
            string category = "Medicine",
            float price = 10f,
            int quantity = 100,
            float discount = 0f,
            int numberOfPills = 10,
            string imagePath = null,
            Dictionary<DateOnly, int> batches = null,
            Dictionary<string, float> activeSubstances = null)
        {
            var item = new Item(
                id,
                name,
                producer,
                category,
                price,
                numberOfPills,
                discount: discount,
                quantity: quantity,
                imagePath: imagePath ?? string.Empty);

            if (batches != null)
            {
                foreach (var batch in batches)
                {
                    item.Batches[batch.Key] = batch.Value;
                }
            }

            if (activeSubstances != null)
            {
                foreach (var substance in activeSubstances)
                {
                    item.ActiveSubstances[substance.Key] = substance.Value;
                }
            }

            return item;
        }

        private static Order CreateOrder(
            int id,
            int userId,
            DateOnly pickUpDate,
            Dictionary<int, Tuple<int, float>> items = null,
            bool isCompleted = false,
            bool isExpired = false)
        {
            var order = new Order(id, userId, pickUpDate);
            order.IsCompleted = isCompleted;
            order.IsExpired = isExpired;

            if (items != null)
            {
                foreach (var entry in items)
                {
                    order.AddItemToOrder(entry.Key, entry.Value.Item1, entry.Value.Item2);
                }
            }

            return order;
        }

        [SetUp]
        public void Setup()
        {
            mockSubstancesRepository = new Mock<ISubstancesRepository>();
            mockItemsRepository = new Mock<IItemsRepository>();
            mockUsersRepository = new Mock<IUsersRepository>();
            mockOrdersRepository = new Mock<IOrdersRepository>();
            activeUser = CreateUser();

            orderService = new OrderService(
                mockSubstancesRepository.Object,
                mockItemsRepository.Object,
                mockUsersRepository.Object,
                mockOrdersRepository.Object,
                activeUser);
        }

        [Test]
        public void RecalculateBasketItemPrices_WithNoDiscount_SetsFinalPriceBeforeDiscountToQuantityTimesPrice()
        {
            var item = new BasketItemViewModel(1, string.Empty, "Medicine", "Producer", 3, 0f, 0f, 0f, 10f);

            orderService.RecalculateBasketItemPrices(item);

            Assert.That(item.FinalPriceBeforeDiscount, Is.EqualTo(30f).Within(0.001f));
        }

        [Test]
        public void RecalculateBasketItemPrices_WithNoDiscount_SetsFinalPriceAfterDiscountEqualToBeforeDiscount()
        {
            var item = new BasketItemViewModel(1, string.Empty, "Medicine", "Producer", 2, 0f, 0f, 0f, 10f);

            orderService.RecalculateBasketItemPrices(item);

            Assert.That(item.FinalPriceAfterDiscount, Is.EqualTo(item.FinalPriceBeforeDiscount).Within(0.001f));
        }

        [Test]
        public void RecalculateBasketItemPrices_WithBaseDiscount_ReducesFinalPriceAfterDiscount()
        {
            var item = new BasketItemViewModel(1, string.Empty, "Medicine", "Producer", 1, 0.1f, 0f, 0f, 100f);

            orderService.RecalculateBasketItemPrices(item);

            Assert.That(item.FinalPriceAfterDiscount, Is.EqualTo(90f).Within(0.001f));
        }

        [Test]
        public void RecalculateBasketItemPrices_WithExtraDiscount_ReducesFinalPriceAfterDiscount()
        {
            var item = new BasketItemViewModel(1, string.Empty, "Medicine", "Producer", 1, 0f, 0.2f, 0f, 100f);

            orderService.RecalculateBasketItemPrices(item);

            Assert.That(item.FinalPriceAfterDiscount, Is.EqualTo(80f).Within(0.001f));
        }

        [Test]
        public void RecalculateBasketItemPrices_WithUserDiscount_ReducesFinalPriceAfterDiscount()
        {
            var item = new BasketItemViewModel(1, string.Empty, "Medicine", "Producer", 1, 0f, 0f, 0.5f, 100f);

            orderService.RecalculateBasketItemPrices(item);

            Assert.That(item.FinalPriceAfterDiscount, Is.EqualTo(50f).Within(0.001f));
        }

        [Test]
        public void RecalculateBasketItemPrices_PriceIsTruncatedNotRounded()
        {
            var item = new BasketItemViewModel(1, string.Empty, "Medicine", "Producer", 3, 0f, 0f, 0f, 3.339f);

            orderService.RecalculateBasketItemPrices(item);

            Assert.That(item.FinalPriceBeforeDiscount, Is.EqualTo(10.01f).Within(0.01f));
        }

        [Test]
        public void CalculateBasketTotalSum_EmptyList_ReturnsBothZero()
        {
            var result = orderService.CalculateBasketTotalSum(new List<BasketItemViewModel>());

            Assert.That(result.Item1, Is.EqualTo(0f).Within(0.001f));
            Assert.That(result.Item2, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void CalculateBasketTotalSum_TwoItems_ReturnsSumOfFinalPricesBefore()
        {
            var firstItem = new BasketItemViewModel(1, string.Empty, "ItemA", "Producer", 1, 0f, 0f, 0f, 10f);
            var secondItem = new BasketItemViewModel(2, string.Empty, "ItemB", "Producer", 1, 0f, 0f, 0f, 20f);
            orderService.RecalculateBasketItemPrices(firstItem);
            orderService.RecalculateBasketItemPrices(secondItem);

            var result = orderService.CalculateBasketTotalSum(new List<BasketItemViewModel> { firstItem, secondItem });

            Assert.That(result.Item1, Is.EqualTo(30f).Within(0.001f));
        }

        [Test]
        public void CalculateBasketTotalSum_TwoItems_ReturnsSumOfFinalPricesAfter()
        {
            var firstItem = new BasketItemViewModel(1, string.Empty, "ItemA", "Producer", 1, 0.1f, 0f, 0f, 10f);
            var secondItem = new BasketItemViewModel(2, string.Empty, "ItemB", "Producer", 1, 0f, 0f, 0f, 20f);
            orderService.RecalculateBasketItemPrices(firstItem);
            orderService.RecalculateBasketItemPrices(secondItem);

            var result = orderService.CalculateBasketTotalSum(new List<BasketItemViewModel> { firstItem, secondItem });

            Assert.That(result.Item2, Is.EqualTo(29f).Within(0.001f));
        }

        [Test]
        public void AddToBasket_NewItem_AddsItemToUserBasket()
        {
            orderService.AddToBasket(1, 2);

            Assert.That(activeUser.Basket.ContainsKey(1), Is.True);
        }

        [Test]
        public void AddToBasket_NewItem_SetsCorrectQuantity()
        {
            orderService.AddToBasket(1, 3);

            Assert.That(activeUser.Basket[1].Quantity, Is.EqualTo(3));
        }

        [Test]
        public void AddItemToBasket_ExistingItem_AccumulatesQuantity()
        {
            orderService.AddToBasket(1, 2);
            orderService.AddToBasket(1, 3);

            Assert.That(activeUser.Basket[1].Quantity, Is.EqualTo(5));
        }

        [Test]
        public void AddItemToBasket_ExistingItemWithHigherExtraDiscount_UpdatesExtraDiscount()
        {
            orderService.AddItemToBasket(1, 2, 0.1f);
            orderService.AddItemToBasket(1, 1, 0.2f);

            Assert.That(activeUser.Basket[1].ExtraDiscountPercentage, Is.EqualTo(0.2f).Within(0.001f));
        }

        [Test]
        public void AddItemToBasket_ExistingItemWithLowerExtraDiscount_DoesNotUpdateExtraDiscount()
        {
            orderService.AddItemToBasket(1, 2, 0.3f);
            orderService.AddItemToBasket(1, 1, 0.1f);

            Assert.That(activeUser.Basket[1].ExtraDiscountPercentage, Is.EqualTo(0.3f).Within(0.001f));
        }

        [Test]
        public void UpdateBasketItemQuantity_PositiveQuantity_UpdatesQuantityInBasket()
        {
            orderService.AddToBasket(1, 2);
            orderService.UpdateBasketItemQuantity(1, 5);

            Assert.That(activeUser.Basket[1].Quantity, Is.EqualTo(5));
        }

        [Test]
        public void UpdateBasketItemQuantity_ZeroQuantity_RemovesItemFromBasket()
        {
            orderService.AddToBasket(1, 2);
            orderService.UpdateBasketItemQuantity(1, 0);

            Assert.That(activeUser.Basket.ContainsKey(1), Is.False);
        }

        [Test]
        public void UpdateBasketItemQuantity_NegativeQuantity_RemovesItemFromBasket()
        {
            orderService.AddToBasket(1, 2);
            orderService.UpdateBasketItemQuantity(1, -1);

            Assert.That(activeUser.Basket.ContainsKey(1), Is.False);
        }

        [Test]
        public void RemoveFromBasket_ExistingItem_RemovesItemFromUserBasket()
        {
            orderService.AddToBasket(1, 2);
            orderService.RemoveFromBasket(1);

            Assert.That(activeUser.Basket.ContainsKey(1), Is.False);
        }

        [Test]
        public void GetBasketItems_EmptyBasket_ReturnsEmptyList()
        {
            var result = orderService.GetBasketItems();

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetBasketItems_OneValidItem_ReturnsOneBasketItemViewModel()
        {
            activeUser.AddItemToBasket(1, 2, 0f);
            var item = CreateItem(1, quantity: 50);
            mockItemsRepository.Setup(repository => repository.GetItemById(1)).Returns(item);

            var result = orderService.GetBasketItems();

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetBasketItems_OneValidItem_ReturnsViewModelWithCorrectItemId()
        {
            activeUser.AddItemToBasket(1, 2, 0f);
            var item = CreateItem(1, quantity: 50);
            mockItemsRepository.Setup(repository => repository.GetItemById(1)).Returns(item);

            var result = orderService.GetBasketItems();

            Assert.That(result[0].ItemId, Is.EqualTo(1));
        }

        [Test]
        public void GetBasketItems_ItemRepositoryThrows_RemovesInvalidItemFromBasket()
        {
            activeUser.AddItemToBasket(99, 1, 0f);
            mockItemsRepository.Setup(repository => repository.GetItemById(99)).Throws(new Exception("not found"));

            orderService.GetBasketItems();

            Assert.That(activeUser.Basket.ContainsKey(99), Is.False);
        }

        [Test]
        public void GetBasketItems_ItemWithUserDiscount_AppliesUserDiscountToViewModel()
        {
            activeUser.AddItemToBasket(1, 1, 0f);
            activeUser.UserDiscounts[1] = 0.10f;
            var item = CreateItem(1, quantity: 50, price: 100f);
            mockItemsRepository.Setup(repository => repository.GetItemById(1)).Returns(item);

            var result = orderService.GetBasketItems();

            Assert.That(result[0].ItemActiveUserDiscount, Is.EqualTo(0.10f).Within(0.001f));
        }

        [Test]
        public void GetBasketItems_ItemWithMsAppxImagePath_PreservesPath()
        {
            activeUser.AddItemToBasket(1, 1, 0f);
            var item = CreateItem(1, quantity: 50, imagePath: "ms-appx:///Assets/image.png");
            mockItemsRepository.Setup(repository => repository.GetItemById(1)).Returns(item);

            var result = orderService.GetBasketItems();

            Assert.That(result[0].ItemThumbnailImagePath, Is.EqualTo("ms-appx:///Assets/image.png"));
        }

        [Test]
        public void GetBasketItems_ItemWithNullImagePath_ReturnsFallbackLogoPath()
        {
            activeUser.AddItemToBasket(1, 1, 0f);
            var item = CreateItem(1, quantity: 50, imagePath: null);
            mockItemsRepository.Setup(repository => repository.GetItemById(1)).Returns(item);

            var result = orderService.GetBasketItems();

            Assert.That(result[0].ItemThumbnailImagePath, Is.EqualTo("ms-appx:///Assets/logo.png"));
        }

        [Test]
        public void GetBasketItems_ItemWithAssetsInPath_BuildsMsAppxPath()
        {
            activeUser.AddItemToBasket(1, 1, 0f);
            var item = CreateItem(1, quantity: 50, imagePath: @"C:\MyApp\Assets\images\med.png");
            mockItemsRepository.Setup(repository => repository.GetItemById(1)).Returns(item);

            var result = orderService.GetBasketItems();

            Assert.That(result[0].ItemThumbnailImagePath, Does.StartWith("ms-appx://"));
        }

        [Test]
        public void GetBasketItems_ItemWithPathContainingAssetsFolder_ReplacesBackslashesWithForwardSlashes()
        {
            activeUser.AddItemToBasket(1, 1, 0f);
            var item = CreateItem(1, quantity: 50, imagePath: @"C:\MyApp\Assets\images\med.png");
            mockItemsRepository.Setup(repository => repository.GetItemById(1)).Returns(item);

            var result = orderService.GetBasketItems();

            Assert.That(result[0].ItemThumbnailImagePath, Does.Not.Contain("\\"));
        }

        [Test]
        public void GetBasketItems_ItemWithUnrecognisedPath_ReturnsFallbackLogoPath()
        {
            activeUser.AddItemToBasket(1, 1, 0f);
            var item = CreateItem(1, quantity: 50, imagePath: @"C:\SomeOtherFolder\image.png");
            mockItemsRepository.Setup(repository => repository.GetItemById(1)).Returns(item);

            var result = orderService.GetBasketItems();

            Assert.That(result[0].ItemThumbnailImagePath, Is.EqualTo("ms-appx:///Assets/logo.png"));
        }

        [Test]
        public void PlaceOrderFromBasket_InsufficientStock_ThrowsArgumentException()
        {
            DateOnly pickUpDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
            activeUser.AddItemToBasket(1, 10, 0f);
            var item = CreateItem(
                1,
                quantity: 2,
                batches: new Dictionary<DateOnly, int> { { pickUpDate, 2 } });

            mockItemsRepository.Setup(repository => repository.GetItemById(1)).Returns(item);

            Assert.That(
                () => orderService.PlaceOrderFromBasket(pickUpDate),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void CancelOrder_ExistingOrder_SetsIsExpiredTrue()
        {
            var order = CreateOrder(1, activeUser.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(3)));
            mockOrdersRepository.Setup(repository => repository.GetOrder(1)).Returns(order);

            orderService.CancelOrder(1);

            Assert.That(order.IsExpired, Is.True);
        }

        [Test]
        public void CancelOrder_ExistingOrder_CallsUpdateOrder()
        {
            var order = CreateOrder(1, activeUser.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(3)));
            mockOrdersRepository.Setup(repository => repository.GetOrder(1)).Returns(order);

            orderService.CancelOrder(1);

            mockOrdersRepository.Verify(repository => repository.UpdateOrder(order), Times.Once);
        }

        [Test]
        public void ResubmitExpiredOrder_InsufficientStock_ThrowsArgumentException()
        {
            DateOnly pickUpDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
            var existingItems = new Dictionary<int, Tuple<int, float>>
            {
                { 1, new Tuple<int, float>(10, 100f) }
            };

            var order = CreateOrder(
                2,
                activeUser.Id,
                DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
                items: existingItems,
                isExpired: true);

            mockOrdersRepository.Setup(repository => repository.GetOrder(2)).Returns(order);

            var item = CreateItem(
                1,
                quantity: 2,
                batches: new Dictionary<DateOnly, int> { { pickUpDate, 2 } });

            mockItemsRepository.Setup(repository => repository.GetItemById(1)).Returns(item);

            Assert.That(
                () => orderService.ResubmitExpiredOrder(2, pickUpDate),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void CompleteOrder_InsufficientStock_ThrowsArgumentException()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);

            var order = CreateOrder(
                1,
                activeUser.Id,
                today,
                items: new Dictionary<int, Tuple<int, float>>
                {
                    { 1, new Tuple<int, float>(2, 20f) }
                });

            mockOrdersRepository.Setup(repository => repository.GetOrder(1)).Returns(order);

            var item = CreateItem(
                1,
                quantity: 1,
                batches: new Dictionary<DateOnly, int> { { today, 1 } });

            mockItemsRepository.Setup(repository => repository.GetItemById(1)).Returns(item);

            var updatedQuantities = new Dictionary<int, Tuple<int, float>>
            {
                { 1, new Tuple<int, float>(5, 50f) }
            };

            Assert.That(
                () => orderService.CompleteOrder(1, updatedQuantities),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void ModifyIncompleteOrder_PickUpDateIsToday_ThrowsArgumentException()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            var order = CreateOrder(1, activeUser.Id, today);
            mockOrdersRepository.Setup(repository => repository.GetOrder(1)).Returns(order);

            Assert.That(
                () => orderService.ModifyIncompleteOrder(1, new Dictionary<int, Tuple<int, float>>(), today),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void ModifyIncompleteOrder_PickUpDateInPast_ThrowsArgumentException()
        {
            DateOnly yesterday = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            var order = CreateOrder(1, activeUser.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(3)));
            mockOrdersRepository.Setup(repository => repository.GetOrder(1)).Returns(order);

            Assert.That(
                () => orderService.ModifyIncompleteOrder(1, new Dictionary<int, Tuple<int, float>>(), yesterday),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void ModifyIncompleteOrder_InsufficientStock_ThrowsArgumentException()
        {
            DateOnly futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5));

            var order = CreateOrder(
                1,
                activeUser.Id,
                DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
                items: new Dictionary<int, Tuple<int, float>>
                {
                    { 1, new Tuple<int, float>(2, 20f) }
                });

            mockOrdersRepository.Setup(repository => repository.GetOrder(1)).Returns(order);

            var item = CreateItem(
                1,
                quantity: 1,
                batches: new Dictionary<DateOnly, int> { { futureDate, 1 } });

            mockItemsRepository.Setup(repository => repository.GetItemById(1)).Returns(item);

            var updatedQuantities = new Dictionary<int, Tuple<int, float>>
            {
                { 1, new Tuple<int, float>(5, 50f) }
            };

            Assert.That(
                () => orderService.ModifyIncompleteOrder(1, updatedQuantities, futureDate),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void ApplyPrescriptionToBasket_ValidPrescription_AddsItemsToBasket()
        {
            // We must supply the exact data PrescriptionService expects ("Nurofen Express", 40 pills)
            Item item = CreateItem(id: 1, name: "Nurofen Express", numberOfPills: 40, quantity: 2);
            mockItemsRepository.Setup(repository => repository.GetItemsByName("Nurofen Express")).Returns(new List<Item> { item });
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(new List<Item> { item });

            orderService.ApplyPrescriptionToBasket("testPrescription");

            Assert.That(activeUser.Basket.ContainsKey(1), Is.True);
        }

        [Test]
        public void ApplyPrescriptionToBasket_EmptyPrescriptionResult_ThrowsArgumentException()
        {
            Assert.That(
                () => orderService.ApplyPrescriptionToBasket("INVALID"),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void FillBasketFromPrescription_Called_DelegatesToRepositoryAndService()
        {
            Item item = CreateItem(id: 5, name: "Nurofen Express", numberOfPills: 40, quantity: 2);
            mockItemsRepository.Setup(repository => repository.GetItemsByName("Nurofen Express")).Returns(new List<Item> { item });
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(new List<Item> { item });

            var expectedDictionary = new Dictionary<int, int> { { 5, 1 } };

            var result = orderService.FillBasketFromPrescription("testPrescription");

            Assert.That(result, Is.EqualTo(expectedDictionary));
        }
    }
}