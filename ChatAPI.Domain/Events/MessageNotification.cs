namespace ChatAPI.Domain.Events;

public record MessageNotification(
    Guid SenderId,
    string Content,
    DateTimeOffset CreatedTime);
