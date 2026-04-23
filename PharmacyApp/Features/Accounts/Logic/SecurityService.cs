using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace PharmacyApp.Features.Accounts.Logic
{
    public class SecurityService : ISecurityService
    {
        public string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                10000,
                HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash);
        }

        public bool VerifyPassword(string password, string stored)
        {
            var parts = stored.Split('.');
            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] storedHash = Convert.FromBase64String(parts[1]);

            var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                10000,
                HashAlgorithmName.SHA256);

            byte[] computedHash = pbkdf2.GetBytes(32);

            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }
    }
}
