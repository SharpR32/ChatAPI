using ChatAPI.Infrastructure.Services.Abstraction;
using ChatAPI.Infrastructure.Services.CurrentUser.Abstractions;
using ChatAPI.Infrastructure.Services.CurrentUser.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Globalization;

namespace ChatAPI.Infrastructure.Services.CurrentUser;
using static TokenConstants;
public class CurrentUserProvider : ICurrentUser, ICurrentUserProvider
{
    public Guid Id => GetDataSafely(dict =>
    {
        StringValues stringData = dict.GetValueOrDefault(ID);

        if (!Guid.TryParse(stringData, CultureInfo.InvariantCulture, out Guid result))
            throw new UnrecognisedUserIdException();

        return result;
    });

    public string? DisplayName => GetDataSafely(dict => dict.GetValueOrDefault(DISPLAY_NAME)).ToString();

    public bool Initiated => _data is not null;

    private IReadOnlyDictionary<string, StringValues>? _data;

    private TResult GetDataSafely<TResult>(Func<IReadOnlyDictionary<string, StringValues>, TResult> accessor)
    {
        if (_data is null)
            throw new UserNotInitialisedException();
        return accessor(_data);
    }

    public void SetUserData(IReadOnlyDictionary<string, StringValues> data)
    {
        this._data = data;
    }
}

public class CurrentUserMiddleware : IMiddleware
{
    private readonly ICurrentUserProvider _provider;
    private readonly ITokenManager _tokenManager;

    public CurrentUserMiddleware(
        ICurrentUserProvider provider,
        ITokenManager tokenManager)
    {
        _provider = provider;
        _tokenManager = tokenManager;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (context.Request.Headers.TryGetValue(HeaderNames.Authorization, out StringValues values))
        {
            IReadOnlyDictionary<string, StringValues> dataDictionary = await _tokenManager.ValidateTokenAsync(values!);
            _provider.SetUserData(dataDictionary);
        }

        await next(context);
    }
}
