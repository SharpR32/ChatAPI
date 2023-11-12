using ChatAPI.Domain.Entities;
using ChatAPI.Infrastructure.Services.Abstraction;

namespace ChatAPI.Infrastructure.Services.UserRepositories;

public sealed class MockRepository : IUserRepository
{
    public ValueTask<User> GetUserMetadataAsync(Guid userId, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(new User
        {
            DisplayName = "TEST"
        });
    }

    public ValueTask<bool> RegisterUserAsync(string userName, string password, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(true);
    }

    public ValueTask<LoginResult> TryLoginAsync(string userName, string password, string ip, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(new LoginResult(true, 1));
    }

    public ValueTask<bool> UserExistsAsync(string userName, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(false);
    }

    public ValueTask<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(false);
    }
}
