using ChatAPI.Application.Common;
using ChatAPI.Domain.Events;
using ChatAPI.Infrastructure.Services.Abstraction;
using MassTransit;

namespace ChatAPI.Application.Messages;

public sealed record SendMessage(Guid ReceiverId, string Content) : IWrappedRequest<SendMessageResult>;

public sealed class SendMessageHandler(
        IMessageRepository repository,
        IPublishEndpoint eventSender,
        ICurrentUser currentUser) : IWrappedRequestHandler<SendMessage, SendMessageResult>
{
    public async ValueTask<Result<SendMessageResult>> Handle(SendMessage request, CancellationToken cancellationToken)
    {
        Guid messageId = await repository.SendMessage(request.ReceiverId, request.Content, cancellationToken);
        // Time of creation might be a bit off, but still within JS error
        // TODO: Refactor to TimeSampler
        await eventSender.Publish(new MessageNotification(currentUser.Id, request.ReceiverId, request.Content, DateTimeOffset.UtcNow));

        return Result.FromData(new SendMessageResult(messageId));
    }
}

public sealed record SendMessageResult(Guid MessageId);