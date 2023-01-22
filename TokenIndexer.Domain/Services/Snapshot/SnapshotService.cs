using AutoMapper;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using TokenIndexer.Data.Repositories;
using TokenIndexer.Domain.Entities;
using TokenIndexer.Domain.Entities.Db;
using TokenIndexer.Domain.Helpers;

namespace TokenIndexer.Domain.Services.Snapshot;

public class SnapshotService : ISnapshotService
{
    private readonly ILogger<SnapshotService> _logger;
    private readonly IDbRepository<DailySnapshotItem> _snapshotRepository;
    private readonly IMapper _mapper;

    public SnapshotService(ILogger<SnapshotService> logger,
        IDbRepository<DailySnapshotItem> dailySnapshotRepository, IMapper mapper)
    {
        _logger = logger;
        _snapshotRepository = dailySnapshotRepository;
        _mapper = mapper;
    }

    public async Task CreateSnapshot(SnapshotDto snapshotDto)
    {
        try
        {
            var id = ObjectId.GenerateNewId().ToString();
            var timestamp = DateTimeHelpers.GetTimestampFromDateTime(DateTime.UtcNow);

            await _snapshotRepository.Create(new DailySnapshotItem(id, timestamp, snapshotDto.Wallet, snapshotDto.Lamports,
                snapshotDto.Tokens));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(CreateSnapshot)} Failed saving snapshot for wallet: {snapshotDto.Wallet}");
            throw;
        }
    }

    public async Task<IEnumerable<SnapshotDto>> GetTokens(string wallet, int page, int pageSize)
    {
        try
        {
            var filter = Builders<DailySnapshotItem>.Filter.Eq(doc => doc.Wallet, wallet);
            var (tokens, totalCount) = await _snapshotRepository.Find(filter, page, pageSize);
            
            return _mapper.Map<IEnumerable<SnapshotDto>>(tokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(GetTokens)} Failed for wallet: {wallet}");
            throw;
        }
    }

    public async Task<IEnumerable<SnapshotDto>> GetTokens(int page, int pageSize)
    {
        try
        {
            var filter = Builders<DailySnapshotItem>.Filter.Empty;
            var (tokens, totalCount) = await _snapshotRepository.Find(filter, page, pageSize);
            
            return _mapper.Map<IEnumerable<SnapshotDto>>(tokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(GetTokens)} Failed");
            throw;
        }
    }
}