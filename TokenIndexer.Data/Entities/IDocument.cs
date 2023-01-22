using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TokenIndexer.Data.Entities;

public interface IDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    string Id { get; init; }
    
    [BsonRepresentation(BsonType.Timestamp)]
    BsonTimestamp Timestamp { get; init; }
}
