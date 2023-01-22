using System.Text.Json.Serialization;

namespace TokenIndexer.Domain.Responses.NFT;

public record MetadataResponse
{
    [JsonPropertyName("mint")]
    public string Mint{ get; set; }
    
    [JsonPropertyName("offChainData")]
    public OffChainData? OffChainData { get; set; }
    
}

public record OffChainData
{
    [JsonPropertyName("image")]
    public string ImageUrl { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }
    
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }
}