using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PharmacyApp.Models;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Common.Repositories;

namespace PharmacyApp.Tests.Unit.Features.Accounts.Logic
{
    public class UserAccountServiceTests
    {
       private const string ValidEmail = "user@test.com";
        private const string ValidPassword = "ValidPass1!";
        private const string WrongPassword = "WrongPass9@";
        private const string InvalidPassword = "short";
        private const string ValidUsername = "john_doe";
        private const string InvalidUsername = "john123";
        private const string ValidPhoneNumber = "0712345678";
        private const string InvalidPhoneNumber = "07abc";
        private const string InvalidEmail = "not-an-email";

        private static User BuildActiveUser(string email = ValidEmail, string password = ValidPassword)
            => new User(1, email, ValidPhoneNumber,
                        SecurityService.HashPassword(password),
                        isAdmin: false, isDisabled: false,
                        ValidUsername, discountNotifications: false, loyaltyPoints: 0);

        private static User BuildDisabledUser()
        {
            var user = BuildActiveUser();
            user.IsDisabled = true;
            return user;
        }

        private static (IUserAccountService service, Mock<IUsersRepository> repositoryMock)
            CreateService()
        {
            var repositoryMock = new Mock<IUsersRepository>();
            var service = new UserAccountService(repositoryMock.Object);
            return (service, repositoryMock);
        }

        [Test]
        public void Login_WithEmptyEmail_ThrowsArgumentException()
        {
            var (service, _) = CreateService();
            Assert.Throws<ArgumentException>(() => service.Login("", ValidPassword));
        }

        [Test]
        public void Login_WithEmptyPassword_ThrowsArgumentException()
        {
            var (service, _) = CreateService();
            Assert.Throws<ArgumentException>(() => service.Login(ValidEmail, ""));
        }

        [Test]
        public void Login_WithInvalidEmailFormat_ThrowsException()
        {
            var (service, _) = CreateService();

            var exception = Assert.Throws<Exception>(
                () => service.Login(InvalidEmail, ValidPassword));

            Assert.That(exception!.Message, Is.EqualTo("Not a valid e-mail"));
        }

        [Test]
        public void Login_WithUnregisteredEmail_ThrowsException()
        {
            var (service, repositoryMock) = CreateService();
            repositoryMock
                .Setup(repository => repository.GetUserByEmail(ValidEmail))
                .Throws<ArgumentException>();

            var exception = Assert.Throws<Exception>(() => service.Login(ValidEmail, ValidPassword));
            Assert.That(exception!.Message, Is.EqualTo("E-mail not found"));
        }

        [Test]
        public void Login_WithDisabledAccount_ThrowsException()
        {
            var (service, repositoryMock) = CreateService();
            repositoryMock
                .Setup(repository => repository.GetUserByEmail(ValidEmail))
                .Returns(BuildDisabledUser());

            var exception = Assert.Throws<Exception>(() => service.Login(ValidEmail, ValidPassword));
            Assert.That(exception!.Message, Is.EqualTo("Account disabled"));
        }

        [Test]
        public void Login_WithWrongPassword_ThrowsException()
        {
            var (service, repositoryMock) = CreateService();
            repositoryMock
                .Setup(repository => repository.GetUserByEmail(ValidEmail))
                .Returns(BuildActiveUser());

            var exception = Assert.Throws<Exception>(() => service.Login(ValidEmail, WrongPassword));
            Assert.That(exception!.Message, Is.EqualTo("Incorrect password"));
        }

        [Test]
        public void Login_WithValidCredentials_SetsCurrentUser()
        {
            var (service, repositoryMock) = CreateService();
            User expectedUser = BuildActiveUser();
            repositoryMock
                .Setup(repository => repository.GetUserByEmail(ValidEmail))
                .Returns(expectedUser);

            service.Login(ValidEmail, ValidPassword);

            Assert.That(service.CurrentUser, Is.EqualTo(expectedUser));
        }

        [Test]
        public void Register_WithInvalidEmailFormat_ThrowsException()
        {
            var (service, _) = CreateService();

            var exception = Assert.Throws<Exception>(
                () => service.Register(InvalidEmail, ValidPassword, ValidPassword, ValidUsername, ValidPhoneNumber));

            Assert.That(exception!.Message, Is.EqualTo("Not a valid email format\nmust be <text>@<text>.<text>"));
        }

        [Test]
        public void Register_WithInvalidPasswordFormat_ThrowsException()
        {
            var (service, _) = CreateService();

            var exception = Assert.Throws<Exception>(
                () => service.Register(ValidEmail, InvalidPassword, InvalidPassword, ValidUsername, ValidPhoneNumber));

            Assert.That(exception!.Message, Is.EqualTo("Incorrect format, must have: min 8 chars\n -1 symbol from {!,@,#,%,^,*}\n -1 capital and 1 small letter\n -1 digit"));
        }

        [Test]
        public void Register_WithMismatchedPasswords_ThrowsException()
        {
            var (service, _) = CreateService();

            var exception = Assert.Throws<Exception>(
                () => service.Register(ValidEmail, ValidPassword, WrongPassword, ValidUsername, ValidPhoneNumber));

            Assert.That(exception!.Message, Is.EqualTo("Passwords don't match"));
        }

        [Test]
        public void Register_WithAlreadyRegisteredEmail_ThrowsException()
        {
            var (service, repositoryMock) = CreateService();

            repositoryMock
                .Setup(repository => repository.GetUserByEmail(ValidEmail))
                .Returns(BuildActiveUser());

            var exception = Assert.Throws<Exception>(
                () => service.Register(ValidEmail, ValidPassword, ValidPassword, ValidUsername, ValidPhoneNumber));

            Assert.That(exception!.Message, Is.EqualTo("Email already linked to an account"));
        }

        [Test]
        public void Register_WithValidData_SetsCurrentUser()
        {
            var (service, repositoryMock) = CreateService();
            User createdUser = BuildActiveUser();

            repositoryMock
                .SetupSequence(repository => repository.GetUserByEmail(ValidEmail))
                .Throws(new ArgumentException())
                .Returns(createdUser);

            service.Register(ValidEmail, ValidPassword, ValidPassword, ValidUsername, ValidPhoneNumber);

            Assert.That(service.CurrentUser, Is.EqualTo(createdUser));
            repositoryMock.Verify(
            repository => repository.AddUser(
                ValidEmail,
                ValidPhoneNumber,
                It.IsAny<string>(),
                ValidUsername,
                false,
                false,
                false,
                0),
                Times.Once);
            }

        [Test]
        public void UpdateProfile_WhenNotLoggedIn_ThrowsException()
        {
            var (service, _) = CreateService();

            var exception = Assert.Throws<Exception>(
                () => service.UpdateProfile(ValidUsername, ValidPhoneNumber));

            Assert.That(exception!.Message, Is.EqualTo("Not logged in"));
        }

        [Test]
        public void UpdateProfile_WithInvalidUsername_ThrowsException()
        {
            var (service, repositoryMock) = CreateService();
            repositoryMock
                .Setup(repository => repository.GetUserByEmail(ValidEmail))
                .Returns(BuildActiveUser());

            service.Login(ValidEmail, ValidPassword);

            var exception = Assert.Throws<Exception>(
                () => service.UpdateProfile(InvalidUsername, ValidPhoneNumber));

            Assert.That(exception!.Message, Is.EqualTo("Invalid new username"));
        }

        [Test]
        public void UpdateProfile_WithInvalidPhoneNumber_ThrowsException()
        {
            var (service, repositoryMock) = CreateService();
            repositoryMock
                .Setup(repository => repository.GetUserByEmail(ValidEmail))
                .Returns(BuildActiveUser());

            service.Login(ValidEmail, ValidPassword);

            var exception = Assert.Throws<Exception>(
                () => service.UpdateProfile(ValidUsername, InvalidPhoneNumber));

            Assert.That(exception!.Message, Is.EqualTo("Invalid new phone number"));
        }

        [Test]
        public void UpdateProfile_WithEmptyUsername_UsesEmailLocalPartAsUsername()
        {
            var (service, repositoryMock) = CreateService();
            repositoryMock.Setup(r => r.GetUserByEmail(ValidEmail)).Returns(BuildActiveUser());
            service.Login(ValidEmail, ValidPassword);

            service.UpdateProfile("", ValidPhoneNumber);

            Assert.That(service.CurrentUser!.Username, Is.EqualTo("user"));
        }

        [Test]
        public void UpdateProfile_WithValidData_PersistsChangesToRepository()
        {
            var (service, repositoryMock) = CreateService();
            repositoryMock.Setup(r => r.GetUserByEmail(ValidEmail)).Returns(BuildActiveUser());
            service.Login(ValidEmail, ValidPassword);

            service.UpdateProfile(ValidUsername, ValidPhoneNumber);

            repositoryMock.Verify(
                repository => repository.UpdateUser(It.IsAny<User>()),
                Times.Once);
        }

        [Test]
        public void ChangePassword_WhenNotLoggedIn_ThrowsException()
        {
            var (service, _) = CreateService();

            var exception = Assert.Throws<Exception>(
                () => service.ChangePassword(ValidPassword, ValidPassword, ValidPassword));

            Assert.That(exception!.Message, Is.EqualTo("Not logged in"));
        }

        [Test]
        public void ChangePassword_WithIncorrectOldPassword_ThrowsException()
        {
            var (service, repositoryMock) = CreateService();
            repositoryMock
                .Setup(repository => repository.GetUserByEmail(ValidEmail))
                .Returns(BuildActiveUser());

            service.Login(ValidEmail, ValidPassword);

            var exception = Assert.Throws<Exception>(
                () => service.ChangePassword(WrongPassword, ValidPassword, ValidPassword));

            Assert.That(exception!.Message, Is.EqualTo("Incorrect password"));
        }

        [Test]
        public void ChangePassword_WithInvalidNewPassword_ThrowsException()
        {
            var (service, repositoryMock) = CreateService();
            repositoryMock
                .Setup(repository => repository.GetUserByEmail(ValidEmail))
                .Returns(BuildActiveUser());

            service.Login(ValidEmail, ValidPassword);

            var exception = Assert.Throws<Exception>(
                () => service.ChangePassword(ValidPassword, InvalidPassword, InvalidPassword));

            Assert.That(exception!.Message, Is.EqualTo("New password must comply with the rules"));
        }

        [Test]
        public void ChangePassword_WithMismatchedNewPasswords_ThrowsException()
        {
            var (service, repositoryMock) = CreateService();
            repositoryMock
                .Setup(repository => repository.GetUserByEmail(ValidEmail))
                .Returns(BuildActiveUser());

            service.Login(ValidEmail, ValidPassword);

            var exception = Assert.Throws<Exception>(
                () => service.ChangePassword(ValidPassword, ValidPassword, WrongPassword));

            Assert.That(exception!.Message, Is.EqualTo("Passwords don't match"));
        }

        [Test]
        public void ChangePassword_WithValidPasswords_UpdatesStoredPasswordHash()
        {
            var (service, repositoryMock) = CreateService();
            repositoryMock.Setup(r => r.GetUserByEmail(ValidEmail)).Returns(BuildActiveUser());
            service.Login(ValidEmail, ValidPassword);
            string originalHash = service.CurrentUser!.PasswordHash;

            service.ChangePassword(ValidPassword, ValidPassword, ValidPassword);

            Assert.That(originalHash, Is.Not.EqualTo(service.CurrentUser.PasswordHash));
        }

        [Test]
        public void Logout_AfterSuccessfulLogin_ClearsCurrentUser()
        {
            var (service, repositoryMock) = CreateService();
            repositoryMock.Setup(r => r.GetUserByEmail(ValidEmail)).Returns(BuildActiveUser());
            service.Login(ValidEmail, ValidPassword);

            service.Logout();

            Assert.That(service.CurrentUser, Is.Null);
        }
    }
}
