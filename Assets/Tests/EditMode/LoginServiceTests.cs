using NUnit.Framework;
using SentryGame.Common;

namespace SentryGame.Tests
{
    public sealed class LoginServiceTests
    {
        [Test]
        public void Validate_ReturnsTrue_WhenCredentialMatches()
        {
            Assert.IsTrue(LoginService.Validate("admin", "123456"));
        }

        [Test]
        public void Validate_ReturnsFalse_WhenCredentialDoesNotMatch()
        {
            Assert.IsFalse(LoginService.Validate("admin", "wrong"));
            Assert.IsFalse(LoginService.Validate("guest", "123456"));
        }
    }
}
