namespace ChatAPI.Domain.Entities;

public abstract class Entity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedTime { get; private set; } = DateTimeOffset.UtcNow;
}