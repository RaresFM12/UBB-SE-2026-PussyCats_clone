using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PharmacyApp.Features.Accounts.Logic;
using NUnit.Framework;

namespace PharmacyApp.Tests.Unit.Features.Accounts.Logic
{
    public class SecurityServiceTests
    {
        private const string SamplePassword = "SamplePass1!";
        private const string DifferentPassword = "OtherPass99@";

        [Test]
        public void HashPassword_WithValidPassword_ReturnsNonEmptyHash()
            => Assert.That(SecurityService.HashPassword(SamplePassword), Is.Not.Empty);

        [Test]
        public void HashPassword_WithValidPassword_ReturnedHashContainsSegmentSeparator()
            => Assert.That( SecurityService.HashPassword(SamplePassword), Does.Contain("."));

        [Test]
        public void HashPassword_CalledTwiceWithSamePassword_ReturnsDifferentHashes()
        {
            Assert.That(
                SecurityService.HashPassword(SamplePassword),
                Is.Not.EqualTo(SecurityService.HashPassword(SamplePassword)));
        }

        [Test]
        public void VerifyPassword_WithMatchingPassword_ReturnsTrue()
        {
            string storedHash = SecurityService.HashPassword(SamplePassword);
            Assert.That(SecurityService.VerifyPassword(SamplePassword, storedHash), Is.True);
        }

        [Test]
        public void VerifyPassword_WithNonMatchingPassword_ReturnsFalse()
        {
            string storedHash = SecurityService.HashPassword(SamplePassword);
            Assert.That(SecurityService.VerifyPassword(DifferentPassword, storedHash), Is.False);
        }
    }
}
