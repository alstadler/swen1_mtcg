using System;
using System.Security.Cryptography;
using System.Text;

namespace MTCG.Utilities
{
    public static class TokenManager
    {
        public static string GenerateToken(string username)
        {
            string TokenInput = $"{username}:{DateTime.UtcNow.Ticks}";
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(TokenInput));
                return Convert.ToBase64String(hashBytes);
            }
        }

        public static bool ValidateToken(string token)
        {
            return true;    // To-Do: Implement token validation logic
        }
    }
}