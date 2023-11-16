using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ChatAPI.Infrastructure.Services.CassandraDbProvider;

public static class CassandraDbProvider
{
    public static IServiceCollection AddCassandraDb(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CassandraOptions>(options => configuration.GetSection("CassandraDb").Bind(options, opt => opt.BindNonPublicProperties = true));
        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<CassandraOptions>>()
                .Value;

            var cluster = Cassandra.Cluster.Builder()
                .AddContactPoints(options.ContactPoints ?? throw new InvalidDataException("CassandraDb contact points were not provided"))
                .Build();

            var session = cluster.Connect("chat");

            return session;
        });
        services.AddScoped<ICassandraQueryProvider, DbQueryProvider>();


        return services;
    }
}

public class CassandraOptions
{
    public string[] ContactPoints { get; private set; } = null!;


}