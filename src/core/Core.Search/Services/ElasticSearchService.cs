using Core.Search.Abstractions;
using Core.Search.Configurations;
using Microsoft.Extensions.Options;
using Nest;

namespace Core.Search.Services;

public class ElasticSearchService<T> : ISearchService<T> where T : class
{
    private readonly ElasticClient _client;

    public ElasticSearchService(IOptions<ElasticSearchOptions> options)
    {
        var settings = new ConnectionSettings(new Uri(options.Value.Url))
            .DefaultIndex(options.Value.DefaultIndex)
            .EnableDebugMode();

        _client = new ElasticClient(settings);
    }

    public async Task IndexAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _client.IndexDocumentAsync(entity, ct: cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        await _client.DeleteAsync<T>(id, ct: cancellationToken);
    }

    public async Task<T?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync<T>(id, ct: cancellationToken);
        return response.Source;
    }

    public async Task<IReadOnlyCollection<T>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var response = await _client.SearchAsync<T>(s => s
            .Query(q => q
                .QueryString(d => d.Query(query))
            ), cancellationToken);

        return response.Documents;
    }
}
