namespace ChatAPI.Infrastructure.Services.Abstraction;

public interface ICurrentUser
{
    public Guid Id { get; }
    public string? DisplayName { get; }
}
