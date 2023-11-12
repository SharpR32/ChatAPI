using ChatAPI.Domain.Entities;
using System.Collections.Immutable;

namespace ChatAPI.Infrastructure.Services.Abstraction;

public interface IMessageRepository
{
    ValueTask<ImmutableArray<Message>> GetMessagesAsync(
        RoomIdentifier identifier,
        DateTimeOffset since,
        CancellationToken cancellationToken,
        int count = 20);
    ValueTask<Guid> SendMessage(Guid receiverId, string content, CancellationToken cancellationToken);
}

public readonly record struct RoomIdentifier(params string[] Participants);
