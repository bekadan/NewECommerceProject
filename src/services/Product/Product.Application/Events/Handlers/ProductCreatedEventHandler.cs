using Core.Logging.Abstractions;
using Core.Search.Abstractions;
using Product.Domain.Events;

namespace Product.Application.Events.Handlers;

public class ProductCreatedEventHandler
: ElasticSearchEventHandlerBase<ProductCreatedEvent>
{
    public ProductCreatedEventHandler(IElasticSearchIndexer indexer, ILogger logger)
        : base(indexer, logger) { }

    protected override string IndexName => "products";

    protected override string DocumentId(ProductCreatedEvent @event) => @event.Id.ToString();

    protected override object MapToDocument(ProductCreatedEvent @event) => new
    {
        @event.Id,
        @event.Name,
        @event.Price,
        @event.OccurredOn
    };
}
