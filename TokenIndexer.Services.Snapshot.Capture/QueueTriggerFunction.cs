using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.ServiceBus;
using Microsoft.Extensions.Logging;
using TokenIndexer.Domain.Events;
using TokenIndexer.Domain.Extensions;
using TokenIndexer.Domain.Services.Capture;

namespace TokenIndexer.Services.Snapshot.Capture;

public class QueueTriggerFunction
{
    private ILogger<QueueTriggerFunction> _logger;
    private ICaptureService _captureService;
    
    public QueueTriggerFunction(ILogger<QueueTriggerFunction> logger, ICaptureService captureService)
    {
        _logger = logger;
        _captureService = captureService;
    }
    
    [FunctionName(nameof(QueueTriggerFunction))]
    public async Task RunAsync([ServiceBusTrigger("%WalletQueue%", Connection = "SbConnectionString", AutoCompleteMessages = false)] 
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageReceiver,
        CancellationToken cancellationToken)
    {
        try
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    $"{nameof(QueueTriggerFunction)} function started processing MessageId: {message.MessageId}");
                
                var eventNotification = JsonSerializer.Deserialize<Event>(Encoding.UTF8.GetString(message.Body));

                if (!eventNotification.EventMessage.IsNullOrEmpty())
                {
                    await _captureService.ProcessMessage(eventNotification.EventMessage, eventNotification.EventType);

                    await messageReceiver.CompleteMessageAsync(message, cancellationToken);

                    _logger.LogInformation(
                        $"{nameof(QueueTriggerFunction)} function finished processing MessageId: {message.MessageId}");
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception,
                $"{nameof(QueueTriggerFunction)} Abandoned MessageId: {message.MessageId} due to unhandled exception");
            await messageReceiver.AbandonMessageAsync(message, cancellationToken: cancellationToken);
        }
    }
}