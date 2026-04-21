using NUnit.Framework;
using Moq;
using PharmacyApp.Common.Repositories;
using PharmacyApp.Features.Products_Catalogue.Service;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PharmacyApp.Tests.Unit.Features.ProductCatalogue
{
    [TestFixture]
    public class ProductCatalogueServiceTests
    {
        private Mock<IItemsRepository> mockItemsRepository;
        private ProductCatalogueService productCatalogueService;

        private static Item CreateItem(int id, string name, string producer, string category,
            float price, int quantity, float discount = 0f, int numberOfPills = 10,
            Dictionary<DateOnly, int>? batches = null, Dictionary<string, float>? activeSubstances = null)
        {
            var item = new Item(id, name, producer, category, price, numberOfPills,
                discount: discount, quantity: quantity);

            if (batches != null)
                foreach (var batch in batches) item.Batches[batch.Key] = batch.Value;

            if (activeSubstances != null)
                foreach (var substance in activeSubstances) item.ActiveSubstances[substance.Key] = substance.Value;

            return item;
        }

        [SetUp]
        public void Setup()
        {
            mockItemsRepository = new Mock<IItemsRepository>();
            productCatalogueService = new ProductCatalogueService(mockItemsRepository.Object);
        }

        // ════════════════════════════════════════════════════════════════════════════
        // TEAMMATE's PART: F4.1 (Product Listing) & F4.2 (Product Search)
        // ════════════════════════════════════════════════════════════════════════════

        [Test]
        public void GetItems_NullSearch_ReturnsCorrectCount()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Paracetamol", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "Vitamin C", "Pharma", "Supplements", 20f, 30)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null);

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void GetItems_EmptySearch_ReturnsCorrectCount()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Paracetamol", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "Vitamin C", "Pharma", "Supplements", 20f, 30)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems("");

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void GetItems_EmptyRepository_ReturnsEmptyListCount()
        {
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(new List<Item>());

            var result = productCatalogueService.GetItems(null);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetItems_Pagination_FirstPageReturnsExactPageSizeCount()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Item1", "Prod", "Cat", 10f, 10),
                CreateItem(2, "Item2", "Prod", "Cat", 10f, 10),
                CreateItem(3, "Item3", "Prod", "Cat", 10f, 10)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var firstPage = productCatalogueService.GetItems(null, page: 0, pageSize: 2);

            Assert.AreEqual(2, firstPage.Count);
        }

        [Test]
        public void GetItems_Pagination_SecondPageReturnsRemainingItemsCount()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Item1", "Prod", "Cat", 10f, 10),
                CreateItem(2, "Item2", "Prod", "Cat", 10f, 10),
                CreateItem(3, "Item3", "Prod", "Cat", 10f, 10)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var secondPage = productCatalogueService.GetItems(null, page: 1, pageSize: 2);

            Assert.AreEqual(1, secondPage.Count);
        }

        [Test]
        public void GetItems_PageBeyondItems_ReturnsEmptyListCount()
        {
            var items = new List<Item> { CreateItem(1, "Item1", "Bayer", "Medicine", 10f, 50) };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, page: 5, pageSize: 10);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetItems_DefaultPageSize_LimitsResultsToTenItems()
        {
            // Explicitly creating 11 items to avoid any for-loops inside the test
            var items = new List<Item>
            {
                CreateItem(1, "Item1", "P", "C", 10f, 50), CreateItem(2, "Item2", "P", "C", 10f, 50),
                CreateItem(3, "Item3", "P", "C", 10f, 50), CreateItem(4, "Item4", "P", "C", 10f, 50),
                CreateItem(5, "Item5", "P", "C", 10f, 50), CreateItem(6, "Item6", "P", "C", 10f, 50),
                CreateItem(7, "Item7", "P", "C", 10f, 50), CreateItem(8, "Item8", "P", "C", 10f, 50),
                CreateItem(9, "Item9", "P", "C", 10f, 50), CreateItem(10, "Item10", "P", "C", 10f, 50),
                CreateItem(11, "Item11", "P", "C", 10f, 50)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null);

            Assert.AreEqual(ProductCatalogueService.DefaultPageSize, result.Count);
        }

        [Test]
        public void GetItems_SearchByName_ReturnsCorrectCount()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Paracetamol", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "Ibuprofen", "Pharma", "Medicine", 20f, 30),
                CreateItem(3, "Paracetamol Extra", "Generic", "Medicine", 15f, 25)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems("Paracetamol");

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void GetItems_SearchCaseInsensitive_ReturnsCorrectCount()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Paracetamol", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "Ibuprofen", "Pharma", "Medicine", 20f, 30)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems("paracetamol");

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void GetItems_SearchCaseInsensitive_ReturnsCorrectItemName()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Paracetamol", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "Ibuprofen", "Pharma", "Medicine", 20f, 30)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems("paracetamol");

            Assert.AreEqual("Paracetamol", result[0].Name);
        }

        [Test]
        public void GetItems_SearchPartialMatch_ReturnsCorrectCount()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Paracetamol", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "Ibuprofen", "Pharma", "Medicine", 20f, 30)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems("para");

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void GetItems_SearchPartialMatch_ReturnsCorrectItemName()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Paracetamol", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "Ibuprofen", "Pharma", "Medicine", 20f, 30)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems("para");

            Assert.AreEqual("Paracetamol", result[0].Name);
        }

        [Test]
        public void GetItems_SearchNoMatch_ReturnsEmptyCount()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Paracetamol", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "Ibuprofen", "Pharma", "Medicine", 20f, 30)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems("Aspirin");

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetItems_SearchWithNullItemName_DoesNotThrowAndReturnsZeroCount()
        {
            var items = new List<Item> { CreateItem(1, null!, "Bayer", "Medicine", 10f, 50) };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems("test");

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetItems_SearchSkipsItemsWithNullName_ReturnsCorrectCount()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Paracetamol", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, null!, "Pharma", "Medicine", 20f, 30)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems("para", pageSize: 20);

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void GetItems_SearchSkipsItemsWithNullName_ReturnsCorrectItemName()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Paracetamol", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, null!, "Pharma", "Medicine", 20f, 30)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems("para", pageSize: 20);

            Assert.AreEqual("Paracetamol", result[0].Name);
        }


        // ════════════════════════════════════════════════════════════════════════════
        // Tiberia PART: F4.3 (Product Filtering) & F4.4 (Product Sorting)
        // ════════════════════════════════════════════════════════════════════════════

        // ── Fix Line 75: Price Range Bounds ──

        [Test]
        public void GetItems_PriceRangeMinIsNegative_ThrowsArgumentException()
        {
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(new List<Item>());
            Action act = () => productCatalogueService.GetItems(null, priceRanges: new List<(float, float)> { (-10f, 50f) });
            Assert.That(act, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GetItems_PriceRangeMaxIsNegative_ThrowsArgumentException()
        {
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(new List<Item>());
            Action act = () => productCatalogueService.GetItems(null, priceRanges: new List<(float, float)> { (10f, -50f) });
            Assert.That(act, Throws.TypeOf<ArgumentException>());
        }

        // ── Fix Line 85: Price Filtering Exact Boundaries ──

        [Test]
        public void GetItems_PriceExactlyOnMinBoundary_ReturnsItem()
        {
            var items = new List<Item> { CreateItem(1, "ExactMin", "Bayer", "Med", 10f, 50) };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, priceRanges: new List<(float, float)> { (10f, 20f) });
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetItems_PriceExactlyOnMaxBoundary_ReturnsItem()
        {
            var items = new List<Item> { CreateItem(1, "ExactMax", "Bayer", "Med", 20f, 50) };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, priceRanges: new List<(float, float)> { (10f, 20f) });
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetItems_PriceOutsideBoundary_ReturnsEmpty()
        {
            var items = new List<Item> { CreateItem(1, "TooExpensive", "Bayer", "Med", 100f, 50) };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, priceRanges: new List<(float, float)> { (10f, 20f) });
            Assert.That(result.Count, Is.EqualTo(0));
        }

        // ── Fix Line 96: Low Stock Math ──

        [Test]
        public void GetItems_StockIsZero_LowStockFilterExcludesIt()
        {
            // Quantity > 0 condition check
            var items = new List<Item> { CreateItem(1, "EmptyStock", "Bayer", "Med", 10f, 0) };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, stockFilter: ProductCatalogueService.StockFilterLowStock);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public void GetItems_StockExactlyAtThreshold_LowStockFilterExcludesIt()
        {
            // Quantity < LowStockThreshold condition check (should exclude if exactly 10)
            var items = new List<Item> { CreateItem(1, "ThresholdStock", "Bayer", "Med", 10f, ProductCatalogueService.LowStockThreshold) };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, stockFilter: ProductCatalogueService.StockFilterLowStock);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        // ── F4.3 Filtering: Stock ──

        [Test]
        public void GetItems_StockFilterInStock_ReturnsCorrectCount()
        {
            var items = new List<Item>
            {
                CreateItem(1, "InStock", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "OutOfStock", "Pharma", "Medicine", 20f, 0)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, stockFilter: ProductCatalogueService.StockFilterInStock);

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void GetItems_StockFilterInStock_ExcludesOutOfStockItems()
        {
            var items = new List<Item>
            {
                CreateItem(1, "InStock", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "OutOfStock", "Pharma", "Medicine", 20f, 0)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, stockFilter: ProductCatalogueService.StockFilterInStock);

            Assert.AreEqual("InStock", result[0].Name);
        }

        [Test]
        public void GetItems_StockFilterLowStock_ReturnsCorrectCount()
        {
            var items = new List<Item>
            {
                CreateItem(1, "HighStock", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "LowStock", "Generic", "Medicine", 15f, 5)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, stockFilter: ProductCatalogueService.StockFilterLowStock);

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void GetItems_InvalidStockFilter_ReturnsOriginalCount()
        {
            var items = new List<Item>
            {
                CreateItem(1, "InStock", "Bayer", "Medicine", 10f, 5),
                CreateItem(2, "OutOfStock", "Pharma", "Medicine", 20f, 0)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, stockFilter: "invalid_filter_string");

            Assert.AreEqual(2, result.Count);
        }

        // ── F4.3 Filtering: Discount ──

        [Test]
        public void GetItems_DiscountedTrue_ReturnsCorrectCount()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Discounted", "Bayer", "Medicine", 10f, 50, discount: 0.2f),
                CreateItem(2, "NotDiscounted", "Pharma", "Medicine", 20f, 30, discount: 0f)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, discounted: true);

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void GetItems_DiscountedFalse_ReturnsNonDiscountedItemName()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Discounted", "Bayer", "Medicine", 10f, 50, discount: 0.2f),
                CreateItem(2, "NotDiscounted", "Pharma", "Medicine", 20f, 30, discount: 0f)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, discounted: false);

            Assert.AreEqual("NotDiscounted", result[0].Name);
        }

        // ── F4.3 Filtering: Price Range & Categories ──

        [Test]
        public void GetItems_PriceRangeFilter_ReturnsCorrectCount()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Cheap", "Bayer", "Medicine", 25f, 50),
                CreateItem(2, "Expensive", "Pharma", "Medicine", 150f, 30)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, priceRanges: new List<(float, float)> { (0f, 49.99f) });

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void GetItems_InvalidPriceRange_ThrowsArgumentException()
        {
            var items = new List<Item> { CreateItem(1, "Item1", "Bayer", "Medicine", 10f, 50) };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            Action act = () => productCatalogueService.GetItems(null, priceRanges: new List<(float, float)> { (100f, 50f) });

            Assert.That(act, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GetItems_CategoryFilter_ReturnsCorrectCount()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Medicine1", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "Supplement1", "Pharma", "Supplements", 20f, 30)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, categories: new List<string> { "Supplements" });

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void GetItems_SubstanceFilter_ReturnsCorrectCount()
        {
            var items = new List<Item>
            {
                CreateItem(1, "WithSubstance", "Bayer", "Medicine", 10f, 50, activeSubstances: new Dictionary<string, float> { { "Aspirin", 0.5f } }),
                CreateItem(2, "WithoutSubstance", "Pharma", "Medicine", 20f, 30)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, substances: new List<string> { "Aspirin" });

            Assert.AreEqual(1, result.Count);
        }

        // ── F4.4 Sorting ──

        [Test]
        public void GetItems_SortByPriceAscending_FirstItemIsCheapest()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Expensive", "Bayer", "Medicine", 100f, 50),
                CreateItem(2, "Cheap", "Pharma", "Medicine", 5f, 30),
                CreateItem(3, "Medium", "Generic", "Medicine", 50f, 25)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, sortBy: ProductCatalogueService.SortByPrice, ascending: true);

            Assert.AreEqual("Cheap", result[0].Name);
        }

        [Test]
        public void GetItems_SortByPriceAscending_LastItemIsMostExpensive()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Expensive", "Bayer", "Medicine", 100f, 50),
                CreateItem(2, "Cheap", "Pharma", "Medicine", 5f, 30),
                CreateItem(3, "Medium", "Generic", "Medicine", 50f, 25)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, sortBy: ProductCatalogueService.SortByPrice, ascending: true);

            Assert.AreEqual("Expensive", result[2].Name);
        }

        [Test]
        public void GetItems_SortByPriceDescending_FirstItemIsMostExpensive()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Expensive", "Bayer", "Medicine", 100f, 50),
                CreateItem(2, "Cheap", "Pharma", "Medicine", 5f, 30),
                CreateItem(3, "Medium", "Generic", "Medicine", 50f, 25)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, sortBy: ProductCatalogueService.SortByPrice, ascending: false);

            Assert.AreEqual("Expensive", result[0].Name);
        }

        [Test]
        public void GetItems_SortByNewestAscending_FirstItemHasOldestOrNoFutureBatch()
        {
            DateOnly nearFuture = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
            DateOnly farFuture = DateOnly.FromDateTime(DateTime.Today.AddDays(20));
            DateOnly expiredDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-2));

            var items = new List<Item>
            {
                CreateItem(1, "NoFutureBatch", "Bayer", "Medicine", 10f, 10, batches: new Dictionary<DateOnly, int> { { expiredDate, 10 } }),
                CreateItem(2, "NearFuture", "Pharma", "Medicine", 10f, 10, batches: new Dictionary<DateOnly, int> { { nearFuture, 10 } }),
                CreateItem(3, "FarFuture", "Generic", "Medicine", 10f, 10, batches: new Dictionary<DateOnly, int> { { farFuture, 10 } })
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, sortBy: ProductCatalogueService.SortByNewest, ascending: true, pageSize: 20);

            Assert.AreEqual("NoFutureBatch", result[0].Name);
        }

        [Test]
        public void GetItems_SortByNewestDescending_FirstItemHasFurthestFutureBatch()
        {
            DateOnly nearFuture = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
            DateOnly farFuture = DateOnly.FromDateTime(DateTime.Today.AddDays(20));
            DateOnly expiredDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-2));

            var items = new List<Item>
            {
                CreateItem(1, "NoFutureBatch", "Bayer", "Medicine", 10f, 10, batches: new Dictionary<DateOnly, int> { { expiredDate, 10 } }),
                CreateItem(2, "NearFuture", "Pharma", "Medicine", 10f, 10, batches: new Dictionary<DateOnly, int> { { nearFuture, 10 } }),
                CreateItem(3, "FarFuture", "Generic", "Medicine", 10f, 10, batches: new Dictionary<DateOnly, int> { { farFuture, 10 } })
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, sortBy: ProductCatalogueService.SortByNewest, ascending: false, pageSize: 20);

            Assert.AreEqual("FarFuture", result[0].Name);
        }

        [Test]
        public void GetItems_InvalidSortBy_ReturnsFirstItemInOriginalOrder()
        {
            var items = new List<Item>
            {
                CreateItem(1, "First", "Bayer", "Medicine", 100f, 5),
                CreateItem(2, "Second", "Pharma", "Medicine", 5f, 5)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, sortBy: "invalid_sort_string");

            Assert.AreEqual("First", result[0].Name);
        }

        [Test]
        public void GetItems_NullSortBy_ReturnsItemsInOriginalOrder()
        {
            var items = new List<Item>
            {
                CreateItem(1, "First", "Bayer", "Medicine", 100f, 50),
                CreateItem(2, "Second", "Pharma", "Medicine", 5f, 30)
            };
            mockItemsRepository.Setup(r => r.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, sortBy: null);

            Assert.AreEqual("First", result[0].Name);
        }
    }
}