using TokenIndexer.Domain.Entities;

namespace TokenIndexer.Domain.Services.Snapshot;

public interface ISnapshotService
{
    Task CreateSnapshot(SnapshotDto snapshotDto);
    Task<IEnumerable<SnapshotDto>> GetTokens(string wallet, int page, int pageSize);
    Task<IEnumerable<SnapshotDto>> GetTokens(int page, int pageSize);
}