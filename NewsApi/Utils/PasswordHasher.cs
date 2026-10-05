using System.Security.Cryptography;

namespace NewsApi.Utils
{
    // Хеширование и проверка паролей (алгоритм PBKDF2 из стандартной библиотеки .NET)
    public static class PasswordHasher
    {
        private const int SaltSize = 16;      // "соль" - случайные байты, чтобы одинаковые пароли давали разные хеши
        private const int HashSize = 32;
        private const int Iterations = 100_000;

        // Превращает пароль в строку вида "итерации.соль.хеш"
        public static string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        // Проверяет, соответствует ли введённый пароль сохранённому хешу
        public static bool Verify(string password, string storedHash)
        {
            var parts = storedHash.Split('.');
            if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
                return false;

            byte[] salt, expected;
            try
            {
                salt = Convert.FromBase64String(parts[1]);
                expected = Convert.FromBase64String(parts[2]);
            }
            catch (FormatException)
            {
                return false;
            }

            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);

            // Сравнение за постоянное время, чтобы нельзя было подобрать пароль по скорости ответа
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
    }
}