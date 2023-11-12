using ChatAPI.Infrastructure.Services;
using ChatAPI.Infrastructure.Services.Abstraction;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatAPI.Infrastructure;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPasswordHasher(configuration)
            .AddScoped<IUserRepository, Services.UserRepositories.MockRepository>()
            .AddScoped<IMessageRepository, Services.MessageRespositories.MockRepository>()
            .AddMassTransit(config =>
            {
                config.UsingRabbitMq((context, rabbitConfig) =>
                {
                    IConfigurationSection config = context.GetRequiredService<IConfiguration>()
                        .GetSection("EventPropagation:RabbitMQ");
                    rabbitConfig.Host(config.GetValue<string>("Host"), hostConfig =>
                    {
                        hostConfig.Username(config.GetValue<string>("UserName"));
                        hostConfig.Password(config.GetValue<string>("Password"));
                        hostConfig.ConfigureBatchPublish(publishConfig =>
                        {
                            publishConfig.Enabled = true;
                            publishConfig.MessageLimit = 40;
                            publishConfig.Timeout = TimeSpan.FromMilliseconds(8);
                        });
                    });

                    rabbitConfig.AutoDelete = false;
                    rabbitConfig.AutoStart = true;
                    rabbitConfig.ConcurrentMessageLimit = config.GetValue<int>("ConcurrentMessageLimit");

                    rabbitConfig.ConfigureEndpoints(context);
                });
            });
        return services;
    }
}
