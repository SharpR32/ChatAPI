using ChatAPI.Infrastructure.Exceptions;
using ChatAPI.Infrastructure.Services.Abstraction;
using Dapper;
using Npgsql;
using static ChatAPI.Infrastructure.Services.UserRepositories.DbConstants;

namespace ChatAPI.Infrastructure.Services.UserRepositories;

/// Contains constants used in db queries
/// </summary>
/// <remarks>
/// Constants are used for string precompilation
/// </remarks>
public static class DbConstants
{
    public const string USER_TABLE = "public.users";
    public const string LOGIN_TABLE = "public.login_log";

    public static class User
    {
        public const string Id = "id";
        public const string UserName = "username";
        public const string PasswordHash = "password";
        public const string LoginCount = "login_count";
    }

    public static class LoginLog
    {
        public const string UserId = "user_id";
        public const string IP = "ip";
    }
}
public sealed class PsqlRepository : IUserRepository
{
    private readonly IPasswordHasher _hasher;
    private readonly NpgsqlConnection _connection;

    public PsqlRepository(
        IPasswordHasher hasher,
        NpgsqlConnection connection)
    {
        _hasher = hasher;
        _connection = connection;
    }

    public async ValueTask<LoginResult> TryLoginAsync(string userName, string password, string ip, CancellationToken cancellationToken)
    {
        if (_connection.State == System.Data.ConnectionState.Closed)
            await _connection.OpenAsync(cancellationToken);
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
                $"INSERT INTO {LOGIN_TABLE}({LoginLog.UserId}, {LoginLog.IP}) VALUES (@UserId, @Ip)",
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

    public async ValueTask<bool> RegisterUserAsync(string userName, string password, CancellationToken cancellationToken)
    {
        var passwordHash = _hasher.Hash(userName, password).ToArray();
        var changedRecords = await _connection.ExecuteAsync(
            $"INSERT INTO {USER_TABLE}({User.UserName}, {User.PasswordHash}) VALUES (@UserName, @PasswordHash)",
            new { UserName = userName, PasswordHash = passwordHash });

        return changedRecords > 0;
    }

    private async ValueTask<long> GetUserIdAsync(string userName)
    {
        IEnumerable<long> results = await _connection.QueryAsync<long>(
            $"SELECT {User.Id} FROM {USER_TABLE} WHERE username = @UserName LIMIT 1",
            new { UserName = userName });

        return results.FirstOrDefault();
    }

    public async ValueTask<bool> UserExistsAsync(string userName, CancellationToken cancellationToken)
    {
        var id = await GetUserIdAsync(userName);
        return id > 0;
    }

    public ValueTask<Domain.Entities.User> GetUserMetadataAsync(Guid userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public ValueTask<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}