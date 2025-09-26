using Core.Search.Abstractions;
using Core.Search.Configurations;
using Core.Search.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nest;

namespace Core.Search;

public static class DependencyInjection
{
    public static IServiceCollection AddCoreSearch(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ElasticSearchOptions>(configuration.GetSection("ElasticSearch"));

        services.AddSingleton<IElasticClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<ElasticSearchOptions>>().Value;
            var settings = new ConnectionSettings(new Uri(options.Url))
                .DefaultIndex(options.DefaultIndex);

            return new ElasticClient(settings);
        });

        services.AddSingleton<IElasticSearchIndexer, ElasticSearchIndexer>();

        services.AddScoped(typeof(ISearchService<>), typeof(ElasticSearchService<>));

        return services;
    }
}
