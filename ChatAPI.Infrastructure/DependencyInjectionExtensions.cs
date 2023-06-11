using ChatAPI.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ChatAPI.Infrastructure;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IUserRepository, PsqlUserRepository>();
        return services;
    }
}
