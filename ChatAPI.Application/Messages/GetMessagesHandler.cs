using ChatAPI.Application.Common;
using ChatAPI.Domain.Entities;
using ChatAPI.Infrastructure.Services.Abstraction;
using System.Collections.Immutable;

namespace ChatAPI.Application.Messages;

public sealed record GetMessages(Guid ParticipantId, DateTimeOffset? Since) : IWrappedRequest<MessageViewModel[]>;
public sealed record MessageViewModel(Guid Id, string Content, DateTimeOffset CreatedTime);
public sealed class GetMessagesHandler : IWrappedRequestHandler<GetMessages, MessageViewModel[]>
{
    private readonly IMessageRepository _repository;
    private readonly ICurrentUser _currentUser;

    public GetMessagesHandler(IMessageRepository repository, ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async ValueTask<Result<MessageViewModel[]>> Handle(GetMessages request, CancellationToken cancellationToken)
    {
        ImmutableArray<Message> messages = await _repository.GetMessagesAsync(
            new RoomIdentifier(request.ParticipantId, _currentUser.Id),
            request.Since ?? DateTimeOffset.UtcNow,
            cancellationToken);

        MessageViewModel[] result = messages.Select(m => new MessageViewModel(m.Id, m.Content, m.CreatedTime))
            .ToArray();

        return Result.FromData(result);
    }
}
