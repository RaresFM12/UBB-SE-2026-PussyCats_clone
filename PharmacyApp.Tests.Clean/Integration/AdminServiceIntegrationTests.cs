using System;
using System.Collections.Generic;
using global::PharmacyApp.Common.Repositories;
using global::PharmacyApp.Common.Services;
using global::PharmacyApp.Models;
using Moq;
using NUnit.Framework;
using PharmacyApp.Common.Repositories;
using PharmacyApp.Common.Services;
using PharmacyApp.Models;

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
                var repositoryMock = new Mock<IItemsRepository>();
                repositoryMock.Setup(r => r.GetAllItems()).Returns(items);

                var service = CreateService(itemsRepository: repositoryMock.Object);

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

                var repositoryMock = new Mock<IItemsRepository>();
                repositoryMock.Setup(r => r.GetAllItems()).Returns(items);

                var service = CreateService(itemsRepository: repositoryMock.Object);

                var result = service.SearchItemsByName("para");

                Assert.That(result.Count, Is.EqualTo(1));
            }

            [Test]
            public void GetItem_WhenCalled_DelegatesToRepository()
            {
                var item = CreateItem(1);
                var repositoryMock = new Mock<IItemsRepository>();
                repositoryMock.Setup(r => r.GetItemById(1)).Returns(item);

                var service = CreateService(itemsRepository: repositoryMock.Object);

                var result = service.GetItemById(1);

                Assert.That(result, Is.EqualTo(item));
            }

            [Test]
            public void SubstanceExists_WhenCalled_DelegatesToRepository()
            {
                var substancesRepository = new Mock<ISubstancesRepository>();
                substancesRepository.Setup(r => r.SubstanceExists("A")).Returns(true);

                var service = CreateService(substancesRepo: substancesRepository.Object);

                var result = service.SubstanceExists("A");

                Assert.That(result, Is.True);
            }

            [Test]
            public void AddItem_WhenValid_CallsRepository()
            {
                var repositoryMock = new Mock<IItemsRepository>();
                var item = CreateValidItem();

                var service = CreateService(itemsRepository: repositoryMock.Object);

                service.AddItem(item);

                repositoryMock.Verify(r => r.AddItemWithQuantity(
                    item.Name, item.Producer, item.Category,
                    item.Price, item.NumberOfPills,
                    item.Quantity, item.ActiveSubstances, item.Batches,
                    item.Label, item.Description, item.ImagePath,
                    item.DiscountPercentage), Times.Once);
            }

            [Test]
            public void AddItem_WhenInvalid_DoesNotCallRepository()
            {
                var repositoryMock = new Mock<IItemsRepository>();
                var item = CreateInvalidItem();

                var service = CreateService(itemsRepository: repositoryMock.Object);

                service.AddItem(item);

                repositoryMock.Verify(r => r.AddItemWithQuantity(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<float>(), It.IsAny<int>(),
                    It.IsAny<int>(), It.IsAny<Dictionary<string, float>>(), It.IsAny<Dictionary<DateOnly, int>>(),
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<float>()), Times.Never);
            }

            [Test]
            public void RemoveItem_WhenCalled_DelegatesToRepository()
            {
                var repositoryMock = new Mock<IItemsRepository>();
                var service = CreateService(itemsRepository: repositoryMock.Object);

                service.RemoveItemById(1);

                repositoryMock.Verify(r => r.RemoveItemById(1), Times.Once);
            }

            [Test]
            public void UpdateItem_WhenItemDoesNotExist_ThrowsArgumentException()
            {
                var repositoryMock = new Mock<IItemsRepository>();
                repositoryMock.Setup(r => r.ItemExists(1)).Returns(false);

                var service = CreateService(itemsRepository: repositoryMock.Object);

                Assert.That(() => service.UpdateItemById(1, CreateItem(1)), Throws.TypeOf<ArgumentException>());
            }

            [Test]
            public void UpdateItem_WhenStockBecomesPositive_SendsNotification()
            {
                var repositoryMock = new Mock<IItemsRepository>();

                var oldItem = CreateItem(1, quantity: 0);
                var newItem = CreateItem(1, quantity: 5);

                repositoryMock.Setup(r => r.ItemExists(1)).Returns(true);
                repositoryMock.Setup(r => r.GetItemById(1)).Returns(oldItem);

                var service = CreateService(itemsRepository: repositoryMock.Object);

                service.UpdateItemById(1, newItem);

                repositoryMock.Verify(result => result.UpdateItemById(It.Is<Item>(item => item.Quantity == 5)), Times.Once);
            }

            [Test]
            public void AddSubstance_WhenCalled_DelegatesToRepository()
            {
                var repositoryMock = new Mock<ISubstancesRepository>();
                var service = CreateService(substancesRepo: repositoryMock.Object);

                var substance = new Substance("A", 10f, "desc");

                service.AddSubstance(substance);

                repositoryMock.Verify(result => result.AddSubstance("A", 10f, "desc"), Times.Once);
            }

            [Test]
            public void RemoveSubstance_WhenCalled_DelegatesToRepository()
            {
                var repositoryMock = new Mock<ISubstancesRepository>();
                var service = CreateService(substancesRepo: repositoryMock.Object);

                var substance = new Substance("A", 10f, "desc");

                service.RemoveSubstanceByName(substance);

                repositoryMock.Verify(result => result.RemoveSubstanceByName("A"), Times.Once);
            }

            [Test]
            public void GetExpiredItems_WhenExpiredExists_ReturnsItems()
            {
                var item = CreateItemWithExpiredBatch();
                var repositoryMock = new Mock<IItemsRepository>();
                repositoryMock.Setup(r => r.GetAllItems()).Returns(new List<Item> { item });

                var service = CreateService(itemsRepository: repositoryMock.Object);

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
                var repositoryMock = new Mock<IItemsRepository>();
                repositoryMock.Setup(r => r.GetAllItems()).Returns(new List<Item> { item });

                var user = CreateUser(isAdmin: true);
                var service = CreateService(itemsRepository: repositoryMock.Object);

                var result = service.GetNotificationsForUser(user);

                Assert.That(result.Count, Is.EqualTo(1));
            }

            [Test]
            public void GetNotificationsForUser_WhenStockAlertTriggered_ReturnsNotification()
            {
                var item = CreateItem(1, quantity: 5);
                var repositoryMock = new Mock<IItemsRepository>();
                repositoryMock.Setup(r => r.GetItemById(1)).Returns(item);

                var user = CreateUser();
                user.StockAlerts.Add(1);

                var service = CreateService(itemsRepository: repositoryMock.Object);

                var result = service.GetNotificationsForUser(user);

                Assert.That(result.Count, Is.EqualTo(1));
            }

            [Test]
            public void GetTop30Items_WhenCalled_DelegatesToRepository()
            {
                var repositoryMock = new Mock<IItemsRepository>();
                var expected = new List<Tuple<int, string, int>>();

                repositoryMock.Setup(r => r.GetTop30Items()).Returns(expected);

                var service = CreateService(itemsRepository: repositoryMock.Object);

                var result = service.GetTop30Items();

                Assert.That(result, Is.EqualTo(expected));
            }

            [Test]
            public void GetTop30Substances_WhenCalled_DelegatesToRepository()
            {
                var repositoryMock = new Mock<ISubstancesRepository>();
                var expected = new Dictionary<string, int>();

                repositoryMock.Setup(r => r.GetTop30Substances()).Returns(expected);

                var service = CreateService(substancesRepo: repositoryMock.Object);

                var result = service.GetTop30Substances();

                Assert.That(result, Is.EqualTo(expected));
            }

            private static AdminService CreateService(
                IItemsRepository? itemsRepository = null,
                ISubstancesRepository? substancesRepo = null)
            {
                return new AdminService(
                    itemsRepository ?? new Mock<IItemsRepository>().Object,
                    substancesRepo ?? new Mock<ISubstancesRepository>().Object);
            }

            private static Item CreateItem(int id, string name = "Test", int quantity = 10)
            {
                var item = new Item(id, name, "Prod", "Cat", 10f, 10, string.Empty, string.Empty, "..\\..\\Assets\\placeholder.png", 0);
                item.AddNewBatchToItem(DateOnly.FromDateTime(DateTime.Now.AddDays(10)), quantity);
                item.ActiveSubstances["A"] = 1f;
                return item;
            }

            private static Item CreateValidItem()
            {
                return CreateItem(1);
            }

            private static Item CreateInvalidItem()
            {
                return new Item(1, string.Empty, string.Empty, string.Empty, 0f, 0, string.Empty, string.Empty, "..\\..\\Assets\\placeholder.png", 0);
            }

            private static Item CreateItemWithExpiredBatch()
            {
                var item = new Item(1, "Expired", "Prod", "Cat", 10f, 10, string.Empty, string.Empty, "..\\..\\Assets\\placeholder.png", 0);
                item.AddNewBatchToItem(DateOnly.FromDateTime(DateTime.Now.AddDays(-1)), 5);
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
