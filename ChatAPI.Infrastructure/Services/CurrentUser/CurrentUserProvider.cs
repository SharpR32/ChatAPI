using ChatAPI.Infrastructure.Services.Abstraction;
using ChatAPI.Infrastructure.Services.CurrentUser.Abstractions;
using ChatAPI.Infrastructure.Services.CurrentUser.Exceptions;
using Microsoft.Extensions.Primitives;
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

    public string? DisplayName => GetDataSafely(dict => dict.GetValueOrDefault(DISPLAY_NAME))?.FirstOrDefault()?.ToString();

    public bool Initiated => _data is not null;

    private IReadOnlyDictionary<string, string[]>? _data;

    private TResult GetDataSafely<TResult>(Func<IReadOnlyDictionary<string, string[]>, TResult> accessor)
    {
        if (_data is null)
            throw new UserNotInitialisedException();
        return accessor(_data);
    }

    public void SetUserData(IReadOnlyDictionary<string, string[]> data)
    {
        this._data = data;
    }
}

