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
    public class ChangePasswordViewModelTests
    {
        private const string ValidPassword = "ValidPass1!";
        private const string WrongPassword = "WrongPass9@";
        private const string ServiceError = "The current password is incorrect.";

        private static ChangePasswordViewModel CreateViewModel(IUserAccountService service)
            => new ChangePasswordViewModel(service);

        [Test]
        public void ExecuteChangePassword_WhenServiceSucceeds_ErrorMessageRemainsNull()
        {
            var serviceMock = new Mock<IUserAccountService>();
            var viewModel = CreateViewModel(serviceMock.Object);
            viewModel.OldPassword = ValidPassword;
            viewModel.NewPassword = ValidPassword;
            viewModel.ConfirmNewPassword = ValidPassword;

            viewModel.ExecuteChangePassword();

            Assert.That(viewModel.ErrorMessage, Is.Null);
        }

        [Test]
        public void ExecuteChangePassword_WhenServiceSucceeds_CallsChangePasswordOnService()
        {
            var serviceMock = new Mock<IUserAccountService>();
            var viewModel = CreateViewModel(serviceMock.Object);
            viewModel.OldPassword = ValidPassword;
            viewModel.NewPassword = ValidPassword;
            viewModel.ConfirmNewPassword = ValidPassword;

            viewModel.ExecuteChangePassword();

            serviceMock.Verify(
                service => service.ChangePassword(ValidPassword, ValidPassword, ValidPassword),
                Times.Once);
        }

        [Test]
        public void ExecuteChangePassword_WhenServiceThrows_SetsErrorMessage()
        {
            var serviceMock = new Mock<IUserAccountService>();
            serviceMock
                .Setup(service => service.ChangePassword(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ArgumentException(ServiceError));

            var viewModel = CreateViewModel(serviceMock.Object);
            viewModel.OldPassword = WrongPassword;
            viewModel.NewPassword = ValidPassword;
            viewModel.ConfirmNewPassword = ValidPassword;

            viewModel.ExecuteChangePassword();

            Assert.That( viewModel.ErrorMessage, Is.EqualTo(ServiceError));
        }
    }
}
