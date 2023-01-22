using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using TokenIndexer.Domain.Entities.Enums;
using TokenIndexer.Domain.Services.Trigger;

namespace TokenIndexer.Services.Snapshot.Trigger;

public class DailyTriggerFunction
{
    private readonly ITriggerService _triggerService;
    private readonly ILogger<DailyTriggerFunction> _logger;
 
    public DailyTriggerFunction(ITriggerService triggerService, ILogger<DailyTriggerFunction> logger)
    {
        _triggerService = triggerService;
        _logger = logger;
    }
    
    [FunctionName(nameof(DailyTriggerFunction))]
    public async Task RunAsync([TimerTrigger("%DailyTriggerTime%")] TimerInfo myTimer, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"{nameof(DailyTriggerFunction)}: executed at: {DateTime.UtcNow}");
        
        try
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                await _triggerService.Run(EventType.DailySnapshot);
            }
            else
            {
                throw new OperationCanceledException();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning($"{nameof(DailyTriggerFunction)} host has requested to stop the function due to a deploy or a timeout");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"{nameof(DailyTriggerFunction)}: Failed. {ex.Message}");
        }
        
        _logger.LogInformation($"{nameof(DailyTriggerFunction)}: completed at: {DateTime.UtcNow}");
    }
}