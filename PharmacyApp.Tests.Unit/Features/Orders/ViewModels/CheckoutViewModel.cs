using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Features.Orders.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyApp.Tests.Unit.Features.Orders.ViewModels
{
    [TestFixture]
    public class CheckoutViewModelTests
    {
        private Mock<IOrderService> mockOrderService;

        [SetUp]
        public void Setup()
        {
            mockOrderService = new Mock<IOrderService>();
        }

        private static BasketItemViewModel MakeBasketItem(int id, float priceBefore, float priceAfter)
        {
            var item = new BasketItemViewModel(id, "img", "Name", "Prod", 1, 0f, 0f, 0f, priceBefore);
            item.SetFinalPrices(priceBefore, priceAfter);
            return item;
        }

        [Test]
        public void Constructor_SetsBasketItemsFromService()
        {
            var items = new List<BasketItemViewModel> { MakeBasketItem(1, 10f, 8f) };
            mockOrderService.Setup(s => s.GetBasketItems()).Returns(items);
            mockOrderService
                .Setup(s => s.CalculateBasketTotalSum(It.IsAny<IEnumerable<BasketItemViewModel>>()))
                .Returns(new Tuple<float, float>(10f, 8f));

            var vm = new CheckoutViewModel(mockOrderService.Object);

            Assert.AreEqual(1, vm.BasketItems.Count);
        }

        [Test]
        public void Constructor_SetsTotalPriceStringToDiscountedTotal()
        {
            mockOrderService.Setup(s => s.GetBasketItems()).Returns(new List<BasketItemViewModel>());
            mockOrderService
                .Setup(s => s.CalculateBasketTotalSum(It.IsAny<IEnumerable<BasketItemViewModel>>()))
                .Returns(new Tuple<float, float>(50f, 37.5f));

            var vm = new CheckoutViewModel(mockOrderService.Object);

            Assert.AreEqual("37.50 RON", vm.TotalPriceString);
        }

        [Test]
        public void Constructor_EmptyBasket_SetsTotalPriceStringToZero()
        {
            mockOrderService.Setup(s => s.GetBasketItems()).Returns(new List<BasketItemViewModel>());
            mockOrderService
                .Setup(s => s.CalculateBasketTotalSum(It.IsAny<IEnumerable<BasketItemViewModel>>()))
                .Returns(new Tuple<float, float>(0f, 0f));

            var vm = new CheckoutViewModel(mockOrderService.Object);

            Assert.AreEqual("0.00 RON", vm.TotalPriceString);
        }
    }
}
