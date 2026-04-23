using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PharmacyApp.Common.Repositories;
using PharmacyApp.Common.Services;
using PharmacyApp.Models;

namespace PharmacyApp.Tests.Clean.Common.Service
{
    [TestFixture]
    public class AdminServiceTests
    {
        private Mock<IItemsRepository> mockItemsRepository;
        private Mock<ISubstancesRepository> mockSubstancesRepository;
        private AdminService adminService;

        private static Item CreateItem(
            int id,
            string name,
            string producer,
            string category,
            float price,
            int quantity,
            float discount = 0f,
            int numberOfPills = 10,
            Dictionary<DateOnly, int>? batches = null,
            Dictionary<string, float>? activeSubstances = null)
        {
            var item = new Item(id, name, producer, category, price, numberOfPills, discount: discount, quantity: quantity);

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
            return new User(id, "test@test.com", "1234567890", "hash", isAdmin, false, "TestUser", false, 0);
        }

        [SetUp]
        public void Setup()
        {
            mockItemsRepository = new Mock<IItemsRepository>();
            mockSubstancesRepository = new Mock<ISubstancesRepository>();
            adminService = new AdminService(mockItemsRepository.Object, mockSubstancesRepository.Object);
        }

        [Test]
        public void GetNotificationsForUser_AdminWithExpiredBatch_ReturnsSingleNotification()
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

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetNotificationsForUser_AdminWithExpiredBatch_ReturnsExpectedExpiredNotificationContent()
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

            Assert.That(
                MatchesExpiredNotification(result[0], "1"),
                Is.True);
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

            Assert.That(result.Count, Is.EqualTo(0));
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

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetNotificationsForUser_UserWithStockAlertItemInStock_ReturnsSingleStockNotification()
        {
            var user = CreateUser(1);
            user.AddStockAlertToUser(1);

            var item = CreateItem(
                1,
                "Paracetamol",
                "Bayer",
                "Medicine",
                10f,
                50,
                numberOfPills: 20,
                activeSubstances: new Dictionary<string, float> { { "Acetaminophen", 500f } });

            mockItemsRepository.Setup(repository => repository.GetItemById(1)).Returns(item);

            var result = adminService.GetNotificationsForUser(user);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetNotificationsForUser_UserWithStockAlertItemInStock_ReturnsExpectedStockNotificationContent()
        {
            var user = CreateUser(1);
            user.AddStockAlertToUser(1);

            var item = CreateItem(
                1,
                "Paracetamol",
                "Bayer",
                "Medicine",
                10f,
                50,
                numberOfPills: 20,
                activeSubstances: new Dictionary<string, float> { { "Acetaminophen", 500f } });

            mockItemsRepository.Setup(repository => repository.GetItemById(1)).Returns(item);

            var result = adminService.GetNotificationsForUser(user);

            Assert.That(
                MatchesStockNotification(
                    result[0],
                    "Paracetamol",
                    "20 pills",
                    "Bayer",
                    "Acetaminophen"),
                Is.True);
        }

        [Test]
        public void GetNotificationsForUser_UserWithStockAlertItemOutOfStock_ReturnsNoStockNotification()
        {
            var user = CreateUser(1);
            user.AddStockAlertToUser(1);
            var item = CreateItem(1, "Paracetamol", "Bayer", "Medicine", 10f, 0);

            mockItemsRepository.Setup(repository => repository.GetItemById(1)).Returns(item);

            var result = adminService.GetNotificationsForUser(user);

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetNotificationsForUser_UserWithNoStockAlerts_ReturnsEmpty()
        {
            var user = CreateUser(1);

            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(new List<Item>());

            var result = adminService.GetNotificationsForUser(user);

            Assert.That(result.Count, Is.EqualTo(0));
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

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetNotificationsForUser_AdminWithMultipleExpiredItems_AllNotificationsAreExpiredType()
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

            Assert.That(result.All(notification => notification.Title == "Product Expired"), Is.True);
        }

        [Test]
        public void GetNotificationsForUser_StockAlertNotificationContainsProductDetails_ReturnsSingleNotification()
        {
            var user = CreateUser(1);
            user.AddStockAlertToUser(5);

            var item = CreateItem(
                5,
                "Aspirin",
                "PharmaCo",
                "Medicine",
                15f,
                100,
                numberOfPills: 30,
                activeSubstances: new Dictionary<string, float> { { "ASA", 100f }, { "Caffeine", 50f } });

            mockItemsRepository.Setup(repository => repository.GetItemById(5)).Returns(item);

            var result = adminService.GetNotificationsForUser(user);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetNotificationsForUser_StockAlertNotificationContainsProductDetails_MessageContainsAllExpectedDetails()
        {
            var user = CreateUser(1);
            user.AddStockAlertToUser(5);

            var item = CreateItem(
                5,
                "Aspirin",
                "PharmaCo",
                "Medicine",
                15f,
                100,
                numberOfPills: 30,
                activeSubstances: new Dictionary<string, float> { { "ASA", 100f }, { "Caffeine", 50f } });

            mockItemsRepository.Setup(repository => repository.GetItemById(5)).Returns(item);

            var result = adminService.GetNotificationsForUser(user);

            Assert.That(
                MatchesStockNotification(
                    result[0],
                    "Aspirin",
                    "30 pills",
                    "PharmaCo",
                    "ASA",
                    "Caffeine"),
                Is.True);
        }

        [Test]
        public void GetNotificationsForUser_AdminAndStockAlerts_ReturnsTwoNotifications()
        {
            var user = CreateUser(1, isAdmin: true);
            user.AddStockAlertToUser(2);

            var expiredDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            var items = new List<Item>
            {
                CreateItem(1, "Expired", "Bayer", "Medicine", 10f, 50,
                    batches: new Dictionary<DateOnly, int> { { expiredDate, 100 } })
            };

            var stockItem = CreateItem(2, "InStock", "Pharma", "Medicine", 20f, 30);

            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);
            mockItemsRepository.Setup(repository => repository.GetItemById(2)).Returns(stockItem);

            var result = adminService.GetNotificationsForUser(user);

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetNotificationsForUser_AdminAndStockAlerts_ContainsExpiredNotification()
        {
            var user = CreateUser(1, isAdmin: true);
            user.AddStockAlertToUser(2);

            var expiredDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            var items = new List<Item>
            {
                CreateItem(1, "Expired", "Bayer", "Medicine", 10f, 50,
                    batches: new Dictionary<DateOnly, int> { { expiredDate, 100 } })
            };

            var stockItem = CreateItem(2, "InStock", "Pharma", "Medicine", 20f, 30);

            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);
            mockItemsRepository.Setup(repository => repository.GetItemById(2)).Returns(stockItem);

            var result = adminService.GetNotificationsForUser(user);

            Assert.That(result.Any(notification => notification.Title == "Product Expired"), Is.True);
        }

        [Test]
        public void GetNotificationsForUser_AdminAndStockAlerts_ContainsStockAlertNotification()
        {
            var user = CreateUser(1, isAdmin: true);
            user.AddStockAlertToUser(2);

            var expiredDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            var items = new List<Item>
            {
                CreateItem(1, "Expired", "Bayer", "Medicine", 10f, 50,
                    batches: new Dictionary<DateOnly, int> { { expiredDate, 100 } })
            };

            var stockItem = CreateItem(2, "InStock", "Pharma", "Medicine", 20f, 30);

            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);
            mockItemsRepository.Setup(repository => repository.GetItemById(2)).Returns(stockItem);

            var result = adminService.GetNotificationsForUser(user);

            Assert.That(result.Any(notification => notification.Title == "Stock Alert"), Is.True);
        }

        [Test]
        public void GetNotificationsForUser_ExpiredNotificationBodyContainsProductId_ReturnsSingleNotification()
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

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetNotificationsForUser_ExpiredNotificationBodyContainsProductId_MessageContainsExpectedDetails()
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

            Assert.That(
                result[0].Message.Contains("42") && result[0].Message.Contains("Please remove it"),
                Is.True);
        }

        [Test]
        public void GetNotificationsForUser_StockAlertItemWithNoSubstances_ShowsSingleNotification()
        {
            var user = CreateUser(1);
            user.AddStockAlertToUser(1);
            var item = CreateItem(1, "SimpleItem", "Bayer", "Medicine", 10f, 25, numberOfPills: 10);

            mockItemsRepository.Setup(repository => repository.GetItemById(1)).Returns(item);

            var result = adminService.GetNotificationsForUser(user);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetNotificationsForUser_StockAlertItemWithNoSubstances_ShowsNoneInMessage()
        {
            var user = CreateUser(1);
            user.AddStockAlertToUser(1);
            var item = CreateItem(1, "SimpleItem", "Bayer", "Medicine", 10f, 25, numberOfPills: 10);

            mockItemsRepository.Setup(repository => repository.GetItemById(1)).Returns(item);

            var result = adminService.GetNotificationsForUser(user);

            Assert.That(result[0].Message.Contains("None"), Is.True);
        }

        [Test]
        public void SendNewStockNotification_ValidItem_ReturnsStockAlertNotification()
        {
            var item = CreateItem(1, "Paracetamol", "Bayer", "Medicine", 10f, 50);

            var result = adminService.SendNewStockNotification(item);

            Assert.That(result.Title, Is.EqualTo("Stock Alert"));
        }

        [Test]
        public void SendAboutToExpireNotification_Called_ReturnsProductExpiredNotification()
        {
            var result = adminService.SendAboutToExpireNotification();

            Assert.That(result.Title, Is.EqualTo("Product Expired"));
        }

        [Test]
        public void GetExpiredItems_ItemWithExpiredBatch_ReturnsSingleItem()
        {
            var expiredDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            var items = new List<Item>
            {
                CreateItem(1, "Expired", "Bayer", "Medicine", 10f, 50,
                    batches: new Dictionary<DateOnly, int> { { expiredDate, 100 } })
            };

            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = adminService.GetExpiredItems();

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetExpiredItems_ItemWithExpiredBatch_ReturnsExpectedItem()
        {
            var expiredDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            var items = new List<Item>
            {
                CreateItem(1, "Expired", "Bayer", "Medicine", 10f, 50,
                    batches: new Dictionary<DateOnly, int> { { expiredDate, 100 } })
            };

            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = adminService.GetExpiredItems();

            Assert.That(result[0].Name, Is.EqualTo("Expired"));
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

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetExpiredItems_EmptyRepository_ReturnsEmptyList()
        {
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(new List<Item>());

            var result = adminService.GetExpiredItems();

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void ValidateItemForAdd_ValidItem_DoesNotThrow()
        {
            var item = CreateItem(
                1,
                "Valid",
                "Producer",
                "Medicine",
                10f,
                5,
                numberOfPills: 10,
                activeSubstances: new Dictionary<string, float> { { "SubstanceA", 0.5f } });

            Assert.DoesNotThrow(() => adminService.ValidateItemForAdd(item));
        }

        [Test]
        public void ValidateItemForAdd_EmptyName_ThrowsArgumentException()
        {
            var item = CreateItem(
                1,
                string.Empty,
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
                string.Empty,
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
            var item = CreateItem(1, "Name", "Producer", "Medicine", 10f, 5, numberOfPills: 10);

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
        public void UpdateItem_ItemGoesFromZeroToPositiveStock_TriggersRepositoryUpdate()
        {
            var previousItem = CreateItem(1, "PrevItem", "Bayer", "Medicine", 10f, 0);
            var updatedItem = CreateItem(1, "UpdatedItem", "Bayer", "Medicine", 10f, 50,
                activeSubstances: new Dictionary<string, float> { { "SubA", 1f } });

            mockItemsRepository.Setup(repository => repository.ItemExists(1)).Returns(true);
            mockItemsRepository.Setup(repository => repository.GetItemById(1)).Returns(previousItem);

            adminService.UpdateItemById(1, updatedItem);

            mockItemsRepository.Verify(repository => repository.UpdateItemById(It.IsAny<Item>()), Times.Once);
        }

        [Test]
        public void UpdateItem_NonExistentItem_ThrowsArgumentException()
        {
            var updatedItem = CreateItem(1, "Item", "Bayer", "Medicine", 10f, 50);

            mockItemsRepository.Setup(repository => repository.ItemExists(1)).Returns(false);

            Assert.Throws<ArgumentException>(() => adminService.UpdateItemById(1, updatedItem));
        }

        [Test]
        public void RemoveItem_ValidId_CallsRepository()
        {
            adminService.RemoveItemById(1);

            mockItemsRepository.Verify(repository => repository.RemoveItemById(1), Times.Once);
        }

        [Test]
        public void AddSubstance_ValidSubstance_CallsRepository()
        {
            var substance = new Substance("TestSubstance", 100f, "Test description");

            adminService.AddSubstance(substance);

            mockSubstancesRepository.Verify(
                repository => repository.AddSubstance("TestSubstance", 100f, "Test description"),
                Times.Once);
        }

        [Test]
        public void RemoveSubstance_ValidSubstance_CallsRepository()
        {
            var substance = new Substance("TestSubstance", 100f, "Test description");

            adminService.RemoveSubstanceByName(substance);

            mockSubstancesRepository.Verify(repository => repository.RemoveSubstanceByName("TestSubstance"), Times.Once);
        }

        [Test]
        public void UpdateSubstance_ValidSubstance_CallsRepository()
        {
            var substance = new Substance("TestSubstance", 100f, "Updated description");

            adminService.UpdateSubstanceByName("TestSubstance", substance);

            mockSubstancesRepository.Verify(repository => repository.UpdateSubstanceByName(substance), Times.Once);
        }

        [Test]
        public void GetTop30Items_ReturnsRepositoryData_CountMatches()
        {
            var topItems = new List<Tuple<int, string, int>>
            {
                Tuple.Create(1, "Paracetamol", 150),
                Tuple.Create(2, "Ibuprofen", 120),
                Tuple.Create(3, "Aspirin", 100)
            };

            mockItemsRepository.Setup(repository => repository.GetTop30Items()).Returns(topItems);

            var result = adminService.GetTop30Items();

            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public void GetTop30Items_ReturnsRepositoryData_FirstEntryMatchesExpectedData()
        {
            var topItems = new List<Tuple<int, string, int>>
            {
                Tuple.Create(1, "Paracetamol", 150),
                Tuple.Create(2, "Ibuprofen", 120),
                Tuple.Create(3, "Aspirin", 100)
            };

            mockItemsRepository.Setup(repository => repository.GetTop30Items()).Returns(topItems);

            var result = adminService.GetTop30Items();

            Assert.That(
                result[0].Item2 == "Paracetamol" && result[0].Item3 == 150,
                Is.True);
        }

        [Test]
        public void GetTop30Items_EmptyData_ReturnsEmptyList()
        {
            mockItemsRepository.Setup(repository => repository.GetTop30Items()).Returns(new List<Tuple<int, string, int>>());

            var result = adminService.GetTop30Items();

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetTop20Substances_ReturnsRepositoryData_CountMatches()
        {
            var topSubstances = new Dictionary<string, int>
            {
                { "Aspirin", 50 },
                { "Caffeine", 30 },
                { "Acetaminophen", 25 }
            };

            mockSubstancesRepository.Setup(repository => repository.GetTop20Substances()).Returns(topSubstances);

            var result = adminService.GetTop20Substances();

            Assert.That(result.Count, Is.EqualTo(3));
        }

        [Test]
        public void GetTop20Substances_ReturnsRepositoryData_ContainsExpectedEntry()
        {
            var topSubstances = new Dictionary<string, int>
            {
                { "Aspirin", 50 },
                { "Caffeine", 30 },
                { "Acetaminophen", 25 }
            };

            mockSubstancesRepository.Setup(repository => repository.GetTop20Substances()).Returns(topSubstances);

            var result = adminService.GetTop20Substances();

            Assert.That(result["Aspirin"], Is.EqualTo(50));
        }

        [Test]
        public void GetTop20Substances_EmptyData_ReturnsEmptyDictionary()
        {
            mockSubstancesRepository.Setup(repository => repository.GetTop20Substances()).Returns(new Dictionary<string, int>());

            var result = adminService.GetTop20Substances();

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetTop30Items_CallsRepositoryExactlyOnce()
        {
            mockItemsRepository.Setup(repository => repository.GetTop30Items()).Returns(new List<Tuple<int, string, int>>());

            adminService.GetTop30Items();

            mockItemsRepository.Verify(repository => repository.GetTop30Items(), Times.Once);
        }

        [Test]
        public void GetTop20Substances_CallsRepositoryExactlyOnce()
        {
            mockSubstancesRepository.Setup(repository => repository.GetTop20Substances()).Returns(new Dictionary<string, int>());

            adminService.GetTop20Substances();

            mockSubstancesRepository.Verify(repository => repository.GetTop20Substances(), Times.Once);
        }

        [Test]
        public void AddItem_ValidItem_CallsRepository()
        {
            var item = CreateItem(
                1,
                "Valid",
                "Producer",
                "Medicine",
                10f,
                5,
                numberOfPills: 10,
                activeSubstances: new Dictionary<string, float> { { "SubstanceA", 0.5f } });

            adminService.AddItem(item);

            mockItemsRepository.Verify(repository => repository.AddItemWithQuantity(
                item.Name,
                item.Producer,
                item.Category,
                item.Price,
                item.NumberOfPills,
                item.Quantity,
                item.ActiveSubstances,
                item.Batches,
                item.Label,
                item.Description,
                item.ImagePath,
                item.DiscountPercentage), Times.Once);
        }

        [Test]
        public void AddItem_InvalidItem_DoesNotThrowBecauseExceptionIsCaught()
        {
            var item = CreateItem(
                1,
                string.Empty,
                "Producer",
                "Medicine",
                10f,
                5,
                numberOfPills: 10,
                activeSubstances: new Dictionary<string, float> { { "SubstanceA", 0.5f } });

            Assert.DoesNotThrow(() => adminService.AddItem(item));
        }

        [Test]
        public void AddItem_InvalidItem_DoesNotCallRepository()
        {
            var item = CreateItem(
                1,
                string.Empty,
                "Producer",
                "Medicine",
                10f,
                5,
                numberOfPills: 10,
                activeSubstances: new Dictionary<string, float> { { "SubstanceA", 0.5f } });

            adminService.AddItem(item);

            mockItemsRepository.Verify(repository => repository.AddItemWithQuantity(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<float>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Dictionary<string, float>>(),
                It.IsAny<Dictionary<DateOnly, int>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<float>()), Times.Never);
        }

        [Test]
        public void AddItemWithQuantity_ValidItem_CallsRepository()
        {
            var item = CreateItem(
                1,
                "Valid",
                "Producer",
                "Medicine",
                10f,
                5,
                numberOfPills: 10,
                activeSubstances: new Dictionary<string, float> { { "SubstanceA", 0.5f } });

            adminService.AddItemWithQuantity(item);

            mockItemsRepository.Verify(repository => repository.AddItemWithQuantity(
                item.Name,
                item.Producer,
                item.Category,
                item.Price,
                item.NumberOfPills,
                item.Quantity,
                item.ActiveSubstances,
                item.Batches,
                item.Label,
                item.Description,
                item.ImagePath,
                item.DiscountPercentage), Times.Once);
        }

        [Test]
        public void AddItemWithQuantity_InvalidItem_DoesNotThrowBecauseExceptionIsCaught()
        {
            var item = CreateItem(
                1,
                "Valid",
                string.Empty,
                "Medicine",
                10f,
                5,
                numberOfPills: 10,
                activeSubstances: new Dictionary<string, float> { { "SubstanceA", 0.5f } });

            Assert.DoesNotThrow(() => adminService.AddItemWithQuantity(item));
        }

        [Test]
        public void AddItemWithQuantity_InvalidItem_DoesNotCallRepository()
        {
            var item = CreateItem(
                1,
                "Valid",
                string.Empty,
                "Medicine",
                10f,
                5,
                numberOfPills: 10,
                activeSubstances: new Dictionary<string, float> { { "SubstanceA", 0.5f } });

            adminService.AddItemWithQuantity(item);

            mockItemsRepository.Verify(repository => repository.AddItemWithQuantity(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<float>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Dictionary<string, float>>(),
                It.IsAny<Dictionary<DateOnly, int>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<float>()), Times.Never);
        }

        [Test]
        public void UpdateItem_WhenPreviousQuantityWasAlreadyPositive_PreservesUpdatedItemIdentifier()
        {
            var previousItem = CreateItem(1, "PrevItem", "Bayer", "Medicine", 10f, 3);
            var updatedItem = CreateItem(1, "UpdatedItem", "Bayer", "Medicine", 10f, 8,
                activeSubstances: new Dictionary<string, float> { { "SubA", 1f } });

            mockItemsRepository.Setup(repository => repository.ItemExists(1)).Returns(true);
            mockItemsRepository.Setup(repository => repository.GetItemById(1)).Returns(previousItem);

            adminService.UpdateItemById(1, updatedItem);

            Assert.That(updatedItem.Id, Is.EqualTo(1));
        }

        [Test]
        public void UpdateItem_WhenPreviousQuantityWasAlreadyPositive_CallsRepositoryUpdate()
        {
            var previousItem = CreateItem(1, "PrevItem", "Bayer", "Medicine", 10f, 3);
            var updatedItem = CreateItem(1, "UpdatedItem", "Bayer", "Medicine", 10f, 8,
                activeSubstances: new Dictionary<string, float> { { "SubA", 1f } });

            mockItemsRepository.Setup(repository => repository.ItemExists(1)).Returns(true);
            mockItemsRepository.Setup(repository => repository.GetItemById(1)).Returns(previousItem);

            adminService.UpdateItemById(1, updatedItem);

            mockItemsRepository.Verify(repository => repository.UpdateItemById(updatedItem), Times.Once);
        }

        [Test]
        public void SendNewStockNotification_ReturnsExpectedTitle()
        {
            var item = CreateItem(7, "Paracetamol", "Bayer", "Medicine", 10f, 50, numberOfPills: 20);

            var result = adminService.SendNewStockNotification(item);

            Assert.That(result.Title, Is.EqualTo("Stock Alert"));
        }

        [Test]
        public void SendNewStockNotification_ReturnsExpectedMessageContent()
        {
            var item = CreateItem(7, "Paracetamol", "Bayer", "Medicine", 10f, 50, numberOfPills: 20);

            var result = adminService.SendNewStockNotification(item);

            Assert.That(result.Message, Is.EqualTo("New item back in stock!"));
        }

        [Test]
        public void GetExpiredItems_ItemWithBatchExpiringToday_IsNotReturnedBecauseComparisonIsStrictlyLessThanToday()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var items = new List<Item>
            {
                CreateItem(1, "TodayItem", "Bayer", "Medicine", 10f, 50,
                    batches: new Dictionary<DateOnly, int> { { today, 100 } })
            };

            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = adminService.GetExpiredItems();

            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void ValidateItemForAdd_EmptyActiveSubstances_ThrowsArgumentException()
        {
            var item = CreateItem(
                1,
                "Name",
                "Producer",
                "Medicine",
                10f,
                5,
                numberOfPills: 10,
                activeSubstances: new Dictionary<string, float>());

            Assert.Throws<ArgumentException>(() => adminService.ValidateItemForAdd(item));
        }

        private static bool MatchesExpiredNotification(Notification notification, string expectedProductId)
        {
            return notification.Title == "Product Expired"
                && notification.Message.Contains(expectedProductId)
                && notification.Message.Contains("expired");
        }

        private static bool MatchesStockNotification(
            Notification notification,
            string expectedName,
            string expectedPillsText,
            string expectedProducer,
            params string[] expectedSubstances)
        {
            bool containsBaseInfo =
                notification.Title == "Stock Alert"
                && notification.Message.Contains(expectedName)
                && notification.Message.Contains(expectedPillsText)
                && notification.Message.Contains(expectedProducer);

            bool containsAllSubstances = expectedSubstances.All(substance => notification.Message.Contains(substance));

            return containsBaseInfo && containsAllSubstances;
        }
    }
}