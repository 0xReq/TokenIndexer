namespace TokenIndexer.Domain.Helpers;

public static class ListHelpers
{
    public static List<List<T>> GetChunks<T>(List<T> collection, int batchCount) 
    {
        var chunks = new List<List<T>>();
        var chunkCount = collection.Count / batchCount;

        if (collection.Count % batchCount > 0)
            chunkCount++;

        for (var i = 0; i < chunkCount; i++)
            chunks.Add(collection.Skip(i * batchCount).Take(batchCount).ToList());

        return chunks;
    }
}