using ChatAPI.Infrastructure.Services.Abstraction;
using ChatAPI.Infrastructure.Services.CommunicationManager.Abstraction;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ChatAPI.SignalRHubs;

using Timer = System.Timers.Timer;

public class NotificationHub(
    IInternalDataBus bus,
    IConfiguration configuration,
    ITokenManager tokenManager,
    ILogger<NotificationHub> logger) : Hub<NotificationHub>
{
    private const string ABORT_TIMER = "_at";
    public override Task OnConnectedAsync()
    {
        var timeout = configuration.GetValue<TimeSpan>("ConnectionTimeout");
        if (timeout == default)
        {
            timeout = TimeSpan.FromSeconds(20);
        }

        var connectionKiller = new Timer(timeout);
        connectionKiller.Elapsed += (_, _) => KillConnection(connectionKiller);

        this.Context.Items.Add(ABORT_TIMER, connectionKiller);
        connectionKiller.Start();

        return Task.CompletedTask;
    }

    public async ValueTask Authorize(string token)
    {
        try
        {
            var data = await tokenManager.ValidateTokenAsync(token);
            if (data == null
                || data.TryGetValue(TokenConstants.ID, out var idString)
                || Guid.TryParse(idString?.FirstOrDefault() ?? string.Empty, out var id))
                throw new AuthorizationException();

            bus.AddConnection(id, Context.ConnectionId);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while authorizing SignalR connection");
            var connectionKiller = (Timer)this.Context.Items[ABORT_TIMER];
            KillConnection(connectionKiller);
        }

    }

    private void KillConnection(Timer connectionKiller)
    {
        this.Context.Abort();
        connectionKiller.Stop();
        connectionKiller.Dispose();
    }
}

file class AuthorizationException : Exception { }
