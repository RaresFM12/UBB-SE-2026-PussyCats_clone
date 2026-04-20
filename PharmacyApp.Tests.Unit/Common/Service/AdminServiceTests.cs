using PharmacyApp.Common.Repositories;
using PharmacyApp.Common.Services;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyApp.Tests.Unit.Common.Service
{
    [TestFixture]
    public class AdminServiceTests
    {
        private Mock<IItemsRepository> mockItemsRepository;
        private Mock<ISubstancesRepository> mockSubstancesRepository;
        private AdminService adminService;

        private static Item CreateItem(int id, string name, string producer, string category,
            float price, int quantity, float discount = 0f, int numberOfPills = 10,
            Dictionary<DateOnly, int>? batches = null, Dictionary<string, float>? activeSubstances = null)
        {
            var item = new Item(id, name, producer, category, price, numberOfPills,
                discount: discount, quantity: quantity);
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

        private static User CreateUser(int id, bool isAdmin = false)
        {
            return new User(id, "test@test.com", "1234567890", "hash", isAdmin, false,
                "TestUser", false, 0);
        }

        [SetUp]
        public void Setup()
        {
            mockItemsRepository = new Mock<IItemsRepository>();
            mockSubstancesRepository = new Mock<ISubstancesRepository>();
            adminService = new AdminService(mockItemsRepository.Object, mockSubstancesRepository.Object);
        }

        [Test]
        public void GetNotificationsForUser_AdminWithExpiredBatch_ReturnsProductExpiredNotification()
        {
            var user = CreateUser(1, isAdmin: true);
            var expiredDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            var items = new List<Item>
            {
                CreateItem(1, "ExpiredItem", "Bayer", "Medicine", 10f, 50,
                    batches: new Dictionary<DateOnly, int> { { expiredDate, 100 } })
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = adminService.GetNotificationsForUser(user);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Product Expired", result[0].Title);
            Assert.IsTrue(result[0].Message.Contains("1"));
            Assert.IsTrue(result[0].Message.Contains("expired"));
        }

        [Test]
        public void GetNotificationsForUser_AdminWithNoExpiredBatch_ReturnsNoExpiredNotifications()
        {
            var user = CreateUser(1, isAdmin: true);
            var futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30));
            var items = new List<Item>
            {
                CreateItem(1, "FreshItem", "Bayer", "Medicine", 10f, 50,
                    batches: new Dictionary<DateOnly, int> { { futureDate, 100 } })
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = adminService.GetNotificationsForUser(user);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetNotificationsForUser_NonAdminWithExpiredBatch_NoExpiredNotification()
        {
            var user = CreateUser(1, isAdmin: false);
            var expiredDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            var items = new List<Item>
            {
                CreateItem(1, "ExpiredItem", "Bayer", "Medicine", 10f, 50,
                    batches: new Dictionary<DateOnly, int> { { expiredDate, 100 } })
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = adminService.GetNotificationsForUser(user);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetNotificationsForUser_UserWithStockAlertItemInStock_ReturnsStockNotification()
        {
            var user = CreateUser(1);
            user.AddStockAlert(1);
            var item = CreateItem(1, "Paracetamol", "Bayer", "Medicine", 10f, 50, numberOfPills: 20,
                activeSubstances: new Dictionary<string, float> { { "Acetaminophen", 500f } });
            mockItemsRepository.Setup(repository => repository.GetItem(1)).Returns(item);

            var result = adminService.GetNotificationsForUser(user);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Stock Alert", result[0].Title);
            Assert.IsTrue(result[0].Message.Contains("Paracetamol"));
            Assert.IsTrue(result[0].Message.Contains("20 pills"));
            Assert.IsTrue(result[0].Message.Contains("Bayer"));
            Assert.IsTrue(result[0].Message.Contains("Acetaminophen"));
        }

        [Test]
        public void GetNotificationsForUser_UserWithStockAlertItemOutOfStock_ReturnsNoStockNotification()
        {
            var user = CreateUser(1);
            user.AddStockAlert(1);
            var item = CreateItem(1, "Paracetamol", "Bayer", "Medicine", 10f, 0);
            mockItemsRepository.Setup(repository => repository.GetItem(1)).Returns(item);

            var result = adminService.GetNotificationsForUser(user);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetNotificationsForUser_UserWithNoStockAlerts_ReturnsEmpty()
        {
            var user = CreateUser(1);
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(new List<Item>());

            var result = adminService.GetNotificationsForUser(user);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetNotificationsForUser_AdminWithMultipleExpiredItems_ReturnsMultipleNotifications()
        {
            var user = CreateUser(1, isAdmin: true);
            var expiredDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            var items = new List<Item>
            {
                CreateItem(1, "Expired1", "Bayer", "Medicine", 10f, 50,
                    batches: new Dictionary<DateOnly, int> { { expiredDate, 100 } }),
                CreateItem(2, "Expired2", "Pharma", "Medicine", 20f, 30,
                    batches: new Dictionary<DateOnly, int> { { expiredDate, 50 } })
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = adminService.GetNotificationsForUser(user);

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(notification => notification.Title == "Product Expired"));
        }

        [Test]
        public void GetNotificationsForUser_StockAlertNotificationContainsProductDetails()
        {
            var user = CreateUser(1);
            user.AddStockAlert(5);
            var item = CreateItem(5, "Aspirin", "PharmaCo", "Medicine", 15f, 100, numberOfPills: 30,
                activeSubstances: new Dictionary<string, float> { { "ASA", 100f }, { "Caffeine", 50f } });
            mockItemsRepository.Setup(repository => repository.GetItem(5)).Returns(item);

            var result = adminService.GetNotificationsForUser(user);

            Assert.AreEqual(1, result.Count);
            string body = result[0].Message;
            Assert.IsTrue(body.Contains("Aspirin"));
            Assert.IsTrue(body.Contains("30 pills"));
            Assert.IsTrue(body.Contains("PharmaCo"));
            Assert.IsTrue(body.Contains("ASA"));
            Assert.IsTrue(body.Contains("Caffeine"));
        }

        [Test]
        public void GetNotificationsForUser_AdminAndStockAlerts_ReturnsBothTypes()
        {
            var user = CreateUser(1, isAdmin: true);
            user.AddStockAlert(2);
            var expiredDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            var items = new List<Item>
            {
                CreateItem(1, "Expired", "Bayer", "Medicine", 10f, 50,
                    batches: new Dictionary<DateOnly, int> { { expiredDate, 100 } })
            };
            var stockItem = CreateItem(2, "InStock", "Pharma", "Medicine", 20f, 30);
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);
            mockItemsRepository.Setup(repository => repository.GetItem(2)).Returns(stockItem);

            var result = adminService.GetNotificationsForUser(user);

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(notification => notification.Title == "Product Expired"));
            Assert.IsTrue(result.Any(notification => notification.Title == "Stock Alert"));
        }

        [Test]
        public void GetNotificationsForUser_ExpiredNotificationBodyContainsProductId()
        {
            var user = CreateUser(1, isAdmin: true);
            var expiredDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            var items = new List<Item>
            {
                CreateItem(42, "TestItem", "Bayer", "Medicine", 10f, 50,
                    batches: new Dictionary<DateOnly, int> { { expiredDate, 100 } })
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = adminService.GetNotificationsForUser(user);

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0].Message.Contains("42"));
            Assert.IsTrue(result[0].Message.Contains("Please remove it"));
        }

        [Test]
        public void GetNotificationsForUser_StockAlertItemWithNoSubstances_ShowsNone()
        {
            var user = CreateUser(1);
            user.AddStockAlert(1);
            var item = CreateItem(1, "SimpleItem", "Bayer", "Medicine", 10f, 25, numberOfPills: 10);
            mockItemsRepository.Setup(repository => repository.GetItem(1)).Returns(item);

            var result = adminService.GetNotificationsForUser(user);

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result[0].Message.Contains("None"));
        }


        [Test]
        public void SendNewStockNotification_ValidItem_ReturnsStockAlertNotification()
        {
            var item = CreateItem(1, "Paracetamol", "Bayer", "Medicine", 10f, 50);

            var result = adminService.SendNewStockNotification(item);

            Assert.AreEqual("Stock Alert", result.Title);
        }

        [Test]
        public void SendAboutToExpireNotification_Called_ReturnsProductExpiredNotification()
        {
            var result = adminService.SendAboutToExpireNotification();

            Assert.AreEqual("Product Expired", result.Title);
        }


        [Test]
        public void GetExpiredItems_ItemWithExpiredBatch_ReturnsItem()
        {
            var expiredDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            var items = new List<Item>
            {
                CreateItem(1, "Expired", "Bayer", "Medicine", 10f, 50,
                    batches: new Dictionary<DateOnly, int> { { expiredDate, 100 } })
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = adminService.GetExpiredItems();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Expired", result[0].Name);
        }

        [Test]
        public void GetExpiredItems_ItemWithFutureBatch_ReturnsEmptyList()
        {
            var futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30));
            var items = new List<Item>
            {
                CreateItem(1, "Fresh", "Bayer", "Medicine", 10f, 50,
                    batches: new Dictionary<DateOnly, int> { { futureDate, 100 } })
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = adminService.GetExpiredItems();

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetExpiredItems_EmptyRepository_ReturnsEmptyList()
        {
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(new List<Item>());

            var result = adminService.GetExpiredItems();

            Assert.AreEqual(0, result.Count);
        }


        [Test]
        public void ValidateItemForAdd_ValidItem_DoesNotThrow()
        {
            var item = CreateItem(1, "Valid", "Producer", "Medicine", 10f, 5, numberOfPills: 10,
                activeSubstances: new Dictionary<string, float> { { "SubstanceA", 0.5f } });

            adminService.ValidateItemForAdd(item);
        }

        [Test]
        public void ValidateItemForAdd_EmptyName_ThrowsArgumentException()
        {
            var item = CreateItem(
                1,
                "",
                "Producer",
                "Medicine",
                10f,
                5,
                numberOfPills: 10,
                activeSubstances: new Dictionary<string, float> { { "SubstanceA", 0.5f } });

            Assert.Throws<ArgumentException>(() => adminService.ValidateItemForAdd(item));
        }

        [Test]
        public void ValidateItemForAdd_EmptyProducer_ThrowsArgumentException()
        {
            var item = CreateItem(
                1,
                "Name",
                "",
                "Medicine",
                10f,
                5,
                numberOfPills: 10,
                activeSubstances: new Dictionary<string, float> { { "SubstanceA", 0.5f } });

            Assert.Throws<ArgumentException>(() => adminService.ValidateItemForAdd(item));
        }

        [Test]
        public void ValidateItemForAdd_ZeroPrice_ThrowsArgumentException()
        {
            var item = CreateItem(
                1,
                "Name",
                "Producer",
                "Medicine",
                0f,
                5,
                numberOfPills: 10,
                activeSubstances: new Dictionary<string, float> { { "SubstanceA", 0.5f } });

            Assert.Throws<ArgumentException>(() => adminService.ValidateItemForAdd(item));
        }

        [Test]
        public void ValidateItemForAdd_NegativePrice_ThrowsArgumentException()
        {
            var item = CreateItem(
                1,
                "Name",
                "Producer",
                "Medicine",
                -5f,
                5,
                numberOfPills: 10,
                activeSubstances: new Dictionary<string, float> { { "SubstanceA", 0.5f } });

            Assert.Throws<ArgumentException>(() => adminService.ValidateItemForAdd(item));
        }

        [Test]
        public void ValidateItemForAdd_ZeroPills_ThrowsArgumentException()
        {
            var item = CreateItem(
                1,
                "Name",
                "Producer",
                "Medicine",
                10f,
                5,
                numberOfPills: 0,
                activeSubstances: new Dictionary<string, float> { { "SubstanceA", 0.5f } });

            Assert.Throws<ArgumentException>(() => adminService.ValidateItemForAdd(item));
        }

        [Test]
        public void ValidateItemForAdd_NoActiveSubstances_ThrowsArgumentException()
        {
            var item = CreateItem(
                1,
                "Name",
                "Producer",
                "Medicine",
                10f,
                5,
                numberOfPills: 10);

            Assert.Throws<ArgumentException>(() => adminService.ValidateItemForAdd(item));
        }

        [Test]
        public void ValidateItemForAdd_NegativeDiscount_ThrowsArgumentException()
        {
            var item = CreateItem(
                1,
                "Name",
                "Producer",
                "Medicine",
                10f,
                5,
                numberOfPills: 10,
                discount: -0.1f,
                activeSubstances: new Dictionary<string, float> { { "SubstanceA", 0.5f } });

            Assert.Throws<ArgumentException>(() => adminService.ValidateItemForAdd(item));
        }


        [Test]
        public void UpdateItem_ItemGoesFromZeroToPositiveStock_TriggersStockNotification()
        {
            var previousItem = CreateItem(1, "PrevItem", "Bayer", "Medicine", 10f, 0);
            var updatedItem = CreateItem(1, "UpdatedItem", "Bayer", "Medicine", 10f, 50,
                activeSubstances: new Dictionary<string, float> { { "SubA", 1f } });
            mockItemsRepository.Setup(repository => repository.ItemExists(1)).Returns(true);
            mockItemsRepository.Setup(repository => repository.GetItem(1)).Returns(previousItem);

            adminService.UpdateItem(1, updatedItem);

            mockItemsRepository.Verify(repository => repository.UpdateItem(It.IsAny<Item>()), Times.Once);
        }

        [Test]
        public void UpdateItem_NonExistentItem_ThrowsArgumentException()
        {
            var updatedItem = CreateItem(1, "Item", "Bayer", "Medicine", 10f, 50);
            mockItemsRepository
                .Setup(repository => repository.ItemExists(1))
                .Returns(false);

            Assert.Throws<ArgumentException>(() =>
            {
                adminService.UpdateItem(1, updatedItem);
            });
        }

        [Test]
        public void RemoveItem_ValidId_CallsRepository()
        {
            adminService.RemoveItem(1);

            mockItemsRepository.Verify(repository => repository.RemoveItem(1), Times.Once);
        }

        [Test]
        public void AddSubstance_ValidSubstance_CallsRepository()
        {
            var substance = new Substance("TestSubstance", 100f, "Test description");

            adminService.AddSubstance(substance);

            mockSubstancesRepository.Verify(repository =>
                repository.AddSubstance("TestSubstance", 100f, "Test description"), Times.Once);
        }

        [Test]
        public void RemoveSubstance_ValidSubstance_CallsRepository()
        {
            var substance = new Substance("TestSubstance", 100f, "Test description");

            adminService.RemoveSubstance(substance);

            mockSubstancesRepository.Verify(repository =>
                repository.RemoveSubstance("TestSubstance"), Times.Once);
        }

        [Test]
        public void UpdateSubstance_ValidSubstance_CallsRepository()
        {
            var substance = new Substance("TestSubstance", 100f, "Updated description");

            adminService.UpdateSubstance("TestSubstance", substance);

            mockSubstancesRepository.Verify(repository =>
                repository.UpdateSubstance(substance), Times.Once);
        }

        [Test]
        public void GetTop30Items_ReturnsRepositoryData()
        {
            var topItems = new List<Tuple<int, string, int>>
            {
                Tuple.Create(1, "Paracetamol", 150),
                Tuple.Create(2, "Ibuprofen", 120),
                Tuple.Create(3, "Aspirin", 100)
            };
            mockItemsRepository.Setup(repository => repository.GetTop30Items()).Returns(topItems);

            var result = adminService.GetTop30Items();

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("Paracetamol", result[0].Item2);
            Assert.AreEqual(150, result[0].Item3);
        }

        [Test]
        public void GetTop30Items_EmptyData_ReturnsEmptyList()
        {
            mockItemsRepository.Setup(repository => repository.GetTop30Items())
                .Returns(new List<Tuple<int, string, int>>());

            var result = adminService.GetTop30Items();

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetTop20Substances_ReturnsRepositoryData()
        {
            var topSubstances = new Dictionary<string, int>
            {
                { "Aspirin", 50 },
                { "Caffeine", 30 },
                { "Acetaminophen", 25 }
            };
            mockSubstancesRepository.Setup(repository => repository.GetTop20Substances()).Returns(topSubstances);

            var result = adminService.GetTop20Substances();

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(50, result["Aspirin"]);
        }

        [Test]
        public void GetTop20Substances_EmptyData_ReturnsEmptyDictionary()
        {
            mockSubstancesRepository.Setup(repository => repository.GetTop20Substances())
                .Returns(new Dictionary<string, int>());

            var result = adminService.GetTop20Substances();

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetTop30Items_CallsRepositoryExactlyOnce()
        {
            mockItemsRepository.Setup(repository => repository.GetTop30Items())
                .Returns(new List<Tuple<int, string, int>>());

            adminService.GetTop30Items();

            mockItemsRepository.Verify(repository => repository.GetTop30Items(), Times.Once);
        }

        [Test]
        public void GetTop20Substances_CallsRepositoryExactlyOnce()
        {
            mockSubstancesRepository.Setup(repository => repository.GetTop20Substances())
                .Returns(new Dictionary<string, int>());

            adminService.GetTop20Substances();

            mockSubstancesRepository.Verify(repository => repository.GetTop20Substances(), Times.Once);
        }
    }
}
