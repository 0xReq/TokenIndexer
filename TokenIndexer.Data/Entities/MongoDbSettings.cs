namespace TokenIndexer.Data.Entities;

public interface IMongoDbSettings
{
    string DatabaseName { get; init; }
    string ConnectionString { get; init; }
}

public record MongoDbSettings(string DatabaseName, string ConnectionString) : IMongoDbSettings;
