using Microsoft.Extensions.Primitives;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ChatAPI.Application.Common.Services;

public interface ITokenManager
{
    string GenerateToken(IReadOnlyDictionary<string, StringValues> claims);
    IReadOnlyDictionary<string, StringValues> ValidateToken(string token);
}
/* -----BEGIN PUBLIC KEY-----
 * MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQC+JylwQKKpe6zKJCp0vgAWrJfa
 * DX4vfKmr/MXa5wNb/lIf78AQLSub4KNX/Olc+FVYM8pCjwI8pA9IWqdDdTWjZd+D
 * 3rNF8LXoOKzsROcGCpoLspbvkxBFahzf64mAHSuJfJVrsgbajLMIW0su2houSS/0
 * UAzre24Ic6NDjYNVKwIDAQAB
 * -----END PUBLIC KEY-----
 */
public sealed class TokenManager : ITokenManager
{
    private const int KEY_BITS = 1024;
    private readonly static string _privateKey = """
        -----BEGIN RSA PRIVATE KEY-----
        MIICXgIBAAKBgQC+JylwQKKpe6zKJCp0vgAWrJfaDX4vfKmr/MXa5wNb/lIf78AQ
        LSub4KNX/Olc+FVYM8pCjwI8pA9IWqdDdTWjZd+D3rNF8LXoOKzsROcGCpoLspbv
        kxBFahzf64mAHSuJfJVrsgbajLMIW0su2houSS/0UAzre24Ic6NDjYNVKwIDAQAB
        AoGBAJqUwYYktVdsV/p4Th9bejz0j/nOsD8wa8qKEdozpLJ7XA3kXFGKNwJgKsnT
        q25N3yt15r4W/e2IPXhHYeRf+3pzjbPhVEyfEE/yCvybVAqvPmxnYTXAWa6lJDXt
        D0wd4+UGGDW/7Rnk5/N6jXnVoacHEcpaXBQrWYtj+iHN7DBpAkEA6OZeKgjkNbbs
        56/3bvzAeyREUiLtfkedl6rK8UUK8jT4h6DlVT/gnt1Ouk9HkSzqCe1B/+Gc/L+c
        HZtrzkL0TQJBANEDZPlX+Fuzu/M7V6VKGWovxAiL0Q/aClOys0UYhOw6YDlNK2z7
        DfdXDUcptpNHOVr3rEDUTG+r8eycDTLPC1cCQQDeyE5W4z55a0verGKB1mDA6oY4
        E5UecJ79D7Elbaf03FrIUk5NZ9cT4BqI+YE6C76sVDoH7ObgluFdKOjM2xsZAkA8
        2PchW8nSsH1r7v+x/+NsNWGld0ayjbBp82EbIWs49jmjFOMqg9/p/K7B49PiBl8d
        K3M7IFLsQ6tXTYrOGtZbAkEAyr4DE9Gcx3d2MpR8H6TE56LKQwOrwn9vEYk2/CFh
        Wx5g8k2emf6qoAJx6/uX27V8fMT8KrZqsXZDR0D9AAeqCA==
        -----END RSA PRIVATE KEY-----
        """;
    private readonly static RSAEncryptionPadding _padding = RSAEncryptionPadding.Pkcs1;
    public string GenerateToken(IReadOnlyDictionary<string, StringValues> claims)
    {
        var serialisedClaims = JsonSerializer.Serialize(claims);
        var encodedClaims = Encoding.UTF8.GetBytes(serialisedClaims);
        using RSA encryption = GenerateEncryptor();

        var encryptedClaims = encryption.Encrypt(encodedClaims, _padding);
        return Convert.ToBase64String(encryptedClaims);
    }

    private static RSA GenerateEncryptor()
    {
        RSA encryption = RSA.Create(KEY_BITS);
        encryption.ImportFromPem(_privateKey);
        return encryption;
    }

    public IReadOnlyDictionary<string, StringValues> ValidateToken(string token)
    {
        var tokenBytes = Convert.FromBase64String(token);

        using RSA rsa = GenerateEncryptor();
        var decryptedData = rsa.Decrypt(tokenBytes, _padding);
        var serialisedData = Encoding.UTF8.GetString(decryptedData);
        return JsonSerializer.Deserialize<IReadOnlyDictionary<string, StringValues>>(serialisedData)
            ?? throw new InvalidDataException("Provided token is invalid");
    }
}
