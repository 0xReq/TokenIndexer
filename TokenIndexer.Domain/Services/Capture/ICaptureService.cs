using TokenIndexer.Domain.Entities.Enums;

namespace TokenIndexer.Domain.Services.Capture;

public interface ICaptureService
{
    Task ProcessMessage(IEnumerable<string> wallets, EventType eventType);
}