using NUnit.Framework;
using PharmacyApp.Common.Repositories;

namespace PharmacyApp.Tests.Integration.Repositories
{
    [TestFixture]
    public class SQLItemsRepositoryTests
    {
        private const int ExistingItemIdentifierInDatabase = 1;

        private IItemsRepository sqlItemsRepository;

        [SetUp]
        public void Setup()
        {
            sqlItemsRepository = new SQLItemsRepository();
        }

        [Test]
        public void GetItem_ValidIdentifier_ReturnsItemFromRealDatabase()
        {
            var retrievedItem = sqlItemsRepository.GetItem(ExistingItemIdentifierInDatabase);

            Assert.IsNotNull(retrievedItem);
            Assert.AreEqual(ExistingItemIdentifierInDatabase, retrievedItem.Id);
        }
    }
}