using MongoDB.Bson;
using TokenIndexer.Data.Entities;

namespace TokenIndexer.Domain.Entities.Db;

[BsonCollection("wallets")]
public record WalletItem(string Id, BsonTimestamp Timestamp, string Wallet, string TwitterHandle) : IDocument
{
    public decimal RecentTokenDelta { get; set; }
}