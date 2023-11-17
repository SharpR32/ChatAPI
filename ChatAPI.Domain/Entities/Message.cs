namespace ChatAPI.Domain.Entities;

public sealed class Message : Entity
{

    public string Content { get; set; }
    public Guid SenderId { get; set; }
    public Guid ReceiverId { get; set; }
    public string RoomIdentifier { get; set; }
}
