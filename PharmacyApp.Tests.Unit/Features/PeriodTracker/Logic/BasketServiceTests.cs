using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Period_Tracker.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyApp.Tests.Unit.Features.PeriodTracker.Logic
{
    [TestFixture]
    public class BasketServiceTests
    {
        private Mock<IOrderService> orderServiceMock = null!;
        private BasketService basketService = null!;

        [SetUp]
        public void SetUp()
        {
            orderServiceMock = new Mock<IOrderService>();
            basketService = new BasketService(orderServiceMock.Object);
        }

        [Test]
        public void AddToBasket_WhenCalled_ForwardsParametersToOrderService()
        {
            basketService.AddToBasket(7, 3, 15f);

            orderServiceMock.Verify(
                service => service.AddToBasket(7, 3, 15f),
                Times.Once);
        }

        [Test]
        public void AddToBasket_WhenExtraDiscountIsOmitted_ForwardsDefaultValue()
        {
            basketService.AddToBasket(4, 1);

            orderServiceMock.Verify(
                service => service.AddToBasket(4, 1, 0f),
                Times.Once);
        }
    }
}
