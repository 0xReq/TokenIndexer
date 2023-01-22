using TokenIndexer.Domain.Responses.NFT;

namespace TokenIndexer.Domain.Clients;

public interface INftApiClient
{
    Task<List<MetadataResponse>> GetTokenMetaData(IEnumerable<string> mintAccounts);
}