using NUnit.Framework;
using PharmacyApp.Features.Accounts.Logic;

namespace PharmacyApp.Tests.Unit.Features.Accounts.Logic
{
    [TestFixture]
    public class UserValidationServiceTests
    {
        private IUserValidationService validationService;

        private const string ValidEmail = "user@domain.com";
        private const string WhitespaceString = "   ";
        private const string MissingAtSignEmail = "userdomain.com";
        private const string MissingDotEmail = "user@domaincom";

        private const string ValidPassword = "ValidPass1!";
        private const string InvalidPasswordShort = "Ab1!";
        private const string InvalidPasswordNoUpperCase = "validpass1!";
        private const string InvalidPasswordNoLowerCase = "VALIDPASS1!";
        private const string InvalidPasswordNoDigit = "ValidPass!!";
        private const string InvalidPasswordNoSpecialChar = "ValidPass11";
        private const string InvalidPasswordDisallowedChar = "ValidPass1$";

        private const string ValidPhoneNumber = "0712345678";
        private const string InvalidPhoneNumberWithLetters = "07abc45678";
        private const string InvalidPhoneNumberWithPlus = "+40712345678";

        private const string ValidUsername = "john_doe";
        private const string InvalidUsernameWithDigits = "john123";
        private const string InvalidUsernameWithSpecialChar = "john@doe";

        [SetUp]
        public void Setup()
        {
            validationService = new UserValidationService();
        }

        [Test]
        public void IsCorrectEmailFormat_WithValidEmail_ReturnsTrue()
            => Assert.That(validationService.IsCorrectEmailFormat(ValidEmail), Is.True);

        [Test]
        public void IsCorrectEmailFormat_WithNullEmail_ReturnsFalse()
            => Assert.That(validationService.IsCorrectEmailFormat(null), Is.False);

        [Test]
        public void IsCorrectEmailFormat_WithEmptyEmail_ReturnsFalse()
            => Assert.That(validationService.IsCorrectEmailFormat(string.Empty), Is.False);

        [Test]
        public void IsCorrectEmailFormat_WithWhitespaceOnlyEmail_ReturnsFalse()
            => Assert.That(validationService.IsCorrectEmailFormat(WhitespaceString), Is.False);

        [Test]
        public void IsCorrectEmailFormat_WithMissingAtSign_ReturnsFalse()
            => Assert.That(validationService.IsCorrectEmailFormat(MissingAtSignEmail), Is.False);

        [Test]
        public void IsCorrectEmailFormat_WithMissingDotAfterDomain_ReturnsFalse()
            => Assert.That(validationService.IsCorrectEmailFormat(MissingDotEmail), Is.False);

        [Test]
        public void IsCorrectPasswordFormat_WithValidPassword_ReturnsTrue()
            => Assert.That(validationService.IsCorrectPasswordFormat(ValidPassword), Is.True);

        [Test]
        public void IsCorrectPasswordFormat_WithNullPassword_ReturnsFalse()
            => Assert.That(validationService.IsCorrectPasswordFormat(null), Is.False);

        [Test]
        public void IsCorrectPasswordFormat_WithEmptyPassword_ReturnsFalse()
            => Assert.That(validationService.IsCorrectPasswordFormat(string.Empty), Is.False);

        [Test]
        public void IsCorrectPasswordFormat_ShorterThanEightCharacters_ReturnsFalse()
            => Assert.That(validationService.IsCorrectPasswordFormat(InvalidPasswordShort), Is.False);

        [Test]
        public void IsCorrectPasswordFormat_WithNoUppercaseLetter_ReturnsFalse()
            => Assert.That(validationService.IsCorrectPasswordFormat(InvalidPasswordNoUpperCase), Is.False);

        [Test]
        public void IsCorrectPasswordFormat_WithNoLowercaseLetter_ReturnsFalse()
            => Assert.That(validationService.IsCorrectPasswordFormat(InvalidPasswordNoLowerCase), Is.False);

        [Test]
        public void IsCorrectPasswordFormat_WithNoDigit_ReturnsFalse()
            => Assert.That(validationService.IsCorrectPasswordFormat(InvalidPasswordNoDigit), Is.False);

        [Test]
        public void IsCorrectPasswordFormat_WithNoSpecialCharacter_ReturnsFalse()
            => Assert.That(validationService.IsCorrectPasswordFormat(InvalidPasswordNoSpecialChar), Is.False);

        [Test]
        public void IsCorrectPasswordFormat_WithDisallowedCharacter_ReturnsFalse()
            => Assert.That(validationService.IsCorrectPasswordFormat(InvalidPasswordDisallowedChar), Is.False);

        [Test]
        public void IsCorrectPhoneNumberFormat_WithDigitsOnlyNumber_ReturnsTrue()
            => Assert.That(validationService.IsCorrectPhoneNumberFormat(ValidPhoneNumber), Is.True);

        [Test]
        public void IsCorrectPhoneNumberFormat_WithNullPhoneNumber_ReturnsFalse()
            => Assert.That(validationService.IsCorrectPhoneNumberFormat(null), Is.False);

        [Test]
        public void IsCorrectPhoneNumberFormat_WithEmptyPhoneNumber_ReturnsFalse()
            => Assert.That(validationService.IsCorrectPhoneNumberFormat(string.Empty), Is.False);

        [Test]
        public void IsCorrectPhoneNumberFormat_WithLettersInPhoneNumber_ReturnsFalse()
            => Assert.That(validationService.IsCorrectPhoneNumberFormat(InvalidPhoneNumberWithLetters), Is.False);

        [Test]
        public void IsCorrectPhoneNumberFormat_WithPlusSignInPhoneNumber_ReturnsFalse()
            => Assert.That(validationService.IsCorrectPhoneNumberFormat(InvalidPhoneNumberWithPlus), Is.False);

        [Test]
        public void IsCorrectUsernameFormat_WithLettersAndUnderscore_ReturnsTrue()
            => Assert.That(validationService.IsCorrectUsernameFormat(ValidUsername), Is.True);

        [Test]
        public void IsCorrectUsernameFormat_WithNullUsername_ReturnsFalse()
            => Assert.That(validationService.IsCorrectUsernameFormat(null), Is.False);

        [Test]
        public void IsCorrectUsernameFormat_WithEmptyUsername_ReturnsFalse()
            => Assert.That(validationService.IsCorrectUsernameFormat(string.Empty), Is.False);

        [Test]
        public void IsCorrectUsernameFormat_WithDigitsInUsername_ReturnsFalse()
            => Assert.That(validationService.IsCorrectUsernameFormat(InvalidUsernameWithDigits), Is.False);

        [Test]
        public void IsCorrectUsernameFormat_WithSpecialCharactersInUsername_ReturnsFalse()
            => Assert.That(validationService.IsCorrectUsernameFormat(InvalidUsernameWithSpecialChar), Is.False);
    }
}