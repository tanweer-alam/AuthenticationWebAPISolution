using System.Security.Cryptography;
using System.Text;

namespace API.Helpers
{
    public static class PasswordHasher
    {
        public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] saltHash)
        {
            using (var hmac = new HMACSHA512())
            {
                var passwordEncoded = Encoding.UTF8.GetBytes(password);
                passwordHash = hmac.ComputeHash(passwordEncoded);
                saltHash = hmac.Key;
            }
        }

        public static bool VerifyPasswordHash(string password, byte[] storedPasswordHash, byte[] storedSaltHash)
        {
            using (var hmac = new HMACSHA512(storedSaltHash))
            {
                var userPassword = Encoding.UTF8.GetBytes(password);
                var computedPassword = hmac.ComputeHash(userPassword);
                return computedPassword.SequenceEqual(storedPasswordHash);
            }
        }
    }
}
