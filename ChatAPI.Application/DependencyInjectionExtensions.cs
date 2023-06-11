using ChatAPI.Application.Common.Services;
using ChatAPI.Application.MediatorPlugins;
using FluentValidation;
using Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace ChatAPI.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Transient);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipeline<,>));
        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjectionExtensions));

        // Transient poniewaz Connection to tylko fasada dla prawdziwego polaczenia,
        // ktore jest zarzadzane przez ADO.NET na podstawie ConnectionStringa.
        services.AddTransient(sp =>
        {
            IConfiguration config = sp.GetRequiredService<IConfiguration>();
            NpgsqlConnection connection = new(config.GetConnectionString("Default"));
            return connection;
        });

        services.AddSingleton<ITokenManager, TokenManager>();
        return services;
    }
}
