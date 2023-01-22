using MongoDB.Bson;

namespace TokenIndexer.Domain.Helpers;

public static class DateTimeHelpers
{
    public static BsonTimestamp GetTimestampFromDateTime(DateTime dateTime)
    {
        var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var diff = dateTime.ToUniversalTime() - unixEpoch;
        var seconds = (diff.TotalMilliseconds + 18000000) / 1000;
        return new BsonTimestamp((int)seconds, 1);
    }
    
    public static DateTime GetDateTimeFromTimestamp(BsonTimestamp timestamp)
    {
        var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return unixEpoch.AddSeconds(timestamp.Timestamp - 18000);
    }
}