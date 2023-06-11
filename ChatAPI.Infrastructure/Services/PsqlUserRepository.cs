using ChatAPI.Infrastructure.Exceptions;
using Dapper;
using Npgsql;

using static ChatAPI.Infrastructure.DbConstants;

namespace ChatAPI.Infrastructure.Services;

public interface IUserRepository
{
    Task<bool> RegisterUserAsync(string userName, string password, CancellationToken cancellationToken);
    Task<LoginResult> TryLoginAsync(string userName, string password, string ip, CancellationToken cancellationToken);
    Task<bool> UserExistsAsync(string userName, CancellationToken cancellationToken);
}

public record LoginResult(bool Success, long Id);

public sealed class PsqlUserRepository : IUserRepository
{
    private readonly IPasswordHasher _hasher;
    private readonly NpgsqlConnection _connection;

    public PsqlUserRepository(
        IPasswordHasher hasher,
        NpgsqlConnection connection)
    {
        _hasher = hasher;
        _connection = connection;
    }

    public async Task<LoginResult> TryLoginAsync(string userName, string password, string ip, CancellationToken cancellationToken)
    {
        await using NpgsqlTransaction transaction = await _connection.BeginTransactionAsync(cancellationToken);

        var id = await GetUserIdAsync(userName);
        var saved = true;

        if (id == default)
            throw new UserDoesntExistException();

        try
        {
            await foreach (var success in LogLoginAsync())
            {
                saved = success;
                if (!saved)
                    break;
            }
        }
        catch
        {
            saved = false;
        }

        if (!saved)
            await transaction.RollbackAsync(default);

        return new(saved, id);

        async IAsyncEnumerable<bool> LogLoginAsync()
        {
            // Adding login log
            yield return 0 < await _connection.ExecuteAsync(
                $"INERT INTO {LOGIN_TABLE}({LoginLog.UserId}, {LoginLog.IP}) VALUES (@UserId, @Ip)",
                new
                {
                    UserId = id,
                    Ip = ip
                });

            // Bumping login count
            yield return 0 < await _connection.ExecuteAsync(
                $"UPDATE {USER_TABLE} SET {User.LoginCount} = {User.LoginCount} + 1 WHERE {User.Id} = @Id",
                new
                {
                    Id = id
                });

            await transaction.CommitAsync(cancellationToken);
        }
    }

    public async Task<bool> RegisterUserAsync(string userName, string password, CancellationToken cancellationToken)
    {
        var passwordHash = _hasher.Hash(userName, password).ToArray();
        var changedRecords = await _connection.ExecuteAsync(
            $"INSERT INTO {USER_TABLE}({User.UserName}, {User.PasswordHash}) VALUES (@UserName, @PasswordHash)",
            new { UserName = userName, PasswordHash = passwordHash });

        return changedRecords > 0;
    }

    private async Task<long> GetUserIdAsync(string userName)
    {
        IEnumerable<long> results = await _connection.QueryAsync<long>(
            $"SELECT {User.Id} FROM {USER_TABLE} WHERE username = @UserName LIMIT 1",
            new { UserName = userName });

        return results.FirstOrDefault();
    }

    public async Task<bool> UserExistsAsync(string userName, CancellationToken cancellationToken)
    {
        var id = await GetUserIdAsync(userName);
        return id > 0;
    }
}