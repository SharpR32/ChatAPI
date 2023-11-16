using Cassandra.Data.Linq;
using ChatAPI.Domain.Entities;
using ChatAPI.Infrastructure.Services.Abstraction;
using ChatAPI.Infrastructure.Services.CassandraDbProvider;
using Microsoft.Extensions.Logging;

namespace ChatAPI.Infrastructure.Services.UserRepositories;

public class CassandraRepository(
    ICassandraQueryProvider queryProvider,
    IPasswordHasher passwordHasher,
    ILogger<CassandraRepository> logger) : IUserRepository
{
    public async ValueTask<User> GetUserMetadataAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await queryProvider.Query<User>()
            .Where(x => x.Id == userId)
            .FirstOrDefault()
            .ExecuteAsync();
    }

    public async ValueTask<bool> RegisterUserAsync(string userName, string password, CancellationToken cancellationToken)
    {
        if (await UserExistsAsync(userName, cancellationToken))
            return false;

        var passwordHash = passwordHasher.Hash(userName, password).ToArray();

        await queryProvider.Insert(new User
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            CreatedTime = DateTime.UtcNow,
            DisplayName = userName,
            PasswordHash = passwordHash
        });

        return true;
    }

    public async ValueTask<LoginResult> TryLoginAsync(string userName, string password, string ip, CancellationToken cancellationToken)
    {
        var result = await queryProvider.Query<User>()
            .Where(x => x.UserName == userName)
            .Select(x => new { x.PasswordHash, x.Id })
            .FirstOrDefault()
            .ExecuteAsync();

        if (result == null)
            return new(false, default);

        var validationResult = passwordHasher.Hash(userName, password).SequenceEqual(result.PasswordHash);
        return new(validationResult, result.Id);
    }

    public async ValueTask<bool> UserExistsAsync(string userName, CancellationToken cancellationToken)
    {
        var result = await queryProvider.Query<User>()
            .Where(x => x.UserName == userName)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefault()
            .ExecuteAsync();

        return result is not null;

    }

    public async ValueTask<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await queryProvider.Query<User>()
            .Where(x => x.Id == userId)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefault()
            .ExecuteAsync();

        return result is not null;
    }
}