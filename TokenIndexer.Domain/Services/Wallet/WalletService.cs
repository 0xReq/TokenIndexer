using AutoMapper;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using TokenIndexer.Data.Repositories;
using TokenIndexer.Domain.Entities;
using TokenIndexer.Domain.Entities.Db;
using TokenIndexer.Domain.Helpers;

namespace TokenIndexer.Domain.Services.Wallet;

public class WalletService : IWalletService
{
    private readonly ILogger<WalletService> _logger;
    private readonly IDbRepository<WalletItem> _walletRepository;
    private readonly IMapper _mapper;

    public WalletService(ILogger<WalletService> logger, IDbRepository<WalletItem> walletRepository, IMapper mapper)
    {
        _logger = logger;
        _walletRepository = walletRepository;
        _mapper = mapper;
    }

    public async Task<(IEnumerable<WalletDto>, long)> GetPaginated(int page, int pageSize)
    {
        try
        {
             (var walletItems, long totalCount) = await _walletRepository.GetPaginated(page, pageSize);
            
            return (_mapper.Map<IEnumerable<WalletDto>>(walletItems), totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(GetPaginated)} Failed");
            throw;
        }
    }

    public async Task<WalletDto?> GetByWallet(string wallet)
    {
        try
        {
            var filter = Builders<WalletItem>.Filter.Eq(doc => doc.Wallet, wallet);
            var walletItem = await _walletRepository.GetByFilter(filter);
            return _mapper.Map<WalletDto>(walletItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(GetByWallet)} Failed");
            throw;
        }
    }

    public async Task<WalletDto> Create(string wallet, string twitterHandle)
    {
        try
        {
            var id = ObjectId.GenerateNewId().ToString();
            var timestamp = DateTimeHelpers.GetTimestampFromDateTime(DateTime.UtcNow);

            var walletItem = await _walletRepository.Create(new WalletItem(id, timestamp, wallet, twitterHandle));

            return _mapper.Map<WalletDto>(walletItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(Create)} Failed for wallet: {wallet}");
            throw;
        }
    }

    public async Task Update(string wallet, decimal recentTokenDelta)
    {
        try
        {
            var filter = Builders<WalletItem>.Filter.Eq(doc => doc.Wallet, wallet);
            var existingItem = await _walletRepository.GetByFilter(filter);

            if (existingItem != null)
            {
                existingItem.RecentTokenDelta = recentTokenDelta;
                await _walletRepository.Replace(existingItem); 
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(GetTotalCount)} Failed");
            throw;
        }
    }

    public async Task<long> GetTotalCount()
    {
        try
        {
            return await _walletRepository.GetCollectionItemCount();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(GetTotalCount)} Failed");
            throw;
        }
    }
}