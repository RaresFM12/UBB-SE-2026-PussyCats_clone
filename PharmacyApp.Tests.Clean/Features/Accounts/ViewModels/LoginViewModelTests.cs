using Moq;
using NUnit.Framework;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Features.Accounts.ViewModels;
using System;

namespace PharmacyApp.Tests.Unit.Features.Accounts.ViewModels
{
    [TestFixture]
    public class LoginViewModelTests
    {
        private const string ValidEmail = "user@test.com";
        private const string ValidPassword = "ValidPass1!";
        private const string ServiceError = "No account found for this e-mail address.";

        private static LoginViewModel CreateViewModel(IUserAccountService service)
        {
            return new LoginViewModel(service);
        }

        [Test]
        public void Login_WhenServiceSucceeds_FiresLoginSuccededEvent()
        {
            Mock<IUserAccountService> serviceMock = new Mock<IUserAccountService>();
            LoginViewModel viewModel = CreateViewModel(serviceMock.Object);
            viewModel.Email = ValidEmail;
            viewModel.Password = ValidPassword;

            bool loginSuccededFired = false;
            viewModel.LoginSucceded += () => loginSuccededFired = true;

            viewModel.Login();

            Assert.That(loginSuccededFired, Is.True);
        }

        [Test]
        public void Login_WhenServiceThrows_SetsErrorMessage()
        {
            Mock<IUserAccountService> serviceMock = new Mock<IUserAccountService>();
            serviceMock
                .Setup(service => service.Login(ValidEmail, ValidPassword))
                .Throws(new ArgumentException(ServiceError));

            LoginViewModel viewModel = CreateViewModel(serviceMock.Object);
            viewModel.Email = ValidEmail;
            viewModel.Password = ValidPassword;

            viewModel.Login();

            Assert.That(viewModel.ErrorMessage, Is.EqualTo(ServiceError));
        }

        [Test]
        public void Login_WhenServiceThrows_DoesNotFireLoginSuccededEvent()
        {
            Mock<IUserAccountService> serviceMock = new Mock<IUserAccountService>();
            serviceMock
                .Setup(service => service.Login(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ArgumentException(ServiceError));

            LoginViewModel viewModel = CreateViewModel(serviceMock.Object);

            bool loginSuccededFired = false;
            viewModel.LoginSucceded += () => loginSuccededFired = true;

            viewModel.Login();

            Assert.That(loginSuccededFired, Is.False);
        }
    }
}