namespace ChatAPI.Domain.Entities;

public sealed class Message : Entity
{

    public required string Content { get; set; }
    public required Guid SenderId { get; set; }
    public required Guid ReceiverId { get; set; }
}
