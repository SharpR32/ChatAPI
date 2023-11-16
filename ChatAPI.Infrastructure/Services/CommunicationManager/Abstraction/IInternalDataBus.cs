namespace ChatAPI.Infrastructure.Services.CommunicationManager.Abstraction
{
    public interface IInternalDataBus
    {
        void AddConnection(Guid userId, string connectionId);
        void RemoveConnection(string connectionId);
    }
}