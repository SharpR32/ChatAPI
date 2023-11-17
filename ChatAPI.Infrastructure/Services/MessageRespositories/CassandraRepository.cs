using Cassandra.Data.Linq;
using ChatAPI.Domain.Entities;
using ChatAPI.Infrastructure.Services.Abstraction;
using ChatAPI.Infrastructure.Services.CassandraDbProvider;

namespace ChatAPI.Infrastructure.Services.MessageRespositories;

public class CassandraRepository(ICassandraQueryProvider queryProvider, ICurrentUser currentUser) : IMessageRepository
{
    public async ValueTask<IEnumerable<Message>> GetMessagesAsync(RoomIdentifier identifier, DateTimeOffset since, CancellationToken cancellationToken, int count = 20)
    {

        var result = await queryProvider.Query<Message>()
            .Where(msg => msg.RoomIdentifier == identifier.Identifier)
            .Take(count)
            .ExecuteAsync();

        return result;
    }

    public async ValueTask<Guid> SendMessage(Guid receiverId, string content, CancellationToken cancellationToken)
    {
        var identifier = new RoomIdentifier([receiverId, currentUser.Id]);

        var message = new Message()
        {
            Id = Guid.NewGuid(),
            CreatedTime = DateTimeOffset.UtcNow,
            Content = content,
            RoomIdentifier = identifier.Identifier,
            ReceiverId = receiverId,
            SenderId = currentUser.Id,
        };

        await queryProvider.Insert(message);

        return message.Id;
    }
}
