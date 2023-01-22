using MongoDB.Driver;
using TokenIndexer.Data.Entities;

namespace TokenIndexer.Data.Repositories;

public interface IDbRepository<TDocument> where TDocument : IDocument
{
    Task<TDocument?> GetById(string id);
    Task<TDocument?> GetByFilter(FilterDefinition<TDocument> filter);
    Task<(IEnumerable<TDocument>, long)> GetPaginated(int page, int pageSize);
    Task<(IEnumerable<TDocument>, long)> Find(FilterDefinition<TDocument> filter, int page, int pageSize);
    Task<long> GetCollectionItemCount();
    Task<TDocument> Create(TDocument document);
    Task<IEnumerable<TDocument>> Create(ICollection<TDocument> documents);
    Task<TDocument> Replace(TDocument document);
    Task DeleteById(string id);
    Task DeleteAll();
}