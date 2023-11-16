using Cassandra;
using Cassandra.Data.Linq;
using Cassandra.Mapping;
using ChatAPI.Domain.Entities;
using MassTransit.Internals;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ChatAPI.Infrastructure.Services.CassandraDbProvider;

public interface ICassandraQueryProvider
{
    ValueTask Insert<TEntity>(TEntity data) where TEntity : Entity;
    ValueTask InsertMany<TEntity>(IEnumerable<TEntity> dataStream) where TEntity : Entity;
    CqlQuery<TEntity> Query<TEntity>() where TEntity : Entity;
}
public partial class DbQueryProvider(ISession session) : ICassandraQueryProvider
{
    public CqlQuery<TEntity> Query<TEntity>()
        where TEntity : Entity
    {
        // Add auto initialisation
        Table<TEntity> table = CreateTable<TEntity>();

        return table;
    }

    private Table<TEntity> CreateTable<TEntity>() where TEntity : Entity
    {
        return new Table<TEntity>(session, _mappings.Value);
    }

    public async ValueTask Insert<TEntity>(TEntity data)
        where TEntity : Entity
    {
        await CreateTable<TEntity>()
            .Insert(data)
            .ExecuteAsync();
    }

    public async ValueTask InsertMany<TEntity>(IEnumerable<TEntity> dataStream)
        where TEntity : Entity
    {
        var batch = new BatchStatement();

        var table = CreateTable<TEntity>();
        foreach (var data in dataStream)
            batch.Add(table.Insert(data));

        await session.ExecuteAsync(batch);
    }


    private Lazy<MappingConfiguration> _mappings = new(() =>
    {
        var config = new MappingConfiguration();

        ITypeDefinition[] definitions = [
            new Map<User>()
                .TableName("users")
                .MapColumns(),
            new Map<Message>()
                .TableName("messages")
                .MapColumns()
        ];

        config.Define(definitions);

        return config;
    });
}


internal static partial class DbHelper
{

    public static Map<T> MapColumns<T>(this Map<T> map)
    {
        var entityType = typeof(T);
        var mapperType = typeof(Map<T>);
        var columnMapperType = typeof(ColumnMap);

        var properties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(x => (x.Name, x.PropertyType))
            .ToArray();

        var accessorParameterExp = Expression.Parameter(entityType);
        var mapperParameterExp = Expression.Parameter(columnMapperType);

        foreach (var (propName, propType) in properties)
        {
            var accessor = Expression.PropertyOrField(accessorParameterExp, propName);
            var accessorLambda = Expression.Lambda(accessor, accessorParameterExp);

            Action<ColumnMap> namingExp = (ColumnMap map) => map.WithName(ToSnakeCase(propName));
            var mappingCallExp = Expression.Call(
                Expression.Constant(map),
                nameof(Map<T>.Column),
                [propType],
                Expression.Constant(accessorLambda),
                Expression.Constant(namingExp));

            var compiledMapping = Expression.Lambda<Func<Map<T>>>(mappingCallExp).CompileFast();
            compiledMapping();
        }

        return map;
    }

    private static string ToSnakeCase(string value)
    {
        return GetNamingRegex()
            .Replace(value, (match) => match.Index == 0 ? match.Value.ToLower() : $"_{match.Value.ToLower()}");
    }

    [GeneratedRegex("([A-Z])")]
    private static partial Regex GetNamingRegex();
}