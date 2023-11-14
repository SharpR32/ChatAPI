using ChatAPI.Application.MediatorPlugins;
using FluentValidation;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace ChatAPI.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Transient);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipeline<,>));
        services.AddValidatorsFromAssemblyContaining(typeof(DependencyInjectionExtensions));

        return services;
    }
}
