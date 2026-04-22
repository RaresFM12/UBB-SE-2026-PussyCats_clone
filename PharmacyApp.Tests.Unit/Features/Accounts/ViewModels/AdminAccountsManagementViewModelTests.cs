using Moq;
using NUnit.Framework;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Features.Accounts.ViewModels;
using PharmacyApp.Models;
using System.Collections.Generic;

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

        private Mock<IUserAccountService> mockUserAccountService;
        private AdminAccountsManagementViewModel adminViewModel;

        private static User CreateStandardUser(int identifier, string userName)
        {
            return new User(identifier, $"{userName}@test.com", "1234567890", "hashedPassword", false, false, userName, false, ZeroLoyaltyPoints);
        }

        [SetUp]
        public void Setup()
        {
            mockUserAccountService = new Mock<IUserAccountService>();
            adminViewModel = new AdminAccountsManagementViewModel(mockUserAccountService.Object);
        }

        [Test]
        public void SearchCommand_WhenCalled_UpdatesUsersCollectionWithResults()
        {
            List<User> searchResults = new List<User>
            {
                CreateStandardUser(FirstUserIdentifier, "FirstUser"),
                CreateStandardUser(SecondUserIdentifier, "SecondUser")
            };

            mockUserAccountService.Setup(service => service.SearchUsers("test")).Returns(searchResults);
            adminViewModel.SearchQuery = "test";

            adminViewModel.SearchCommand.Execute(null);

            Assert.AreEqual(ExpectedListCountWithTwoUsers, adminViewModel.Users.Count);
        }

        [Test]
        public void PromoteUserCommand_WhenExecuted_CallsPromoteToAdminOnService()
        {
            User targetUserToPromote = CreateStandardUser(FirstUserIdentifier, "TargetUser");

            adminViewModel.PromoteUserCommand.Execute(targetUserToPromote);

            mockUserAccountService.Verify(service => service.PromoteToAdmin(targetUserToPromote), Times.Once);
        }

        [Test]
        public void DisableUserCommand_WhenExecuted_CallsDisableAccountOnService()
        {
            User targetUserToDisable = CreateStandardUser(FirstUserIdentifier, "TargetUser");

            adminViewModel.DisableUserCommand.Execute(targetUserToDisable);

            mockUserAccountService.Verify(service => service.DisableAccount(targetUserToDisable), Times.Once);
        }

        [Test]
        public void SearchQuery_WhenChangedToEmpty_ClearsTheUsersCollection()
        {
            adminViewModel.Users.Add(CreateStandardUser(FirstUserIdentifier, "ExistingUser"));

            adminViewModel.SearchQuery = EmptySearchQuery;
            adminViewModel.SearchCommand.Execute(null);

            Assert.AreEqual(ExpectedListCountWhenEmpty, adminViewModel.Users.Count);
        }
    }
}