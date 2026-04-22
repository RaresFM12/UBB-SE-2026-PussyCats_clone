using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PharmacyApp.Features.Accounts.Logic;
using NUnit.Framework;

namespace PharmacyApp.Tests.Unit.Features.Accounts.Logic
{
    public class UserValidationServiceTests
    {
       [Test]
        public void IsValidEmailFormat_WithValidEmail_ReturnsTrue()
            => Assert.That(UserValidationService.IsValidEmailFormat("user@domain.com"), Is.True);

        [Test]
        public void IsValidEmailFormat_WithNullEmail_ReturnsFalse()
            => Assert.That(UserValidationService.IsValidEmailFormat(null), Is.False);

        [Test]
        public void IsValidEmailFormat_WithEmptyEmail_ReturnsFalse()
            => Assert.That(UserValidationService.IsValidEmailFormat(""), Is.False);

        [Test]
        public void IsValidEmailFormat_WithWhitespaceOnlyEmail_ReturnsFalse()
            => Assert.That(UserValidationService.IsValidEmailFormat("   "), Is.False);

        [Test]
        public void IsValidEmailFormat_WithMissingAtSign_ReturnsFalse()
            => Assert.That(UserValidationService.IsValidEmailFormat("userdomain.com"), Is.False);

        [Test]
        public void IsValidEmailFormat_WithMissingDotAfterDomain_ReturnsFalse()
            => Assert.That(UserValidationService.IsValidEmailFormat("user@domaincom"), Is.False);

        [Test]
        public void IsValidPasswordFormat_WithValidPassword_ReturnsTrue()
            => Assert.That(UserValidationService.IsValidPasswordFormat("ValidPass1!"), Is.True);

        [Test]
        public void IsValidPasswordFormat_WithNullPassword_ReturnsFalse()
            => Assert.That(UserValidationService.IsValidPasswordFormat(null), Is.False);

        [Test]
        public void IsValidPasswordFormat_WithEmptyPassword_ReturnsFalse()
            => Assert.That(UserValidationService.IsValidPasswordFormat(""), Is.False);

        [Test]
        public void IsValidPasswordFormat_ShorterThanEightCharacters_ReturnsFalse()
            => Assert.That(UserValidationService.IsValidPasswordFormat("Ab1!"), Is.False);

        [Test]
        public void IsValidPasswordFormat_WithNoUppercaseLetter_ReturnsFalse()
            => Assert.That(UserValidationService.IsValidPasswordFormat("validpass1!"), Is.False);

        [Test]
        public void IsValidPasswordFormat_WithNoLowercaseLetter_ReturnsFalse()
            => Assert.That(UserValidationService.IsValidPasswordFormat("VALIDPASS1!"), Is.False);

        [Test]
        public void IsValidPasswordFormat_WithNoDigit_ReturnsFalse()
            => Assert.That(UserValidationService.IsValidPasswordFormat("ValidPass!!"), Is.False);

        [Test]
        public void IsValidPasswordFormat_WithNoSpecialCharacter_ReturnsFalse()
            => Assert.That(UserValidationService.IsValidPasswordFormat("ValidPass11"), Is.False);

        [Test]
        public void IsValidPasswordFormat_WithDisallowedCharacter_ReturnsFalse()
            => Assert.That(UserValidationService.IsValidPasswordFormat("ValidPass1$"), Is.False);

        [Test]
        public void IsValidPhoneNumberFormat_WithDigitsOnlyNumber_ReturnsTrue()
            => Assert.That(UserValidationService.IsValidPhoneNumberFormat("0712345678"), Is.True);

        [Test]
        public void IsValidPhoneNumberFormat_WithNullPhoneNumber_ReturnsFalse()
            => Assert.That(UserValidationService.IsValidPhoneNumberFormat(null), Is.False);

        [Test]
        public void IsValidPhoneNumberFormat_WithEmptyPhoneNumber_ReturnsFalse()
            => Assert.That(UserValidationService.IsValidPhoneNumberFormat(""), Is.False);

        [Test]
        public void IsValidPhoneNumberFormat_WithLettersInPhoneNumber_ReturnsFalse()
            => Assert.That(UserValidationService.IsValidPhoneNumberFormat("07abc45678"), Is.False);

        [Test]
        public void IsValidPhoneNumberFormat_WithPlusSignInPhoneNumber_ReturnsFalse()
            => Assert.That(UserValidationService.IsValidPhoneNumberFormat("+40712345678"), Is.False);

        [Test]
        public void IsValidUsernameFormat_WithLettersAndUnderscore_ReturnsTrue()
            => Assert.That(UserValidationService.IsValidUsernameFormat("john_doe"), Is.True);

        [Test]
        public void IsValidUsernameFormat_WithNullUsername_ReturnsFalse()
            => Assert.That(UserValidationService.IsValidUsernameFormat(null), Is.False);

        [Test]
        public void IsValidUsernameFormat_WithEmptyUsername_ReturnsFalse()
            => Assert.That(UserValidationService.IsValidUsernameFormat(""), Is.False);

        [Test]
        public void IsValidUsernameFormat_WithDigitsInUsername_ReturnsFalse()
            => Assert.That(UserValidationService.IsValidUsernameFormat("john123"), Is.False);

        [Test]
        public void IsValidUsernameFormat_WithSpecialCharactersInUsername_ReturnsFalse()
            => Assert.That(UserValidationService.IsValidUsernameFormat("john@doe"), Is.False);
    }
}
