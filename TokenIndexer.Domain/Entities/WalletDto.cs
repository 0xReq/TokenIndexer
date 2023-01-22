using MongoDB.Bson;

namespace TokenIndexer.Domain.Entities;

public record WalletDto(string Wallet, BsonTimestamp Timestamp, string TwitterHandle, decimal RecentTokenDelta);