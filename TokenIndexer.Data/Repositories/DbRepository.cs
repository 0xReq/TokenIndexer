using MongoDB.Driver;
using TokenIndexer.Data.Entities;

namespace TokenIndexer.Data.Repositories;

public class DbRepository<TDocument> : IDbRepository<TDocument>
    where TDocument : IDocument
{
    private readonly IMongoCollection<TDocument> _collection;
    
    public DbRepository(IMongoDbSettings settings, CreateIndexModel<TDocument>? indexModel)
    {
        var database = new MongoClient(settings.ConnectionString).GetDatabase(settings.DatabaseName);
        _collection = database.GetCollection<TDocument>(GetCollectionName(typeof(TDocument)));
        
        if(indexModel != null) 
            _collection.Indexes.CreateOne(indexModel, cancellationToken: default);
    }
    
    public async Task<TDocument?> GetById(string id)
    {
        var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);
        return await _collection.Find(filter).SingleOrDefaultAsync();
    }
    
    public async Task<TDocument?> GetByFilter(FilterDefinition<TDocument> filterDefinition)
    {
        return await _collection.Find(filterDefinition).SingleOrDefaultAsync();
    }
    
    public async Task<(IEnumerable<TDocument>, long)> GetPaginated(int page, int pageSize)
    {
        try
        {
            var filter = Builders<TDocument>.Filter.Empty; // Matches everything
            
            var (countFacet, dataFacet) = GetAggregateFacets(page, pageSize);
            
            var aggregation = await _collection.Aggregate()
                .Match(filter)
                .Facet(countFacet, dataFacet)
                .ToListAsync();
            
            var count = aggregation.First()
                .Facets.First(x => x.Name == "count")
                .Output<AggregateCountResult>()
                ?.FirstOrDefault()
                ?.Count ?? 0;
            
            var totalPages = (int)count / pageSize;
            
            var data = aggregation.First()
                .Facets.First(x => x.Name == "data")
                .Output<TDocument>();
            
            return (data, totalPages);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    
    public async Task<(IEnumerable<TDocument>, long)> Find(FilterDefinition<TDocument> filterDefinition, int page, int pageSize)
    {
        try
        {
            var (countFacet, dataFacet) = GetAggregateFacets(page, pageSize);
            
            var aggregation = await _collection.Aggregate()
                .Match(filterDefinition)
                .Facet(countFacet, dataFacet)
                .ToListAsync();
            
            var count = aggregation.First()
                .Facets.First(x => x.Name == "count")
                .Output<AggregateCountResult>()
                ?.FirstOrDefault()
                ?.Count ?? 0;

            var totalPages = (int)count / pageSize;
            
            var data = aggregation.First()
                .Facets.First(x => x.Name == "data")
                .Output<TDocument>();
            
            return (data, totalPages);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<long> GetCollectionItemCount()
    {
        try
        {
            var filter = Builders<TDocument>.Filter.Empty; // Matches everything
            
            var (countFacet, dataFacet) = GetAggregateFacets(1, 1);
            
            var aggregation = await _collection.Aggregate()
                .Match(filter)
                .Facet(countFacet, dataFacet)
                .ToListAsync();
            
            var count = aggregation.First()
                .Facets.First(x => x.Name == "count")
                .Output<AggregateCountResult>()
                ?.FirstOrDefault()
                ?.Count ?? 0;

            return count;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<TDocument> Create(TDocument document)
    {
        try
        {
            await _collection.InsertOneAsync(document);
            return document;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    
    public async Task<IEnumerable<TDocument>> Create(ICollection<TDocument> documents)
    {
        try
        {
            await _collection.InsertManyAsync(documents);
            return documents;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    
    public async Task<TDocument> Replace(TDocument document)
    {
        try
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
            await _collection.FindOneAndReplaceAsync(filter, document);

            return document;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task DeleteAll()
    {
        try
        {
            var filter = Builders<TDocument>.Filter.Empty; // Matches everything
            await _collection.DeleteManyAsync(filter);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    
    public async Task DeleteById(string id)
    {
        try
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);
            await _collection.FindOneAndDeleteAsync(filter);
        }
        catch (Exception ex)
        {
            throw;
        }
    }
    
    private string GetCollectionName(Type documentType)
    {
        return ((BsonCollectionAttribute) documentType.GetCustomAttributes(
                typeof(BsonCollectionAttribute),
                true)
            .FirstOrDefault()!).CollectionName;
    }
    
    private (AggregateFacet<TDocument, AggregateCountResult>, AggregateFacet<TDocument, TDocument>) GetAggregateFacets(int page, int pageSize)
    {
        var countFacet = AggregateFacet.Create("count",
            PipelineDefinition<TDocument, AggregateCountResult>.Create(new[]
            {
                PipelineStageDefinitionBuilder.Count<TDocument>()
            }));
            
        var dataFacet = AggregateFacet.Create("data",
            PipelineDefinition<TDocument, TDocument>.Create(new[]
            {
                PipelineStageDefinitionBuilder.Sort(Builders<TDocument>.Sort.Descending(x => x.Timestamp)),
                PipelineStageDefinitionBuilder.Skip<TDocument>((page - 1) * pageSize),
                PipelineStageDefinitionBuilder.Limit<TDocument>(pageSize)
            }));

        return (countFacet, dataFacet);
    }
}