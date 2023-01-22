using System.Text.Json.Serialization;

namespace TokenIndexer.Domain.Responses.NFT;

public record MetadataRequest
{
    [JsonPropertyName("mintAccounts")]
    public IEnumerable<string> MintAccounts { get; set; }
}