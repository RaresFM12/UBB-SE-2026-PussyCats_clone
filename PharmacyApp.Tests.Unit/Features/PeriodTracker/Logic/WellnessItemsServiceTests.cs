using PharmacyApp.Common.Repositories;
using PharmacyApp.Features.Period_Tracker.Logic;
using PharmacyApp.Models;


namespace PharmacyApp.Tests.Unit.Features.PeriodTracker.Logic
{
    [TestFixture]
    public class WellnessItemsServiceTests
    {
        private Mock<IItemsRepository> itemsRepositoryMock = null!;
        private WellnessItemsService service = null!;

        [SetUp]
        public void SetUp()
        {
            itemsRepositoryMock = new Mock<IItemsRepository>();
            service = new WellnessItemsService(itemsRepositoryMock.Object);
        }

        [Test]
        public void GetWellnessItems_WhenNoItemsExist_ReturnsEmptyList()
        {
            itemsRepositoryMock.Setup(repository => repository.GetAllItems()).Returns(new List<Item>());

            List<Item> result = service.GetWellnessItems();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void GetWellnessItems_WhenItemsHaveDifferentCategories_ReturnsOnlyWellnessItems()
        {
            List<Item> items = new List<Item>
            {
                CreateItem(3, "Painkiller", "medicine"),
                CreateItem(1, "Tea", "wellness"),
                CreateItem(2, "Cream", "Wellness"),
                CreateItem(4, "Mask", null!)
            };

            itemsRepositoryMock.Setup(repository => repository.GetAllItems()).Returns(items);

            List<Item> result = service.GetWellnessItems();

            Assert.That(result.Select(item => item.Id).ToList(), Is.EqualTo(new List<int> { 1, 2 }));
        }

        [Test]
        public void GetWellnessItems_WhenCategoryCaseDiffers_MatchesIgnoringCase()
        {
            List<Item> items = new List<Item>
            {
                CreateItem(10, "Oil", "WELLNESS")
            };

            itemsRepositoryMock.Setup(repository => repository.GetAllItems()).Returns(items);

            List<Item> result = service.GetWellnessItems();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(10));
        }

        [Test]
        public void GetWellnessItems_WhenWellnessItemsExist_ReturnsItemsOrderedById()
        {
            List<Item> items = new List<Item>
            {
                CreateItem(9, "Item 9", "wellness"),
                CreateItem(2, "Item 2", "wellness"),
                CreateItem(5, "Item 5", "wellness")
            };

            itemsRepositoryMock.Setup(repository => repository.GetAllItems()).Returns(items);

            List<Item> result = service.GetWellnessItems();

            Assert.That(result.Select(item => item.Id).ToList(), Is.EqualTo(new List<int> { 2, 5, 9 }));
        }

        private static Item CreateItem(int id, string name, string category)
        {
            return new Item(id, name, "producer", category, 10f, 1, label: "", description: "", imagePath: "..\\..\\Assets\\placeholder.png", discount: 0f);
        }
    }
}
