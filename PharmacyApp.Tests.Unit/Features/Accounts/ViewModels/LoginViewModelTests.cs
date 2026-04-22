using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Features.Accounts.ViewModels;
using NUnit.Framework;

namespace PharmacyApp.Tests.Unit.Features.Accounts.ViewModels
{
    public class LoginViewModelTests
    {
        private const string ValidEmail = "user@test.com";
        private const string ValidPassword = "ValidPass1!";
        private const string ServiceError = "No account found for this e-mail address.";

        private static LoginViewModel CreateViewModel(IUserAccountService service)
            => new LoginViewModel(service);

        [Test]
        public void ExecuteLogin_WhenServiceSucceeds_FiresLoginSucceededEvent()
        {
            var serviceMock = new Mock<IUserAccountService>();
            var viewModel = CreateViewModel(serviceMock.Object);
            viewModel.Email = ValidEmail;
            viewModel.Password = ValidPassword;

            bool loginSucceededFired = false;
            viewModel.LoginSucceeded += () => loginSucceededFired = true;

            viewModel.ExecuteLogin();

            Assert.That(loginSucceededFired, Is.True);
        }

        [Test]
        public void ExecuteLogin_WhenServiceThrows_SetsErrorMessage()
        {
            var serviceMock = new Mock<IUserAccountService>();
            serviceMock
                .Setup(service => service.Login(ValidEmail, ValidPassword))
                .Throws(new ArgumentException(ServiceError));

            var viewModel = CreateViewModel(serviceMock.Object);
            viewModel.Email = ValidEmail;
            viewModel.Password = ValidPassword;

            viewModel.ExecuteLogin();

            Assert.That( viewModel.ErrorMessage, Is.EqualTo(ServiceError));
        }

        [Test]
        public void ExecuteLogin_WhenServiceThrows_DoesNotFireLoginSucceededEvent()
        {
            var serviceMock = new Mock<IUserAccountService>();
            serviceMock
                .Setup(service => service.Login(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ArgumentException(ServiceError));

            var viewModel = CreateViewModel(serviceMock.Object);
            bool loginSucceededFired = false;
            viewModel.LoginSucceeded += () => loginSucceededFired = true;

            viewModel.ExecuteLogin();

            Assert.That(loginSucceededFired, Is.False);
        }
    }
}
