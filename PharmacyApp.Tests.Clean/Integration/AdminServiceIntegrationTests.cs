using global::PharmacyApp.Common.Repositories;
using global::PharmacyApp.Common.Services;
using global::PharmacyApp.Models;
using Moq;
using NUnit.Framework;
using PharmacyApp.Common.Repositories;
using PharmacyApp.Common.Services;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;

namespace PharmacyApp.Tests.Integration.FeaturesIntegration.Admin
{
    namespace PharmacyApp.Tests.Integration.FeaturesIntegration.Admin
    {
        [TestFixture]
        public class AdminServiceIntegrationTests
        {
            [Test]
            public void GetAllItems_WhenCalled_ReturnsItemsFromRepository()
            {
                var items = new List<Item> { CreateItem(1) };
                var repoMock = new Mock<IItemsRepository>();
                repoMock.Setup(r => r.GetAllItems()).Returns(items);

                var service = CreateService(itemsRepo: repoMock.Object);

                var result = service.GetAllItems();

                Assert.That(result, Is.EqualTo(items));
            }

            [Test]
            public void SearchItemsByName_WhenMatchExists_ReturnsFilteredItems()
            {
                var items = new List<Item>
            {
                CreateItem(1, "Paracetamol"),
                CreateItem(2, "Ibuprofen")
            };

                var repoMock = new Mock<IItemsRepository>();
                repoMock.Setup(r => r.GetAllItems()).Returns(items);

                var service = CreateService(itemsRepo: repoMock.Object);

                var result = service.SearchItemsByName("para");

                Assert.That(result.Count, Is.EqualTo(1));
            }

            [Test]
            public void GetItem_WhenCalled_DelegatesToRepository()
            {
                var item = CreateItem(1);
                var repoMock = new Mock<IItemsRepository>();
                repoMock.Setup(r => r.GetItem(1)).Returns(item);

                var service = CreateService(itemsRepo: repoMock.Object);

                var result = service.GetItem(1);

                Assert.That(result, Is.EqualTo(item));
            }

            [Test]
            public void SubstanceExists_WhenCalled_DelegatesToRepository()
            {
                var subRepo = new Mock<ISubstancesRepository>();
                subRepo.Setup(r => r.SubstanceExists("A")).Returns(true);

                var service = CreateService(substancesRepo: subRepo.Object);

                var result = service.SubstanceExists("A");

                Assert.That(result, Is.True);
            }

            [Test]
            public void AddItem_WhenValid_CallsRepository()
            {
                var repoMock = new Mock<IItemsRepository>();
                var item = CreateValidItem();

                var service = CreateService(itemsRepo: repoMock.Object);

                service.AddItem(item);

                repoMock.Verify(r => r.AddItemWithQuantity(
                    item.Name, item.Producer, item.Category,
                    item.Price, item.NumberOfPills,
                    item.Quantity, item.ActiveSubstances, item.Batches,
                    item.Label, item.Description, item.ImagePath,
                    item.DiscountPercentage), Times.Once);
            }

            [Test]
            public void AddItem_WhenInvalid_DoesNotCallRepository()
            {
                var repoMock = new Mock<IItemsRepository>();
                var item = CreateInvalidItem();

                var service = CreateService(itemsRepo: repoMock.Object);

                service.AddItem(item);

                repoMock.Verify(r => r.AddItemWithQuantity(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<float>(), It.IsAny<int>(),
                    It.IsAny<int>(), It.IsAny<Dictionary<string, float>>(), It.IsAny<Dictionary<DateOnly, int>>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<float>()), Times.Never);
            }

            [Test]
            public void RemoveItem_WhenCalled_DelegatesToRepository()
            {
                var repoMock = new Mock<IItemsRepository>();
                var service = CreateService(itemsRepo: repoMock.Object);

                service.RemoveItem(1);

                repoMock.Verify(r => r.RemoveItem(1), Times.Once);
            }

            [Test]
            public void UpdateItem_WhenItemDoesNotExist_ThrowsArgumentException()
            {
                var repoMock = new Mock<IItemsRepository>();
                repoMock.Setup(r => r.ItemExists(1)).Returns(false);

                var service = CreateService(itemsRepo: repoMock.Object);

                Assert.That(() => service.UpdateItem(1, CreateItem(1)), Throws.TypeOf<ArgumentException>());
            }

            [Test]
            public void UpdateItem_WhenStockBecomesPositive_SendsNotification()
            {
                var repoMock = new Mock<IItemsRepository>();

                var oldItem = CreateItem(1, quantity: 0);
                var newItem = CreateItem(1, quantity: 5);

                repoMock.Setup(r => r.ItemExists(1)).Returns(true);
                repoMock.Setup(r => r.GetItem(1)).Returns(oldItem);

                var service = CreateService(itemsRepo: repoMock.Object);

                service.UpdateItem(1, newItem);

                repoMock.Verify(r => r.UpdateItem(It.Is<Item>(i => i.Quantity == 5)), Times.Once);
            }

            [Test]
            public void AddSubstance_WhenCalled_DelegatesToRepository()
            {
                var repoMock = new Mock<ISubstancesRepository>();
                var service = CreateService(substancesRepo: repoMock.Object);

                var sub = new Substance("A", 10f, "desc");

                service.AddSubstance(sub);

                repoMock.Verify(r => r.AddSubstance("A", 10f, "desc"), Times.Once);
            }

            [Test]
            public void RemoveSubstance_WhenCalled_DelegatesToRepository()
            {
                var repoMock = new Mock<ISubstancesRepository>();
                var service = CreateService(substancesRepo: repoMock.Object);

                var sub = new Substance("A", 10f, "desc");

                service.RemoveSubstance(sub);

                repoMock.Verify(r => r.RemoveSubstance("A"), Times.Once);
            }

            [Test]
            public void GetExpiredItems_WhenExpiredExists_ReturnsItems()
            {
                var item = CreateItemWithExpiredBatch();
                var repoMock = new Mock<IItemsRepository>();
                repoMock.Setup(r => r.GetAllItems()).Returns(new List<Item> { item });

                var service = CreateService(itemsRepo: repoMock.Object);

                var result = service.GetExpiredItems();

                Assert.That(result.Count, Is.EqualTo(1));
            }

            [Test]
            public void ValidateItemForAdd_WhenInvalid_ThrowsArgumentException()
            {
                var service = CreateService();

                Assert.That(() => service.ValidateItemForAdd(CreateInvalidItem()),
                    Throws.TypeOf<ArgumentException>());
            }

            [Test]
            public void GetNotificationsForUser_WhenAdminAndExpiredItems_ReturnsNotifications()
            {
                var item = CreateItemWithExpiredBatch();
                var repoMock = new Mock<IItemsRepository>();
                repoMock.Setup(r => r.GetAllItems()).Returns(new List<Item> { item });

                var user = CreateUser(isAdmin: true);
                var service = CreateService(itemsRepo: repoMock.Object);

                var result = service.GetNotificationsForUser(user);

                Assert.That(result.Count, Is.EqualTo(1));
            }

            [Test]
            public void GetNotificationsForUser_WhenStockAlertTriggered_ReturnsNotification()
            {
                var item = CreateItem(1, quantity: 5);
                var repoMock = new Mock<IItemsRepository>();
                repoMock.Setup(r => r.GetItem(1)).Returns(item);

                var user = CreateUser();
                user.StockAlerts.Add(1);

                var service = CreateService(itemsRepo: repoMock.Object);

                var result = service.GetNotificationsForUser(user);

                Assert.That(result.Count, Is.EqualTo(1));
            }

            [Test]
            public void GetTop30Items_WhenCalled_DelegatesToRepository()
            {
                var repoMock = new Mock<IItemsRepository>();
                var expected = new List<Tuple<int, string, int>>();

                repoMock.Setup(r => r.GetTop30Items()).Returns(expected);

                var service = CreateService(itemsRepo: repoMock.Object);

                var result = service.GetTop30Items();

                Assert.That(result, Is.EqualTo(expected));
            }

            [Test]
            public void GetTop20Substances_WhenCalled_DelegatesToRepository()
            {
                var repoMock = new Mock<ISubstancesRepository>();
                var expected = new Dictionary<string, int>();

                repoMock.Setup(r => r.GetTop20Substances()).Returns(expected);

                var service = CreateService(substancesRepo: repoMock.Object);

                var result = service.GetTop20Substances();

                Assert.That(result, Is.EqualTo(expected));
            }

            private static AdminService CreateService(
                IItemsRepository? itemsRepo = null,
                ISubstancesRepository? substancesRepo = null)
            {
                return new AdminService(
                    itemsRepo ?? new Mock<IItemsRepository>().Object,
                    substancesRepo ?? new Mock<ISubstancesRepository>().Object);
            }

            private static Item CreateItem(int id, string name = "Test", int quantity = 10)
            {
                var item = new Item(id, name, "Prod", "Cat", 10f, 10, "", "", "..\\..\\Assets\\placeholder.png", 0);
                item.addNewBatch(DateOnly.FromDateTime(DateTime.Now.AddDays(10)), quantity);
                item.ActiveSubstances["A"] = 1f;
                return item;
            }

            private static Item CreateValidItem()
            {
                return CreateItem(1);
            }

            private static Item CreateInvalidItem()
            {
                return new Item(1, "", "", "", 0f, 0, "", "", "..\\..\\Assets\\placeholder.png", 0);
            }

            private static Item CreateItemWithExpiredBatch()
            {
                var item = new Item(1, "Expired", "Prod", "Cat", 10f, 10, "", "", "..\\..\\Assets\\placeholder.png", 0);
                item.addNewBatch(DateOnly.FromDateTime(DateTime.Now.AddDays(-1)), 5);
                item.ActiveSubstances["A"] = 1f;
                return item;
            }

            private static User CreateUser(bool isAdmin = false)
            {
                return new User(1, "test@test.com", "0700", "hash", isAdmin, false, "user", false, 0);
            }
        }
    }
}
