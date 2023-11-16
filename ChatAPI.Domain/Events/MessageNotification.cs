namespace ChatAPI.Domain.Events;

public record MessageNotification(
    Guid SenderId,
    Guid ReceiverId,
    string Content,
    DateTimeOffset CreatedTime);
