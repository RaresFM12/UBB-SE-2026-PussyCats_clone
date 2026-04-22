using PharmacyApp.Common.Repositories;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Period_Tracker.Logic;

namespace PharmacyApp.Tests.Unit.Features.PeriodTracker.Logic
{
    [TestFixture]
    public class PeriodTrackerServiceFactoryTests
    {
        [Test]
        public void CreatePeriodTrackerService_WhenCalled_ReturnsPeriodTrackerServiceInstance()
        {
            PeriodTrackerServiceFactory factory = CreateFactory();

            IPeriodTrackerService result = factory.CreatePeriodTrackerService();

            Assert.That(result, Is.TypeOf<PeriodTrackerService>());
        }

        [Test]
        public void CreateWellnessItemsService_WhenCalled_ReturnsWellnessItemsServiceInstance()
        {
            PeriodTrackerServiceFactory factory = CreateFactory();

            IWellnessItemsService result = factory.CreateWellnessItemsService();

            Assert.That(result, Is.TypeOf<WellnessItemsService>());
        }

        [Test]
        public void CreateBasketService_WhenCalled_ReturnsBasketServiceInstance()
        {
            PeriodTrackerServiceFactory factory = CreateFactory();

            IBasketService result = factory.CreateBasketService();

            Assert.That(result, Is.TypeOf<BasketService>());
        }

        private static PeriodTrackerServiceFactory CreateFactory()
        {
            return new PeriodTrackerServiceFactory(
                new Mock<IUsersRepository>().Object,
                new Mock<IItemsRepository>().Object,
                new Mock<ICurrentUserService>().Object,
                new Mock<IOrderService>().Object);
        }
    }
}
