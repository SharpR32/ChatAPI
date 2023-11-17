namespace ChatAPI.Domain.Entities;

public sealed class User : Entity
{
    public string DisplayName { get; set; }
    public string UserName { get; set; }
    public byte[] PasswordHash { get; set; }
}
