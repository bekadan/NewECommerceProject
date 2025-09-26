using Core.Search.Abstractions;
using Microsoft.Extensions.Logging;
using Nest;

namespace Core.Search;

internal class ElasticSearchIndexer : IElasticSearchIndexer
{
    private readonly IElasticClient _client;
    private readonly ILogger<ElasticSearchIndexer> _logger;

    public ElasticSearchIndexer(IElasticClient client, ILogger<ElasticSearchIndexer> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task IndexAsync<T>(string indexName, T document) where T : class
    {
        var response = await _client.IndexAsync(document, i => i.Index(indexName));

        if (!response.IsValid)
        {
            _logger.LogError("Failed to index document into {IndexName}. Reason: {Reason}", indexName, response.ServerError?.Error.Reason);
            throw new Exception($"Failed to index document: {response.ServerError?.Error.Reason}");
        }

        _logger.LogInformation("✅ Successfully indexed document into {IndexName}", indexName);
    }
}
