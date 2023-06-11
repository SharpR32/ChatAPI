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

/// -----BEGIN PUBLIC KEY-----
/// MIGeMA0GCSqGSIb3DQEBAQUAA4GMADCBiAKBgFRcG7oBHwN4GPYvTg7yFajmokzz
/// SijTrBvRGrbE3c54boxN8TzWUt+i8r/fqMIjQcBJSBA4h5nnZMkrfA7u44xpJfND
/// O/GGlgeODffZII652VisYQXzj+tAOOfoOcZAITzI6idOut2Z2Mxw9a/xC5jla4bs
/// g/H91KOm0zzgsNgjAgMBAAE=
/// -----END PUBLIC KEY-----
public sealed class TokenManager : ITokenManager
{
    private const int KEY_BITS = 1024;
    private readonly static string _privateKey = """
        -----BEGIN RSA PRIVATE KEY-----
        MIICWgIBAAKBgFRcG7oBHwN4GPYvTg7yFajmokzzSijTrBvRGrbE3c54boxN8TzW
        Ut+i8r/fqMIjQcBJSBA4h5nnZMkrfA7u44xpJfNDO/GGlgeODffZII652VisYQXz
        j+tAOOfoOcZAITzI6idOut2Z2Mxw9a/xC5jla4bsg/H91KOm0zzgsNgjAgMBAAEC
        gYAtQ8MI2iMm/GEAb7+Fm3Xty9rYSU7Ie1OFFX0tBpMxf0Np+0Ru7V1IbCmutLbb
        fImCQI/vTDXOae+VVcTaSmpDxhSwivo7Dl348UlOVYk4rkGbdQr/Xg9N3MfINpm5
        x+uXT9GZGretIcRxO4fKLjH8K3Jx8eiN2Ti40ZMSNcxpuQJBAJsRYk1hYkaUeBQi
        1YPup4E2Rp688hsRfkyqKj7R7GrOfcQj+feBwG3eQPx8FnXTBm0pL33sqzCmiLG0
        GWQweIUCQQCLRMqewCem2ZdYEJl+D4SyrvuNr1gLQmraqaXNcSMqvl8aJDhjsCDA
        FAC2rcjQ1ubGoXsFa8BdLlLK7YFgg0KHAkBtmv0DuujPAJRbjz+iMGcfcrC59M2g
        Cl5ebAzOOG1GFUxZ/h/qLUFJp0YB8OejQpSRRgI2nLln+t411Rn5cjVNAkBgBaQ2
        ZIJyVeA1hexusEBr+p3SiK0JxldqQEHjLjhzBiMIISUIBq3uAVykl5m39BPVrAzo
        JezR0lifNbZYVugNAkAp9ojRO0z4896gaZW7XAb100FGQcnMvTipC8dAI6Z1AjII
        Aub7AQQw57iNdXG3mATXCVhVwloBnuhhJOWPJfrN
        -----END RSA PRIVATE KEY-----
        """;
    private readonly static RSAEncryptionPadding _padding = RSAEncryptionPadding.OaepSHA512;
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
