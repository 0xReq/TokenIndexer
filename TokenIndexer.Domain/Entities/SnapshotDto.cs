using MongoDB.Bson;

namespace TokenIndexer.Domain.Entities;

public record SnapshotDto(string Wallet, ulong Lamports)
{
    public IEnumerable<TokenModel> Tokens { get; set; }
    public BsonTimestamp Timestamp { get; set; }
}