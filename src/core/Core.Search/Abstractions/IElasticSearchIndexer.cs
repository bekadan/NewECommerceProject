namespace Core.Search.Abstractions;

public interface IElasticSearchIndexer
{
    /// <summary>
    /// Indexes the given document into the specified Elasticsearch index.
    /// </summary>
    /// <typeparam name="T">The type of the document.</typeparam>
    /// <param name="indexName">The name of the Elasticsearch index.</param>
    /// <param name="document">The document to index.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task IndexAsync<T>(string indexName, T document) where T : class;
}
