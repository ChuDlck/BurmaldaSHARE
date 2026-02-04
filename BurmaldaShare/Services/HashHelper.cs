using System.Security.Cryptography;
using System.Text;

namespace BurmaldaSHARE.Services
{
    public static class HashHelper
    {
        private const int KeySize = 32;//256 бит
        private const int Iterations = 100000;
        private static readonly HashAlgorithmName _algorithm = HashAlgorithmName.SHA256;

        public static string HashPassword(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(KeySize);//генераторка соли

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(//PBKDF2
                password,
                salt,
                Iterations,
                _algorithm,
                KeySize
            );

            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        public static bool VerifyPassword(string password, string passwordHashFromDb)
        {
            var parts = passwordHashFromDb.Split(':');
            if (parts.Length != 2) return false;//если формат битый

            var salt = Convert.FromBase64String(parts[0]);
            var hashFromDb = Convert.FromBase64String(parts[1]);

            var inputHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                _algorithm,
                KeySize
            );

            return CryptographicOperations.FixedTimeEquals(inputHash, hashFromDb);//CryptographicOperations.FixedTimeEquals хуйня которая защищает от атак по времени
        }
    }
}
