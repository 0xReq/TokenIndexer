namespace TokenIndexer.Domain.Services.ServiceBus;

public interface IMessageService<T>
{
    Task SendAsync(T item, string subject);
}