using System.Security.Cryptography;
using System.Text;

namespace e360_clone.BusinessObjects.Helpers
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            // Simple SHA256 hash (for production, use BCrypt or Argon2)
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(string password, string passwordHash)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == passwordHash;
        }
    }
}
