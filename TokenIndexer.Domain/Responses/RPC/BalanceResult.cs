using System.Text.Json.Serialization;

namespace TokenIndexer.Domain.Responses.RPC;

public record BalanceResult
{
    [JsonPropertyName("value")]
    public ulong Value { get; set; }
}