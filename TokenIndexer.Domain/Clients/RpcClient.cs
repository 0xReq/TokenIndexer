using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using TokenIndexer.Domain.Requests.RPC;
using TokenIndexer.Domain.Responses.RPC;

namespace TokenIndexer.Domain.Clients;

public class RpcClient : IRpcClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public RpcClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["ApiKey"] ?? throw new ArgumentNullException(nameof(_apiKey));
    }

    public async Task<TokenResult> GetTokensByOwner(string wallet, string programId)
    {
        var request = new JsonRequest
        {
            Method = "getTokenAccountsByOwner",
            Params = new List<object>
            {
                wallet,
                new JsonRequestProgram {ProgramId = programId},
                new JsonRequestEncoding {Encoding = "jsonParsed"}
            }
        };

        var response = await _httpClient.PostAsJsonAsync(_apiKey, request, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        var result = await response.Content.ReadFromJsonAsync<JsonResponse<TokenResult>>();

        return result.Result;
    }

    public async Task<ulong> GetBalance(string wallet)
    {
        var request = new JsonRequest
        {
            Method = "getBalance",
            Params = new List<object> {wallet}
        };

        var jsonSerializerOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        
        var response = await _httpClient.PostAsJsonAsync(_apiKey, request, jsonSerializerOptions);

        var result = await response.Content.ReadFromJsonAsync<JsonResponse<BalanceResult>>();

        return result.Result.Value;
    }
}