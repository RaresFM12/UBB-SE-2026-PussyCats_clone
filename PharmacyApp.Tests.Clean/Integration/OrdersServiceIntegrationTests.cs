using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PharmacyApp.Common.Repositories;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Orders.ViewModels;
using PharmacyApp.Models;
using Moq;
using NUnit.Framework;

namespace PharmacyApp.Tests.Integration.FeaturesIntegration.Orders
{
    [TestFixture]
    public class OrderServiceIntegrationTests
    {
        [Test]
        public void AddToBasket_WhenItemNotInBasket_AddsItemWithCorrectQuantity()
        {
            User user = CreateUser();
            OrderService service = CreateService(user: user);

            service.AddToBasket(1, 3);

            Assert.That(user.Basket.ContainsKey(1) && user.Basket[1].Quantity == 3, Is.True);
        }

        [Test]
        public void AddToBasket_WhenItemNotInBasket_AddsItemWithNoExtraDiscount()
        {
            User user = CreateUser();
            OrderService service = CreateService(user: user);

            service.AddToBasket(1, 3);

            Assert.That(user.Basket[1].ExtraDiscountPercentage, Is.EqualTo(0f));
        }

        [Test]
        public void AddItemToBasket_WhenItemAlreadyInBasket_AccumulatesQuantity()
        {
            User user = CreateUser();
            user.AddItemToBasket(1, 2, 0f);
            OrderService service = CreateService(user: user);

            service.AddItemToBasket(1, 5, 0f);

            Assert.That(user.Basket[1].Quantity, Is.EqualTo(7));
        }

        [Test]
        public void AddItemToBasket_WhenHigherExtraDiscountProvided_UpdatesExtraDiscount()
        {
            User user = CreateUser();
            user.AddItemToBasket(1, 2, 10f);
            OrderService service = CreateService(user: user);

            service.AddItemToBasket(1, 1, 20f);

            Assert.That(user.Basket[1].ExtraDiscountPercentage, Is.EqualTo(20f));
        }

        [Test]
        public void AddItemToBasket_WhenLowerExtraDiscountProvided_KeepsHigherExistingDiscount()
        {
            User user = CreateUser();
            user.AddItemToBasket(1, 2, 30f);
            OrderService service = CreateService(user: user);

            service.AddItemToBasket(1, 1, 10f);

            Assert.That(user.Basket[1].ExtraDiscountPercentage, Is.EqualTo(30f));
        }

        [Test]
        public void UpdateBasketItemQuantity_WhenQuantityPositive_UpdatesQuantity()
        {
            User user = CreateUser();
            user.AddItemToBasket(1, 3, 0f);
            OrderService service = CreateService(user: user);

            service.UpdateBasketItemQuantity(1, 10);

            Assert.That(user.Basket[1].Quantity, Is.EqualTo(10));
        }

        [Test]
        public void UpdateBasketItemQuantity_WhenQuantityIsZero_RemovesItemFromBasket()
        {
            User user = CreateUser();
            user.AddItemToBasket(1, 3, 0f);
            OrderService service = CreateService(user: user);

            service.UpdateBasketItemQuantity(1, 0);

            Assert.That(user.Basket.ContainsKey(1), Is.False);
        }

        [Test]
        public void UpdateBasketItemQuantity_WhenQuantityIsNegative_RemovesItemFromBasket()
        {
            User user = CreateUser();
            user.AddItemToBasket(1, 3, 0f);
            OrderService service = CreateService(user: user);

            service.UpdateBasketItemQuantity(1, -5);

            Assert.That(user.Basket.ContainsKey(1), Is.False);
        }

        [Test]
        public void RemoveFromBasket_WhenCalled_RemovesItemFromUserBasket()
        {
            User user = CreateUser();
            user.AddItemToBasket(1, 3, 0f);
            OrderService service = CreateService(user: user);

            service.RemoveFromBasket(1);

            Assert.That(user.Basket.ContainsKey(1), Is.False);
        }

        [Test]
        public void RemoveFromBasket_WhenItemNotInBasket_ThrowsArgumentException()
        {
            User user = CreateUser();
            OrderService service = CreateService(user: user);

            Assert.That(() => service.RemoveFromBasket(99), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GetBasketItems_WhenBasketHasValidItems_ReturnsCorrectViewModels()
        {
            User user = CreateUser();
            user.AddItemToBasket(1, 2, 0f);
            Item item = CreateItem(id: 1, price: 10f, discount: 0f);

            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            itemsRepositoryMock.Setup(repository => repository.GetItemById(1)).Returns(item);

            OrderService service = CreateService(itemsRepository: itemsRepositoryMock.Object, user: user);

            List<BasketItemViewModel> result = service.GetBasketItems();
            Assert.That(result[0].ItemId == 1 && result.Count == 1, Is.True);
        }

        [Test]
        public void GetBasketItems_WhenItemRepoThrows_RemovesInvalidItemsAndReturnsRest()
        {
            User user = CreateUser();
            user.AddItemToBasket(1, 2, 0f);
            user.AddItemToBasket(99, 1, 0f);

            Item item = CreateItem(id: 1, price: 10f, discount: 0f);

            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            itemsRepositoryMock.Setup(repository => repository.GetItemById(1)).Returns(item);
            itemsRepositoryMock.Setup(repository => repository.GetItemById(99)).Throws(new Exception("Not found"));

            OrderService service = CreateService(itemsRepository: itemsRepositoryMock.Object, user: user);

            List<BasketItemViewModel> result = service.GetBasketItems();

            Assert.That(!user.Basket.ContainsKey(99) && result.Count == 1, Is.True);
        }

        [Test]
        public void GetBasketItems_WhenUserBasketIsEmpty_ReturnsEmptyList()
        {
            User user = CreateUser();
            OrderService service = CreateService(user: user);

            List<BasketItemViewModel> result = service.GetBasketItems();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void RecalculateBasketItemPrices_WithNoDiscounts_SetsFinalPriceEqualToBase()
        {
            User user = CreateUser();
            OrderService service = CreateService(user: user);
            BasketItemViewModel basketItem = CreateBasketItemViewModel(
                pricePerBox: 10f, quantity: 2,
                baseDiscount: 0f, extraDiscount: 0f, userDiscount: 0f);

            service.RecalculateBasketItemPrices(basketItem);

            Assert.That(basketItem.FinalPriceBeforeDiscount == 20f && basketItem.FinalPriceAfterDiscount == 20f, Is.True);
        }

        [Test]
        public void RecalculateBasketItemPrices_WithBaseDiscount_ReducesFinalPrice()
        {
            User user = CreateUser();
            OrderService service = CreateService(user: user);
            BasketItemViewModel basketItem = CreateBasketItemViewModel(
                pricePerBox: 10f, quantity: 2,
                baseDiscount: 0.5f, extraDiscount: 0f, userDiscount: 0f);

            service.RecalculateBasketItemPrices(basketItem);

            Assert.That(basketItem.FinalPriceBeforeDiscount == 20f && basketItem.FinalPriceAfterDiscount == 10f, Is.True);
        }

        [Test]
        public void RecalculateBasketItemPrices_WithAllDiscountsStacked_AppliesThemMultiplicatively()
        {
            User user = CreateUser();
            OrderService service = CreateService(user: user);
            BasketItemViewModel basketItem = CreateBasketItemViewModel(
                pricePerBox: 10f, quantity: 2,
                baseDiscount: 0.1f, extraDiscount: 0.1f, userDiscount: 0.1f);

            service.RecalculateBasketItemPrices(basketItem);

            Assert.That(basketItem.FinalPriceBeforeDiscount, Is.EqualTo(20f));
            Assert.That(basketItem.FinalPriceAfterDiscount, Is.EqualTo(14.58f).Within(0.01f));
        }

        [Test]
        public void CalculateBasketTotalSum_WhenCalled_SumsPricesCorrectly()
        {
            User user = CreateUser();
            OrderService service = CreateService(user: user);

            var firstItem = CreateBasketItemViewModel(pricePerBox: 10f, quantity: 1, baseDiscount: 0f, extraDiscount: 0f, userDiscount: 0f);
            var secondItem = CreateBasketItemViewModel(pricePerBox: 20f, quantity: 1, baseDiscount: 0f, extraDiscount: 0f, userDiscount: 0f);
            firstItem.SetFinalPrices(10f, 10f);
            secondItem.SetFinalPrices(20f, 20f);

            Tuple<float, float> result = service.CalculateBasketTotalSum(new[] { firstItem, secondItem });

            Assert.That(result.Item1 == 30f && result.Item2 == 30f, Is.True);
        }

        [Test]
        public void CalculateBasketTotalSum_WhenBasketIsEmpty_ReturnsZeroTotals()
        {
            User user = CreateUser();
            OrderService service = CreateService(user: user);

            Tuple<float, float> result = service.CalculateBasketTotalSum(new List<BasketItemViewModel>());

            Assert.That(result.Item1 == 0f && result.Item2 == 0f, Is.True);
        }

        [Test]
        public void PlaceOrderFromBasket_WhenStockSufficient_CallsAddOrderWithItems()
        {
            User user = CreateUser();
            DateOnly pickUpDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5));
            Item item = CreateItemWithBatch(id: 1, price: 10f, discount: 0f, batchDate: pickUpDate.AddDays(30), batchQuantity: 10);
            user.AddItemToBasket(1, 2, 0f);

            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            Mock<IOrdersRepository> ordersRepositoryMock = new Mock<IOrdersRepository>();
            itemsRepositoryMock.Setup(repository => repository.GetItemById(1)).Returns(item);

            OrderService service = CreateService(itemsRepository: itemsRepositoryMock.Object, ordersRepository: ordersRepositoryMock.Object, user: user);

            service.PlaceOrderFromBasket(pickUpDate);

            ordersRepositoryMock.Verify(repository => repository.AddOrder(
                user.Id,
                pickUpDate,
                false,
                false),
                Times.Once);
        }

        [Test]
        public void PlaceOrderFromBasket_WhenStockSufficient_ClearsUserBasket()
        {
            User user = CreateUser();
            DateOnly pickUpDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5));
            Item item = CreateItemWithBatch(id: 1, price: 10f, discount: 0f, batchDate: pickUpDate.AddDays(30), batchQuantity: 10);
            user.AddItemToBasket(1, 2, 0f);

            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            Mock<IOrdersRepository> ordersRepositoryMock = new Mock<IOrdersRepository>();
            itemsRepositoryMock.Setup(repository => repository.GetItemById(1)).Returns(item);

            OrderService service = CreateService(itemsRepository: itemsRepositoryMock.Object, ordersRepository: ordersRepositoryMock.Object, user: user);

            service.PlaceOrderFromBasket(pickUpDate);

            Assert.That(user.Basket, Is.Empty);
        }

        [Test]
        public void PlaceOrderFromBasket_WhenStockInsufficient_ThrowsArgumentException()
        {
            User user = CreateUser();
            DateOnly pickUpDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5));

            Item item = CreateItemWithBatch(id: 1, price: 10f, discount: 0f, batchDate: pickUpDate.AddDays(30), batchQuantity: 1);
            user.AddItemToBasket(1, 5, 0f);

            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            itemsRepositoryMock.Setup(repository => repository.GetItemById(1)).Returns(item);

            OrderService service = CreateService(itemsRepository: itemsRepositoryMock.Object, user: user);

            Assert.That(() => service.PlaceOrderFromBasket(pickUpDate), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void PlaceOrderFromBasket_WhenStockInsufficient_DoesNotPlaceOrder()
        {
            User user = CreateUser();
            DateOnly pickUpDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5));
            Item item = CreateItemWithBatch(id: 1, price: 10f, discount: 0f, batchDate: pickUpDate.AddDays(30), batchQuantity: 1);
            user.AddItemToBasket(1, 5, 0f);

            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            Mock<IOrdersRepository> ordersRepositoryMock = new Mock<IOrdersRepository>();
            itemsRepositoryMock.Setup(repository => repository.GetItemById(1)).Returns(item);

            OrderService service = CreateService(itemsRepository: itemsRepositoryMock.Object, ordersRepository: ordersRepositoryMock.Object, user: user);

            try
            {
                service.PlaceOrderFromBasket(pickUpDate);
            }
            catch
            {
            }

            ordersRepositoryMock.Verify(repository => repository.AddOrder(
                It.IsAny<int>(), It.IsAny<DateOnly>(),
                false,
                false),
                Times.Never);
        }

        [Test]
        public void PlaceOrderFromBasket_WithItemDiscountAndUserDiscount_ComputesFinalPriceCorrectly()
        {
            User user = CreateUser();
            DateOnly pickUpDate = DateOnly.FromDateTime(DateTime.Now.AddDays(5));
            Item item = CreateItemWithBatch(id: 1, price: 100f, discount: 10f, batchDate: pickUpDate.AddDays(30), batchQuantity: 10);
            user.AddItemToBasket(1, 1, 0f);
            user.AddUserDiscount(1, 20f);

            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            Mock<IOrdersRepository> ordersRepositoryMock = new Mock<IOrdersRepository>();
            itemsRepositoryMock.Setup(returnsItem => returnsItem.GetItemById(1)).Returns(item);

            Order capturedOrder = null;
            ordersRepositoryMock
                .Setup(repository => repository.UpdateOrder(It.IsAny<Order>()))
                .Callback<Order>((order) => capturedOrder = order);

            OrderService service = CreateService(itemsRepository: itemsRepositoryMock.Object, ordersRepository: ordersRepositoryMock.Object, user: user);

            service.PlaceOrderFromBasket(pickUpDate);
            Assert.That(capturedOrder != null && capturedOrder.ItemQuantitiesWithFinalPrice[1].Item2 == 72f, Is.True);
        }

        [Test]
        public void CancelOrder_WhenCalled_MarksOrderAsExpired()
        {
            Order order = CreateOrder(id: 1, isExpired: false);
            Mock<IOrdersRepository> ordersRepositoryMock = new Mock<IOrdersRepository>();
            ordersRepositoryMock.Setup(repository => repository.GetOrder(1)).Returns(order);

            OrderService service = CreateService(ordersRepository: ordersRepositoryMock.Object);

            service.CancelOrder(1);

            Assert.That(order.IsExpired, Is.True);
        }

        [Test]
        public void CancelOrder_WhenCalled_PersistsThroughRepository()
        {
            Order order = CreateOrder(id: 1);
            Mock<IOrdersRepository> ordersRepositoryMock = new Mock<IOrdersRepository>();
            ordersRepositoryMock.Setup(repository => repository.GetOrder(1)).Returns(order);

            OrderService service = CreateService(ordersRepository: ordersRepositoryMock.Object);

            service.CancelOrder(1);

            ordersRepositoryMock.Verify(repository => repository.UpdateOrder(order), Times.Once);
        }

        [Test]
        public void CompleteOrder_WhenStockSufficient_MarksOrderAsCompleted()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            Order order = CreateOrder(id: 1);
            order.AddItemToOrder(1, 2, 10f);
            Item item = CreateItemWithBatch(id: 1, price: 10f, discount: 0f, batchDate: today.AddDays(30), batchQuantity: 10);

            Mock<IOrdersRepository> ordersRepositoryMock = new Mock<IOrdersRepository>();
            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            ordersRepositoryMock.Setup(repository => repository.GetOrder(1)).Returns(order);
            itemsRepositoryMock.Setup(repository => repository.GetItemById(1)).Returns(item);

            OrderService service = CreateService(itemsRepository: itemsRepositoryMock.Object, ordersRepository: ordersRepositoryMock.Object);

            var updatedQuantities = new Dictionary<int, Tuple<int, float>>
            {
                { 1, new Tuple<int, float>(2, 10f) }
            };

            service.CompleteOrder(1, updatedQuantities);

            Assert.That(order.IsCompleted, Is.True);
        }

        [Test]
        public void CompleteOrder_WhenStockSufficient_UpdatesOrderInRepository()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            Order order = CreateOrder(id: 1);
            order.AddItemToOrder(1, 2, 10f);
            Item item = CreateItemWithBatch(id: 1, price: 10f, discount: 0f, batchDate: today.AddDays(30), batchQuantity: 10);

            Mock<IOrdersRepository> ordersRepositoryMock = new Mock<IOrdersRepository>();
            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            ordersRepositoryMock.Setup(repository => repository.GetOrder(1)).Returns(order);
            itemsRepositoryMock.Setup(repository => repository.GetItemById(1)).Returns(item);

            OrderService service = CreateService(itemsRepository: itemsRepositoryMock.Object, ordersRepository: ordersRepositoryMock.Object);

            var updatedQuantities = new Dictionary<int, Tuple<int, float>>
            {
                { 1, new Tuple<int, float>(2, 10f) }
            };

            service.CompleteOrder(1, updatedQuantities);

            ordersRepositoryMock.Verify(repository => repository.UpdateOrder(order), Times.Once);
        }

        [Test]
        public void CompleteOrder_WhenStockSufficient_SubtractsQuantityFromItem()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            Order order = CreateOrder(id: 1);
            order.AddItemToOrder(1, 2, 10f);
            Item item = CreateItemWithBatch(id: 1, price: 10f, discount: 0f, batchDate: today.AddDays(30), batchQuantity: 10);

            Mock<IOrdersRepository> ordersRepositoryMock = new Mock<IOrdersRepository>();
            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            ordersRepositoryMock.Setup(repository => repository.GetOrder(1)).Returns(order);
            itemsRepositoryMock.Setup(repository => repository.GetItemById(1)).Returns(item);

            OrderService service = CreateService(itemsRepository: itemsRepositoryMock.Object, ordersRepository: ordersRepositoryMock.Object);

            var updatedQuantities = new Dictionary<int, Tuple<int, float>>
            {
                { 1, new Tuple<int, float>(2, 10f) }
            };

            service.CompleteOrder(1, updatedQuantities);

            itemsRepositoryMock.Verify(repository => repository.UpdateItemById(item), Times.Once);
            Assert.That(item.Quantity, Is.EqualTo(8));
        }

        [Test]
        public void CompleteOrder_WhenStockInsufficient_ThrowsArgumentException()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            Order order = CreateOrder(id: 1);
            order.AddItemToOrder(1, 10, 10f);
            Item item = CreateItemWithBatch(id: 1, price: 10f, discount: 0f, batchDate: today.AddDays(30), batchQuantity: 1);

            Mock<IOrdersRepository> ordersRepositoryMock = new Mock<IOrdersRepository>();
            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            ordersRepositoryMock.Setup(repository => repository.GetOrder(1)).Returns(order);
            itemsRepositoryMock.Setup(repository => repository.GetItemById(1)).Returns(item);

            OrderService service = CreateService(itemsRepository: itemsRepositoryMock.Object, ordersRepository: ordersRepositoryMock.Object);

            var updatedQuantities = new Dictionary<int, Tuple<int, float>>
            {
                { 1, new Tuple<int, float>(10, 10f) }
            };

            Assert.That(() => service.CompleteOrder(1, updatedQuantities), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void CompleteOrder_WhenStockInsufficient_DoesNotUpdateOrder()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            Order order = CreateOrder(id: 1);
            order.AddItemToOrder(1, 10, 10f);
            Item item = CreateItemWithBatch(id: 1, price: 10f, discount: 0f, batchDate: today.AddDays(30), batchQuantity: 1);

            Mock<IOrdersRepository> ordersRepositoryMock = new Mock<IOrdersRepository>();
            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            ordersRepositoryMock.Setup(repository => repository.GetOrder(1)).Returns(order);
            itemsRepositoryMock.Setup(repository => repository.GetItemById(1)).Returns(item);

            OrderService service = CreateService(itemsRepository: itemsRepositoryMock.Object, ordersRepository: ordersRepositoryMock.Object);

            var updatedQuantities = new Dictionary<int, Tuple<int, float>>
            {
                { 1, new Tuple<int, float>(10, 10f) }
            };

            try
            {
                service.CompleteOrder(1, updatedQuantities);
            }
            catch
            {
            }

            ordersRepositoryMock.Verify(repository => repository.UpdateOrder(It.IsAny<Order>()), Times.Never);
        }

        [Test]
        public void CompleteOrder_WhenStockSufficient_ReplacesOrderItemsWithUpdatedOnes()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            Order order = CreateOrder(id: 1);
            order.AddItemToOrder(1, 2, 10f);
            Item item = CreateItemWithBatch(id: 1, price: 10f, discount: 0f, batchDate: today.AddDays(30), batchQuantity: 10);

            Mock<IOrdersRepository> ordersRepositoryMock = new Mock<IOrdersRepository>();
            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            ordersRepositoryMock.Setup(repository => repository.GetOrder(1)).Returns(order);
            itemsRepositoryMock.Setup(repository => repository.GetItemById(1)).Returns(item);

            OrderService service = CreateService(itemsRepository: itemsRepositoryMock.Object, ordersRepository: ordersRepositoryMock.Object);

            var updatedQuantities = new Dictionary<int, Tuple<int, float>>
            {
                { 1, new Tuple<int, float>(3, 15f) }
            };

            service.CompleteOrder(1, updatedQuantities);

            Assert.That(order.ItemQuantitiesWithFinalPrice[1].Item1 == 3 &&
                order.ItemQuantitiesWithFinalPrice[1].Item2 == 15f, Is.True);
        }

        [Test]
        public void ModifyIncompleteOrder_WhenNewPickUpDateInFuture_UpdatesOrderPickUpDate()
        {
            DateOnly futurePickUp = DateOnly.FromDateTime(DateTime.Now.AddDays(10));
            Order order = CreateOrder(id: 1);
            order.AddItemToOrder(1, 2, 10f);
            Item item = CreateItemWithBatch(id: 1, price: 10f, discount: 0f, batchDate: futurePickUp.AddDays(30), batchQuantity: 10);

            Mock<IOrdersRepository> ordersRepositoryMock = new Mock<IOrdersRepository>();
            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            ordersRepositoryMock.Setup(repository => repository.GetOrder(1)).Returns(order);
            itemsRepositoryMock.Setup(repository => repository.GetItemById(1)).Returns(item);

            OrderService service = CreateService(itemsRepository: itemsRepositoryMock.Object, ordersRepository: ordersRepositoryMock.Object);

            var updatedQuantities = new Dictionary<int, Tuple<int, float>>
            {
                { 1, new Tuple<int, float>(2, 10f) }
            };

            service.ModifyIncompleteOrder(1, updatedQuantities, futurePickUp);

            Assert.That(order.PickUpDate, Is.EqualTo(futurePickUp));
        }

        [Test]
        public void ModifyIncompleteOrder_WhenNewPickUpDateInFuture_PersistsThroughRepository()
        {
            DateOnly futurePickUp = DateOnly.FromDateTime(DateTime.Now.AddDays(10));
            Order order = CreateOrder(id: 1);
            order.AddItemToOrder(1, 2, 10f);
            Item item = CreateItemWithBatch(id: 1, price: 10f, discount: 0f, batchDate: futurePickUp.AddDays(30), batchQuantity: 10);

            Mock<IOrdersRepository> ordersRepositoryMock = new Mock<IOrdersRepository>();
            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            ordersRepositoryMock.Setup(repository => repository.GetOrder(1)).Returns(order);
            itemsRepositoryMock.Setup(repository => repository.GetItemById(1)).Returns(item);

            OrderService service = CreateService(itemsRepository: itemsRepositoryMock.Object, ordersRepository: ordersRepositoryMock.Object);

            var updatedQuantities = new Dictionary<int, Tuple<int, float>>
            {
                { 1, new Tuple<int, float>(2, 10f) }
            };

            service.ModifyIncompleteOrder(1, updatedQuantities, futurePickUp);

            ordersRepositoryMock.Verify(repository => repository.UpdateOrder(order), Times.Once);
        }

        [Test]
        public void ModifyIncompleteOrder_WhenPickUpDateIsToday_ThrowsArgumentException()
        {
            DateOnly today = DateOnly.FromDateTime(DateTime.Now);
            Order order = CreateOrder(id: 1);

            Mock<IOrdersRepository> ordersRepositoryMock = new Mock<IOrdersRepository>();
            ordersRepositoryMock.Setup(repository => repository.GetOrder(1)).Returns(order);

            OrderService service = CreateService(ordersRepository: ordersRepositoryMock.Object);

            Assert.That(
                () => service.ModifyIncompleteOrder(1, new Dictionary<int, Tuple<int, float>>(), today),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void ModifyIncompleteOrder_WhenPickUpDateInPast_ThrowsArgumentException()
        {
            DateOnly yesterday = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));
            Order order = CreateOrder(id: 1);

            Mock<IOrdersRepository> ordersRepositoryMock = new Mock<IOrdersRepository>();
            ordersRepositoryMock.Setup(repository => repository.GetOrder(1)).Returns(order);

            OrderService service = CreateService(ordersRepository: ordersRepositoryMock.Object);

            Assert.That(
                () => service.ModifyIncompleteOrder(1, new Dictionary<int, Tuple<int, float>>(), yesterday),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void ModifyIncompleteOrder_WhenStockInsufficient_ThrowsArgumentException()
        {
            DateOnly futurePickUp = DateOnly.FromDateTime(DateTime.Now.AddDays(10));
            Order order = CreateOrder(id: 1);
            order.AddItemToOrder(1, 10, 10f);
            Item item = CreateItemWithBatch(id: 1, price: 10f, discount: 0f, batchDate: futurePickUp.AddDays(30), batchQuantity: 1);

            Mock<IOrdersRepository> ordersRepositoryMock = new Mock<IOrdersRepository>();
            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            ordersRepositoryMock.Setup(repository => repository.GetOrder(1)).Returns(order);
            itemsRepositoryMock.Setup(repository => repository.GetItemById(1)).Returns(item);

            OrderService service = CreateService(itemsRepository: itemsRepositoryMock.Object, ordersRepository: ordersRepositoryMock.Object);

            var updatedQuantities = new Dictionary<int, Tuple<int, float>>
            {
                { 1, new Tuple<int, float>(10, 10f) }
            };

            Assert.That(
                () => service.ModifyIncompleteOrder(1, updatedQuantities, futurePickUp),
                Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void ModifyIncompleteOrder_WhenStockInsufficient_DoesNotUpdateOrder()
        {
            DateOnly futurePickUp = DateOnly.FromDateTime(DateTime.Now.AddDays(10));
            Order order = CreateOrder(id: 1);
            order.AddItemToOrder(1, 10, 10f);
            Item item = CreateItemWithBatch(id: 1, price: 10f, discount: 0f, batchDate: futurePickUp.AddDays(30), batchQuantity: 1);

            Mock<IOrdersRepository> ordersRepositoryMock = new Mock<IOrdersRepository>();
            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            ordersRepositoryMock.Setup(repository => repository.GetOrder(1)).Returns(order);
            itemsRepositoryMock.Setup(repository => repository.GetItemById(1)).Returns(item);

            OrderService service = CreateService(itemsRepository: itemsRepositoryMock.Object, ordersRepository: ordersRepositoryMock.Object);

            var updatedQuantities = new Dictionary<int, Tuple<int, float>>
            {
                { 1, new Tuple<int, float>(10, 10f) }
            };

            try
            {
                service.ModifyIncompleteOrder(1, updatedQuantities, futurePickUp);
            }
            catch
            {
            }

            ordersRepositoryMock.Verify(repository => repository.UpdateOrder(It.IsAny<Order>()), Times.Never);
        }

        [Test]
        public void ModifyIncompleteOrder_WhenCalled_ReplacesOrderItemsWithUpdatedOnes()
        {
            DateOnly futurePickUp = DateOnly.FromDateTime(DateTime.Now.AddDays(10));
            Order order = CreateOrder(id: 1);
            order.AddItemToOrder(1, 2, 10f);
            Item item = CreateItemWithBatch(id: 1, price: 10f, discount: 0f, batchDate: futurePickUp.AddDays(30), batchQuantity: 10);

            Mock<IOrdersRepository> ordersRepositoryMock = new Mock<IOrdersRepository>();
            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            ordersRepositoryMock.Setup(repository => repository.GetOrder(1)).Returns(order);
            itemsRepositoryMock.Setup(repository => repository.GetItemById(1)).Returns(item);

            OrderService service = CreateService(itemsRepository: itemsRepositoryMock.Object, ordersRepository: ordersRepositoryMock.Object);

            var updatedQuantities = new Dictionary<int, Tuple<int, float>>
            {
                { 1, new Tuple<int, float>(5, 25f) }
            };

            service.ModifyIncompleteOrder(1, updatedQuantities, futurePickUp);

            Assert.That(order.ItemQuantitiesWithFinalPrice[1].Item1 == 5 &&
                 order.ItemQuantitiesWithFinalPrice[1].Item2 == 25f, Is.True);
        }

        [Test]
        public void ResubmitExpiredOrder_WhenStockSufficient_PlacesNewOrder()
        {
            User user = CreateUser();
            DateOnly newPickUp = DateOnly.FromDateTime(DateTime.Now.AddDays(5));
            Order expiredOrder = CreateOrder(id: 1, isExpired: true);
            expiredOrder.AddItemToOrder(1, 2, 10f);
            Item item = CreateItemWithBatch(id: 1, price: 10f, discount: 0f, batchDate: newPickUp.AddDays(30), batchQuantity: 10);

            Mock<IOrdersRepository> ordersRepositoryMock = new Mock<IOrdersRepository>();
            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            ordersRepositoryMock.Setup(repository => repository.GetOrder(1)).Returns(expiredOrder);
            itemsRepositoryMock.Setup(repository => repository.GetItemById(1)).Returns(item);

            OrderService service = CreateService(itemsRepository: itemsRepositoryMock.Object, ordersRepository: ordersRepositoryMock.Object, user: user);

            service.ResubmitExpiredOrder(1, newPickUp);

            ordersRepositoryMock.Verify(repository => repository.AddOrder(
                user.Id,
                newPickUp,
                false, false),
                Times.Once);
        }

        [Test]
        public void ResubmitExpiredOrder_WhenStockInsufficient_ThrowsArgumentException()
        {
            User user = CreateUser();
            DateOnly newPickUp = DateOnly.FromDateTime(DateTime.Now.AddDays(5));
            Order expiredOrder = CreateOrder(id: 1);
            expiredOrder.AddItemToOrder(1, 10, 10f);
            Item item = CreateItemWithBatch(id: 1, price: 10f, discount: 0f, batchDate: newPickUp.AddDays(30), batchQuantity: 1);

            Mock<IOrdersRepository> ordersRepositoryMock = new Mock<IOrdersRepository>();
            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            ordersRepositoryMock.Setup(repository => repository.GetOrder(1)).Returns(expiredOrder);
            itemsRepositoryMock.Setup(repository => repository.GetItemById(1)).Returns(item);

            OrderService service = CreateService(itemsRepository: itemsRepositoryMock.Object, ordersRepository: ordersRepositoryMock.Object, user: user);

            Assert.That(() => service.ResubmitExpiredOrder(1, newPickUp), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void ResubmitExpiredOrder_WhenStockInsufficient_DoesNotPlaceNewOrder()
        {
            User user = CreateUser();
            DateOnly newPickUp = DateOnly.FromDateTime(DateTime.Now.AddDays(5));
            Order expiredOrder = CreateOrder(id: 1);
            expiredOrder.AddItemToOrder(1, 10, 10f);
            Item item = CreateItemWithBatch(id: 1, price: 10f, discount: 0f, batchDate: newPickUp.AddDays(30), batchQuantity: 1);

            Mock<IOrdersRepository> ordersRepositoryMock = new Mock<IOrdersRepository>();
            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            ordersRepositoryMock.Setup(repository => repository.GetOrder(1)).Returns(expiredOrder);
            itemsRepositoryMock.Setup(repository => repository.GetItemById(1)).Returns(item);

            OrderService service = CreateService(itemsRepository: itemsRepositoryMock.Object, ordersRepository: ordersRepositoryMock.Object, user: user);

            try
            {
                service.ResubmitExpiredOrder(1, newPickUp);
            }
            catch
            {
            }

            ordersRepositoryMock.Verify(repository => repository.AddOrder(
                It.IsAny<int>(), It.IsAny<DateOnly>(),
                false,
                true),
                Times.Never);
        }

        [Test]
        public void ApplyPrescriptionToBasket_WhenPrescriptionHasItems_AddsItemsToBasket()
        {
            User user = CreateUser();

            Item item = CreateItem(id: 1, price: 10f, discount: 0f, name: "Nurofen Express", numberOfPills: 40, quantity: 2);

            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            itemsRepositoryMock.Setup(repository => repository.GetItemsByName("Nurofen Express")).Returns(new List<Item> { item });
            itemsRepositoryMock.Setup(repository => repository.GetAllItems()).Returns(new List<Item> { item });

            OrderService service = CreateService(itemsRepository: itemsRepositoryMock.Object, user: user);

            service.ApplyPrescriptionToBasket("testPrescription");

            Assert.That(user.Basket.ContainsKey(1) && user.Basket[1].Quantity == 1, Is.True);
        }

        [Test]
        public void ApplyPrescriptionToBasket_WhenPrescriptionReturnsEmpty_ThrowsArgumentException()
        {
            User user = CreateUser();
            OrderService service = CreateService(user: user);

            Assert.That(() => service.ApplyPrescriptionToBasket("INVALID"), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void FillBasketFromPrescription_WhenCalled_DelegatesToItemsRepository()
        {
            User user = CreateUser();
            var expectedDictionary = new Dictionary<int, int> { { 5, 1 } };

            Item item = CreateItem(id: 5, price: 10f, discount: 0f, name: "Nurofen Express", numberOfPills: 40, quantity: 2);

            Mock<IItemsRepository> itemsRepositoryMock = new Mock<IItemsRepository>();
            itemsRepositoryMock.Setup(repository => repository.GetItemsByName("Nurofen Express")).Returns(new List<Item> { item });
            itemsRepositoryMock.Setup(repository => repository.GetAllItems()).Returns(new List<Item> { item });

            OrderService service = CreateService(itemsRepository: itemsRepositoryMock.Object, user: user);

            Dictionary<int, int> result = service.FillBasketFromPrescription("testPrescription");

            Assert.That(result, Is.EqualTo(expectedDictionary));
        }

        private static OrderService CreateService(
            ISubstancesRepository? substancesRepository = null,
            IItemsRepository? itemsRepository = null,
            IUsersRepository? usersRepository = null,
            IOrdersRepository? ordersRepository = null,
            User? user = null)
        {
            return new OrderService(
                substancesRepository ?? new Mock<ISubstancesRepository>().Object,
                itemsRepository ?? new Mock<IItemsRepository>().Object,
                usersRepository ?? new Mock<IUsersRepository>().Object,
                ordersRepository ?? new Mock<IOrdersRepository>().Object,
                user ?? CreateUser());
        }

        private static User CreateUser()
        {
            return new User(
                1,
                "user@test.com",
                "0700000000",
                "hash",
                false,
                false,
                "user",
                false,
                0);
        }

        private static Item CreateItem(int id, float price, float discount, string name = "TestItem", int numberOfPills = 10, int quantity = 0)
        {
            return new Item(id, name, "TestProducer", "TestCategory",
                price, numberOfPills, string.Empty, string.Empty, "..\\..\\Assets\\placeholder.png", discount, quantity);
        }

        private static Item CreateItemWithBatch(int id, float price, float discount, DateOnly batchDate, int batchQuantity)
        {
            Item item = CreateItem(id, price, discount);
            item.AddNewBatchToItem(batchDate, batchQuantity);
            return item;
        }

        private static Order CreateOrder(int id, bool isCompleted = false, bool isExpired = false)
        {
            return new Order(id, 1, DateOnly.FromDateTime(DateTime.Now.AddDays(3)), isCompleted, isExpired);
        }

        private static BasketItemViewModel CreateBasketItemViewModel(
            float pricePerBox, int quantity,
            float baseDiscount, float extraDiscount, float userDiscount)
        {
            return new BasketItemViewModel(
                itemId: 1,
                imagePath: "ms-appx:///Assets/logo.png",
                name: "TestItem",
                producer: "TestProducer",
                quantity: quantity,
                baseItemDiscount: baseDiscount,
                extraItemDiscount: extraDiscount,
                userDiscount: userDiscount,
                initialPrice: pricePerBox);
        }
    }
}