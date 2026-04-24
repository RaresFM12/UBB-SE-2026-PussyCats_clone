using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Features.Accounts.ViewModels;
using PharmacyApp.Models;

namespace PharmacyApp.Tests.Unit.Features.Accounts.ViewModels
{
    [TestFixture]
    public class AdminAccountsManagementViewModelTests
    {
        private const int FirstUserIdentifier = 1;
        private const int SecondUserIdentifier = 2;
        private const int ZeroLoyaltyPoints = 0;
        private const int ExpectedListCountWhenEmpty = 0;
        private const int ExpectedListCountWithTwoUsers = 2;
        private const string EmptySearchQuery = "";
        private const string TestSearchQuery = "test";
        private const string ErrorMessageText = "Search failed";

        private Mock<IUserAccountService> mockUserAccountService;
        private AdminAccountsManagementViewModel adminViewModel;

        private static User CreateStandardUser(int identifier, string userName)
        {
            return new User(
                identifier,
                $"{userName}@test.com",
                "1234567890",
                "hashedPassword",
                false,
                false,
                userName,
                false,
                ZeroLoyaltyPoints);
        }

        [SetUp]
        public void Setup()
        {
            mockUserAccountService = new Mock<IUserAccountService>();
            mockUserAccountService
                .Setup(service => service.SearchUsers(string.Empty))
                .Returns(new List<User>());

            adminViewModel = new AdminAccountsManagementViewModel(mockUserAccountService.Object);
        }

        [Test]
        public void Constructor_WhenCreated_LoadsUsers()
        {
            mockUserAccountService.Verify(
                service => service.SearchUsers(string.Empty),
                Times.AtLeastOnce);
        }

        [Test]
        public void Search_WhenCalled_UpdatesUsersCollectionWithResults()
        {
            List<User> searchResults = new List<User>
            {
                CreateStandardUser(FirstUserIdentifier, "FirstUser"),
                CreateStandardUser(SecondUserIdentifier, "SecondUser")
            };

            mockUserAccountService
                .Setup(service => service.SearchUsers(TestSearchQuery))
                .Returns(searchResults);

            adminViewModel.SearchQuery = TestSearchQuery;

            adminViewModel.Search();

            Assert.That(adminViewModel.Users.Count, Is.EqualTo(ExpectedListCountWithTwoUsers));
            Assert.That(adminViewModel.Users[0].User.Username, Is.EqualTo("FirstUser"));
            Assert.That(adminViewModel.Users[1].User.Username, Is.EqualTo("SecondUser"));
        }

        [Test]
        public void Search_WhenSearchQueryIsEmpty_UpdatesUsersCollectionWithEmptyResults()
        {
            mockUserAccountService
                .Setup(service => service.SearchUsers(EmptySearchQuery))
                .Returns(new List<User>());

            adminViewModel.SearchQuery = EmptySearchQuery;

            adminViewModel.Search();

            Assert.That(adminViewModel.Users.Count, Is.EqualTo(ExpectedListCountWhenEmpty));
        }

        [Test]
        public void Search_WhenServiceThrows_SetsErrorMessage()
        {
            mockUserAccountService
                .Setup(service => service.SearchUsers(TestSearchQuery))
                .Throws(new System.Exception(ErrorMessageText));

            adminViewModel.SearchQuery = TestSearchQuery;

            adminViewModel.Search();

            Assert.That(adminViewModel.ErrorMessage, Is.EqualTo(ErrorMessageText));
        }

        [Test]
        public void Promote_WhenCalled_CallsPromoteToAdminOnService()
        {
            User targetUserToPromote = CreateStandardUser(FirstUserIdentifier, "TargetUser");
            UserItemViewModel targetUserItem = new UserItemViewModel(targetUserToPromote);

            adminViewModel.Promote(targetUserItem);

            mockUserAccountService.Verify(
                service => service.PromoteToAdmin(targetUserToPromote),
                Times.Once);
        }

        [Test]
        public void Disable_WhenCalled_CallsDisableAccountOnService()
        {
            User targetUserToDisable = CreateStandardUser(FirstUserIdentifier, "TargetUser");
            UserItemViewModel targetUserItem = new UserItemViewModel(targetUserToDisable);

            adminViewModel.Disable(targetUserItem);

            mockUserAccountService.Verify(
                service => service.DisableAccount(It.Is<User>(user => user.Id == targetUserToDisable.Id)),
                Times.Once);
        }

        [Test]
        public void Promote_WhenServiceThrows_SetsErrorMessage()
        {
            User targetUserToPromote = CreateStandardUser(FirstUserIdentifier, "TargetUser");
            UserItemViewModel targetUserItem = new UserItemViewModel(targetUserToPromote);

            mockUserAccountService
                .Setup(service => service.SearchUsers(It.IsAny<string>()))
                .Returns(new List<User>());

            mockUserAccountService
                .Setup(service => service.PromoteToAdmin(It.Is<User>(user => user.Id == targetUserToPromote.Id)))
                .Throws(new Exception(ErrorMessageText));

            adminViewModel.Promote(targetUserItem);

            Assert.That(adminViewModel.ErrorMessage, Is.EqualTo(ErrorMessageText));
        }

        [Test]
        public void Disable_WhenServiceThrows_SetsErrorMessage()
        {
            User targetUserToDisable = CreateStandardUser(FirstUserIdentifier, "TargetUser");
            UserItemViewModel targetUserItem = new UserItemViewModel(targetUserToDisable);

            mockUserAccountService
                .Setup(service => service.SearchUsers(It.IsAny<string>()))
                .Returns(new List<User>());

            mockUserAccountService
                .Setup(service => service.DisableAccount(It.Is<User>(user => user.Id == targetUserToDisable.Id)))
                .Throws(new Exception(ErrorMessageText));

            adminViewModel.Disable(targetUserItem);

            Assert.That(adminViewModel.ErrorMessage, Is.EqualTo(ErrorMessageText));
        }

        [Test]
        public void LoadUsers_WhenServiceReturnsUsers_UpdatesUsersCollection()
        {
            List<User> users = new List<User>
            {
                CreateStandardUser(FirstUserIdentifier, "FirstUser"),
                CreateStandardUser(SecondUserIdentifier, "SecondUser")
            };

            mockUserAccountService
                .Setup(service => service.SearchUsers(string.Empty))
                .Returns(users);

            adminViewModel.LoadUsers();

            Assert.That(adminViewModel.Users.Count, Is.EqualTo(ExpectedListCountWithTwoUsers));
        }
    }
}