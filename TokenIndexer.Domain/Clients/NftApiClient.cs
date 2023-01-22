using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using TokenIndexer.Domain.Responses.NFT;

namespace TokenIndexer.Domain.Clients;

public class NftApiClient : INftApiClient
{
    private const string MetaDataPath = "/v0/tokens/metadata";
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    
    public NftApiClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["ApiKey"] ?? throw new ArgumentNullException(nameof(_apiKey));
    }
    
    public async Task<List<MetadataResponse>> GetTokenMetaData(IEnumerable<string> mintAccounts)
    {
        var request = new MetadataRequest
        {
            MintAccounts = mintAccounts
        };

        var jsonSerializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var response = await _httpClient.PostAsJsonAsync($"{MetaDataPath}{_apiKey}", request, jsonSerializerOptions);
        
        return await response.Content.ReadFromJsonAsync<List<MetadataResponse>>() ?? new List<MetadataResponse>();
    }
}