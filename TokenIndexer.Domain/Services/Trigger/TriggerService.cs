using System.Globalization;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using TokenIndexer.Domain.Entities.Enums;
using TokenIndexer.Domain.Events;
using TokenIndexer.Domain.Helpers;
using TokenIndexer.Domain.Services.ServiceBus;
using TokenIndexer.Domain.Services.Wallet;

namespace TokenIndexer.Domain.Services.Trigger;

public class TriggerService : ITriggerService
{
    private readonly ILogger<TriggerService> _logger;
    private readonly IMessageService<Event> _messageService;
    private readonly TelemetryClient _telemetryClient;
    private readonly IWalletService _walletService;
    private const int QueueChunkCount = 5;

    public TriggerService(ILogger<TriggerService> logger,
        IMessageService<Event> messageService, IWalletService walletService, TelemetryClient telemetryClient)
    {
        _logger = logger;
        _messageService = messageService;
        _walletService = walletService;
        _telemetryClient = telemetryClient;
    }
    
    public async Task Run(EventType eventType)
    {
        long totalPages;
        var page = 0;
        var pageSize = 20;

        do
        {
            try
            {
                page++;
                (var walletItems, totalPages) = await _walletService.GetPaginated(page, pageSize);
                
                var wallets = walletItems.Select(_ => _.Wallet).ToList();
                
                await SendWalletChunksToQueue(wallets, eventType);
                
                TrackRequest("wallets_tracked", wallets.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    $"{nameof(TriggerService)} Failed getting wallets");
                throw;
            }
        } while (page < totalPages);
    }
    
    private async Task SendWalletChunksToQueue(List<string> wallets, EventType eventType)
    {
        try
        {
            var walletChunks = ListHelpers.GetChunks(wallets, QueueChunkCount);

            foreach (var walletChunk in walletChunks)
            {
                var walletEventMessage = new Event(Guid.NewGuid().ToString("D", CultureInfo.InvariantCulture),
                    "Wallets", nameof(TriggerService), eventType, DateTime.UtcNow, walletChunk);

                await _messageService.SendAsync(walletEventMessage, nameof(TriggerService));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"{nameof(TriggerService)} Failed sending wallets to queue due to a unhandled exception");
        }
    }
    
    private void TrackRequest(string action, int count)
    {
        _telemetryClient
            .GetMetric(action)
            .TrackValue(count);
    }
}