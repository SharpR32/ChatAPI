namespace ChatAPI.Infrastructure.Services.CurrentUser.Abstractions;

public interface ICurrentUserProvider
{
    public void SetUserData(IReadOnlyDictionary<string, string[]> data);
}
