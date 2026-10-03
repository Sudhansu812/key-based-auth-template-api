namespace KeyBasedAuthenticationTemplate_API.Helpers.Interfaces
{
    public record GeneratedApiKey(string FullKey, string KeyId, string KeyHash);

    public interface IApiKeyGenerator
    {
        GeneratedApiKey Generate(string prefix);
        bool TryParse(string presentedKey, string prefix, out string keyId, out string secret);
        string ComputeHash(string secret);
        bool VerifyHash(string secret, string expectedHash);
    }
}
