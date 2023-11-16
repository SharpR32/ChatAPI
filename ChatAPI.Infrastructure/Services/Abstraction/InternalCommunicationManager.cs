namespace ChatAPI.Infrastructure.Services.Abstraction
{
    public interface IInternalCommunicationManager
    {
        public ValueTask SendData<TData>(TData data, Guid userId, string? methodName = null)
            where TData : class;
    }
}
