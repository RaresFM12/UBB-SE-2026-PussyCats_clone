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
        private ProductDetailsPageViewModel viewModel;
        private OrderService realOrderService;
        private User testUser;
        private Item testItem;

        [SetUp]
        public void Setup()
        {
            testUser = new User(id: 1, email: "integration@pharmacy.com", phoneNumber: "0700000000",
                passwordHash: "hash!", isAdmin: false, isDisabled: false,
                userName: "testuser", discountNotifications: false, loyaltyPoints: 0);

            testItem = new Item(id: 1, name: "Integration Test Med", producer: "Prod", category: "Cat",
                                 price: 50.0f, numberOfPills: 30, quantity: 20);

            realOrderService = new OrderService(
                new SQLSubstancesRepository(),
                new SQLItemsRepository(),
                new SQLUsersRepository(),
                new SQLOrdersRepository(),
                testUser);

            viewModel = new ProductDetailsPageViewModel();
            viewModel.Initialize(testItem, testUser, realOrderService);
        }

        [Test]
        public void TryAddToBasketrealOrderService_ReturnsSuccessTrue()
        {
            var result = viewModel.TryAddToBasket("2");

            Assert.That(result.success, Is.True);
        }

        [Test]
        public void TryAddToBasketrealOrderService_SuccessfullyModifiesUserBasket()
        {
            viewModel.TryAddToBasket("2");

            Assert.That(testUser.Basket.ContainsKey(testItem.Id), Is.True);
        }

        [Test]
        public void TryAddToBasketrealOrderService_UpdatesQuantityIfAlreadyInBasket()
        {
            testUser.AddItemToBasket(testItem.Id, 1);

            var result = viewModel.TryAddToBasket("2");

            Assert.That(result.success, Is.True);
            Assert.That(testUser.Basket[testItem.Id].Quantity, Is.EqualTo(3));
        }
    }
}