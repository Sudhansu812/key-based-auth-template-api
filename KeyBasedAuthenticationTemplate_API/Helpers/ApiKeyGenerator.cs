using System.Security.Cryptography;
using System.Text;
using KeyBasedAuthenticationTemplate_API.Helpers.Interfaces;

namespace KeyBasedAuthenticationTemplate_API.Helpers
{
    // Key shape: "{prefix}_{keyId}.{secret}", e.g. "kbat_8f3a1b2c9d4e.Xy7h2K...".
    // keyId is a plaintext, indexed lookup value; secret is only ever stored as a SHA-256 hash.
    public class ApiKeyGenerator : IApiKeyGenerator
    {
        public GeneratedApiKey Generate(string prefix)
        {
            var keyId = Convert.ToHexString(RandomNumberGenerator.GetBytes(8)).ToLowerInvariant();
            var secret = Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
            var fullKey = $"{prefix}_{keyId}.{secret}";
            var keyHash = ComputeHash(secret);

            return new GeneratedApiKey(fullKey, keyId, keyHash);
        }

        public bool TryParse(string presentedKey, string prefix, out string keyId, out string secret)
        {
            keyId = string.Empty;
            secret = string.Empty;

            if (string.IsNullOrWhiteSpace(presentedKey))
                return false;

            var expectedStart = $"{prefix}_";
            if (!presentedKey.StartsWith(expectedStart, StringComparison.Ordinal))
                return false;

            var remainder = presentedKey[expectedStart.Length..];
            var separatorIndex = remainder.IndexOf('.');
            if (separatorIndex <= 0 || separatorIndex == remainder.Length - 1)
                return false;

            keyId = remainder[..separatorIndex];
            secret = remainder[(separatorIndex + 1)..];
            return true;
        }

        public string ComputeHash(string secret)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        public bool VerifyHash(string secret, string expectedHash)
        {
            var actualHash = ComputeHash(secret);
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(actualHash),
                Encoding.UTF8.GetBytes(expectedHash));
        }

        private static string Base64UrlEncode(byte[] bytes) =>
            Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
