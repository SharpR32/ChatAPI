using Npgsql;

namespace ChatAPI.Persistance;

public abstract class Repository<T>
{
    protected abstract string TableName { get; }
    private readonly Lazy<string> _baseSelectQuery;
    protected readonly NpgsqlConnection _connection;

    protected Repository(NpgsqlConnection connection)
    {
        _baseSelectQuery = new(() => $"SELECT * FROM {TableName}");
        _connection = connection;
    }


    protected async Task EnsureOpenConnection()
    {
        switch (_connection.State)
        {
            case System.Data.ConnectionState.Closed:
                await _connection.OpenAsync();
                break;

            case System.Data.ConnectionState.Broken:
                await _connection.CloseAsync();
                await _connection.OpenAsync();
                break;
        }
    }
}
