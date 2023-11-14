using Microsoft.Extensions.Primitives;

namespace ChatAPI.Infrastructure.Services.Abstraction;

public interface ITokenManager
{
    ValueTask<string> GenerateTokenAsync(IReadOnlyDictionary<string, StringValues> claims);
    ValueTask<IReadOnlyDictionary<string, StringValues>> ValidateTokenAsync(string token);
}

public static class TokenConstants
{
    public const string ID = "id";
    public const string DISPLAY_NAME = "displayName";
}