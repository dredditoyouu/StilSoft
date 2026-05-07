using System.Security.Cryptography;
using System.Text;

namespace StilsoftIRS.Utilities
{
    internal static class Sha256Hasher
    {
        public static string ComputeHash(string value)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
                var hash = sha256.ComputeHash(bytes);
                var builder = new StringBuilder(hash.Length * 2);

                foreach (var b in hash)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
