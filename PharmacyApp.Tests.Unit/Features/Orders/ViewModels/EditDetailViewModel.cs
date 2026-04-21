using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Orders.ViewModels;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyApp.Tests.Unit.Features.Orders.ViewModels
{
    [TestFixture]
    public class EditDetailViewModelTests
    {
        private Mock<PharmacyApp.Common.Repositories.IItemsRepository> mockItemsRepo;
        private Mock<PharmacyApp.Common.Repositories.IOrdersRepository> mockOrdersRepo;
        private OrderService orderService;

        private static Item CreateItem(int id, string name = "ItemName", string producer = "Prod",
            string imagePath = @"C:\App\Assets\img.png")
        {
            return new Item(id, name, producer, "Medicine", 10f, 10,
                discount: 0f, quantity: 50, imagePath: imagePath);
        }

        private static Order CreateOrder(int id, int userId, DateOnly pickUpDate,
            Dictionary<int, Tuple<int, float>> items = null,
            bool isCompleted = false, bool isExpired = false)
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
            mockItemsRepo = new Mock<PharmacyApp.Common.Repositories.IItemsRepository>();
            mockOrdersRepo = new Mock<PharmacyApp.Common.Repositories.IOrdersRepository>();
            var user = new User(1, "u@u.com", "0700", "hash", false, false, "U", false, 0);

            orderService = new OrderService(
                new Mock<PharmacyApp.Common.Repositories.ISubstancesRepository>().Object,
                mockItemsRepo.Object,
                new Mock<PharmacyApp.Common.Repositories.IUsersRepository>().Object,
                mockOrdersRepo.Object,
                user);
        }

        private (Order, Item) SetupOrderWithOneItem(
            int orderId = 1,
            bool isCompleted = false,
            bool isExpired = false,
            DateOnly? pickUpDate = null)
        {
            DateOnly date = pickUpDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(3));
            var item = CreateItem(10, "Paracetamol", "Bayer");
            var order = CreateOrder(orderId, 1, date,
                items: new Dictionary<int, Tuple<int, float>> { { 10, new Tuple<int, float>(2, 20f) } },
                isCompleted: isCompleted,
                isExpired: isExpired);
            mockOrdersRepo.Setup(r => r.GetOrder(orderId)).Returns(order);
            mockItemsRepo.Setup(r => r.GetItem(10)).Returns(item);
            return (order, item);
        }

        [Test]
        public void Constructor_LoadsOrderItemsFromRepository()
        {
            SetupOrderWithOneItem();
            var vm = new EditDetailViewModel(orderService, 1);

            Assert.AreEqual(1, vm.OrderItems.Count);
        }

        [Test]
        public void Constructor_CalculatesTotalPriceCorrectly()
        {
            SetupOrderWithOneItem();
            var vm = new EditDetailViewModel(orderService, 1);

            Assert.AreEqual("20.00 RON", vm.TotalPriceString);
        }

        [Test]
        public void Constructor_IncompleteOrder_SetsStatusStringToIncomplete()
        {
            SetupOrderWithOneItem(isCompleted: false, isExpired: false);
            var vm = new EditDetailViewModel(orderService, 1);

            Assert.AreEqual("Incomplete", vm.StatusString);
        }

        [Test]
        public void Constructor_ExpiredOrder_SetsStatusStringToExpired()
        {
            SetupOrderWithOneItem(isCompleted: false, isExpired: true);
            var vm = new EditDetailViewModel(orderService, 1);

            Assert.AreEqual("Expired", vm.StatusString);
        }

        [Test]
        public void Constructor_CompletedOrder_SetsStatusStringToComplete()
        {
            SetupOrderWithOneItem(isCompleted: true, isExpired: false);
            var vm = new EditDetailViewModel(orderService, 1);

            Assert.AreEqual("Complete", vm.StatusString);
        }

        [Test]
        public void Constructor_SetsPickUpDateFromOrder()
        {
            DateOnly expectedDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7));
            SetupOrderWithOneItem(pickUpDate: expectedDate);
            var vm = new EditDetailViewModel(orderService, 1);

            Assert.AreEqual(expectedDate, vm.PickUpDate);
        }

        [Test]
        public void PickUpDateString_FormatsDateAsYearMonthDay()
        {
            DateOnly expectedDate = new DateOnly(2026, 5, 10);
            SetupOrderWithOneItem(pickUpDate: expectedDate);
            var vm = new EditDetailViewModel(orderService, 1);

            Assert.AreEqual("2026.05.10", vm.PickUpDateString);
        }

        [Test]
        public void Constructor_StoresShownOrderId()
        {
            SetupOrderWithOneItem(orderId: 99);
            var vm = new EditDetailViewModel(orderService, 99);

            Assert.AreEqual(99, vm.shownOrderID);
        }

        [Test]
        public void RemoveItemCommand_RemovesItemFromOrderItems()
        {
            SetupOrderWithOneItem();
            var vm = new EditDetailViewModel(orderService, 1);
            var itemToRemove = vm.OrderItems[0];

            vm.RemoveItemCommand.Execute(itemToRemove);

            Assert.AreEqual(0, vm.OrderItems.Count);
        }

        [Test]
        public void RemoveItemCommand_UpdatesTotalPriceToZeroAfterRemoval()
        {
            SetupOrderWithOneItem();
            var vm = new EditDetailViewModel(orderService, 1);
            var itemToRemove = vm.OrderItems[0];

            vm.RemoveItemCommand.Execute(itemToRemove);

            Assert.AreEqual("0.00 RON", vm.TotalPriceString);
        }

        [Test]
        public void TotalPriceString_SetNewValue_RaisesPropertyChanged()
        {
            SetupOrderWithOneItem();
            var vm = new EditDetailViewModel(orderService, 1);
            bool raised = false;
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(EditDetailViewModel.TotalPriceString))
                    raised = true;
            };

            vm.TotalPriceString = "999.00 RON";

            Assert.IsTrue(raised);
        }
    }
}
