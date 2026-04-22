using NUnit.Framework;
using PharmacyApp.Features.Products_Catalogue.ViewModels;
using PharmacyApp.Models;

namespace PharmacyApp.Tests.Unit.Features.ProductCatalogue
{
    [TestFixture]
    public class HomePageViewModelTests
    {
        private HomePageViewModel viewModel;

        [SetUp]
        public void Setup()
        {
            viewModel = new HomePageViewModel();
        }

        [Test]
        public void IsAdminDashboardVisible_UserIsAdmin_ReturnsTrue()
        {
            User adminUser = new User(
                1,
                "admin@test.com",
                "0700",
                "hash",
                isAdmin: true,
                isDisabled: false,
                "admin",
                false,
                0);

            viewModel.Initialize(adminUser);

            Assert.That(viewModel.IsAdminDashboardVisible, Is.True);
        }

        [Test]
        public void IsAdminDashboardVisible_UserIsClient_ReturnsFalse()
        {
            User clientUser = new User(
                1,
                "client@test.com",
                "0700",
                "hash",
                isAdmin: false,
                isDisabled: false,
                "client",
                false,
                0);

            viewModel.Initialize(clientUser);

            Assert.That(viewModel.IsAdminDashboardVisible, Is.False);
        }

        [Test]
        public void IsAdminDashboardVisible_UserIsNull_ReturnsFalse()
        {
            viewModel.Initialize(null);

            Assert.That(viewModel.IsAdminDashboardVisible, Is.False);
        }

        [Test]
        public void IsMyAccountVisible_UserIsNull_ReturnsTrue()
        {
            viewModel.Initialize(null);

            Assert.That(viewModel.IsMyAccountVisible, Is.True);
        }

        [Test]
        public void IsLoginVisible_UserIsNull_ReturnsFalse()
        {
            viewModel.Initialize(null);

            Assert.That(viewModel.IsLoginVisible, Is.False);
        }

        [Test]
        public void IsRegisterVisible_UserIsNull_ReturnsFalse()
        {
            viewModel.Initialize(null);

            Assert.That(viewModel.IsRegisterVisible, Is.False);
        }

        [Test]
        public void HandleNavigationRequest_UserIsNullAndRequestsCart_ReturnsLoginView()
        {
            viewModel.Initialize(null);

            string destination = viewModel.HandleNavigationRequest("Cart");

            Assert.That(destination, Is.EqualTo("LoginView"));
        }

        [Test]
        public void HandleNavigationRequest_UserIsNullAndRequestsCycleTracker_ReturnsLoginView()
        {
            viewModel.Initialize(null);

            string destination = viewModel.HandleNavigationRequest("CycleTracker");

            Assert.That(destination, Is.EqualTo("LoginView"));
        }

        [Test]
        public void HandleNavigationRequest_UserIsNullAndRequestsProducts_ReturnsProducts()
        {
            viewModel.Initialize(null);

            string destination = viewModel.HandleNavigationRequest("Products");

            Assert.That(destination, Is.EqualTo("Products"));
        }

        [Test]
        public void HandleNavigationRequest_UserIsAdminAndRequestsCart_ReturnsCart()
        {
            User adminUser = new User(
                1,
                "admin@test.com",
                "0700",
                "hash",
                isAdmin: true,
                isDisabled: false,
                "admin",
                false,
                0);

            viewModel.Initialize(adminUser);

            string destination = viewModel.HandleNavigationRequest("Cart");

            Assert.That(destination, Is.EqualTo("Cart"));
        }

        [Test]
        public void OnPropertyChanged_WithSubscriber_InvokesEvent()
        {
            bool eventFired = false;
            viewModel.PropertyChanged += (_, _) => eventFired = true;

            viewModel.Initialize(null);

            Assert.That(eventFired, Is.True);
        }
    }
}