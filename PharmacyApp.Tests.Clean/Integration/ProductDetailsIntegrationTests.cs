using NUnit.Framework;
using PharmacyApp.Features.Products_Catalogue.ViewModels;
using PharmacyApp.Features.Orders.Logic;
using PharmacyApp.Models;
using PharmacyApp.Common.Repositories;

namespace PharmacyApp.Tests.Integration.Features.ProductCatalogue
{
    [TestFixture]
    public class ProductDetailsIntegrationTests
    {
        private ProductDetailsPageViewModel _viewModel;
        private OrderService _realOrderService;
        private User _testUser;
        private Item _testItem;

        [SetUp]
        public void Setup()
        {
            _testUser = new User(id: 1, email: "integration@pharmacy.com", phoneNumber: "0700000000",
                passwordHash: "hash!", isAdmin: false, isDisabled: false,
                userName: "testuser", discountNotifications: false, loyaltyPoints: 0);

            _testItem = new Item(id: 1, name: "Integration Test Med", producer: "Prod", category: "Cat",
                                 price: 50.0f, numberOfPills: 30, quantity: 20);

            _realOrderService = new OrderService(
                new SQLSubstancesRepository(),
                new SQLItemsRepository(),
                new SQLUsersRepository(),
                new SQLOrdersRepository(),
                _testUser
            );

            _viewModel = new ProductDetailsPageViewModel();
            _viewModel.Initialize(_testItem, _testUser, _realOrderService);
        }

        [Test]
        public void TryAddToBasket_RealOrderService_ReturnsSuccessTrue()
        {
            var result = _viewModel.TryAddToBasket("2");

            Assert.That(result.success, Is.True);
        }

        [Test]
        public void TryAddToBasket_RealOrderService_SuccessfullyModifiesUserBasket()
        {
            _viewModel.TryAddToBasket("2");

            Assert.That(_testUser.Basket.ContainsKey(_testItem.Id), Is.True);
        }

        [Test]
        public void TryAddToBasket_RealOrderService_UpdatesQuantityIfAlreadyInBasket()
        {
            _testUser.AddItemToBasket(_testItem.Id, 1);

            var result = _viewModel.TryAddToBasket("2");

            Assert.That(result.success, Is.True);
            Assert.That(_testUser.Basket[_testItem.Id].Quantity, Is.EqualTo(3));
        }
    }
}