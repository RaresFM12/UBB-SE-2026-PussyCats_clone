using NUnit.Framework;
using PharmacyApp.Features.Accounts.Logic;

namespace PharmacyApp.Tests.Unit.Features.Accounts.Logic
{
    [TestFixture]
    public class UserValidationServiceTests
    {
        [Test]
        public void IsCorrectEmailFormat_WithValidEmail_ReturnsTrue()
            => Assert.That(UserValidationService.isCorrectEmailFormat("user@domain.com"), Is.True);

        [Test]
        public void IsCorrectEmailFormat_WithNullEmail_ReturnsFalse()
            => Assert.That(UserValidationService.isCorrectEmailFormat(null), Is.False);

        [Test]
        public void IsCorrectEmailFormat_WithEmptyEmail_ReturnsFalse()
            => Assert.That(UserValidationService.isCorrectEmailFormat(""), Is.False);

        [Test]
        public void IsCorrectEmailFormat_WithWhitespaceOnlyEmail_ReturnsFalse()
            => Assert.That(UserValidationService.isCorrectEmailFormat("   "), Is.False);

        [Test]
        public void IsCorrectEmailFormat_WithMissingAtSign_ReturnsFalse()
            => Assert.That(UserValidationService.isCorrectEmailFormat("userdomain.com"), Is.False);

        [Test]
        public void IsCorrectEmailFormat_WithMissingDotAfterDomain_ReturnsFalse()
            => Assert.That(UserValidationService.isCorrectEmailFormat("user@domaincom"), Is.False);

        [Test]
        public void IsCorrectPasswordFormat_WithValidPassword_ReturnsTrue()
            => Assert.That(UserValidationService.isCorrectPasswordFormat("ValidPass1!"), Is.True);

        [Test]
        public void IsCorrectPasswordFormat_WithNullPassword_ReturnsFalse()
            => Assert.That(UserValidationService.isCorrectPasswordFormat(null), Is.False);

        [Test]
        public void IsCorrectPasswordFormat_WithEmptyPassword_ReturnsFalse()
            => Assert.That(UserValidationService.isCorrectPasswordFormat(""), Is.False);

        [Test]
        public void IsCorrectPasswordFormat_ShorterThanEightCharacters_ReturnsFalse()
            => Assert.That(UserValidationService.isCorrectPasswordFormat("Ab1!"), Is.False);

        [Test]
        public void IsCorrectPasswordFormat_WithNoUppercaseLetter_ReturnsFalse()
            => Assert.That(UserValidationService.isCorrectPasswordFormat("validpass1!"), Is.False);

        [Test]
        public void IsCorrectPasswordFormat_WithNoLowercaseLetter_ReturnsFalse()
            => Assert.That(UserValidationService.isCorrectPasswordFormat("VALIDPASS1!"), Is.False);

        [Test]
        public void IsCorrectPasswordFormat_WithNoDigit_ReturnsFalse()
            => Assert.That(UserValidationService.isCorrectPasswordFormat("ValidPass!!"), Is.False);

        [Test]
        public void IsCorrectPasswordFormat_WithNoSpecialCharacter_ReturnsFalse()
            => Assert.That(UserValidationService.isCorrectPasswordFormat("ValidPass11"), Is.False);

        [Test]
        public void IsCorrectPasswordFormat_WithDisallowedCharacter_ReturnsFalse()
            => Assert.That(UserValidationService.isCorrectPasswordFormat("ValidPass1$"), Is.False);

        [Test]
        public void IsCorrectPhoneNumberFormat_WithDigitsOnlyNumber_ReturnsTrue()
            => Assert.That(UserValidationService.isCorrectPhoneNumberFormat("0712345678"), Is.True);

        [Test]
        public void IsCorrectPhoneNumberFormat_WithNullPhoneNumber_ReturnsFalse()
            => Assert.That(UserValidationService.isCorrectPhoneNumberFormat(null), Is.False);

        [Test]
        public void IsCorrectPhoneNumberFormat_WithEmptyPhoneNumber_ReturnsFalse()
            => Assert.That(UserValidationService.isCorrectPhoneNumberFormat(""), Is.False);

        [Test]
        public void IsCorrectPhoneNumberFormat_WithLettersInPhoneNumber_ReturnsFalse()
            => Assert.That(UserValidationService.isCorrectPhoneNumberFormat("07abc45678"), Is.False);

        [Test]
        public void IsCorrectPhoneNumberFormat_WithPlusSignInPhoneNumber_ReturnsFalse()
            => Assert.That(UserValidationService.isCorrectPhoneNumberFormat("+40712345678"), Is.False);

        [Test]
        public void IsCorrectUsernameFormat_WithLettersAndUnderscore_ReturnsTrue()
            => Assert.That(UserValidationService.isCorrectUsernameFormat("john_doe"), Is.True);

        [Test]
        public void IsCorrectUsernameFormat_WithNullUsername_ReturnsFalse()
            => Assert.That(UserValidationService.isCorrectUsernameFormat(null), Is.False);

        [Test]
        public void IsCorrectUsernameFormat_WithEmptyUsername_ReturnsFalse()
            => Assert.That(UserValidationService.isCorrectUsernameFormat(""), Is.False);

        [Test]
        public void IsCorrectUsernameFormat_WithDigitsInUsername_ReturnsFalse()
            => Assert.That(UserValidationService.isCorrectUsernameFormat("john123"), Is.False);

        [Test]
        public void IsCorrectUsernameFormat_WithSpecialCharactersInUsername_ReturnsFalse()
            => Assert.That(UserValidationService.isCorrectUsernameFormat("john@doe"), Is.False);
    }
}