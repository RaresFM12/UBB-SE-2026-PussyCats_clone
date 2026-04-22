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
    public class RegisterViewModelTests
    {
        private const string ValidEmail = "user@test.com";
        private const string ValidPassword = "ValidPass1!";
        private const string ValidUsername = "john_doe";
        private const string ValidPhoneNumber = "0712345678";
        private const string ServiceError = "This e-mail address is already linked to an account.";

        private static RegisterViewModel CreateViewModel(IUserAccountService service)
            => new RegisterViewModel(service);

        [Test]
        public void ExecuteRegister_WhenServiceSucceeds_FiresRegisterSucceededEvent()
        {
            var serviceMock = new Mock<IUserAccountService>();
            var viewModel = CreateViewModel(serviceMock.Object);
            viewModel.Email = ValidEmail;
            viewModel.Password = ValidPassword;
            viewModel.ConfirmPassword = ValidPassword;
            viewModel.Username = ValidUsername;
            viewModel.PhoneNumber = ValidPhoneNumber;

            bool registerSucceededFired = false;
            viewModel.RegisterSucceeded += () => registerSucceededFired = true;

            viewModel.ExecuteRegister();

            Assert.That(registerSucceededFired, Is.True);
        }

        [Test]
        public void ExecuteRegister_WhenServiceThrows_SetsErrorMessage()
        {
            var serviceMock = new Mock<IUserAccountService>();
            serviceMock
                .Setup(service => service.Register(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ArgumentException(ServiceError));

            var viewModel = CreateViewModel(serviceMock.Object);
            viewModel.Email = ValidEmail;

            viewModel.ExecuteRegister();

            Assert.That( viewModel.ErrorMessage, Is.EqualTo(ServiceError));
        }

        [Test]
        public void ExecuteRegister_WhenServiceThrows_DoesNotFireRegisterSucceededEvent()
        {
            var serviceMock = new Mock<IUserAccountService>();
            serviceMock
                .Setup(service => service.Register(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ArgumentException(ServiceError));

            var viewModel = CreateViewModel(serviceMock.Object);
            bool registerSucceededFired = false;
            viewModel.RegisterSucceeded += () => registerSucceededFired = true;

            viewModel.ExecuteRegister();

            Assert.That(registerSucceededFired, Is.False);
        }
    }
}
