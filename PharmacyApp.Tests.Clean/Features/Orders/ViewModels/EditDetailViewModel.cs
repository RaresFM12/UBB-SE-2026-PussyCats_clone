using Moq;
using NUnit.Framework;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Orders.ViewModels;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;

namespace PharmacyApp.Tests.Unit.Features.Orders.ViewModels
{
    [TestFixture]
    public class EditDetailViewModelTests
    {
        private Mock<PharmacyApp.Common.Repositories.IItemsRepository> mockItemsRepo;
        private Mock<PharmacyApp.Common.Repositories.IOrdersRepository> mockOrdersRepo;
        private OrderService orderService;

        private static Item CreateItem(
            int id,
            string name = "ItemName",
            string producer = "Prod",
            string imagePath = @"C:\App\Assets\img.png")
        {
            return new Item(
                id,
                name,
                producer,
                "Medicine",
                10f,
                10,
                discount: 0f,
                quantity: 50,
                imagePath: imagePath);
        }

        private static Order CreateOrder(
            int id,
            int userId,
            DateOnly pickUpDate,
            Dictionary<int, Tuple<int, float>> items = null,
            bool isCompleted = false,
            bool isExpired = false)
        {
            Order order = new Order(id, userId, pickUpDate);
            order.IsCompleted = isCompleted;
            order.IsExpired = isExpired;

            if (items != null)
            {
                foreach (KeyValuePair<int, Tuple<int, float>> entry in items)
                {
                    order.AddItemToOrder(entry.Key, entry.Value.Item1, entry.Value.Item2);
                }
            }

            return order;
        }

        [SetUp]
        public void Setup()
        {
            mockItemsRepo = new Mock<PharmacyApp.Common.Repositories.IItemsRepository>();
            mockOrdersRepo = new Mock<PharmacyApp.Common.Repositories.IOrdersRepository>();
            User user = new User(1, "u@u.com", "0700", "hash", false, false, "U", false, 0);

            orderService = new OrderService(
                new Mock<PharmacyApp.Common.Repositories.ISubstancesRepository>().Object,
                mockItemsRepo.Object,
                new Mock<PharmacyApp.Common.Repositories.IUsersRepository>().Object,
                mockOrdersRepo.Object,
                user);
        }

        private (Order Order, Item Item) SetupOrderWithOneItem(
            int orderId = 1,
            bool isCompleted = false,
            bool isExpired = false,
            DateOnly? pickUpDate = null)
        {
            DateOnly date = pickUpDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(3));
            Item item = CreateItem(10, "Paracetamol", "Bayer");
            Order order = CreateOrder(
                orderId,
                1,
                date,
                items: new Dictionary<int, Tuple<int, float>>
                {
                    { 10, new Tuple<int, float>(2, 20f) }
                },
                isCompleted: isCompleted,
                isExpired: isExpired);

            mockOrdersRepo.Setup(repository => repository.GetOrder(orderId)).Returns(order);
            mockItemsRepo.Setup(repository => repository.GetItemById(10)).Returns(item);

            return (order, item);
        }

        [Test]
        public void Constructor_LoadsOrderItemsFromRepository()
        {
            SetupOrderWithOneItem();
            EditDetailViewModel viewModel = new EditDetailViewModel(orderService, 1);

            Assert.That(viewModel.OrderItems.Count, Is.EqualTo(1));
        }

        [Test]
        public void Constructor_CalculatesTotalPriceCorrectly()
        {
            SetupOrderWithOneItem();
            EditDetailViewModel viewModel = new EditDetailViewModel(orderService, 1);

            Assert.That(viewModel.TotalPriceString, Is.EqualTo("20.00 RON"));
        }

        [Test]
        public void Constructor_IncompleteOrder_SetsStatusStringToIncomplete()
        {
            SetupOrderWithOneItem(isCompleted: false, isExpired: false);
            EditDetailViewModel viewModel = new EditDetailViewModel(orderService, 1);

            Assert.That(viewModel.StatusString, Is.EqualTo("Incomplete"));
        }

        [Test]
        public void Constructor_ExpiredOrder_SetsStatusStringToExpired()
        {
            SetupOrderWithOneItem(isCompleted: false, isExpired: true);
            EditDetailViewModel viewModel = new EditDetailViewModel(orderService, 1);

            Assert.That(viewModel.StatusString, Is.EqualTo("Expired"));
        }

        [Test]
        public void Constructor_CompletedOrder_SetsStatusStringToComplete()
        {
            SetupOrderWithOneItem(isCompleted: true, isExpired: false);
            EditDetailViewModel viewModel = new EditDetailViewModel(orderService, 1);

            Assert.That(viewModel.StatusString, Is.EqualTo("Complete"));
        }

        [Test]
        public void Constructor_SetsPickUpDateFromOrder()
        {
            DateOnly expectedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
            SetupOrderWithOneItem(pickUpDate: expectedDate);
            EditDetailViewModel viewModel = new EditDetailViewModel(orderService, 1);

            Assert.That(viewModel.PickUpDate, Is.EqualTo(expectedDate));
        }

        [Test]
        public void PickUpDateString_FormatsDateAsYearMonthDay()
        {
            DateOnly expectedDate = new DateOnly(2026, 5, 10);
            SetupOrderWithOneItem(pickUpDate: expectedDate);
            EditDetailViewModel viewModel = new EditDetailViewModel(orderService, 1);

            Assert.That(viewModel.PickUpDateString, Is.EqualTo("2026.05.10"));
        }

        [Test]
        public void Constructor_StoresShownOrderId()
        {
            SetupOrderWithOneItem(orderId: 99);
            EditDetailViewModel viewModel = new EditDetailViewModel(orderService, 99);

            Assert.That(viewModel.shownOrderID, Is.EqualTo(99));
        }

        [Test]
        public void RemoveItemCommand_RemovesItemFromOrderItems()
        {
            SetupOrderWithOneItem();
            EditDetailViewModel viewModel = new EditDetailViewModel(orderService, 1);
            ItemDetail itemToRemove = viewModel.OrderItems[0];

            viewModel.RemoveItemCommand.Execute(itemToRemove);

            Assert.That(viewModel.OrderItems.Count, Is.EqualTo(0));
        }

        [Test]
        public void RemoveItemCommand_UpdatesTotalPriceToZeroAfterRemoval()
        {
            SetupOrderWithOneItem();
            EditDetailViewModel viewModel = new EditDetailViewModel(orderService, 1);
            ItemDetail itemToRemove = viewModel.OrderItems[0];

            viewModel.RemoveItemCommand.Execute(itemToRemove);

            Assert.That(viewModel.TotalPriceString, Is.EqualTo("0.00 RON"));
        }

        [Test]
        public void TotalPriceString_SetNewValue_RaisesPropertyChanged()
        {
            SetupOrderWithOneItem();
            EditDetailViewModel viewModel = new EditDetailViewModel(orderService, 1);
            bool raised = false;

            viewModel.PropertyChanged += (_, eventArgs) =>
            {
                if (eventArgs.PropertyName == nameof(EditDetailViewModel.TotalPriceString))
                {
                    raised = true;
                }
            };

            viewModel.TotalPriceString = "999.00 RON";

            Assert.That(raised, Is.True);
        }
    }
}