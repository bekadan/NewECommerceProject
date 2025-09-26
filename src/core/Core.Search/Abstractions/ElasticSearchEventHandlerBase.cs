using Core.BackgroundJobs.Abstractions;
using Core.Events.Abstractions;
using Core.Logging.Abstractions;

namespace Core.Search.Abstractions;

public abstract class ElasticSearchEventHandlerBase<TEvent> : IBackgroundJobHandler<TEvent>
where TEvent : IIntegrationEvent
{
    private readonly IElasticSearchIndexer _indexer;
    private readonly ILogger _logger;

    protected ElasticSearchEventHandlerBase(IElasticSearchIndexer indexer, ILogger logger)
    {
        _indexer = indexer;
        _logger = logger;
    }

    protected abstract string IndexName { get; }
    protected abstract string DocumentId(TEvent @event);
    protected abstract object MapToDocument(TEvent @event);

    public async Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default)
    {
        var eventName = typeof(TEvent).Name;

        _logger.Information("📬 Handling event {EventName} ({EventId}) occurred at {OccurredOn}",
            eventName, @event.Id, @event.OccurredOn);

        try
        {
            var document = MapToDocument(@event);

            await _indexer.IndexAsync(IndexName, document);

            _logger.Information("✅ Indexed {EventType} into {IndexName}", typeof(TEvent).Name, IndexName);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "❌ Failed to index event {EventName} ({EventId}) into Elasticsearch",
                eventName, @event.Id);
            throw;
        }

       
    }
}
