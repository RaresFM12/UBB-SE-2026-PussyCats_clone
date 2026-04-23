using NUnit.Framework;
using PharmacyApp.Features.Accounts.Logic;

namespace PharmacyApp.Tests.Unit.Features.Accounts.Logic
{
    [TestFixture]
    public class UserValidationServiceTests
    {
        [Test]
        public void IsCorrectEmailFormat_WithValidEmail_ReturnsTrue()
            => Assert.That(UserValidationService.IsCorrectEmailFormat("user@domain.com"), Is.True);

        [Test]
        public void IsCorrectEmailFormat_WithNullEmail_ReturnsFalse()
            => Assert.That(UserValidationService.IsCorrectEmailFormat(null), Is.False);

        [Test]
        public void IsCorrectEmailFormat_WithEmptyEmail_ReturnsFalse()
            => Assert.That(UserValidationService.IsCorrectEmailFormat(string.Empty), Is.False);

        [Test]
        public void IsCorrectEmailFormat_WithWhitespaceOnlyEmail_ReturnsFalse()
            => Assert.That(UserValidationService.IsCorrectEmailFormat("   "), Is.False);

        [Test]
        public void IsCorrectEmailFormat_WithMissingAtSign_ReturnsFalse()
            => Assert.That(UserValidationService.IsCorrectEmailFormat("userdomain.com"), Is.False);

        [Test]
        public void IsCorrectEmailFormat_WithMissingDotAfterDomain_ReturnsFalse()
            => Assert.That(UserValidationService.IsCorrectEmailFormat("user@domaincom"), Is.False);

        [Test]
        public void IsCorrectPasswordFormat_WithValidPassword_ReturnsTrue()
            => Assert.That(UserValidationService.IsCorrectPasswordFormat("ValidPass1!"), Is.True);

        [Test]
        public void IsCorrectPasswordFormat_WithNullPassword_ReturnsFalse()
            => Assert.That(UserValidationService.IsCorrectPasswordFormat(null), Is.False);

        [Test]
        public void IsCorrectPasswordFormat_WithEmptyPassword_ReturnsFalse()
            => Assert.That(UserValidationService.IsCorrectPasswordFormat(string.Empty), Is.False);

        [Test]
        public void IsCorrectPasswordFormat_ShorterThanEightCharacters_ReturnsFalse()
            => Assert.That(UserValidationService.IsCorrectPasswordFormat("Ab1!"), Is.False);

        [Test]
        public void IsCorrectPasswordFormat_WithNoUppercaseLetter_ReturnsFalse()
            => Assert.That(UserValidationService.IsCorrectPasswordFormat("validpass1!"), Is.False);

        [Test]
        public void IsCorrectPasswordFormat_WithNoLowercaseLetter_ReturnsFalse()
            => Assert.That(UserValidationService.IsCorrectPasswordFormat("VALIDPASS1!"), Is.False);

        [Test]
        public void IsCorrectPasswordFormat_WithNoDigit_ReturnsFalse()
            => Assert.That(UserValidationService.IsCorrectPasswordFormat("ValidPass!!"), Is.False);

        [Test]
        public void IsCorrectPasswordFormat_WithNoSpecialCharacter_ReturnsFalse()
            => Assert.That(UserValidationService.IsCorrectPasswordFormat("ValidPass11"), Is.False);

        [Test]
        public void IsCorrectPasswordFormat_WithDisallowedCharacter_ReturnsFalse()
            => Assert.That(UserValidationService.IsCorrectPasswordFormat("ValidPass1$"), Is.False);

        [Test]
        public void IsCorrectPhoneNumberFormat_WithDigitsOnlyNumber_ReturnsTrue()
            => Assert.That(UserValidationService.IsCorrectPhoneNumberFormat("0712345678"), Is.True);

        [Test]
        public void IsCorrectPhoneNumberFormat_WithNullPhoneNumber_ReturnsFalse()
            => Assert.That(UserValidationService.IsCorrectPhoneNumberFormat(null), Is.False);

        [Test]
        public void IsCorrectPhoneNumberFormat_WithEmptyPhoneNumber_ReturnsFalse()
            => Assert.That(UserValidationService.IsCorrectPhoneNumberFormat(string.Empty), Is.False);

        [Test]
        public void IsCorrectPhoneNumberFormat_WithLettersInPhoneNumber_ReturnsFalse()
            => Assert.That(UserValidationService.IsCorrectPhoneNumberFormat("07abc45678"), Is.False);

        [Test]
        public void IsCorrectPhoneNumberFormat_WithPlusSignInPhoneNumber_ReturnsFalse()
            => Assert.That(UserValidationService.IsCorrectPhoneNumberFormat("+40712345678"), Is.False);

        [Test]
        public void IsCorrectUsernameFormat_WithLettersAndUnderscore_ReturnsTrue()
            => Assert.That(UserValidationService.IsCorrectUsernameFormat("john_doe"), Is.True);

        [Test]
        public void IsCorrectUsernameFormat_WithNullUsername_ReturnsFalse()
            => Assert.That(UserValidationService.IsCorrectUsernameFormat(null), Is.False);

        [Test]
        public void IsCorrectUsernameFormat_WithEmptyUsername_ReturnsFalse()
            => Assert.That(UserValidationService.IsCorrectUsernameFormat(string.Empty), Is.False);

        [Test]
        public void IsCorrectUsernameFormat_WithDigitsInUsername_ReturnsFalse()
            => Assert.That(UserValidationService.IsCorrectUsernameFormat("john123"), Is.False);

        [Test]
        public void IsCorrectUsernameFormat_WithSpecialCharactersInUsername_ReturnsFalse()
            => Assert.That(UserValidationService.IsCorrectUsernameFormat("john@doe"), Is.False);
    }
}