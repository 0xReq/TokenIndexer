using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Messaging.ServiceBus;

namespace TokenIndexer.Domain.Services.ServiceBus;

public class MessageService<T> : IMessageService<T> where T : class
{
    private readonly ServiceBusSender _serviceBusSender;

    public MessageService(ServiceBusClient serviceBusClient, string path)
    {
        _serviceBusSender = serviceBusClient.CreateSender(path);
    }
        
    /// <summary>
    /// Maximum message size: 256 KB
    /// </summary>
    /// <param name="item">The message to be sent</param>
    /// <param name="subject">The subject of the message</param>
    /// <returns></returns>
    public async Task SendAsync(T item, string subject)
    {
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
            
        var messageBody = JsonSerializer.SerializeToUtf8Bytes(item, options);

        var message = new ServiceBusMessage(messageBody)
        {
            Subject = subject,
            ContentType = "application/json"
        };

        try
        {
            await _serviceBusSender.SendMessageAsync(message);
        }
        catch (Exception)
        {
            throw;
        }
    }
}