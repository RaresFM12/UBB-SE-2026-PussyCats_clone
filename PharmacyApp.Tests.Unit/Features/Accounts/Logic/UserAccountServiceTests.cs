using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public void Login_WithInvalidEmailFormat_ThrowsArgumentException()
        {
            var (service, _) = CreateService();
            Assert.Throws<ArgumentException>(() => service.Login(InvalidEmail, ValidPassword));
        }

        [Test]
        public void Login_WithUnregisteredEmail_ThrowsArgumentException()
        {
            var (service, repositoryMock) = CreateService();
            repositoryMock
                .Setup(repository => repository.GetUserByEmail(ValidEmail))
                .Throws<ArgumentException>();

            Assert.Throws<ArgumentException>(() => service.Login(ValidEmail, ValidPassword));
        }

        [Test]
        public void Login_WithDisabledAccount_ThrowsInvalidOperationException()
        {
            var (service, repositoryMock) = CreateService();
            repositoryMock
                .Setup(repository => repository.GetUserByEmail(ValidEmail))
                .Returns(BuildDisabledUser());

            Assert.Throws<InvalidOperationException>(() => service.Login(ValidEmail, ValidPassword));
        }

        [Test]
        public void Login_WithWrongPassword_ThrowsArgumentException()
        {
            var (service, repositoryMock) = CreateService();
            repositoryMock
                .Setup(repository => repository.GetUserByEmail(ValidEmail))
                .Returns(BuildActiveUser());

            Assert.Throws<ArgumentException>(() => service.Login(ValidEmail, WrongPassword));
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
        public void Register_WithInvalidEmailFormat_ThrowsArgumentException()
        {
            var (service, _) = CreateService();
            Assert.Throws<ArgumentException>(
                () => service.Register(InvalidEmail, ValidPassword, ValidPassword, ValidUsername, ValidPhoneNumber));
        }

        [Test]
        public void Register_WithInvalidPasswordFormat_ThrowsArgumentException()
        {
            var (service, _) = CreateService();
            Assert.Throws<ArgumentException>(
                () => service.Register(ValidEmail, InvalidPassword, InvalidPassword, ValidUsername, ValidPhoneNumber));
        }

        [Test]
        public void Register_WithMismatchedPasswords_ThrowsArgumentException()
        {
            var (service, _) = CreateService();
            Assert.Throws<ArgumentException>(
                () => service.Register(ValidEmail, ValidPassword, WrongPassword, ValidUsername, ValidPhoneNumber));
        }

        [Test]
        public void Register_WithAlreadyRegisteredEmail_ThrowsArgumentException()
        {
            var (service, repositoryMock) = CreateService();
            repositoryMock
                .Setup(repository => repository.UserExists(ValidEmail))
                .Returns(true);

            Assert.Throws<ArgumentException>(
                () => service.Register(ValidEmail, ValidPassword, ValidPassword, ValidUsername, ValidPhoneNumber));
        }

        [Test]
        public void Register_WithValidData_SetsCurrentUser()
        {
            var (service, repositoryMock) = CreateService();
            User createdUser = BuildActiveUser();
            repositoryMock.Setup(repository => repository.UserExists(ValidEmail)).Returns(false);
            repositoryMock.Setup(repository => repository.GetUserByEmail(ValidEmail)).Returns(createdUser);

            service.Register(ValidEmail, ValidPassword, ValidPassword, ValidUsername, ValidPhoneNumber);

            Assert.That(service.CurrentUser, Is.EqualTo(createdUser));
        }

        [Test]
        public void UpdateProfile_WhenNotLoggedIn_ThrowsInvalidOperationException()
        {
            var (service, _) = CreateService();
            Assert.Throws<InvalidOperationException>(
                () => service.UpdateProfile(ValidUsername, ValidPhoneNumber));
        }

        [Test]
        public void UpdateProfile_WithInvalidUsername_ThrowsArgumentException()
        {
            var (service, repositoryMock) = CreateService();
            repositoryMock.Setup(r => r.GetUserByEmail(ValidEmail)).Returns(BuildActiveUser());
            service.Login(ValidEmail, ValidPassword);

            Assert.Throws<ArgumentException>(
                () => service.UpdateProfile(InvalidUsername, ValidPhoneNumber));
        }

        [Test]
        public void UpdateProfile_WithInvalidPhoneNumber_ThrowsArgumentException()
        {
            var (service, repositoryMock) = CreateService();
            repositoryMock.Setup(r => r.GetUserByEmail(ValidEmail)).Returns(BuildActiveUser());
            service.Login(ValidEmail, ValidPassword);

            Assert.Throws<ArgumentException>(
                () => service.UpdateProfile(ValidUsername, InvalidPhoneNumber));
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
        public void ChangePassword_WhenNotLoggedIn_ThrowsInvalidOperationException()
        {
            var (service, _) = CreateService();
            Assert.Throws<InvalidOperationException>(
                () => service.ChangePassword(ValidPassword, ValidPassword, ValidPassword));
        }

        [Test]
        public void ChangePassword_WithIncorrectOldPassword_ThrowsArgumentException()
        {
            var (service, repositoryMock) = CreateService();
            repositoryMock.Setup(r => r.GetUserByEmail(ValidEmail)).Returns(BuildActiveUser());
            service.Login(ValidEmail, ValidPassword);

            Assert.Throws<ArgumentException>(
                () => service.ChangePassword(WrongPassword, ValidPassword, ValidPassword));
        }

        [Test]
        public void ChangePassword_WithInvalidNewPassword_ThrowsArgumentException()
        {
            var (service, repositoryMock) = CreateService();
            repositoryMock.Setup(r => r.GetUserByEmail(ValidEmail)).Returns(BuildActiveUser());
            service.Login(ValidEmail, ValidPassword);

            Assert.Throws<ArgumentException>(
                () => service.ChangePassword(ValidPassword, InvalidPassword, InvalidPassword));
        }

        [Test]
        public void ChangePassword_WithMismatchedNewPasswords_ThrowsArgumentException()
        {
            var (service, repositoryMock) = CreateService();
            repositoryMock.Setup(r => r.GetUserByEmail(ValidEmail)).Returns(BuildActiveUser());
            service.Login(ValidEmail, ValidPassword);

            Assert.Throws<ArgumentException>(
                () => service.ChangePassword(ValidPassword, ValidPassword, WrongPassword));
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
