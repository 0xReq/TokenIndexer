using TokenIndexer.Domain.Entities.Enums;

namespace TokenIndexer.Domain.Services.Trigger;

public interface ITriggerService
{
    Task Run(EventType eventType);
}