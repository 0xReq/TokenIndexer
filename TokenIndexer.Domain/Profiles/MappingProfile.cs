using AutoMapper;
using TokenIndexer.Domain.Entities;
using TokenIndexer.Domain.Entities.Db;

namespace TokenIndexer.Domain.Profiles;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        #region Wallet
        CreateMap<WalletItem, WalletDto>();
        CreateMap<WalletDto, WalletItem>();

        #endregion
        
        #region Snapshot
        CreateMap<DailySnapshotItem, SnapshotDto>();
        #endregion
    }
}