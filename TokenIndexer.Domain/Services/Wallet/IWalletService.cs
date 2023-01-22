using TokenIndexer.Domain.Entities;

namespace TokenIndexer.Domain.Services.Wallet;

public interface IWalletService
{
    Task<(IEnumerable<WalletDto>, long)> GetPaginated(int page, int pageSize);
    Task<WalletDto?> GetByWallet(string wallet);
    Task<WalletDto> Create(string wallet, string twitterHandle);
    Task Update(string wallet, decimal recentTokenDelta);
    Task<long> GetTotalCount();
}