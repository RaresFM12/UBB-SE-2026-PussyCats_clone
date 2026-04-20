using PharmacyApp.Common.Repositories;
using PharmacyApp.Features.Products_Catalogue.Service;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        [SetUp]
        public void Setup()
        {
            mockItemsRepository = new Mock<IItemsRepository>();
            productCatalogueService = new ProductCatalogueService(mockItemsRepository.Object);
        }

        // F4.1 - Product Listing Tests

        [Test]
        public void GetItems_NullSearch_ReturnsAllItems()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Paracetamol", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "Vitamin C", "Pharma", "Supplements", 20f, 30)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null);

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void GetItems_EmptySearch_ReturnsAllItems()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Paracetamol", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "Vitamin C", "Pharma", "Supplements", 20f, 30)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems("");

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void GetItems_WithPagination_ReturnsCorrectPage()
        {
            var items = new List<Item>();
            for (int i = 1; i <= 25; i++)
            {
                items.Add(CreateItem(i, $"Item{i}", "Producer", "Medicine", 10f, 50));
            }
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var firstPage = productCatalogueService.GetItems(null, page: 0, pageSize: 10);
            var secondPage = productCatalogueService.GetItems(null, page: 1, pageSize: 10);
            var thirdPage = productCatalogueService.GetItems(null, page: 2, pageSize: 10);

            Assert.AreEqual(10, firstPage.Count);
            Assert.AreEqual(10, secondPage.Count);
            Assert.AreEqual(5, thirdPage.Count);
        }

        [Test]
        public void GetItems_DefaultPageSize_ReturnsTenItems()
        {
            var items = new List<Item>();
            for (int i = 1; i <= 15; i++)
            {
                items.Add(CreateItem(i, $"Item{i}", "Producer", "Medicine", 10f, 50));
            }
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null);

            Assert.AreEqual(ProductCatalogueService.DefaultPageSize, result.Count);
        }

        [Test]
        public void GetItems_StockFilterInStock_ReturnsOnlyInStockItems()
        {
            var items = new List<Item>
            {
                CreateItem(1, "InStock", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "OutOfStock", "Pharma", "Medicine", 20f, 0),
                CreateItem(3, "LowStock", "Generic", "Medicine", 15f, 5)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, stockFilter: ProductCatalogueService.StockFilterInStock);

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(item => item.Quantity > 0));
        }

        [Test]
        public void GetItems_StockFilterLowStock_ReturnsOnlyLowStockItems()
        {
            var items = new List<Item>
            {
                CreateItem(1, "InStock", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "OutOfStock", "Pharma", "Medicine", 20f, 0),
                CreateItem(3, "LowStock", "Generic", "Medicine", 15f, 5)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, stockFilter: ProductCatalogueService.StockFilterLowStock);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("LowStock", result[0].Name);
        }

        [Test]
        public void GetItems_DiscountedTrue_ReturnsOnlyDiscountedItems()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Discounted", "Bayer", "Medicine", 10f, 50, discount: 0.2f),
                CreateItem(2, "NotDiscounted", "Pharma", "Medicine", 20f, 30, discount: 0f)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, discounted: true);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Discounted", result[0].Name);
        }

        [Test]
        public void GetItems_DiscountedFalse_ReturnsOnlyNonDiscountedItems()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Discounted", "Bayer", "Medicine", 10f, 50, discount: 0.2f),
                CreateItem(2, "NotDiscounted", "Pharma", "Medicine", 20f, 30, discount: 0f)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, discounted: false);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("NotDiscounted", result[0].Name);
        }

        [Test]
        public void GetItems_CategoryFilter_ReturnsMatchingCategories()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Medicine1", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "Supplement1", "Pharma", "Supplements", 20f, 30),
                CreateItem(3, "Medicine2", "Generic", "Medicine", 15f, 25)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, categories: new List<string> { "Medicine" });

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(item => item.Category == "Medicine"));
        }

        [Test]
        public void GetItems_PriceRangeFilter_ReturnsItemsInRange()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Cheap", "Bayer", "Medicine", 5f, 50),
                CreateItem(2, "Medium", "Pharma", "Medicine", 50f, 30),
                CreateItem(3, "Expensive", "Generic", "Medicine", 150f, 25)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, priceRanges: new List<(float, float)> { (0, 49) });

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Cheap", result[0].Name);
        }

        [Test]
        public void GetItems_InvalidPriceRange_ThrowsArgumentException()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Item1", "Bayer", "Medicine", 10f, 50)
            };

            mockItemsRepository
                .Setup(repository => repository.GetAllItems())
                .Returns(items);

            Action act = () => productCatalogueService.GetItems(
                null,
                priceRanges: new List<(float, float)> { (100f, 50f) });

            Assert.That(act, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GetItems_SortByPriceAscending_ReturnsSortedItems()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Expensive", "Bayer", "Medicine", 100f, 50),
                CreateItem(2, "Cheap", "Pharma", "Medicine", 5f, 30),
                CreateItem(3, "Medium", "Generic", "Medicine", 50f, 25)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, sortBy: ProductCatalogueService.SortByPrice, ascending: true);

            Assert.AreEqual("Cheap", result[0].Name);
            Assert.AreEqual("Medium", result[1].Name);
            Assert.AreEqual("Expensive", result[2].Name);
        }

        [Test]
        public void GetItems_SortByPriceDescending_ReturnsSortedItems()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Expensive", "Bayer", "Medicine", 100f, 50),
                CreateItem(2, "Cheap", "Pharma", "Medicine", 5f, 30),
                CreateItem(3, "Medium", "Generic", "Medicine", 50f, 25)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, sortBy: ProductCatalogueService.SortByPrice, ascending: false);

            Assert.AreEqual("Expensive", result[0].Name);
            Assert.AreEqual("Medium", result[1].Name);
            Assert.AreEqual("Cheap", result[2].Name);
        }

        [Test]
        public void GetItems_SubstanceFilter_ReturnsItemsContainingSubstance()
        {
            var items = new List<Item>
            {
                CreateItem(1, "WithSubstance", "Bayer", "Medicine", 10f, 50,
                    activeSubstances: new Dictionary<string, float> { { "Aspirin", 0.5f } }),
                CreateItem(2, "WithoutSubstance", "Pharma", "Medicine", 20f, 30)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, substances: new List<string> { "Aspirin" });

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("WithSubstance", result[0].Name);
        }

        [Test]
        public void GetItems_NullCategoryFilter_ReturnsAllItems()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Item1", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "Item2", "Pharma", "Supplements", 20f, 30)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, categories: null);

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void GetItems_NullStockFilter_ReturnsAllItems()
        {
            var items = new List<Item>
            {
                CreateItem(1, "InStock", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "OutOfStock", "Pharma", "Medicine", 20f, 0)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, stockFilter: null);

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void GetItems_NullDiscountFilter_ReturnsAllItems()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Discounted", "Bayer", "Medicine", 10f, 50, discount: 0.2f),
                CreateItem(2, "Full", "Pharma", "Medicine", 20f, 30, discount: 0f)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, discounted: null);

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void GetItems_NullSortBy_ReturnsItemsInOriginalOrder()
        {
            var items = new List<Item>
            {
                CreateItem(1, "First", "Bayer", "Medicine", 100f, 50),
                CreateItem(2, "Second", "Pharma", "Medicine", 5f, 30)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, sortBy: null);

            Assert.AreEqual("First", result[0].Name);
            Assert.AreEqual("Second", result[1].Name);
        }

        [Test]
        public void GetItems_EmptyRepository_ReturnsEmptyList()
        {
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(new List<Item>());

            var result = productCatalogueService.GetItems(null);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetItems_PageBeyondItems_ReturnsEmptyList()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Item1", "Bayer", "Medicine", 10f, 50)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, page: 5, pageSize: 10);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetItems_MultiplePriceRanges_ReturnsItemsInAnyRange()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Cheap", "Bayer", "Medicine", 5f, 50),
                CreateItem(2, "Medium", "Pharma", "Medicine", 75f, 30),
                CreateItem(3, "Expensive", "Generic", "Medicine", 150f, 25)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null,
                priceRanges: new List<(float, float)> { (0, 49), (100, 200) });

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Any(item => item.Name == "Cheap"));
            Assert.IsTrue(result.Any(item => item.Name == "Expensive"));
        }

        [Test]
        public void GetItems_DiscountedItemPriceFilter_UsesDiscountedPrice()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Discounted", "Bayer", "Medicine", 100f, 50, discount: 0.5f)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null,
                priceRanges: new List<(float, float)> { (40, 60) });

            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void GetItems_SortByNewestAscending_ReturnsSortedByDate()
        {
            var futureDate1 = DateOnly.FromDateTime(DateTime.Now.AddDays(10));
            var futureDate2 = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            var items = new List<Item>
            {
                CreateItem(1, "NewerItem", "Bayer", "Medicine", 10f, 50,
                    batches: new Dictionary<DateOnly, int> { { futureDate2, 20 } }),
                CreateItem(2, "OlderItem", "Pharma", "Medicine", 20f, 30,
                    batches: new Dictionary<DateOnly, int> { { futureDate1, 10 } })
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, sortBy: ProductCatalogueService.SortByNewest, ascending: true);

            Assert.AreEqual("OlderItem", result[0].Name);
            Assert.AreEqual("NewerItem", result[1].Name);
        }


        [Test]
        public void GetItems_SearchByName_ReturnsMatchingItems()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Paracetamol", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "Ibuprofen", "Pharma", "Medicine", 20f, 30),
                CreateItem(3, "Paracetamol Extra", "Generic", "Medicine", 15f, 25)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems("Paracetamol");

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(item => item.Name.Contains("Paracetamol")));
        }

        [Test]
        public void GetItems_SearchCaseInsensitive_ReturnsMatchingItems()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Paracetamol", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "Ibuprofen", "Pharma", "Medicine", 20f, 30)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems("paracetamol");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Paracetamol", result[0].Name);
        }

        [Test]
        public void GetItems_SearchPartialMatch_ReturnsMatchingItems()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Paracetamol", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "Ibuprofen", "Pharma", "Medicine", 20f, 30)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems("para");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Paracetamol", result[0].Name);
        }

        [Test]
        public void GetItems_SearchNoMatch_ReturnsEmptyList()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Paracetamol", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "Ibuprofen", "Pharma", "Medicine", 20f, 30)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems("Aspirin");

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetItems_SearchWithNullItemName_DoesNotThrow()
        {
            var itemWithNullName = CreateItem(1, null, "Bayer", "Medicine", 10f, 50);
            var items = new List<Item> { itemWithNullName };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems("test");

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void GetItems_SearchWithFilters_AppliesBothSearchAndFilters()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Paracetamol", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "Paracetamol Plus", "Pharma", "Supplements", 20f, 30),
                CreateItem(3, "Ibuprofen", "Generic", "Medicine", 15f, 25)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems("Paracetamol", categories: new List<string> { "Medicine" });

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Paracetamol", result[0].Name);
        }

        [Test]
        public void GetItems_MultipleFiltersApplied_ReturnsCorrectSubset()
        {
            var items = new List<Item>
            {
                CreateItem(1, "DiscountedMedicine", "Bayer", "Medicine", 10f, 5, discount: 0.1f),
                CreateItem(2, "FullPriceMedicine", "Pharma", "Medicine", 20f, 50, discount: 0f),
                CreateItem(3, "DiscountedSupplement", "Generic", "Supplements", 15f, 25, discount: 0.2f)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null,
                categories: new List<string> { "Medicine" },
                discounted: true,
                stockFilter: ProductCatalogueService.StockFilterLowStock);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("DiscountedMedicine", result[0].Name);
        }

        [Test]
        public void GetItems_OutOfStockItem_StockFilterInStockExcludesIt()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Available", "Bayer", "Medicine", 10f, 1),
                CreateItem(2, "Unavailable", "Pharma", "Medicine", 20f, 0)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, stockFilter: ProductCatalogueService.StockFilterInStock);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Available", result[0].Name);
        }

        [Test]
        public void GetItems_LowStockThreshold_ItemWithExactlyTenNotLowStock()
        {
            var items = new List<Item>
            {
                CreateItem(1, "ExactThreshold", "Bayer", "Medicine", 10f, ProductCatalogueService.LowStockThreshold),
                CreateItem(2, "BelowThreshold", "Pharma", "Medicine", 20f, ProductCatalogueService.LowStockThreshold - 1)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, stockFilter: ProductCatalogueService.StockFilterLowStock);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("BelowThreshold", result[0].Name);
        }

        [Test]
        public void GetItems_NegativePriceRange_ThrowsArgumentException()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Item1", "Bayer", "Medicine", 10f, 50)
            };

            mockItemsRepository
                .Setup(repository => repository.GetAllItems())
                .Returns(items);

            Action act = () => productCatalogueService.GetItems(
                null,
                priceRanges: new List<(float, float)> { (-10f, 50f) });

            Assert.That(act, Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GetItems_EmptyCategories_ReturnsAllItems()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Item1", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "Item2", "Pharma", "Supplements", 20f, 30)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, categories: new List<string>());

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void GetItems_EmptySubstances_ReturnsAllItems()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Item1", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "Item2", "Pharma", "Medicine", 20f, 30)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, substances: new List<string>());

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void GetItems_UnknownStockFilter_ReturnsAllItems()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Item1", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "Item2", "Pharma", "Medicine", 20f, 0)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, stockFilter: "unknown_filter");

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void GetItems_WhitespaceSearch_ReturnsAllItems()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Item1", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, "Item2", "Pharma", "Medicine", 20f, 30)
            };
            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems("   ");

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void GetItems_SearchSkipsItemsWithNullName()
        {
            var items = new List<Item>
            {
                CreateItem(1, "Paracetamol", "Bayer", "Medicine", 10f, 50),
                CreateItem(2, null!, "Pharma", "Medicine", 20f, 30)
            };

            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems("para", pageSize: 20);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("Paracetamol", result[0].Name);
        }

        [Test]
        public void GetItems_InvalidStockFilter_ReturnsUnchangedItems()
        {
            var items = new List<Item>
            {
                CreateItem(1, "InStock", "Bayer", "Medicine", 10f, 5),
                CreateItem(2, "OutOfStock", "Pharma", "Medicine", 20f, 0)
            };

            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, stockFilter: "unknown_filter", pageSize: 20);

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void GetItems_InvalidSortBy_ReturnsItemsInOriginalOrder()
        {
            var items = new List<Item>
            {
                CreateItem(1, "First", "Bayer", "Medicine", 100f, 5),
                CreateItem(2, "Second", "Pharma", "Medicine", 5f, 5)
            };

            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null, sortBy: "unknown_sort", pageSize: 20);

            Assert.AreEqual("First", result[0].Name);
            Assert.AreEqual("Second", result[1].Name);
        }

        [Test]
        public void GetItems_SortByNewestAscending_ReturnsItemsOrderedByLatestValidFutureBatchDate()
        {
            DateOnly nearFuture = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
            DateOnly farFuture = DateOnly.FromDateTime(DateTime.Today.AddDays(20));
            DateOnly expiredDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-2));

            var items = new List<Item>
            {
                CreateItem(1, "NoFutureBatch", "Bayer", "Medicine", 10f, 10,
                    batches: new Dictionary<DateOnly, int> { { expiredDate, 10 } }),
                CreateItem(2, "NearFuture", "Pharma", "Medicine", 10f, 10,
                    batches: new Dictionary<DateOnly, int> { { nearFuture, 10 } }),
                CreateItem(3, "FarFuture", "Generic", "Medicine", 10f, 10,
                    batches: new Dictionary<DateOnly, int> { { farFuture, 10 } })
            };

            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null,
                sortBy: ProductCatalogueService.SortByNewest,
                ascending: true,
                pageSize: 20);

            Assert.AreEqual("NoFutureBatch", result[0].Name);
            Assert.AreEqual("NearFuture", result[1].Name);
            Assert.AreEqual("FarFuture", result[2].Name);
        }

        [Test]
        public void GetItems_SortByNewestDescending_ReturnsItemsOrderedByLatestValidFutureBatchDate()
        {
            DateOnly nearFuture = DateOnly.FromDateTime(DateTime.Today.AddDays(5));
            DateOnly farFuture = DateOnly.FromDateTime(DateTime.Today.AddDays(20));
            DateOnly expiredDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-2));

            var items = new List<Item>
            {
                CreateItem(1, "NoFutureBatch", "Bayer", "Medicine", 10f, 10,
                    batches: new Dictionary<DateOnly, int> { { expiredDate, 10 } }),
                CreateItem(2, "NearFuture", "Pharma", "Medicine", 10f, 10,
                    batches: new Dictionary<DateOnly, int> { { nearFuture, 10 } }),
                CreateItem(3, "FarFuture", "Generic", "Medicine", 10f, 10,
                    batches: new Dictionary<DateOnly, int> { { farFuture, 10 } })
            };

            mockItemsRepository.Setup(repository => repository.GetAllItems()).Returns(items);

            var result = productCatalogueService.GetItems(null,
                sortBy: ProductCatalogueService.SortByNewest,
                ascending: false,
                pageSize: 20);

            Assert.AreEqual("FarFuture", result[0].Name);
            Assert.AreEqual("NearFuture", result[1].Name);
            Assert.AreEqual("NoFutureBatch", result[2].Name);
        }

        [Test]
        public void FilterByProducer_WhenProducerFilterIsNull_ReturnsOriginalItems()
        {
            var method = typeof(ProductCatalogueService)
                .GetMethod("FilterByProducer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var items = new List<Item>
            {
                CreateItem(1, "Item1", "Bayer", "Medicine", 10f, 10),
                CreateItem(2, "Item2", "Pharma", "Medicine", 10f, 10)
            };

            var result = (List<Item>)method!.Invoke(
                productCatalogueService,
                new object[] { items, null! })!;

            Assert.AreEqual(2, result.Count);
        }
    }
}
