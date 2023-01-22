using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TokenIndexer.Domain.Events;
using TokenIndexer.Domain.Services.ServiceBus;

namespace TokenIndexer.Domain.Initialization;

public static class ServiceBusInitExtension
{
    public static void AddServiceBus(this IServiceCollection services, IConfiguration configuration)
    {
        string sbConnectionString = configuration["SbConnectionString"] ?? throw new ArgumentNullException(nameof(sbConnectionString));
        string walletQueue = configuration["WalletQueue"] ?? throw new ArgumentNullException(nameof(walletQueue));

        services.TryAddSingleton(i =>
        {
            var options = new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpTcp,
                RetryOptions = new ServiceBusRetryOptions
                {
                    Mode = ServiceBusRetryMode.Fixed,
                    Delay = TimeSpan.FromSeconds(5),
                    MaxRetries = 3
                }
            };
            var serviceBusClient = new ServiceBusClient(sbConnectionString, options);
            return serviceBusClient;
        });
            
        services.AddSingleton<IMessageService<Event>>(s =>
            new MessageService<Event>(s.GetRequiredService<ServiceBusClient>(),
                walletQueue));
    }
}