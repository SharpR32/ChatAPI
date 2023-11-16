using ChatAPI.Infrastructure.Services.Abstraction;
using ChatAPI.Infrastructure.Services.CommunicationManager.Abstraction;
using ChatAPI.SignalRHubs;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using System.Text.Json;

namespace ChatAPI.Infrastructure.Services.CommunicationManager;

public class DataBus(IHubContext<NotificationHub> hub) : IInternalCommunicationManager, IInternalDataBus
{
    private readonly List<ConnectionData> _connections = new();

    public async ValueTask SendData<TData>(TData data, Guid userId, string? methodName = null)
        where TData : class
    {
        var validConnections = _connections.Where(data => data.UserId == userId)
            .Select(x => x.ConnectionId)
            .ToArray();

        Debug.WriteLine($"Sending {JsonSerializer.Serialize(data)} to [{string.Join(", ", validConnections)}]");

        var sendingProxy = hub.Clients.Clients(validConnections);
        await sendingProxy.SendCoreAsync(methodName ?? data.GetType().Name, [data])
            .ConfigureAwait(false);
    }

    public void AddConnection(Guid userId, string connectionId)
    {
        lock (_connections)
        {
            _connections.Add(new(userId, connectionId));
        }
    }

    public void RemoveConnection(string connectionId)
    {
        lock (_connections)
        {
            _connections.RemoveAt(_connections.FindIndex(data => data.ConnectionId == connectionId));
        }
    }


    private readonly record struct ConnectionData(Guid UserId, string ConnectionId);
}
