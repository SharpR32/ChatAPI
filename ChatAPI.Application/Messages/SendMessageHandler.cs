using ChatAPI.Application.Common;
using ChatAPI.Domain.Events;
using ChatAPI.Infrastructure.Services.Abstraction;
using MassTransit;

namespace ChatAPI.Application.Messages;

public sealed record SendMessage(Guid ReceiverId, string Content) : IWrappedRequest<SendMessageResult>;

public sealed class SendMessageHandler : IWrappedRequestHandler<SendMessage, SendMessageResult>
{
    private readonly IMessageRepository _repository;
    private readonly ISendEndpointProvider _sendEndpointProvider;
    private readonly ICurrentUser _currentUser;

    public SendMessageHandler(
        IMessageRepository repository,
        ISendEndpointProvider sendEndpointProvider,
        ICurrentUser currentUser)
    {
        _repository = repository;
        this._sendEndpointProvider = sendEndpointProvider;
        _currentUser = currentUser;
    }

    public async ValueTask<Result<SendMessageResult>> Handle(SendMessage request, CancellationToken cancellationToken)
    {
        Guid messageId = await _repository.SendMessage(request.ReceiverId, request.Content, cancellationToken);
        // Time of creation might be a bit off, but still within JS error
        // TODO: Refactor to TimeSampler
        await _sendEndpointProvider.Send(new MessageNotification(_currentUser.Id, request.Content, DateTimeOffset.UtcNow));

        return Result.FromData(new SendMessageResult(messageId));
    }
}

public sealed record SendMessageResult(Guid MessageId);