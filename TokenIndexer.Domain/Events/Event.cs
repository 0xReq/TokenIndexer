using TokenIndexer.Domain.Entities.Enums;

namespace TokenIndexer.Domain.Events;

public record Event(string EventId, string EventName, string EventSource, EventType EventType, DateTime EventDate, IEnumerable<string> EventMessage);