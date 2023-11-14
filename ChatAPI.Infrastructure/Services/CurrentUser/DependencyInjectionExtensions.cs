using ChatAPI.Infrastructure.Services.Abstraction;
using ChatAPI.Infrastructure.Services.CurrentUser.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ChatAPI.Infrastructure.Services.CurrentUser;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddCurrentUserProvider(this IServiceCollection services)
    {
        services.AddScoped<CurrentUserProvider>();
        services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<CurrentUserProvider>());
        services.AddScoped<ICurrentUserProvider>(sp => sp.GetRequiredService<CurrentUserProvider>());
        services.AddScoped<CurrentUserMiddleware>();

        return services;
    }

    public static IApplicationBuilder MapCurrentUser(this IApplicationBuilder app)
    {
        app.UseMiddleware<CurrentUserMiddleware>();
        return app;
    }
}