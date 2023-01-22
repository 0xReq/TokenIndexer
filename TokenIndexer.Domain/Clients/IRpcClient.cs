using TokenIndexer.Domain.Responses.RPC;

namespace TokenIndexer.Domain.Clients;

public interface IRpcClient
{
    Task<TokenResult> GetTokensByOwner(string wallet, string programId);
    Task<ulong> GetBalance(string wallet);
}