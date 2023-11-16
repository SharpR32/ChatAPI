using ChatAPI.Infrastructure.Services.Abstraction;
using ChatAPI.Infrastructure.Services.TokenHandler.Exceptions;
using Microsoft.Extensions.Primitives;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ChatAPI.Infrastructure.Services.TokenHandler;
public sealed class TokenHandler : ITokenManager
{
    private const int KEY_BITS = 1024;
    private readonly Lazy<Task<string>> _cryptoKeyAccessor = new Lazy<Task<string>>(async () =>
    {
        var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var cryptoKeyPath = Path.Combine(directory, "token_key.perm");
        using FileStream fileHandler = File.OpenRead(cryptoKeyPath);

        var buffer = new byte[fileHandler.Length];
        await fileHandler.ReadExactlyAsync(buffer, default);

        return Encoding.UTF8.GetString(buffer);
    }, isThreadSafe: true);

    private readonly static RSAEncryptionPadding _padding = RSAEncryptionPadding.Pkcs1;
    public async ValueTask<string> GenerateTokenAsync(IReadOnlyDictionary<string, StringValues> claims)
    {
        var serialisedClaims = JsonSerializer.Serialize(claims);
        var encodedClaims = Encoding.UTF8.GetBytes(serialisedClaims);
        using RSA encryption = GenerateEncryptor(await _cryptoKeyAccessor.Value);

        var encryptedClaims = encryption.Encrypt(encodedClaims, _padding);
        return Convert.ToBase64String(encryptedClaims);
    }

    private static RSA GenerateEncryptor(string key)
    {
        RSA encryption = RSA.Create(KEY_BITS);
        encryption.ImportFromPem(key);
        return encryption;
    }

    public async ValueTask<IReadOnlyDictionary<string, string[]>> ValidateTokenAsync(string token)
    {
        var tokenBytes = Convert.FromBase64String(token);

        using RSA rsa = GenerateEncryptor(await _cryptoKeyAccessor.Value);
        var decryptedData = rsa.Decrypt(tokenBytes, _padding);
        var serialisedData = Encoding.UTF8.GetString(decryptedData);
        Dictionary<string, string[]> result = JsonSerializer.Deserialize<Dictionary<string, string[]>>(serialisedData)
            ?? throw new InvalidTokenException();

        return result;
    }
}
