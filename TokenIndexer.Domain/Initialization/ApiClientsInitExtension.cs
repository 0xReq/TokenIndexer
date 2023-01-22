using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using TokenIndexer.Domain.Clients;

namespace TokenIndexer.Domain.Initialization;

public static class ApiClientsInitExtension
{
    public static void AddRpcClient(this IServiceCollection services, IConfiguration configuration)
    {
        string rpcBaseAddress = configuration["RpcBaseAddress"] ?? throw new ArgumentNullException(nameof(rpcBaseAddress));
        
        services
            .AddHttpClient<IRpcClient, RpcClient>(c =>
            {
                c.BaseAddress = new Uri(rpcBaseAddress);
                c.DefaultRequestHeaders.Add("Accept", "application/json");
                c.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                c.Timeout = TimeSpan.FromSeconds(30);
            })
            .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
            })
            .AddPolicyHandler(GetRetryPolicy());
    }

    public static void AddNftApiClient(this IServiceCollection services, IConfiguration configuration)
    {
        string nftApiBaseAddress = configuration["NftApiBaseAddress"] ?? throw new ArgumentNullException(nameof(nftApiBaseAddress));

        services
            .AddHttpClient<INftApiClient, NftApiClient>(c =>
            {
                c.BaseAddress = new Uri(nftApiBaseAddress);
                c.DefaultRequestHeaders.Add("Accept", "application/json");
                c.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                c.Timeout = TimeSpan.FromSeconds(30);
            })
            .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli
            })
            .AddPolicyHandler(GetRetryPolicy());
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
     {
         return HttpPolicyExtensions
             .HandleTransientHttpError()
             .Or<TimeoutRejectedException>()
             .WaitAndRetryAsync(
                 2,
                 retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
             )
             .WrapAsync(Policy.TimeoutAsync(20));
     }
}