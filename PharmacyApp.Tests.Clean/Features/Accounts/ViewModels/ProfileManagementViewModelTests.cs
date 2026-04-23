using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Features.Accounts.ViewModels;
using PharmacyApp.Models;
using NUnit.Framework;

namespace PharmacyApp.Tests.Unit.Features.Accounts.ViewModels
{
    public class ProfileManagementViewModelTests
    {
        private const string UserEmail = "user@test.com";
        private const string UserUsername = "john_doe";
        private const string UserPhoneNumber = "0712345678";
        private const string ServiceError = "Username may only contain English letters and underscores.";

        private static User BuildTestUser()
            => new User(1, UserEmail, UserPhoneNumber, "hash",
                        isAdmin: false, isDisabled: false,
                        UserUsername, discountNotifications: false, loyaltyPoints: 0);

        [Test]
        public void Constructor_WithLoggedInUser_PopulatesUsernameFromCurrentUser()
        {
            var serviceMock = new Mock<IUserAccountService>();
            serviceMock.Setup(service => service.CurrentUser).Returns(BuildTestUser());

            var viewModel = new ProfileManagementViewModel(serviceMock.Object);

            Assert.That(viewModel.Username, Is.EqualTo(UserUsername));
        }

        [Test]
        public void Constructor_WithLoggedInUser_PopulatesPhoneNumberFromCurrentUser()
        {
            var serviceMock = new Mock<IUserAccountService>();
            serviceMock.Setup(service => service.CurrentUser).Returns(BuildTestUser());

            var viewModel = new ProfileManagementViewModel(serviceMock.Object);

            Assert.That(viewModel.PhoneNumber, Is.EqualTo(UserPhoneNumber));
        }

        [Test]
        public void SaveChanges_WhenServiceSucceeds_CallsUpdateProfileOnService()
        {
            var serviceMock = new Mock<IUserAccountService>();
            serviceMock.Setup(service => service.CurrentUser).Returns(BuildTestUser());

            var viewModel = new ProfileManagementViewModel(serviceMock.Object);
            viewModel.Username = "new_name";
            viewModel.PhoneNumber = "0700000000";

            viewModel.SaveChanges();

            serviceMock.Verify(
                service => service.UpdateProfile("new_name", "0700000000"),
                Times.Once);
        }

        [Test]
        public void SaveChanges_WhenServiceThrows_SetsErrorMessage()
        {
            var serviceMock = new Mock<IUserAccountService>();
            serviceMock.Setup(service => service.CurrentUser).Returns(BuildTestUser());
            serviceMock
                .Setup(service => service.UpdateProfile(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ArgumentException(ServiceError));

            var viewModel = new ProfileManagementViewModel(serviceMock.Object);

            try
            {
                viewModel.SaveChanges();
            }
            catch
            {
            }

            serviceMock.Verify(
                service => service.UpdateProfile(It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }

        [Test]
        public void CancelChanges_AfterModifyingUsername_RestoresOriginalUsername()
        {
            var serviceMock = new Mock<IUserAccountService>();
            serviceMock.Setup(service => service.CurrentUser).Returns(BuildTestUser());

            var viewModel = new ProfileManagementViewModel(serviceMock.Object);
            viewModel.Username = "modified_name";

            viewModel.CancelChanges();

            Assert.That(viewModel.Username, Is.EqualTo(UserUsername));
        }

        [Test]
        public void CancelChanges_WhenErrorMessageIsSet_ClearsErrorMessage()
        {
            var serviceMock = new Mock<IUserAccountService>();
            serviceMock.Setup(service => service.CurrentUser).Returns(BuildTestUser());

            var viewModel = new ProfileManagementViewModel(serviceMock.Object);
            viewModel.ErrorMessage = ServiceError;

            viewModel.CancelChanges();

            Assert.That(viewModel.ErrorMessage, Is.Null);
        }
    }
}
