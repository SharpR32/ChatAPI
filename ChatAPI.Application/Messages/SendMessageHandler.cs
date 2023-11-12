using ChatAPI.Application.Common;
using ChatAPI.Infrastructure.Services.Abstraction;

namespace ChatAPI.Application.Messages;

public sealed record SendMessage(Guid ReceiverId, string Content) : IWrappedRequest<SendMessageResult>;

public sealed class SendMessageHandler : IWrappedRequestHandler<SendMessage, SendMessageResult>
{
    private readonly IMessageRepository _repository;

    public SendMessageHandler(IMessageRepository repository)
    {
        _repository = repository;
    }

    public async ValueTask<Result<SendMessageResult>> Handle(SendMessage request, CancellationToken cancellationToken)
    {
        Guid messageId = await _repository.SendMessage(request.ReceiverId, request.Content, cancellationToken);
        return Result.FromData(new SendMessageResult(messageId));
    }
}

public sealed record SendMessageResult(Guid MessageId);