using ChatAPI.Domain.Events;
using ChatAPI.Infrastructure.Services;
using ChatAPI.Infrastructure.Services.Abstraction;
using ChatAPI.Infrastructure.Services.CommunicationManager;
using ChatAPI.Infrastructure.Services.CommunicationManager.Abstraction;
using ChatAPI.Infrastructure.Services.CommunicationManager.Consumers;
using ChatAPI.Infrastructure.Services.CurrentUser;
using ChatAPI.Infrastructure.Services.TokenHandler;
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
            .AddSingleton<ITokenManager, TokenHandler>()
            .AddCurrentUserProvider()
            .AddSingleton<DataBus>()
            .AddSingleton<IInternalCommunicationManager>(sp => sp.GetRequiredService<DataBus>())
            .AddSingleton<IInternalDataBus>(sp => sp.GetRequiredService<DataBus>())
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

                    rabbitConfig.AddPublishMessageTypesFromNamespaceContaining<MessageNotification>();

                    rabbitConfig.AutoDelete = false;
                    rabbitConfig.AutoStart = true;
                    rabbitConfig.ConcurrentMessageLimit = config.GetValue<int>("ConcurrentMessageLimit");

                    rabbitConfig.ConfigureEndpoints(context);

                });

                config.AddConsumer<MessageNotificationConsumer>();
            });
        return services;
    }
}
