using Moq;
using NUnit.Framework;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Features.Accounts.ViewModels;
using System;

namespace PharmacyApp.Tests.Unit.Features.Accounts.ViewModels
{
    [TestFixture]
    public class RegisterViewModelTests
    {
        private const string ValidEmail = "user@test.com";
        private const string ValidPassword = "ValidPass1!";
        private const string ValidUsername = "john_doe";
        private const string ValidPhoneNumber = "0712345678";
        private const string ServiceError = "This e-mail address is already linked to an account.";

        private static RegisterViewModel CreateViewModel(IUserAccountService service)
        {
            return new RegisterViewModel(service);
        }

        [Test]
        public void RegisterCommand_WhenServiceSucceeds_FiresRegisterSuccededEvent()
        {
            Mock<IUserAccountService> serviceMock = new Mock<IUserAccountService>();
            RegisterViewModel viewModel = CreateViewModel(serviceMock.Object);

            viewModel.Email = ValidEmail;
            viewModel.Password = ValidPassword;
            viewModel.ConfirmPassword = ValidPassword;
            viewModel.Username = ValidUsername;
            viewModel.PhoneNumber = ValidPhoneNumber;

            bool registerSuccededFired = false;
            viewModel.RegisterSucceded += () => registerSuccededFired = true;

            viewModel.RegisterCommand.Execute(null);

            Assert.That(registerSuccededFired, Is.True);
        }

        [Test]
        public void RegisterCommand_WhenServiceThrows_SetsErrorMessage()
        {
            Mock<IUserAccountService> serviceMock = new Mock<IUserAccountService>();
            serviceMock
                .Setup(service => service.Register(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Throws(new ArgumentException(ServiceError));

            RegisterViewModel viewModel = CreateViewModel(serviceMock.Object);
            viewModel.Email = ValidEmail;
            viewModel.Password = ValidPassword;
            viewModel.ConfirmPassword = ValidPassword;
            viewModel.Username = ValidUsername;
            viewModel.PhoneNumber = ValidPhoneNumber;

            viewModel.RegisterCommand.Execute(null);

            Assert.That(viewModel.ErrorMessage, Is.EqualTo(ServiceError));
        }

        [Test]
        public void RegisterCommand_WhenServiceThrows_DoesNotFireRegisterSuccededEvent()
        {
            Mock<IUserAccountService> serviceMock = new Mock<IUserAccountService>();
            serviceMock
                .Setup(service => service.Register(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Throws(new ArgumentException(ServiceError));

            RegisterViewModel viewModel = CreateViewModel(serviceMock.Object);

            bool registerSuccededFired = false;
            viewModel.RegisterSucceded += () => registerSuccededFired = true;

            viewModel.RegisterCommand.Execute(null);

            Assert.That(registerSuccededFired, Is.False);
        }
    }
}