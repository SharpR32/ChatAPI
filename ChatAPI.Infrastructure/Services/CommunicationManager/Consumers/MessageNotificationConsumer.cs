using ChatAPI.Domain.Events;
using ChatAPI.Infrastructure.Services.Abstraction;
using MassTransit;

namespace ChatAPI.Infrastructure.Services.CommunicationManager.Consumers;

public class MessageNotificationConsumer(IInternalCommunicationManager communicationManager) : IConsumer<MessageNotification>
{
    public async Task Consume(ConsumeContext<MessageNotification> context)
    {
        var (senderId, receiverId, content, createdTime) = context.Message;

        await communicationManager.SendData(new ReceivedMessage(senderId, content, createdTime), receiverId);
    }
}

public record ReceivedMessage(Guid SenderId, string Content, DateTimeOffset CreatedTime);