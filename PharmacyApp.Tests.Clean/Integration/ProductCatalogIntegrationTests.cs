using NUnit.Framework;
using PharmacyApp.Features.Products_Catalogue.Service;
using PharmacyApp.Common.Repositories;

namespace PharmacyApp.Tests.Integration.Features.ProductCatalogue
{
    [TestFixture]
    public class ProductCatalogueIntegrationTests
    {
        private ProductCatalogueService service;
        private IItemsRepository realRepository;

        [SetUp]
        public void Setup()
        {
            realRepository = new SQLItemsRepository();
            service = new ProductCatalogueService(realRepository);
        }

        [Test]
        public void GetItems_RealRepository_RetrievesDataWithoutCrashing()
        {
            var results = service.GetItems(search: null);

            Assert.That(results, Is.Not.Null);
        }

        [Test]
        public void GetItems_RealRepositoryPagination_LimitsResultsToPageSize()
        {
            var results = service.GetItems(search: null, page: 0, pageSize: 5);

            Assert.That(results.Count, Is.LessThanOrEqualTo(5));
        }
    }
}