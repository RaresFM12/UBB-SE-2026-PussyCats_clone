using NUnit.Framework;
using PharmacyApp.Features.Products_Catalogue.ViewModels;
using PharmacyApp.Models;

namespace PharmacyApp.Tests.Unit.Features.ProductCatalogue
{
    [TestFixture]
    public class HomePageViewModelTests
    {
        private HomePageViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            _viewModel = new HomePageViewModel();
        }

        // ════════════════════════════════════════════════════════════════════════════
        // F4.6 - VALIDATION 1: ADMIN DASHBOARD VISIBILITY
        // ════════════════════════════════════════════════════════════════════════════

        [Test]
        public void IsAdminDashboardVisible_UserIsAdmin_ReturnsTrue()
        {
            var adminUser = new User(1, "admin@test.com", "0700", "hash",
                                     isAdmin: true, isDisabled: false, "admin", false, 0);

            _viewModel.Initialize(adminUser);

            Assert.IsTrue(_viewModel.IsAdminDashboardVisible);
        }

        [Test]
        public void IsAdminDashboardVisible_UserIsClient_ReturnsFalse()
        {
            var clientUser = new User(1, "client@test.com", "0700", "hash",
                                      isAdmin: false, isDisabled: false, "client", false, 0);

            _viewModel.Initialize(clientUser);

            Assert.IsFalse(_viewModel.IsAdminDashboardVisible);
        }

        [Test]
        public void IsAdminDashboardVisible_UserIsNull_ReturnsFalse()
        {
            _viewModel.Initialize(null);

            Assert.IsFalse(_viewModel.IsAdminDashboardVisible);
        }

        // ════════════════════════════════════════════════════════════════════════════
        // F4.6 - VALIDATION 2: BUTTON VISIBILITY FOR GUESTS
        // ════════════════════════════════════════════════════════════════════════════

        [Test]
        public void IsMyAccountVisible_UserIsNull_ReturnsTrue()
        {
            _viewModel.Initialize(null);

            Assert.IsTrue(_viewModel.IsMyAccountVisible);
        }

        [Test]
        public void IsLoginVisible_UserIsNull_ReturnsFalse()
        {
            _viewModel.Initialize(null);

            Assert.IsFalse(_viewModel.IsLoginVisible);
        }

        [Test]
        public void IsRegisterVisible_UserIsNull_ReturnsFalse()
        {
            _viewModel.Initialize(null);

            Assert.IsFalse(_viewModel.IsRegisterVisible);
        }

        // ════════════════════════════════════════════════════════════════════════════
        // F4.6 - VALIDATION 3: NAVIGATION RESTRICTIONS
        // ════════════════════════════════════════════════════════════════════════════

        [Test]
        public void HandleNavigationRequest_UserIsNullAndRequestsCart_ReturnsLoginView()
        {
            _viewModel.Initialize(null);

            var destination = _viewModel.HandleNavigationRequest("Cart");

            Assert.AreEqual("LoginView", destination);
        }

        [Test]
        public void HandleNavigationRequest_UserIsNullAndRequestsCycleTracker_ReturnsLoginView()
        {
            _viewModel.Initialize(null);

            var destination = _viewModel.HandleNavigationRequest("CycleTracker");

            Assert.AreEqual("LoginView", destination);
        }

        [Test]
        public void HandleNavigationRequest_UserIsNullAndRequestsProducts_ReturnsProducts()
        {
            _viewModel.Initialize(null);

            var destination = _viewModel.HandleNavigationRequest("Products");

            Assert.AreEqual("Products", destination);
        }

        [Test]
        public void HandleNavigationRequest_UserIsAdminAndRequestsCart_ReturnsCart()
        {
            var adminUser = new User(1, "admin@test.com", "0700", "hash",
                                     isAdmin: true, isDisabled: false, "admin", false, 0);
            _viewModel.Initialize(adminUser);

            var destination = _viewModel.HandleNavigationRequest("Cart");

            Assert.AreEqual("Cart", destination);
        }
    }
}   