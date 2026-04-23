using Moq;
using NUnit.Framework;
using PharmacyApp.Features.Accounts.Logic;
using PharmacyApp.Features.Accounts.ViewModels;

namespace PharmacyApp.Tests.Unit.Features.Accounts.ViewModels
{
    [TestFixture]
    public class ChangePasswordViewModelTests
    {
        private const string ValidPassword = "ValidPass1!";
        private const string WrongPassword = "WrongPass9@";
        private const string ServiceError = "The current password is incorrect.";

        private static ChangePasswordViewModel CreateViewModel(IUserAccountService service)
        {
            return new ChangePasswordViewModel(service);
        }

        [Test]
        public void ChangePassword_WhenServiceSucceeds_ErrorMessageRemainsNull()
        {
            Mock<IUserAccountService> serviceMock = new Mock<IUserAccountService>();
            ChangePasswordViewModel viewModel = CreateViewModel(serviceMock.Object);

            viewModel.OldPassword = ValidPassword;
            viewModel.NewPassword = ValidPassword;
            viewModel.ConfirmPassword = ValidPassword;

            viewModel.ChangePassword();

            Assert.That(viewModel.ErrorMessage, Is.Null);
        }

        [Test]
        public void ChangePassword_WhenServiceSucceeds_CallsChangePasswordOnService()
        {
            Mock<IUserAccountService> serviceMock = new Mock<IUserAccountService>();
            ChangePasswordViewModel viewModel = CreateViewModel(serviceMock.Object);

            viewModel.OldPassword = ValidPassword;
            viewModel.NewPassword = ValidPassword;
            viewModel.ConfirmPassword = ValidPassword;

            viewModel.ChangePassword();

            serviceMock.Verify(
                service => service.ChangePassword(ValidPassword, ValidPassword, ValidPassword),
                Times.Once);
        }

        [Test]
        public void ChangePassword_WhenServiceThrows_SetsErrorMessage()
        {
            Mock<IUserAccountService> serviceMock = new Mock<IUserAccountService>();
            serviceMock
                .Setup(service => service.ChangePassword(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Throws(new ArgumentException(ServiceError));

            ChangePasswordViewModel viewModel = CreateViewModel(serviceMock.Object);

            viewModel.OldPassword = WrongPassword;
            viewModel.NewPassword = ValidPassword;
            viewModel.ConfirmPassword = ValidPassword;

            viewModel.ChangePassword();

            Assert.That(viewModel.ErrorMessage, Is.EqualTo(ServiceError));
        }
    }
}