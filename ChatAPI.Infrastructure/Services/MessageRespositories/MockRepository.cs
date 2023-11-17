using ChatAPI.Domain.Entities;
using ChatAPI.Infrastructure.Services.Abstraction;
using System.Collections.Immutable;

namespace ChatAPI.Infrastructure.Services.MessageRespositories;

public class MockRepository : IMessageRepository
{
    public ValueTask<IEnumerable<Message>> GetMessagesAsync(
        RoomIdentifier identifier,
        DateTimeOffset since,
        CancellationToken cancellationToken,
        int count = 20)
    {
        ImmutableArray<Message> result = new List<Message>()
        {
            new()
            {
                Content = "TEST 1",
                SenderId = new Guid(),
                ReceiverId = new Guid(),
            },
            new()
            {
                Content = "TEST 2",
                SenderId = new Guid(),
                ReceiverId = new Guid(),
            }
        }.OrderByDescending(x => x.CreatedTime)
        .ToImmutableArray();

        return ValueTask.FromResult(result.AsEnumerable());
    }

    public ValueTask<Guid> SendMessage(Guid receiverId, string content, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(Guid.NewGuid());
    }
}
