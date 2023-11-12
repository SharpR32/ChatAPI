namespace ChatAPI.Domain.Entities;

public sealed class User : Entity
{
    public required string? DisplayName { get; set; }
}
