using ChatAPI.Domain.Entities;

namespace ChatAPI.Infrastructure.Services.Abstraction;

public interface IUserRepository
{
    ValueTask<bool> RegisterUserAsync(string userName, string password, CancellationToken cancellationToken);
    ValueTask<LoginResult> TryLoginAsync(string userName, string password, string ip, CancellationToken cancellationToken);
    ValueTask<bool> UserExistsAsync(string userName, CancellationToken cancellationToken);
    ValueTask<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken);
    ValueTask<User> GetUserMetadataAsync(Guid userId, CancellationToken cancellationToken);
}

public record LoginResult(bool Success, long Id);