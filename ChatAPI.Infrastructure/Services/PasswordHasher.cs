using System.Security.Cryptography;
using System.Text;

namespace ChatAPI.Infrastructure.Services;

public interface IPasswordHasher
{
    Span<byte> Hash(string userName, string password);
}

public sealed class PasswordHasher : IPasswordHasher
{
    private const string SALT = "RandomString";

    public Span<byte> Hash(string userName, string password)
    {
        int writtenBytes;
        Span<byte> hashData = stackalloc byte[userName.Length + password.Length + SALT.Length];

        writtenBytes = Encoding.UTF8.GetBytes(userName, hashData);
        writtenBytes += Encoding.UTF8.GetBytes(password, hashData[(writtenBytes - 1)..]);
        Encoding.UTF8.GetBytes(SALT, hashData[(writtenBytes - 1)..]);

        return SHA256.HashData(hashData);
    }
}
