namespace TokenIndexer.Web.Entities;

public record ActivityData(DateTime Date, string Name, ActivityType ActivityType);

public enum ActivityType
{
    Added = 0,
    Removed = 10
}