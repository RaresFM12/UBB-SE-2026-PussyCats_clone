using Moq;
using NUnit.Framework;
using PharmacyApp.Common.Repositories;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Models;
using System;
using System.Collections.Generic;

namespace PharmacyApp.Tests.Unit.Features.Accounts.Logic
{
    [TestFixture]
    public class UserAccountServiceTests
    {
        private const int AdministratorUserIdentifier = 1;
        private const int StandardUserIdentifier = 2;
        private const int TargetSearchUserIdentifier = 5;
        private const int ZeroLoyaltyPoints = 0;
        private const string DefaultPhoneNumber = "1234567890";
        private const string DefaultPasswordHash = "hashedPassword";

        private Mock<IUsersRepository> mockUsersRepository;
        private UserAccountService userAccountService;
        private User administratorUser;

        [SetUp]
        public void Setup()
        {
            mockUsersRepository = new Mock<IUsersRepository>();
            userAccountService = new UserAccountService(mockUsersRepository.Object);

            administratorUser = new User(AdministratorUserIdentifier, "admin@test.com", DefaultPhoneNumber, DefaultPasswordHash, true, false, "Administrator", false, ZeroLoyaltyPoints);
        }

        [Test]
        public void Login_UserIsDisabled_ThrowsException()
        {
            User disabledUser = new User(StandardUserIdentifier, "user@test.com", DefaultPhoneNumber, DefaultPasswordHash, false, true, "StandardUser", false, ZeroLoyaltyPoints);
            mockUsersRepository.Setup(repository => repository.GetUserByEmail("user@test.com")).Returns(disabledUser);

            Assert.Throws<Exception>(() => userAccountService.Login("user@test.com", "anyPassword"));
        }

        [Test]
        public void Register_PasswordsDoNotMatch_ThrowsException()
        {
            Assert.Throws<Exception>(() => userAccountService.Register("new@test.com", "PasswordOne!", "PasswordTwo!"));
        }

        [Test]
        public void PromoteToAdmin_TargetIsAlreadyAdmin_DoesNotCallUpdate()
        {
            User targetUser = new User(StandardUserIdentifier, "target@test.com", DefaultPhoneNumber, DefaultPasswordHash, true, false, "TargetUser", false, ZeroLoyaltyPoints);
            userAccountService.CurrentUser = administratorUser;

            userAccountService.PromoteToAdmin(targetUser);

            mockUsersRepository.Verify(repository => repository.UpdateUser(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public void DisableAccount_TargetIsAdmin_DoesNotChangeDisabledStatus()
        {
            User targetAdministrator = new User(StandardUserIdentifier, "admin2@test.com", DefaultPhoneNumber, DefaultPasswordHash, true, false, "TargetAdmin", false, ZeroLoyaltyPoints);
            userAccountService.CurrentUser = administratorUser;

            userAccountService.DisableAccount(targetAdministrator);

            Assert.IsFalse(targetAdministrator.IsDisabled);
        }

        [Test]
        public void SearchUsers_QueryByEmail_ReturnsCorrectUser()
        {
            userAccountService.CurrentUser = administratorUser;
            User targetSearchUser = new User(TargetSearchUserIdentifier, "find@me.com", DefaultPhoneNumber, DefaultPasswordHash, false, false, "SearchUser", false, ZeroLoyaltyPoints);

            mockUsersRepository.Setup(repository => repository.GetAllUsers()).Returns(new List<User> { administratorUser, targetSearchUser });

            List<User> searchResults = userAccountService.SearchUsers("find@me.com");

            Assert.Contains(targetSearchUser, searchResults);
        }
    }
}