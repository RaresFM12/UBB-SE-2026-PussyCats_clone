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
        private ISecurityService securityService;
        private const string SamplePassword = "SamplePass1!";
        private const string DifferentPassword = "OtherPass99@";

        [SetUp]
        public void Setup()
        {
            securityService = new SecurityService();
        }

        [Test]
        public void HashPassword_WithValidPassword_ReturnsNonEmptyHash()
            => Assert.That(securityService.HashPassword(SamplePassword), Is.Not.Empty);

        [Test]
        public void HashPassword_WithValidPassword_ReturnedHashContainsSegmentSeparator()
            => Assert.That(securityService.HashPassword(SamplePassword), Does.Contain("."));

        [Test]
        public void HashPassword_CalledTwiceWithSamePassword_ReturnsDifferentHashes()
        {
            Assert.That(
                securityService.HashPassword(SamplePassword),
                Is.Not.EqualTo(securityService.HashPassword(SamplePassword)));
        }

        [Test]
        public void VerifyPassword_WithMatchingPassword_ReturnsTrue()
        {
            string storedHash = securityService.HashPassword(SamplePassword);
            Assert.That(securityService.VerifyPassword(SamplePassword, storedHash), Is.True);
        }

        [Test]
        public void VerifyPassword_WithNonMatchingPassword_ReturnsFalse()
        {
            string storedHash = securityService.HashPassword(SamplePassword);
            Assert.That(securityService.VerifyPassword(DifferentPassword, storedHash), Is.False);
        }
    }
}
