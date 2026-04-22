using Moq;
using NUnit.Framework;
using PharmacyApp.Common.Repositories;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Orders.ViewModels;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private static Item CreateItem(int id, string name = "ItemName", string producer = "Producer",
            string category = "Medicine", float price = 10f, int quantity = 100,
            float discount = 0f, int numberOfPills = 10, string imagePath = null,
            Dictionary<DateOnly, int> batches = null,
            Dictionary<string, float> activeSubstances = null)
        {
            var item = new Item(id, name, producer, category, price, numberOfPills,
                discount: discount, quantity: quantity, imagePath: imagePath ?? "");
            if (batches != null)
                foreach (var batch in batches)
                    item.Batches[batch.Key] = batch.Value;
            if (activeSubstances != null)
                foreach (var substance in activeSubstances)
                    item.ActiveSubstances[substance.Key] = substance.Value;
            return item;
        }

        private static Order CreateOrder(int id, int userId,
            DateOnly pickUpDate,
            Dictionary<int, Tuple<int, float>> items = null,
            bool isCompleted = false,
            bool isExpired = false)
        {
            var order = new Order(id, userId, pickUpDate);
            order.IsCompleted = isCompleted;
            order.IsExpired = isExpired;
            if (items != null)
                foreach (var entry in items)
                    order.AddItemToOrder(entry.Key, entry.Value.Item1, entry.Value.Item2);
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
            var item = new BasketItemViewModel(1, "", "Med", "Prod", 3, 0f, 0f, 0f, 10f);

            orderService.RecalculateBasketItemPrices(item);

            Assert.AreEqual(30f, item.FinalPriceBeforeDiscount, 0.001f);
        }

        [Test]
        public void RecalculateBasketItemPrices_WithNoDiscount_SetsFinalPriceAfterDiscountEqualToBeforeDiscount()
        {
            var item = new BasketItemViewModel(1, "", "Med", "Prod", 2, 0f, 0f, 0f, 10f);

            orderService.RecalculateBasketItemPrices(item);

            Assert.AreEqual(item.FinalPriceBeforeDiscount, item.FinalPriceAfterDiscount, 0.001f);
        }

        [Test]
        public void RecalculateBasketItemPrices_WithBaseDiscount_ReducesFinalPriceAfterDiscount()
        {
            var item = new BasketItemViewModel(1, "", "Med", "Prod", 1, 0.1f, 0f, 0f, 100f);

            orderService.RecalculateBasketItemPrices(item);

            Assert.AreEqual(90f, item.FinalPriceAfterDiscount, 0.001f);
        }

        [Test]
        public void RecalculateBasketItemPrices_WithExtraDiscount_ReducesFinalPriceAfterDiscount()
        {
            var item = new BasketItemViewModel(1, "", "Med", "Prod", 1, 0f, 0.2f, 0f, 100f);

            orderService.RecalculateBasketItemPrices(item);

            Assert.AreEqual(80f, item.FinalPriceAfterDiscount, 0.001f);
        }

        [Test]
        public void RecalculateBasketItemPrices_WithUserDiscount_ReducesFinalPriceAfterDiscount()
        {
            var item = new BasketItemViewModel(1, "", "Med", "Prod", 1, 0f, 0f, 0.5f, 100f);

            orderService.RecalculateBasketItemPrices(item);

            Assert.AreEqual(50f, item.FinalPriceAfterDiscount, 0.001f);
        }

        [Test]
        public void RecalculateBasketItemPrices_PriceIsTruncatedNotRounded()
        {
            var item = new BasketItemViewModel(1, "", "Med", "Prod", 3, 0f, 0f, 0f, 3.339f);

            orderService.RecalculateBasketItemPrices(item);

            Assert.AreEqual(10.01f, item.FinalPriceBeforeDiscount, 0.01f);
        }

        [Test]
        public void CalculateBasketTotalSum_EmptyList_ReturnsBothZero()
        {
            var result = orderService.CalculateBasketTotalSum(new List<BasketItemViewModel>());

            Assert.AreEqual(0f, result.Item1, 0.001f);
        }

        [Test]
        public void CalculateBasketTotalSum_EmptyList_ReturnsDiscountedZero()
        {
            var result = orderService.CalculateBasketTotalSum(new List<BasketItemViewModel>());

            Assert.AreEqual(0f, result.Item2, 0.001f);
        }

        [Test]
        public void CalculateBasketTotalSum_TwoItems_ReturnsSumOfFinalPricesBefore()
        {
            var item1 = new BasketItemViewModel(1, "", "A", "P", 1, 0f, 0f, 0f, 10f);
            var item2 = new BasketItemViewModel(2, "", "B", "P", 1, 0f, 0f, 0f, 20f);
            orderService.RecalculateBasketItemPrices(item1);
            orderService.RecalculateBasketItemPrices(item2);

            var result = orderService.CalculateBasketTotalSum(new List<BasketItemViewModel> { item1, item2 });

            Assert.AreEqual(30f, result.Item1, 0.001f);
        }

        [Test]
        public void CalculateBasketTotalSum_TwoItems_ReturnsSumOfFinalPricesAfter()
        {
            var item1 = new BasketItemViewModel(1, "", "A", "P", 1, 0.1f, 0f, 0f, 10f);
            var item2 = new BasketItemViewModel(2, "", "B", "P", 1, 0f, 0f, 0f, 20f);
            orderService.RecalculateBasketItemPrices(item1);
            orderService.RecalculateBasketItemPrices(item2);

            var result = orderService.CalculateBasketTotalSum(new List<BasketItemViewModel> { item1, item2 });

            Assert.AreEqual(29f, result.Item2, 0.001f);
        }

        [Test]
        public void AddToBasket_NewItem_AddsItemToUserBasket()
        {
            orderService.AddToBasket(1, 2);

            Assert.IsTrue(activeUser.Basket.ContainsKey(1));
        }

        [Test]
        public void AddToBasket_NewItem_SetsCorrectQuantity()
        {
            orderService.AddToBasket(1, 3);

            Assert.AreEqual(3, activeUser.Basket[1].Quantity);
        }

        [Test]
        public void AddItemToBasket_ExistingItem_AccumulatesQuantity()
        {
            orderService.AddToBasket(1, 2);
            orderService.AddToBasket(1, 3);

            Assert.AreEqual(5, activeUser.Basket[1].Quantity);
        }

        [Test]
        public void AddItemToBasket_ExistingItemWithHigherExtraDiscount_UpdatesExtraDiscount()
        {
            orderService.AddItemToBasket(1, 2, 0.1f);
            orderService.AddItemToBasket(1, 1, 0.2f);

            Assert.AreEqual(0.2f, activeUser.Basket[1].ExtraDiscountPercentage, 0.001f);
        }

        [Test]
        public void AddItemToBasket_ExistingItemWithLowerExtraDiscount_DoesNotUpdateExtraDiscount()
        {
            orderService.AddItemToBasket(1, 2, 0.3f);
            orderService.AddItemToBasket(1, 1, 0.1f);

            Assert.AreEqual(0.3f, activeUser.Basket[1].ExtraDiscountPercentage, 0.001f);
        }

        [Test]
        public void UpdateBasketItemQuantity_PositiveQuantity_UpdatesQuantityInBasket()
        {
            orderService.AddToBasket(1, 2);
            orderService.UpdateBasketItemQuantity(1, 5);

            Assert.AreEqual(5, activeUser.Basket[1].Quantity);
        }

        [Test]
        public void UpdateBasketItemQuantity_ZeroQuantity_RemovesItemFromBasket()
        {
            orderService.AddToBasket(1, 2);
            orderService.UpdateBasketItemQuantity(1, 0);

            Assert.IsFalse(activeUser.Basket.ContainsKey(1));
        }

        [Test]
        public void UpdateBasketItemQuantity_NegativeQuantity_RemovesItemFromBasket()
        {
            orderService.AddToBasket(1, 2);
            orderService.UpdateBasketItemQuantity(1, -1);

            Assert.IsFalse(activeUser.Basket.ContainsKey(1));
        }

        [Test]
        public void RemoveFromBasket_ExistingItem_RemovesItemFromUserBasket()
        {
            orderService.AddToBasket(1, 2);
            orderService.RemoveFromBasket(1);

            Assert.IsFalse(activeUser.Basket.ContainsKey(1));
        }

        [Test]
        public void GetBasketItems_EmptyBasket_ReturnsEmptyList()
        {
            var result = orderService.GetBasketItems();

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetBasketItems_OneValidItem_ReturnsOneBasketItemViewModel()
        {
            activeUser.AddItemToBasket(1, 2, 0f);
            var item = CreateItem(1, quantity: 50);
            mockItemsRepository.Setup(r => r.GetItem(1)).Returns(item);

            var result = orderService.GetBasketItems();

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void GetBasketItems_OneValidItem_ReturnsViewModelWithCorrectItemId()
        {
            activeUser.AddItemToBasket(1, 2, 0f);
            var item = CreateItem(1, quantity: 50);
            mockItemsRepository.Setup(r => r.GetItem(1)).Returns(item);

            var result = orderService.GetBasketItems();

            Assert.AreEqual(1, result[0].ItemId);
        }

        [Test]
        public void GetBasketItems_ItemRepositoryThrows_RemovesInvalidItemFromBasket()
        {
            activeUser.AddItemToBasket(99, 1, 0f);
            mockItemsRepository.Setup(r => r.GetItem(99)).Throws(new Exception("not found"));

            orderService.GetBasketItems();

            Assert.IsFalse(activeUser.Basket.ContainsKey(99));
        }

        [Test]
        public void GetBasketItems_ItemWithUserDiscount_AppliesUserDiscountToViewModel()
        {
            activeUser.AddItemToBasket(1, 1, 0f);
            activeUser.UserDiscounts[1] = 0.10f;
            var item = CreateItem(1, quantity: 50, price: 100f);
            mockItemsRepository.Setup(r => r.GetItem(1)).Returns(item);

            var result = orderService.GetBasketItems();

            Assert.AreEqual(0.10f, result[0].ItemActiveUserDiscount, 0.001f);
        }

        [Test]
        public void GetBasketItems_ItemWithMsAppxImagePath_PreservesPath()
        {
            activeUser.AddItemToBasket(1, 1, 0f);
            var item = CreateItem(1, quantity: 50, imagePath: "ms-appx:///Assets/image.png");
            mockItemsRepository.Setup(r => r.GetItem(1)).Returns(item);

            var result = orderService.GetBasketItems();

            Assert.AreEqual("ms-appx:///Assets/image.png", result[0].ItemThumbnailImagePath);
        }

        [Test]
        public void GetBasketItems_ItemWithNullImagePath_ReturnsFallbackLogoPath()
        {
            activeUser.AddItemToBasket(1, 1, 0f);
            var item = CreateItem(1, quantity: 50, imagePath: null);
            mockItemsRepository.Setup(r => r.GetItem(1)).Returns(item);

            var result = orderService.GetBasketItems();

            Assert.AreEqual("ms-appx:///Assets/logo.png", result[0].ItemThumbnailImagePath);
        }

        [Test]
        public void GetBasketItems_ItemWithAssetsInPath_BuildsMsAppxPath()
        {
            activeUser.AddItemToBasket(1, 1, 0f);
            var item = CreateItem(1, quantity: 50, imagePath: @"C:\MyApp\Assets\images\med.png");
            mockItemsRepository.Setup(r => r.GetItem(1)).Returns(item);

            var result = orderService.GetBasketItems();

            StringAssert.StartsWith("ms-appx://", result[0].ItemThumbnailImagePath);
        }

        [Test]
        public void GetBasketItems_ItemWithPathContainingAssetsFolder_ReplacesBackslashesWithForwardSlashes()
        {
            activeUser.AddItemToBasket(1, 1, 0f);
            var item = CreateItem(1, quantity: 50, imagePath: @"C:\MyApp\Assets\images\med.png");
            mockItemsRepository.Setup(r => r.GetItem(1)).Returns(item);

            var result = orderService.GetBasketItems();

            StringAssert.DoesNotContain("\\", result[0].ItemThumbnailImagePath);
        }

        [Test]
        public void GetBasketItems_ItemWithUnrecognisedPath_ReturnsFallbackLogoPath()
        {
            activeUser.AddItemToBasket(1, 1, 0f);
            var item = CreateItem(1, quantity: 50, imagePath: @"C:\SomeOtherFolder\image.png");
            mockItemsRepository.Setup(r => r.GetItem(1)).Returns(item);

            var result = orderService.GetBasketItems();

            Assert.AreEqual("ms-appx:///Assets/logo.png", result[0].ItemThumbnailImagePath);
        }

        [Test]
        public void PlaceOrderFromBasket_InsufficientStock_ThrowsArgumentException()
        {
            DateOnly pickUpDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
            activeUser.AddItemToBasket(1, 10, 0f);
            var item = CreateItem(1, quantity: 2,
                batches: new Dictionary<DateOnly, int> { { pickUpDate, 2 } });
            mockItemsRepository.Setup(r => r.GetItem(1)).Returns(item);

            Assert.Throws<ArgumentException>(() => orderService.PlaceOrderFromBasket(pickUpDate));
        }

        [Test]
        public void CancelOrder_ExistingOrder_SetsIsExpiredTrue()
        {
            var order = CreateOrder(1, activeUser.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(3)));
            mockOrdersRepository.Setup(r => r.GetOrder(1)).Returns(order);

            orderService.CancelOrder(1);

            Assert.IsTrue(order.IsExpired);
        }

        [Test]
        public void CancelOrder_ExistingOrder_CallsUpdateOrder()
        {
            var order = CreateOrder(1, activeUser.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(3)));
            mockOrdersRepository.Setup(r => r.GetOrder(1)).Returns(order);

            orderService.CancelOrder(1);

            mockOrdersRepository.Verify(r => r.UpdateOrder(order), Times.Once);
        }

        [Test]
        public void ResubmitExpiredOrder_InsufficientStock_ThrowsArgumentException()
        {
            DateOnly pickUpDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
            var existingItems = new Dictionary<int, Tuple<int, float>>
            {
                { 1, new Tuple<int, float>(10, 100f) }
            };
            var order = CreateOrder(2, activeUser.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(-1)),
                items: existingItems, isExpired: true);
            mockOrdersRepository.Setup(r => r.GetOrder(2)).Returns(order);
            var item = CreateItem(1, quantity: 2,
                batches: new Dictionary<DateOnly, int> { { pickUpDate, 2 } });
            mockItemsRepository.Setup(r => r.GetItem(1)).Returns(item);

            Assert.Throws<ArgumentException>(() => orderService.ResubmitExpiredOrder(2, pickUpDate));
        }

        [Test]
        public void CompleteOrder_InsufficientStock_ThrowsArgumentException()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            var order = CreateOrder(1, activeUser.Id, today,
                items: new Dictionary<int, Tuple<int, float>> { { 1, new Tuple<int, float>(2, 20f) } });
            mockOrdersRepository.Setup(r => r.GetOrder(1)).Returns(order);
            var item = CreateItem(1, quantity: 1,
                batches: new Dictionary<DateOnly, int> { { today, 1 } });
            mockItemsRepository.Setup(r => r.GetItem(1)).Returns(item);
            var updatedQuantities = new Dictionary<int, Tuple<int, float>>
            {
                { 1, new Tuple<int, float>(5, 50f) }
            };

            Assert.Throws<ArgumentException>(() => orderService.CompleteOrder(1, updatedQuantities));
        }

        [Test]
        public void ModifyIncompleteOrder_PickUpDateIsToday_ThrowsArgumentException()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            var order = CreateOrder(1, activeUser.Id, today);
            mockOrdersRepository.Setup(r => r.GetOrder(1)).Returns(order);

            Assert.Throws<ArgumentException>(() =>
                orderService.ModifyIncompleteOrder(1, new Dictionary<int, Tuple<int, float>>(), today));
        }

        [Test]
        public void ModifyIncompleteOrder_PickUpDateInPast_ThrowsArgumentException()
        {
            DateOnly yesterday = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            var order = CreateOrder(1, activeUser.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(3)));
            mockOrdersRepository.Setup(r => r.GetOrder(1)).Returns(order);

            Assert.Throws<ArgumentException>(() =>
                orderService.ModifyIncompleteOrder(1, new Dictionary<int, Tuple<int, float>>(), yesterday));
        }

        [Test]
        public void ModifyIncompleteOrder_InsufficientStock_ThrowsArgumentException()
        {
            DateOnly futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
            var order = CreateOrder(1, activeUser.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
                items: new Dictionary<int, Tuple<int, float>> { { 1, new Tuple<int, float>(2, 20f) } });
            mockOrdersRepository.Setup(r => r.GetOrder(1)).Returns(order);
            var item = CreateItem(1, quantity: 1,
                batches: new Dictionary<DateOnly, int> { { futureDate, 1 } });
            mockItemsRepository.Setup(r => r.GetItem(1)).Returns(item);
            var updatedQuantities = new Dictionary<int, Tuple<int, float>>
            {
                { 1, new Tuple<int, float>(5, 50f) }
            };

            Assert.Throws<ArgumentException>(() =>
                orderService.ModifyIncompleteOrder(1, updatedQuantities, futureDate));
        }

        [Test]
        public void ApplyPrescriptionToBasket_ValidPrescription_AddsItemsToBasket()
        {
            mockItemsRepository
                .Setup(r => r.GetItemsFromPrescription("RX001", activeUser.UserDiscounts))
                .Returns(new Dictionary<int, int> { { 1, 2 } });

            orderService.ApplyPrescriptionToBasket("RX001");

            Assert.IsTrue(activeUser.Basket.ContainsKey(1));
        }

        [Test]
        public void ApplyPrescriptionToBasket_EmptyPrescriptionResult_ThrowsArgumentException()
        {
            mockItemsRepository
                .Setup(r => r.GetItemsFromPrescription("EMPTY", activeUser.UserDiscounts))
                .Returns(new Dictionary<int, int>());

            Assert.Throws<ArgumentException>(() => orderService.ApplyPrescriptionToBasket("EMPTY"));
        }

        [Test]
        public void FillBasketFromPrescription_Called_DelegatesToRepository()
        {
            var expected = new Dictionary<int, int> { { 5, 3 } };
            mockItemsRepository
                .Setup(r => r.GetItemsFromPrescription("RX999", activeUser.UserDiscounts))
                .Returns(expected);

            var result = orderService.FillBasketFromPrescription("RX999");

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void FillBasketFromPrescription_InvalidId_ThrowsArgumentException()
        {
            string invalidId = "wrong_id";

            Assert.Throws<ArgumentException>(() => orderService.FillBasketFromPrescription(invalidId));
        }

        [Test]
        public void PlaceOrderFromBasket_ValidBasket_ClearsUserBasket()
        {
            DateOnly pickupDate = DateOnly.FromDateTime(DateTime.Now.AddDays(2));
            int itemId = 10;
            orderService.AddToBasket(itemId, 2);
            Item mockItem = CreateItem(itemId, quantity: 50);
            mockItemsRepository.Setup(repo => repo.GetItem(itemId)).Returns(mockItem);

            orderService.PlaceOrderFromBasket(pickupDate);

            Assert.AreEqual(0, activeUser.Basket.Count);
        }

        [Test]
        public void CompleteOrder_ValidQuantities_SetsOrderIsCompletedToTrue()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Today);
            int orderId = 1;
            int itemId = 1;
            var order = CreateOrder(orderId, activeUser.Id, today,
                items: new Dictionary<int, Tuple<int, float>> { { itemId, new Tuple<int, float>(2, 20f) } });
            mockOrdersRepository.Setup(r => r.GetOrder(orderId)).Returns(order);

            Item mockItem = CreateItem(itemId, quantity: 10, batches: new Dictionary<DateOnly, int> { { today, 10 } });
            mockItemsRepository.Setup(r => r.GetItem(itemId)).Returns(mockItem);

            var updatedQuantities = new Dictionary<int, Tuple<int, float>>
            {
                { itemId, new Tuple<int, float>(2, 20f) }
            };

            orderService.CompleteOrder(orderId, updatedQuantities);

            Assert.IsTrue(order.IsCompleted);
        }

        [Test]
        public void ModifyIncompleteOrder_ValidData_SetsNewPickUpDate()
        {
            int orderId = 1;
            int itemId = 1;
            DateOnly futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
            var order = CreateOrder(orderId, activeUser.Id, DateOnly.FromDateTime(DateTime.Today.AddDays(2)));
            mockOrdersRepository.Setup(r => r.GetOrder(orderId)).Returns(order);

            Item mockItem = CreateItem(itemId, quantity: 10, batches: new Dictionary<DateOnly, int> { { futureDate, 10 } });
            mockItemsRepository.Setup(r => r.GetItem(itemId)).Returns(mockItem);

            var updatedQuantities = new Dictionary<int, Tuple<int, float>>
            {
                { itemId, new Tuple<int, float>(2, 20f) }
            };

            orderService.ModifyIncompleteOrder(orderId, updatedQuantities, futureDate);

            Assert.AreEqual(futureDate, order.PickUpDate);
        }
    }
}