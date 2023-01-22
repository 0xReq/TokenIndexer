using MongoDB.Bson;
using TokenIndexer.Data.Entities;

namespace TokenIndexer.Domain.Entities.Db;

[BsonCollection("dailySnapshots")]
public record DailySnapshotItem(string Id, BsonTimestamp Timestamp, string Wallet, ulong Lamports, IEnumerable<TokenModel> Tokens) : IDocument;