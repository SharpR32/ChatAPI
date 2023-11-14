using ChatAPI.Infrastructure.Services.Abstraction;
using Microsoft.Extensions.Primitives;

namespace ChatAPI.Infrastructure.Services.CurrentUser.Abstractions;

public interface ICurrentUserProvider
{
    public void SetUserData(IReadOnlyDictionary<string, StringValues> data);
}
