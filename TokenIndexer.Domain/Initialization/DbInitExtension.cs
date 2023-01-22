using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using TokenIndexer.Data.Entities;
using TokenIndexer.Data.Repositories;
using TokenIndexer.Domain.Entities.Db;

namespace TokenIndexer.Domain.Initialization;

public static class DbInitExtension
{
    public static void AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        string dbName = configuration["DbName"] ?? throw new ArgumentNullException(nameof(dbName));
        string connectionString = configuration["DbConnectionString"] ?? throw new ArgumentNullException(nameof(connectionString));

        services.AddSingleton<IMongoDbSettings>(_ 
            => new MongoDbSettings(dbName, connectionString));
        
        services.AddSingleton<IDbRepository<WalletItem>>(_ =>
            new DbRepository<WalletItem>(_.GetRequiredService<IMongoDbSettings>(), null));

        var dailyItemBuilder = Builders<DailySnapshotItem>.IndexKeys;
        var dailyIndexModel = new CreateIndexModel<DailySnapshotItem>(dailyItemBuilder.Ascending(x => x.Wallet));
        
        services.AddSingleton<IDbRepository<DailySnapshotItem>>(p => 
            new DbRepository<DailySnapshotItem>(p.GetRequiredService<IMongoDbSettings>(), dailyIndexModel));
    }
}